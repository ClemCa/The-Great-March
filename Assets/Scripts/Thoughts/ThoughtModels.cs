using System;
using System.Collections.Generic;
using UnityEngine;

public enum ThoughtNodeKind
{
    Branch,
    Leaf,
    BranchAndLeaf
}

/// <summary>
/// How a character reacts when a thought path holds no data.
/// </summary>
public enum TalkPolicy
{
    Open,       // talks readily, invents a plausible setting if none exists
    Reluctant,  // changes subject / gives a short non-answer
    Secretive,  // actively refuses
    Unknown,    // genuinely does not know
    Enthusiastic // eager to share, invents a setting with relish
}

public enum EventScope
{
    Personal,
    Local,
    Region,
    World
}

/// <summary>
/// How disruptive a (non-canon) event is. Major events are too world-shaping to be allowed to
/// touch canonical characters, whose history is fixed; minor beats can colour anyone.
/// </summary>
public enum EventImportance
{
    Minor,
    Major
}

/// <summary>
/// A single thing a character "has a feeling about". Mirrors the JSON ingested by the LLM:
/// ex { "Event X": { "sentiment": -0.6, "decay": 0.2, "impact": 0.7 } }
/// </summary>
[Serializable]
public class ThoughtEntry
{
    public string Id = "";
    public string NodeId = "";
    public string Subject = "";
    public float Sentiment = 0f;   // -1 (terrible) .. +1 (wonderful)
    public float Impact = 0.5f;    // 0..1 peak strength right after creation
    public float Decay = 0.05f;    // fraction of strength lost per in-game day
    public long CreatedTick = 0L;
    public long LastTick = 0L;
    public string SourceEventId = "";
    public List<string> Tags = new List<string>();
    public string RawLabel = "";   // short unstructured fragment, e.g. "Dad death"
    public string Detail = "";     // longer note used to seed the LLM
    public bool Authored = false;  // written by hand
    public bool Procedural = false; // invented at runtime
    public int TimesSurfaced = 0;   // how often the player has already drawn this out
    public long LastSurfacedTick = 0L;

    public float Strength(long now)
    {
        float days = (now - CreatedTick) / (float)GameClock.MinutesPerDay;
        if (days <= 0f)
            return Mathf.Clamp01(Impact);
        float remaining = Mathf.Pow(1f - Mathf.Clamp01(Decay), days);
        return Mathf.Clamp01(Impact * remaining);
    }

    public bool IsAlive(long now, float threshold)
    {
        return Strength(now) >= threshold;
    }

    public ThoughtEntry Clone()
    {
        return new ThoughtEntry
        {
            Id = Id,
            NodeId = NodeId,
            Subject = Subject,
            Sentiment = Sentiment,
            Impact = Impact,
            Decay = Decay,
            CreatedTick = CreatedTick,
            LastTick = LastTick,
            SourceEventId = SourceEventId,
            Tags = new List<string>(Tags),
            RawLabel = RawLabel,
            Detail = Detail,
            Authored = Authored,
            Procedural = Procedural,
            TimesSurfaced = TimesSurfaced,
            LastSurfacedTick = LastSurfacedTick
        };
    }
}

/// <summary>
/// A personality trait and the way it warps the reception, retention and discussion of thoughts.
/// Authored or procedurally rolled; never LLM generated.
/// </summary>
[Serializable]
public class TraitInfo
{
    public string Id = "";
    public string Display = "";
    public List<string> Tags = new List<string>();     // event tags this trait reacts to; empty = all
    public bool Empath = false;                        // receives World/Region events without a personal link
    public float ImpactMultiplier = 1f;
    public float DecayMultiplier = 1f;
    public float SentimentMultiplier = 1f;
    public float ValenceBias = 0f;                     // -1 drawn to bad memories .. +1 drawn to good
    public List<string> EagerNodes = new List<string>();   // talkative about these paths
    public List<string> AvoidNodes = new List<string>();   // reluctant or secretive about these paths
}

/// <summary>
/// A mood bucket (normal, depressed, ...). The active mood is derived from active thoughts and
/// decides which traits + sample line are injected into the LLM context.
/// </summary>
[Serializable]
public class MoodInfo
{
    public string Id = "";
    public List<string> Traits = new List<string>();
    public string Sample = "";
    public float ValenceCenter = 0.5f; // 0 = miserable, 1 = elated
    public bool Authored = true;
}

[Serializable]
public class RelationshipInfo
{
    public string PersonId = "";
    public string PersonName = "";
    public string Label = "";   // "mother"
    public string NodeId = "";  // taxonomy node, e.g. relationships/family/mother
    public float Opinion = 0.5f;
    public List<string> Notes = new List<string>();  // authored facts injected into the LLM context
}

[Serializable]
public class KnowledgeInfo
{
    public string Key = "";
    public string Value = "";
    public string NodeId = "";
}

/// <summary>
/// Authored world event template.
/// </summary>
[Serializable]
public class GameEventDefinition
{
    public string Id = "";
    public string Name = "";
    public EventScope Scope = EventScope.Local;
    public List<string> Tags = new List<string>();
    public float BaseSentiment = 0f;
    public float Impact = 0.5f;
    public float Decay = 0.05f;
    public string Detail = "";
    public bool Canonical = false;
    public EventImportance Importance = EventImportance.Minor;
    public List<string> AffectedNodeIds = new List<string>();
}

/// <summary>
/// A fixed timeline anchor: it fires at one precise in-game moment, in every playthrough, and can
/// name the exact people it lands on. Deterministic, so it is never re-rolled.
/// </summary>
[Serializable]
public class CanonEventDefinition
{
    public string Id = "";
    public string Name = "";
    public EventScope Scope = EventScope.Local;
    public long Day = 1L;
    public int MinuteOfDay = 0;
    public List<string> Tags = new List<string>();
    public float BaseSentiment = 0f;
    public float Impact = 0.5f;
    public float Decay = 0.05f;
    public string Detail = "";
    public List<string> AffectedNodeIds = new List<string>();
    public List<string> AffectedCharacterIds = new List<string>();
}

/// <summary>
/// A live instance of an event. Self-contained so it can be saved and rehydrated without
/// the definition asset; DefinitionId is resolved through EventSystem.
/// </summary>
[Serializable]
public class GameEvent
{
    public string Id = "";
    public string DefinitionId = "";
    public string Name = "";
    public EventScope Scope = EventScope.Local;
    public long StartedTick = 0L;
    public string PlaceId = "";
    public float BaseSentiment = 0f;
    public float Impact = 0.5f;
    public float Decay = 0.05f;
    public string Detail = "";
    public bool Canonical = false;
    public EventImportance Importance = EventImportance.Minor;
    public List<string> AffectedCharacterIds = new List<string>();
    public List<string> AffectedNodeIds = new List<string>();
    public List<string> Tags = new List<string>();
}
