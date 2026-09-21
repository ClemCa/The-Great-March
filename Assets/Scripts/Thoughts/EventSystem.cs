using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Raises world/region/local/personal events and lets every registered character react to them.
/// Events scale from a private moment (one character) up to galaxy-shaking news (everyone).
/// Canon events are separate: fixed timeline anchors fired by the clock at a precise moment.
/// </summary>
public class WorldEventSystem : MonoBehaviour
{
    private static WorldEventSystem _instance;

    [SerializeField] private List<GameEventDefinition> _authored = new List<GameEventDefinition>();
    [SerializeField] private List<CanonEventDefinition> _authoredCanonical = new List<CanonEventDefinition>();

    private readonly Dictionary<string, GameEventDefinition> _definitions = new Dictionary<string, GameEventDefinition>();
    private readonly List<GameEvent> _live = new List<GameEvent>();
    private readonly List<CanonEventDefinition> _canonical = new List<CanonEventDefinition>();
    private readonly HashSet<string> _canonicalFired = new HashSet<string>();
    private readonly System.Random _rng = new System.Random();

    private int _counter = 0;
    private bool _initialized = false;

    public static WorldEventSystem Instance { get { return _instance; } }
    public IReadOnlyList<GameEvent> Live { get { return _live; } }
    public IReadOnlyCollection<string> FiredCanonical { get { return _canonicalFired; } }
    public event Action<GameEvent> Raised;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureInitialized();
    }

    private void Update()
    {
        AdvanceCanonical();
    }

    public void EnsureInitialized()
    {
        if (_initialized)
            return;
        _initialized = true;
        foreach (var def in _authored)
            Register(def);
        foreach (var def in _authoredCanonical)
            RegisterCanonical(def);
    }

    public void Register(GameEventDefinition definition)
    {
        if (definition == null || string.IsNullOrEmpty(definition.Id))
            return;
        _definitions[definition.Id] = definition;
    }

    public void RegisterCanonical(CanonEventDefinition definition)
    {
        if (definition == null || string.IsNullOrEmpty(definition.Id))
            return;
        for (int i = 0; i < _canonical.Count; i++)
        {
            if (_canonical[i].Id == definition.Id)
            {
                _canonical[i] = definition;
                return;
            }
        }
        _canonical.Add(definition);
    }

    public GameEventDefinition GetDefinition(string id)
    {
        EnsureInitialized();
        GameEventDefinition def;
        _definitions.TryGetValue(id, out def);
        return def;
    }

    public GameEvent Raise(string definitionId, string placeId = "", IEnumerable<string> affected = null)
    {
        EnsureInitialized();
        GameEventDefinition def;
        if (!_definitions.TryGetValue(definitionId, out def))
        {
            Debug.LogWarning("Unknown event definition: " + definitionId);
            return null;
        }
        return Raise(def, placeId, affected);
    }

    public GameEvent Raise(GameEventDefinition definition, string placeId = "", IEnumerable<string> affected = null)
    {
        if (definition == null)
            return null;

        _counter++;
        var ev = new GameEvent
        {
            Id = "ev" + _counter,
            DefinitionId = definition.Id,
            Name = definition.Name,
            Scope = definition.Scope,
            StartedTick = GameClock.Now,
            PlaceId = placeId == null ? "" : placeId,
            BaseSentiment = definition.BaseSentiment,
            Impact = definition.Impact,
            Decay = definition.Decay,
            Detail = definition.Detail,
            Canonical = definition.Canonical,
            Importance = definition.Importance,
            AffectedNodeIds = new List<string>(definition.AffectedNodeIds),
            Tags = new List<string>(definition.Tags)
        };
        if (affected != null)
            ev.AffectedCharacterIds.AddRange(affected);

        Publish(ev);
        return ev;
    }

    /// <summary>
    /// Fires every canon event now due, oldest first. Safe to call every frame: each anchor fires
    /// exactly once per campaign, and overdue anchors (e.g. after restoring a later save) catch up
    /// in their authored order.
    /// </summary>
    public int AdvanceCanonical()
    {
        EnsureInitialized();
        if (_canonical.Count == 0)
            return 0;

        long now = GameClock.Now;
        bool anyDue = false;
        for (int i = 0; i < _canonical.Count; i++)
        {
            var candidate = _canonical[i];
            if (candidate == null || string.IsNullOrEmpty(candidate.Id))
                continue;
            if (_canonicalFired.Contains(candidate.Id))
                continue;
            if (CanonScheduler.TriggerTick(candidate) > now)
                continue;
            anyDue = true;
            break;
        }
        if (!anyDue)
            return 0;

        var due = CanonScheduler.Due(_canonical, _canonicalFired, now);
        for (int i = 0; i < due.Count; i++)
            RaiseCanonical(due[i]);
        return due.Count;
    }

    public GameEvent RaiseCanonical(CanonEventDefinition definition)
    {
        if (definition == null)
            return null;

        _counter++;
        var ev = new GameEvent
        {
            Id = "ev" + _counter,
            DefinitionId = definition.Id,
            Name = definition.Name,
            Scope = definition.Scope,
            StartedTick = CanonScheduler.TriggerTick(definition),
            BaseSentiment = definition.BaseSentiment,
            Impact = definition.Impact,
            Decay = definition.Decay,
            Detail = definition.Detail,
            Canonical = true,
            Importance = EventImportance.Major,
            AffectedNodeIds = new List<string>(definition.AffectedNodeIds),
            AffectedCharacterIds = new List<string>(definition.AffectedCharacterIds),
            Tags = new List<string>(definition.Tags)
        };

        Publish(ev);
        return ev;
    }

    private void Publish(GameEvent ev)
    {
        _live.Add(ev);

        var characters = ThoughtCharacterRegistry.AllList();
        for (int i = 0; i < characters.Count; i++)
        {
            var character = characters[i];
            bool direct;
            if (!ThoughtPropagation.ShouldApply(ev, character, out direct))
            {
                if (direct)
                    Debug.LogWarning("Major event '" + ev.DefinitionId + "' names canonical character '" + character.Id + "'; ignored to protect the timeline.");
                continue;
            }

            ThoughtPropagation.Apply(ev, character, _rng, direct);
        }

        if (Raised != null)
            Raised(ev);
    }

    public void RestoreEvents(List<GameEvent> events)
    {
        _live.Clear();
        if (events != null)
            _live.AddRange(events);
    }

    public void RestoreCanonicalFired(IEnumerable<string> fired)
    {
        _canonicalFired.Clear();
        if (fired == null)
            return;
        foreach (var id in fired)
            if (!string.IsNullOrEmpty(id))
                _canonicalFired.Add(id);
    }

    public int Purge(long now, float minStrength)
    {
        int removed = 0;
        for (int i = _live.Count - 1; i >= 0; i--)
        {
            var def = GetDefinition(_live[i].DefinitionId);
            float decay = def == null ? 0.05f : def.Decay;
            float impact = def == null ? 0.5f : def.Impact;
            float days = (now - _live[i].StartedTick) / (float)GameClock.MinutesPerDay;
            float strength = days <= 0f ? impact : impact * Mathf.Pow(1f - Mathf.Clamp01(decay), days);
            if (strength < minStrength)
            {
                _live.RemoveAt(i);
                removed++;
            }
        }
        return removed;
    }
}
