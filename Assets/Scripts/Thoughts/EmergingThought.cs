using System.Collections.Generic;

/// <summary>
/// The thought that rises to the surface when a character is met cold, with no topic offered.
/// <see cref="SmallTalk"/> is raised whether or not anything surfaced: meeting someone always
/// warrants a greeting, so the cue and the thought are independent. Raw mode never uses this;
/// it exists so LLM mode can give a character a line on first contact.
/// </summary>
public class EmergingThought
{
    public bool HasThought;
    public bool SmallTalk = true;
    public string NodeId = "";
    public string Topic = "";
    public ThoughtEntry Entry;
    public string Fragment = "";
    public string Detail = "";

    /// <summary>Records that the thought has been spoken, so it does not dominate every meeting.</summary>
    public void MarkSurfaced(long now)
    {
        if (Entry == null)
            return;
        Entry.TimesSurfaced++;
        Entry.LastSurfacedTick = now;
    }
}

/// <summary>
/// Picks the most fitting thought a character would offer on meeting, or nothing. It never invents
/// content and takes no path: it runs the same weighting an explored topic would, across every
/// living thought. Meaningful only in LLM mode, where it feeds a meeting line.
/// </summary>
public static class EmergingThoughtResolver
{
    public static EmergingThought MostFitting(ThoughtCharacter character, System.Random rng)
    {
        return MostFitting(character, rng, GameClock.Now);
    }

    public static EmergingThought MostFitting(ThoughtCharacter character, System.Random rng, long now)
    {
        var result = new EmergingThought { SmallTalk = true };
        if (character == null || character.Taxonomy == null || character.Thoughts == null)
            return result;

        var alive = new List<ThoughtEntry>();
        for (int i = 0; i < character.Thoughts.Entries.Count; i++)
        {
            var entry = character.Thoughts.Entries[i];
            if (entry != null && entry.IsAlive(now, 0.001f))
                alive.Add(entry);
        }

        var pick = ThoughtSelector.PickEmerging(character, alive, now, rng);
        if (pick == null)
            return result;

        result.HasThought = true;
        result.NodeId = pick.NodeId;
        result.Topic = character.Taxonomy.Exists(pick.NodeId)
            ? character.Taxonomy.DisplayPath(pick.NodeId)
            : pick.NodeId;
        result.Entry = pick;
        result.Fragment = RawRenderer.Render(pick);
        result.Detail = pick.Detail;
        return result;
    }
}
