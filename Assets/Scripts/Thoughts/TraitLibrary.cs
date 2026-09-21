using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Aggregated effect of every trait a character has, used when receiving an event.
/// </summary>
public class TraitProfile
{
    public bool Empath;
    public float ImpactMultiplier = 1f;
    public float DecayMultiplier = 1f;
    public float SentimentMultiplier = 1f;
    public HashSet<string> Tags = new HashSet<string>();
    public List<string> EagerNodes = new List<string>();
    public List<string> AvoidNodes = new List<string>();
}

/// <summary>
/// Registry of trait definitions. Authored traits are loaded from data; CreateStarter provides
/// a playable baseline so the system works before any content exists.
/// </summary>
public class TraitLibrary
{
    private readonly Dictionary<string, TraitInfo> _traits = new Dictionary<string, TraitInfo>();

    public IReadOnlyDictionary<string, TraitInfo> All { get { return _traits; } }

    public void Add(TraitInfo trait)
    {
        if (trait == null || string.IsNullOrEmpty(trait.Id))
            return;
        _traits[trait.Id] = trait;
    }

    public TraitInfo Get(string id)
    {
        TraitInfo t;
        _traits.TryGetValue(id, out t);
        return t;
    }

    public bool Has(string id)
    {
        return !string.IsNullOrEmpty(id) && _traits.ContainsKey(id);
    }

    public TraitProfile Aggregate(IEnumerable<string> ids)
    {
        var profile = new TraitProfile();
        if (ids == null)
            return profile;
        foreach (var id in ids)
        {
            var t = Get(id);
            if (t == null)
                continue;
            profile.Empath |= t.Empath;
            profile.ImpactMultiplier *= t.ImpactMultiplier;
            profile.DecayMultiplier *= t.DecayMultiplier;
            profile.SentimentMultiplier *= t.SentimentMultiplier;
            for (int i = 0; i < t.Tags.Count; i++)
                profile.Tags.Add(t.Tags[i]);
            profile.EagerNodes.AddRange(t.EagerNodes);
            profile.AvoidNodes.AddRange(t.AvoidNodes);
        }
        return profile;
    }

    public static TraitLibrary CreateStarter()
    {
        var lib = new TraitLibrary();
        lib.Add(new TraitInfo
        {
            Id = "empathetic",
            Display = "Empathetic",
            Empath = true,
            ImpactMultiplier = 1.3f,
            DecayMultiplier = 0.7f
        });
        lib.Add(new TraitInfo
        {
            Id = "optimist",
            Display = "Optimist",
            SentimentMultiplier = 1.25f
        });
        lib.Add(new TraitInfo
        {
            Id = "pessimist",
            Display = "Pessimist",
            SentimentMultiplier = 1.25f,
            ImpactMultiplier = 1.1f
        });
        lib.Add(new TraitInfo
        {
            Id = "stoic",
            Display = "Stoic",
            ImpactMultiplier = 0.6f,
            DecayMultiplier = 1.4f
        });
        lib.Add(new TraitInfo
        {
            Id = "hotheaded",
            Display = "Hot-headed",
            ImpactMultiplier = 1.4f,
            DecayMultiplier = 1.6f
        });
        lib.Add(new TraitInfo
        {
            Id = "gossip",
            Display = "Gossip",
            EagerNodes = new List<string> { "world/rumors", "relationships/friends", "relationships/friends/acquaintances" }
        });
        lib.Add(new TraitInfo
        {
            Id = "private",
            Display = "Private",
            AvoidNodes = new List<string> { "relationships/family", "relationships/self", "history/personal" }
        });
        lib.Add(new TraitInfo
        {
            Id = "warm",
            Display = "Warm",
            EagerNodes = new List<string> { "relationships/friends", "relationships/family" },
            ImpactMultiplier = 1.1f
        });
        lib.Add(new TraitInfo
        {
            Id = "curious",
            Display = "Curious",
            EagerNodes = new List<string> { "knowledge", "world" }
        });
        return lib;
    }
}
