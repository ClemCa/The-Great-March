using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turns a live event into a thought for one character. Relevance, impact and retention are
/// all warped by the character's traits (an empath feels distant world events; a stoic shrugs).
/// </summary>
public static class ThoughtPropagation
{
    public static bool Apply(GameEvent ev, ThoughtCharacter character, System.Random rng)
    {
        if (ev == null || character == null || character.Taxonomy == null || character.Traits == null)
            return false;

        var profile = character.Traits.Aggregate(character.TraitIds);
        float relevance = Relevance(ev, character, profile);
        if (relevance <= 0.001f)
            return false;

        float sentiment = Mathf.Clamp(ev.BaseSentiment * profile.SentimentMultiplier, -1f, 1f);
        float impact = Mathf.Clamp01(ev.Impact * relevance * profile.ImpactMultiplier);
        float decay = Mathf.Clamp01(ev.Decay * profile.DecayMultiplier);

        var nodes = RelevantNodes(ev, character);
        for (int i = 0; i < nodes.Count; i++)
        {
            character.Thoughts.Add(new ThoughtEntry
            {
                NodeId = nodes[i],
                Subject = ev.Name,
                Sentiment = sentiment,
                Impact = impact,
                Decay = decay,
                CreatedTick = ev.StartedTick,
                LastTick = ev.StartedTick,
                SourceEventId = ev.Id,
                Tags = new List<string>(ev.Tags)
            });
        }
        return true;
    }

    /// <summary>
    /// The event colours every topic it touches that the character has a link to (or that is
    /// universally held, like opinions/feelings); otherwise it lands on the scope's default node.
    /// </summary>
    private static List<string> RelevantNodes(GameEvent ev, ThoughtCharacter character)
    {
        var nodes = new List<string>();
        for (int i = 0; i < ev.AffectedNodeIds.Count; i++)
        {
            var node = ev.AffectedNodeIds[i];
            if (!character.Taxonomy.Exists(node) || nodes.Contains(node))
                continue;
            if (character.KnowsNode(character.Taxonomy, node) || IsUniversal(character.Taxonomy, node))
                nodes.Add(node);
        }
        if (nodes.Count == 0)
            nodes.Add(ScopeDefault(ev.Scope));
        return nodes;
    }

    private static bool IsUniversal(ThoughtTaxonomy taxonomy, string nodeId)
    {
        var root = RootOf(taxonomy, nodeId);
        return root == "opinions" || root == "feelings";
    }

    private static string RootOf(ThoughtTaxonomy taxonomy, string nodeId)
    {
        var node = taxonomy.Get(nodeId);
        int guard = 0;
        while (node != null && !string.IsNullOrEmpty(node.ParentId) && guard++ < 64)
            node = taxonomy.Get(node.ParentId);
        return node == null ? "" : node.Id;
    }

    private static float Relevance(GameEvent ev, ThoughtCharacter character, TraitProfile profile)
    {
        if (ev.AffectedCharacterIds.Contains(character.Id))
            return 1f;

        float relevance = 0f;

        for (int i = 0; i < ev.AffectedNodeIds.Count; i++)
        {
            if (character.KnowsNode(character.Taxonomy, ev.AffectedNodeIds[i]))
            {
                relevance = Mathf.Max(relevance, 0.8f);
            }
        }

        if (relevance <= 0.001f && profile.Empath && (ev.Scope == EventScope.Region || ev.Scope == EventScope.World))
            relevance = 0.5f;

        if (relevance <= 0.001f && ev.Scope == EventScope.World)
            relevance = 0.25f;

        return relevance;
    }

    private static string ScopeDefault(EventScope scope)
    {
        switch (scope)
        {
            case EventScope.Personal:
                return "history/recent";
            case EventScope.Local:
                return "present/news";
            default:
                return "world/events";
        }
    }
}
