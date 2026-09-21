using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Decides which thought surfaces when a resolution point holds more than one candidate.
/// The base is how alive the memory is (<see cref="ThoughtEntry.Strength"/>); on top of that
/// the pick is weighted by recency, relevancy to the path being explored, personality (traits)
/// and whether the player has already drawn it out before.
/// </summary>
public static class ThoughtSelector
{
    public static ThoughtEntry Pick(ThoughtCharacter character, IEnumerable<ThoughtEntry> pool, string nodeId, long now, System.Random rng)
    {
        if (character == null || pool == null)
            return null;

        var profile = character.Traits != null ? character.Traits.Aggregate(character.TraitIds) : null;
        var taxonomy = character.Taxonomy;

        var candidates = new List<ThoughtEntry>();
        var weights = new List<float>();
        float total = 0f;
        foreach (var entry in pool)
        {
            float score = Score(entry, taxonomy, profile, nodeId, now);
            if (score <= 0.0001f)
                continue;
            candidates.Add(entry);
            weights.Add(score);
            total += score;
        }

        if (candidates.Count == 0)
            return null;

        float roll = (float)rng.NextDouble() * total;
        for (int i = 0; i < candidates.Count; i++)
        {
            roll -= weights[i];
            if (roll <= 0f)
                return candidates[i];
        }
        return candidates[candidates.Count - 1];
    }

    public static float Score(ThoughtEntry entry, ThoughtTaxonomy taxonomy, TraitProfile profile, string nodeId, long now)
    {
        if (entry == null)
            return 0f;

        float strength = entry.Strength(now);
        if (strength <= 0.0001f)
            return 0f;

        return strength
             * Recency(entry, now)
             * Relevancy(taxonomy, entry, nodeId)
             * Personality(profile, taxonomy, entry, nodeId)
             * ReadPenalty(entry);
    }

    /// <summary>Recently formed or updated memories surface more readily.</summary>
    private static float Recency(ThoughtEntry entry, long now)
    {
        long last = entry.LastTick > entry.CreatedTick ? entry.LastTick : entry.CreatedTick;
        float days = (now - last) / (float)GameClock.MinutesPerDay;
        if (days < 0f)
            days = 0f;
        return 1f + 1f / (1f + days);
    }

    /// <summary>An exact hit on the path beats a loose relative; siblings fall off with distance.</summary>
    private static float Relevancy(ThoughtTaxonomy taxonomy, ThoughtEntry entry, string nodeId)
    {
        if (taxonomy == null || string.IsNullOrEmpty(nodeId))
            return 1f;
        if (entry.NodeId == nodeId)
            return 1.5f;
        int distance = Distance(taxonomy, entry.NodeId, nodeId);
        if (distance < 0)
            return 0.5f;
        return 1f / (1f + 0.5f * distance);
    }

    /// <summary>
    /// What the character is like: talkative about eager topics, and temperament/outlook warp
    /// which of its memories feel most present.
    /// </summary>
    private static float Personality(TraitProfile profile, ThoughtTaxonomy taxonomy, ThoughtEntry entry, string nodeId)
    {
        if (profile == null)
            return 1f;

        float factor = 1f;
        if (MatchesOrAncestor(taxonomy, nodeId, profile.EagerNodes))
            factor *= 1.6f;
        if (MatchesOrAncestor(taxonomy, nodeId, profile.AvoidNodes))
            factor *= 0.4f;

        // Hot-headed characters dwell on impactful memories; stoic ones let them lie.
        factor *= Mathf.Clamp(1f + (profile.ImpactMultiplier - 1f) * entry.Impact, 0.4f, 1.8f);

        // Optimists surface the good, pessimists the bad.
        factor *= Mathf.Clamp(1f + profile.ValenceBias * entry.Sentiment, 0.4f, 1.8f);
        return factor;
    }

    /// <summary>Anything already read drops back so new material gets its turn.</summary>
    private static float ReadPenalty(ThoughtEntry entry)
    {
        return 1f / (1f + 2f * entry.TimesSurfaced);
    }

    private static int Distance(ThoughtTaxonomy taxonomy, string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return -1;
        var chainA = Chain(taxonomy, a);
        var chainB = Chain(taxonomy, b);
        for (int i = 0; i < chainA.Count; i++)
        {
            int j = chainB.IndexOf(chainA[i]);
            if (j >= 0)
                return i + j;
        }
        return -1;
    }

    private static List<string> Chain(ThoughtTaxonomy taxonomy, string id)
    {
        var chain = new List<string>();
        var node = taxonomy.Get(id);
        int guard = 0;
        while (node != null && guard++ < 64)
        {
            chain.Add(node.Id);
            node = taxonomy.Get(node.ParentId);
        }
        return chain;
    }

    private static bool MatchesOrAncestor(ThoughtTaxonomy taxonomy, string nodeId, List<string> ids)
    {
        if (ids == null || ids.Count == 0 || string.IsNullOrEmpty(nodeId))
            return false;
        string current = nodeId;
        int guard = 0;
        while (!string.IsNullOrEmpty(current) && guard++ < 64)
        {
            for (int i = 0; i < ids.Count; i++)
                if (ids[i] == current)
                    return true;
            var node = taxonomy != null ? taxonomy.Get(current) : null;
            current = node == null ? "" : node.ParentId;
        }
        return false;
    }
}
