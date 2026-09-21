using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every thought a single character holds, with decay and weighted selection helpers.
/// </summary>
[Serializable]
public class ThoughtStore
{
    public List<ThoughtEntry> Entries = new List<ThoughtEntry>();

    private int _counter = 0;

    public string NextId()
    {
        _counter++;
        return "t" + _counter.ToString();
    }

    public ThoughtEntry Add(ThoughtEntry entry)
    {
        if (entry == null)
            return null;
        if (string.IsNullOrEmpty(entry.Id))
            entry.Id = NextId();
        if (entry.LastTick == 0L)
            entry.LastTick = GameClock.Now;
        Entries.Add(entry);
        return entry;
    }

    public List<ThoughtEntry> AtNode(string nodeId)
    {
        var result = new List<ThoughtEntry>();
        for (int i = 0; i < Entries.Count; i++)
            if (Entries[i].NodeId == nodeId)
                result.Add(Entries[i]);
        return result;
    }

    public List<ThoughtEntry> UnderNode(ThoughtTaxonomy taxonomy, string nodeId)
    {
        var ids = new HashSet<string>(taxonomy.Descendants(nodeId, true));
        var result = new List<ThoughtEntry>();
        for (int i = 0; i < Entries.Count; i++)
            if (ids.Contains(Entries[i].NodeId))
                result.Add(Entries[i]);
        return result;
    }

    public bool HasAny(ThoughtTaxonomy taxonomy, string nodeId)
    {
        var ids = taxonomy.Descendants(nodeId, true);
        for (int i = 0; i < Entries.Count; i++)
            if (ids.Contains(Entries[i].NodeId))
                return true;
        return false;
    }

    public int Purge(long now, float minStrength)
    {
        int removed = 0;
        for (int i = Entries.Count - 1; i >= 0; i--)
        {
            if (Entries[i].Strength(now) < minStrength)
            {
                Entries.RemoveAt(i);
                removed++;
            }
        }
        return removed;
    }

    public ThoughtEntry WeightedPick(IEnumerable<ThoughtEntry> pool, long now, System.Random rng)
    {
        var list = new List<ThoughtEntry>();
        float total = 0f;
        foreach (var e in pool)
        {
            float w = e.Strength(now);
            if (w <= 0.0001f)
                continue;
            list.Add(e);
            total += w;
        }
        if (list.Count == 0)
            return null;
        float roll = (float)rng.NextDouble() * total;
        for (int i = 0; i < list.Count; i++)
        {
            roll -= list[i].Strength(now);
            if (roll <= 0f)
                return list[i];
        }
        return list[list.Count - 1];
    }

    /// <summary>
    /// Strength-weighted average sentiment, remapped into 0..1 mood space (0.5 = neutral).
    /// </summary>
    public float AverageValence(IEnumerable<ThoughtEntry> pool, long now)
    {
        float sum = 0f;
        float weight = 0f;
        foreach (var e in pool)
        {
            float w = e.Strength(now);
            sum += e.Sentiment * w;
            weight += w;
        }
        if (weight <= 0.0001f)
            return 0.5f;
        return Mathf.Clamp01(0.5f + 0.5f * (sum / weight));
    }

    public float StrongestImpact(IEnumerable<ThoughtEntry> pool, long now)
    {
        float max = 0f;
        foreach (var e in pool)
        {
            float s = e.Strength(now);
            if (s > max)
                max = s;
        }
        return max;
    }

    public ThoughtStore Clone()
    {
        var copy = new ThoughtStore();
        copy._counter = _counter;
        for (int i = 0; i < Entries.Count; i++)
            copy.Entries.Add(Entries[i].Clone());
        return copy;
    }
}
