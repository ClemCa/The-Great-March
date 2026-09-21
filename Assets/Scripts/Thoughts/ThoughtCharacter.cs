using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class TalkPolicyRule
{
    public string NodeId = "";
    public TalkPolicy Policy = TalkPolicy.Open;
}

/// <summary>
/// A living character: traits, moods, thoughts, discussion rules, relationships and knowledge.
/// The Taxonomy and TraitLibrary references are runtime-only and re-injected on load.
/// </summary>
[Serializable]
public class ThoughtCharacter
{
    public string Id = "";
    public string DisplayName = "";
    public string Gender = "neutral";
    public bool Canonical = false;   // fixed-history character; major random events never touch them

    public List<string> TraitIds = new List<string>();
    public List<MoodInfo> Moods = MoodResolver.CreateStarter();
    public List<TalkPolicyRule> TalkPolicies = new List<TalkPolicyRule>();
    public List<RelationshipInfo> Relationships = new List<RelationshipInfo>();
    public List<KnowledgeInfo> Knowledge = new List<KnowledgeInfo>();
    public List<string> AuthoredNotes = new List<string>();
    public List<string> ExampleSentences = new List<string>();

    public ThoughtStore Thoughts = new ThoughtStore();
    public ConversationHistory History = new ConversationHistory();
    public string CurrentMoodId = "normal";

    [JsonIgnore, System.NonSerialized] public ThoughtTaxonomy Taxonomy;
    [JsonIgnore, System.NonSerialized] public TraitLibrary Traits;

    public ThoughtCharacter()
    {
    }

    public ThoughtCharacter(string id, string displayName, ThoughtTaxonomy taxonomy, TraitLibrary traits)
    {
        Id = id;
        DisplayName = displayName;
        Taxonomy = taxonomy;
        Traits = traits;
    }

    public void Bind(ThoughtTaxonomy taxonomy, TraitLibrary traits)
    {
        Taxonomy = taxonomy;
        Traits = traits;
    }

    public MoodInfo CurrentMood
    {
        get
        {
            for (int i = 0; i < Moods.Count; i++)
                if (Moods[i].Id == CurrentMoodId)
                    return Moods[i];
            return Moods.Count > 0 ? Moods[0] : null;
        }
    }

    public MoodInfo RefreshMood(long now)
    {
        var resolved = MoodResolver.Resolve(Thoughts, Moods, now, CurrentMood);
        if (resolved != null)
            CurrentMoodId = resolved.Id;
        return resolved;
    }

    /// <summary>
    /// Most specific authored rule wins; otherwise traits decide (avoid nodes imply reluctance).
    /// </summary>
    public TalkPolicy PolicyFor(string nodeId)
    {
        if (Taxonomy == null || string.IsNullOrEmpty(nodeId))
            return TalkPolicy.Open;

        var current = nodeId;
        int guard = 0;
        while (!string.IsNullOrEmpty(current) && guard++ < 64)
        {
            for (int i = 0; i < TalkPolicies.Count; i++)
                if (TalkPolicies[i].NodeId == current)
                    return TalkPolicies[i].Policy;
            var node = Taxonomy.Get(current);
            current = node == null ? "" : node.ParentId;
        }

        if (Traits != null)
        {
            var profile = Traits.Aggregate(TraitIds);
            current = nodeId;
            guard = 0;
            while (!string.IsNullOrEmpty(current) && guard++ < 64)
            {
                if (profile.AvoidNodes.Contains(current))
                    return TalkPolicy.Reluctant;
                if (profile.EagerNodes.Contains(current))
                    return TalkPolicy.Open;
                var node = Taxonomy.Get(current);
                current = node == null ? "" : node.ParentId;
            }
        }

        return TalkPolicy.Open;
    }

    public bool KnowsNode(ThoughtTaxonomy taxonomy, string nodeId)
    {
        for (int i = 0; i < Relationships.Count; i++)
            if (IsAtOrUnder(taxonomy, Relationships[i].NodeId, nodeId))
                return true;
        for (int i = 0; i < Knowledge.Count; i++)
            if (IsAtOrUnder(taxonomy, Knowledge[i].NodeId, nodeId))
                return true;
        return false;
    }

    private static bool IsAtOrUnder(ThoughtTaxonomy taxonomy, string candidate, string ancestor)
    {
        if (string.IsNullOrEmpty(candidate))
            return false;
        if (candidate == ancestor)
            return true;
        var current = taxonomy.Get(candidate);
        int guard = 0;
        while (current != null && guard++ < 64)
        {
            if (current.ParentId == ancestor)
                return true;
            current = taxonomy.Get(current.ParentId);
        }
        return false;
    }
}

/// <summary>
/// Global lookup for live characters, used by events to find who reacts.
/// </summary>
public static class ThoughtCharacterRegistry
{
    private static readonly Dictionary<string, ThoughtCharacter> _characters = new Dictionary<string, ThoughtCharacter>();

    public static event Action<ThoughtCharacter> Registered;

    public static IReadOnlyDictionary<string, ThoughtCharacter> All { get { return _characters; } }

    public static void Register(ThoughtCharacter character)
    {
        if (character == null || string.IsNullOrEmpty(character.Id))
            return;
        _characters[character.Id] = character;
        if (Registered != null)
            Registered(character);
    }

    public static void Unregister(string id)
    {
        if (!string.IsNullOrEmpty(id))
            _characters.Remove(id);
    }

    public static ThoughtCharacter Get(string id)
    {
        ThoughtCharacter c;
        _characters.TryGetValue(id, out c);
        return c;
    }

    public static void Clear()
    {
        _characters.Clear();
    }

    public static List<ThoughtCharacter> AllList()
    {
        return new List<ThoughtCharacter>(_characters.Values);
    }
}
