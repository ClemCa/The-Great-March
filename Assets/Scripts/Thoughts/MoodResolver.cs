using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Derives the active mood from the character's live thoughts. A weak emotional field keeps
/// the character in their default mood; a strong one pulls them toward the matching bucket.
/// </summary>
public static class MoodResolver
{
    public static MoodInfo Resolve(ThoughtStore store, IEnumerable<MoodInfo> moods, long now, MoodInfo fallback = null)
    {
        MoodInfo best = null;
        float bestDistance = float.MaxValue;
        float valence = 0.5f;
        float confidence = 0f;

        if (store != null)
        {
            valence = store.AverageValence(store.Entries, now);
            confidence = store.StrongestImpact(store.Entries, now);
        }

        // Blend towards neutral when nothing is strongly felt.
        float blended = Mathf.Lerp(0.5f, valence, Mathf.Clamp01(confidence));

        if (moods != null)
        {
            foreach (var mood in moods)
            {
                if (mood == null)
                    continue;
                float distance = Mathf.Abs(mood.ValenceCenter - blended);
                // Prefer the closest bucket, breaking ties in favour of the default (normal) mood.
                if (distance < bestDistance - 0.0001f)
                {
                    bestDistance = distance;
                    best = mood;
                }
            }
        }

        return best != null ? best : fallback;
    }

    public static List<MoodInfo> CreateStarter()
    {
        return new List<MoodInfo>
        {
            new MoodInfo
            {
                Id = "normal",
                ValenceCenter = 0.68f,
                Traits = new List<string> { "bubbly", "quirky", "my-way" },
                Sample = "Yoooooo, if it isn't Mr President!"
            },
            new MoodInfo
            {
                Id = "depressed",
                ValenceCenter = 0.12f,
                Traits = new List<string> { "slow", "open", "honest" },
                Sample = "It's not against you, you know. I know I'm ruining the mood and all, but... Honestly, I... I just... I just can't keep it up"
            }
        };
    }
}
