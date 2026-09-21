using System;
using System.Collections.Generic;

/// <summary>
/// Deterministic timeline anchors. A canon event fires exactly once per campaign, at a fixed
/// in-game moment, in a fixed order, so it happens the same way in every timeline. Kept free of
/// Unity so it can be exercised off-editor; WorldEventSystem owns the actual raising.
/// </summary>
public static class CanonScheduler
{
    /// <summary>
    /// Day is 1-based and minuteOfDay is minutes past midnight, but both may be zero or negative:
    /// a tick before the campaign start is backstory, applied the moment the game begins. Tick 0
    /// is the start of day 1.
    /// </summary>
    public static long TriggerTick(CanonEventDefinition definition)
    {
        if (definition == null)
            return 0L;
        return (definition.Day - 1L) * GameClock.MinutesPerDay + definition.MinuteOfDay;
    }

    /// <summary>
    /// Returns every canon event due at or before <paramref name="now"/> that has not fired yet,
    /// oldest first (id breaks ties), and records them as fired so they never repeat.
    /// </summary>
    public static List<CanonEventDefinition> Due(List<CanonEventDefinition> canonical, HashSet<string> fired, long now)
    {
        var due = new List<CanonEventDefinition>();
        if (canonical == null || fired == null)
            return due;

        for (int i = 0; i < canonical.Count; i++)
        {
            var definition = canonical[i];
            if (definition == null || string.IsNullOrEmpty(definition.Id))
                continue;
            if (fired.Contains(definition.Id))
                continue;
            if (TriggerTick(definition) > now)
                continue;
            due.Add(definition);
        }

        due.Sort(Compare);
        for (int i = 0; i < due.Count; i++)
            fired.Add(due[i].Id);
        return due;
    }

    private static int Compare(CanonEventDefinition a, CanonEventDefinition b)
    {
        int byTick = TriggerTick(a).CompareTo(TriggerTick(b));
        return byTick != 0 ? byTick : string.CompareOrdinal(a.Id, b.Id);
    }
}
