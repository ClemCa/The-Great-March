using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The two kinds of node in a story graph. A Decision is a pure entry point into a sub-branch:
/// it carries its own gate and completes the instant it is chosen. A Beat holds content
/// (dialogue, character appearances, gameplay triggers) and completes once that content finishes.
/// </summary>
public enum StoryNodeType
{
    Decision,
    Beat
}

public enum StoryConditionSource
{
    Resource,
    YarnVariable,
    Time
}

public enum StoryComparison
{
    Less,
    LessOrEqual,
    Equal,
    NotEqual,
    GreaterOrEqual,
    Greater
}

public enum StoryResourceScope
{
    Global,
    PlayerPlanet,
    NamedPlanet
}

public enum StoryValueKind
{
    Number,
    Bool,
    String
}

/// <summary>
/// One clause of a node's gate. Every condition on a node must pass for the node to be eligible.
/// </summary>
[Serializable]
public class StoryCondition
{
    public StoryConditionSource Source = StoryConditionSource.Resource;
    public StoryComparison Comparison = StoryComparison.GreaterOrEqual;

    // Resource
    public bool Advanced = false;
    public int ResourceIndex = 0;
    public int Amount = 0;
    public StoryResourceScope Scope = StoryResourceScope.Global;
    public string PlanetName = "";

    // Yarn
    public string Variable = "";
    public StoryValueKind ValueKind = StoryValueKind.Number;
    public float NumberValue = 0f;
    public bool BoolValue = false;
    public string StringValue = "";

    // Time (compared against GameClock; UTC minutes since campaign start)
    public bool TimeUseDay = true;
    public long TimeValue = 0L;

    public StoryCondition Clone()
    {
        return new StoryCondition
        {
            Source = Source,
            Comparison = Comparison,
            Advanced = Advanced,
            ResourceIndex = ResourceIndex,
            Amount = Amount,
            Scope = Scope,
            PlanetName = PlanetName,
            Variable = Variable,
            ValueKind = ValueKind,
            NumberValue = NumberValue,
            BoolValue = BoolValue,
            StringValue = StringValue,
            TimeUseDay = TimeUseDay,
            TimeValue = TimeValue
        };
    }
}

[Serializable]
public class StoryAppearance
{
    public string CharacterId = "";
    public string Slot = "";
    public string SpawnPoint = "";
    public bool Remove = false;
}

[Serializable]
public class StoryTrigger
{
    public string Key = "";
    public string Payload = "";
}

/// <summary>
/// A single node of the story tree. Conditions are the gate; the date is only a lower bound
/// (it delays a node but never fires it on its own, and is treated as retroactively satisfied
/// once the clock has passed it).
/// </summary>
[Serializable]
public class StoryNode
{
    public string Id = "";
    public string Name = "New Node";
    public StoryNodeType Type = StoryNodeType.Beat;
    public Vector2 EditorPosition = Vector2.zero;

    public List<StoryCondition> Conditions = new List<StoryCondition>();
    public float Weight = 1f;

    public bool HasDate = false;
    public long Day = 1L;
    public int MinuteOfDay = 0;

    // Beat-only content.
    public List<string> YarnNodes = new List<string>();
    public List<StoryAppearance> Appearances = new List<StoryAppearance>();
    public List<StoryTrigger> Triggers = new List<StoryTrigger>();

    public long DateTick()
    {
        return (Day - 1L) * GameClock.MinutesPerDay + MinuteOfDay;
    }

    public bool HasContent()
    {
        return (YarnNodes != null && YarnNodes.Count > 0)
            || (Appearances != null && Appearances.Count > 0)
            || (Triggers != null && Triggers.Count > 0);
    }
}

[Serializable]
public class StoryEdge
{
    public string ParentId = "";
    public string ChildId = "";

    public StoryEdge() { }

    public StoryEdge(string parentId, string childId)
    {
        ParentId = parentId;
        ChildId = childId;
    }
}
