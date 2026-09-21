using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A snapshot of a character's current inner state, used to bias procedural thought generation.
/// It captures how the character feels right now (live mood valence + intensity), the traits
/// currently in play, and any world events they are still feeling and how personally.
/// </summary>
public class ThoughtState
{
    public ThoughtCharacter Character;
    public long Now;

    public float Valence = 0.5f;   // 0..1 blended mood (0.5 neutral)
    public float Intensity;        // 0..1 distance from neutral
    public MoodInfo Mood;
    public TraitProfile Profile;
    public readonly List<GameEvent> Events = new List<GameEvent>();

    public bool Low { get { return Valence < 0.40f; } }
    public bool High { get { return Valence > 0.62f; } }
    public bool Average { get { return !Low && !High; } }
    public float Signed { get { return Valence * 2f - 1f; } }
    public bool HasAnyEvent { get { return Events.Count > 0; } }

    public static ThoughtState From(ThoughtCharacter character)
    {
        var state = new ThoughtState();
        state.Character = character;
        state.Now = GameClock.Now;
        if (character == null)
            return state;

        state.Mood = character.CurrentMood;
        state.Profile = character.Traits != null ? character.Traits.Aggregate(character.TraitIds) : new TraitProfile();

        float valence = 0.5f;
        float confidence = 0f;
        if (character.Thoughts != null)
        {
            valence = character.Thoughts.AverageValence(character.Thoughts.Entries, state.Now);
            confidence = character.Thoughts.StrongestImpact(character.Thoughts.Entries, state.Now);
        }
        state.Valence = Mathf.Lerp(0.5f, valence, Mathf.Clamp01(confidence));
        state.Intensity = Mathf.Abs(state.Valence - 0.5f) * 2f;

        var system = WorldEventSystem.Instance;
        if (system != null && system.Live != null)
        {
            for (int i = 0; i < system.Live.Count; i++)
            {
                var ev = system.Live[i];
                if (state.EventStrength(ev) > 0.05f)
                    state.Events.Add(ev);
            }
        }
        return state;
    }

    public float EventStrength(GameEvent ev)
    {
        if (ev == null)
            return 0f;
        float days = (Now - ev.StartedTick) / (float)GameClock.MinutesPerDay;
        float decay = Mathf.Clamp01(ev.Decay);
        float impact = Mathf.Clamp01(ev.Impact);
        return days <= 0f ? impact : Mathf.Clamp01(impact * Mathf.Pow(1f - decay, days));
    }

    /// <summary>How personally an event lands: named, on a known topic, or merely nearby (0..1).</summary>
    public float Relevance(GameEvent ev)
    {
        if (ev == null || Character == null)
            return 0f;

        if (ev.AffectedCharacterIds != null && ev.AffectedCharacterIds.Contains(Character.Id))
            return 1f;

        if (ev.AffectedNodeIds != null && Character.Taxonomy != null)
            for (int i = 0; i < ev.AffectedNodeIds.Count; i++)
                if (Character.KnowsNode(Character.Taxonomy, ev.AffectedNodeIds[i]))
                    return 0.9f;

        float byScope;
        switch (ev.Scope)
        {
            case EventScope.Personal: byScope = 0.7f; break;
            case EventScope.Local: byScope = 0.45f; break;
            case EventScope.Region: byScope = 0.3f; break;
            default: byScope = 0.2f; break;
        }
        if (Profile != null && Profile.Empath)
            byScope *= 1.6f;
        return Mathf.Clamp01(byScope);
    }

    /// <summary>Mood implied by the events still being felt, falling back to thought valence.</summary>
    public float EventValence(bool relevantOnly)
    {
        float sum = 0f;
        float weight = 0f;
        for (int i = 0; i < Events.Count; i++)
        {
            var ev = Events[i];
            float relevance = Relevance(ev);
            if (relevantOnly && relevance < 0.3f)
                continue;
            float w = EventStrength(ev) * relevance;
            sum += ev.BaseSentiment * w;
            weight += w;
        }
        if (weight <= 0.0001f)
            return Valence;
        return Mathf.Clamp01(0.5f + 0.5f * (sum / weight));
    }

    /// <summary>Strongest live event matching the filters, weighted by how much it touches the character.</summary>
    public GameEvent BestEvent(bool negativeOnly, bool positiveOnly, string tag, params EventScope[] scopes)
    {
        GameEvent best = null;
        float bestScore = 0f;
        for (int i = 0; i < Events.Count; i++)
        {
            var ev = Events[i];
            if (scopes != null && scopes.Length > 0)
            {
                bool inScope = false;
                for (int j = 0; j < scopes.Length; j++)
                    if (ev.Scope == scopes[j]) { inScope = true; break; }
                if (!inScope)
                    continue;
            }
            if (negativeOnly && ev.BaseSentiment >= 0f)
                continue;
            if (positiveOnly && ev.BaseSentiment <= 0f)
                continue;
            if (!string.IsNullOrEmpty(tag) && (ev.Tags == null || !ev.Tags.Contains(tag)))
                continue;

            float score = EventStrength(ev) * (0.5f + Relevance(ev));
            if (score > bestScore)
            {
                bestScore = score;
                best = ev;
            }
        }
        return best;
    }
}
