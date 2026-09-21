using System;
using System.Collections.Generic;

/// <summary>
/// One choice offered while drilling the thought tree. IsResolve marks the "talk about this
/// node itself" option available on resumable nodes.
/// </summary>
public class DrillOption
{
    public string NodeId = "";
    public string Label = "";
    public bool IsResolve = false;
}

/// <summary>
/// Result of asking a character about a thought path.
/// </summary>
public class ThoughtResolution
{
    public bool Found;
    public TalkPolicy Policy = TalkPolicy.Open;
    public string NodeId = "";
    public ThoughtEntry Entry;
    public List<ThoughtEntry> Entries = new List<ThoughtEntry>();
    public string RawText = "";
    public string Detail = "";
}

/// <summary>
/// Drives Raw-mode exploration: which branches to offer at each step, and what the character
/// actually says when a node is resolved (including the missing-data behaviours).
/// </summary>
public static class ThoughtResolver
{
    public static List<DrillOption> Options(ThoughtCharacter character, string pathId)
    {
        var options = new List<DrillOption>();
        if (character == null || character.Taxonomy == null)
            return options;

        if (string.IsNullOrEmpty(pathId))
        {
            foreach (var root in character.Taxonomy.Roots)
                options.Add(new DrillOption { NodeId = root, Label = character.Taxonomy.NameOf(root) });
            return options;
        }

        if (character.Taxonomy.IsResolvable(pathId))
            options.Add(new DrillOption { NodeId = pathId, Label = "Bring up " + character.Taxonomy.NameOf(pathId), IsResolve = true });

        foreach (var child in character.Taxonomy.ChildrenOf(pathId))
            options.Add(new DrillOption { NodeId = child, Label = character.Taxonomy.NameOf(child) });

        return options;
    }

    public static ThoughtResolution Resolve(ThoughtCharacter character, string nodeId, System.Random rng, bool allowGenerate = true)
    {
        var result = new ThoughtResolution { NodeId = nodeId, Policy = character.PolicyFor(nodeId) };
        long now = GameClock.Now;

        var pool = character.Thoughts.UnderNode(character.Taxonomy, nodeId);
        var alive = new List<ThoughtEntry>();
        for (int i = 0; i < pool.Count; i++)
            if (pool[i].IsAlive(now, 0.001f))
                alive.Add(pool[i]);
        result.Entries = alive;

        var pick = character.Thoughts.WeightedPick(alive, now, rng);
        if (pick != null)
        {
            result.Found = true;
            result.Entry = pick;
            result.RawText = RawRenderer.Render(pick);
            result.Detail = pick.Detail;
            return result;
        }

        if (result.Policy == TalkPolicy.Open && allowGenerate)
        {
            var made = SettingGenerator.Generate(character, nodeId, rng);
            if (made != null)
            {
                result.Found = true;
                result.Entry = made;
                result.RawText = RawRenderer.Render(made);
                result.Detail = made.Detail;
                return result;
            }
        }

        switch (result.Policy)
        {
            case TalkPolicy.Reluctant:
                result.RawText = "I'd rather not get into that.";
                break;
            case TalkPolicy.Secretive:
                result.RawText = "That's not something I talk about.";
                break;
            default:
                result.RawText = "I don't know.";
                break;
        }
        return result;
    }
}
