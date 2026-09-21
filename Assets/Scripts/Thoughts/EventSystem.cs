using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Raises world/region/local/personal events and lets every registered character react to them.
/// Events scale from a private moment (one character) up to galaxy-shaking news (everyone).
/// </summary>
public class WorldEventSystem : MonoBehaviour
{
    private static WorldEventSystem _instance;

    [SerializeField] private List<GameEventDefinition> _authored = new List<GameEventDefinition>();

    private readonly Dictionary<string, GameEventDefinition> _definitions = new Dictionary<string, GameEventDefinition>();
    private readonly List<GameEvent> _live = new List<GameEvent>();
    private readonly System.Random _rng = new System.Random();

    private int _counter = 0;
    private bool _initialized = false;

    public static WorldEventSystem Instance { get { return _instance; } }
    public IReadOnlyList<GameEvent> Live { get { return _live; } }
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

    public void EnsureInitialized()
    {
        if (_initialized)
            return;
        _initialized = true;
        foreach (var def in _authored)
            Register(def);
        if (_definitions.Count == 0)
            foreach (var def in CreateStarter())
                Register(def);
    }

    public void Register(GameEventDefinition definition)
    {
        if (definition == null || string.IsNullOrEmpty(definition.Id))
            return;
        _definitions[definition.Id] = definition;
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
            AffectedNodeIds = new List<string>(definition.AffectedNodeIds),
            Tags = new List<string>(definition.Tags)
        };
        if (affected != null)
            ev.AffectedCharacterIds.AddRange(affected);

        _live.Add(ev);

        var characters = ThoughtCharacterRegistry.AllList();
        for (int i = 0; i < characters.Count; i++)
            ThoughtPropagation.Apply(ev, characters[i], _rng);

        if (Raised != null)
            Raised(ev);
        return ev;
    }

    public void RestoreEvents(List<GameEvent> events)
    {
        _live.Clear();
        if (events != null)
            _live.AddRange(events);
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

    public static List<GameEventDefinition> CreateStarter()
    {
        return new List<GameEventDefinition>
        {
            new GameEventDefinition { Id = "war_declared", Name = "A war was declared", Scope = EventScope.World, Tags = new List<string> { "war", "politics" }, BaseSentiment = -0.7f, Impact = 0.8f, Decay = 0.03f, AffectedNodeIds = new List<string> { "opinions/politics", "world/events" } },
            new GameEventDefinition { Id = "plague", Name = "A plague is spreading", Scope = EventScope.Region, Tags = new List<string> { "disease", "death" }, BaseSentiment = -0.9f, Impact = 0.85f, Decay = 0.02f, AffectedNodeIds = new List<string> { "present/troubles", "world/events" } },
            new GameEventDefinition { Id = "harvest_festival", Name = "The harvest festival", Scope = EventScope.Local, Tags = new List<string> { "festival", "celebration" }, BaseSentiment = 0.6f, Impact = 0.4f, Decay = 0.15f, AffectedNodeIds = new List<string> { "present/news", "world/events" } },
            new GameEventDefinition { Id = "scandal", Name = "A political scandal", Scope = EventScope.Local, Tags = new List<string> { "politics", "scandal" }, BaseSentiment = -0.5f, Impact = 0.5f, Decay = 0.08f, AffectedNodeIds = new List<string> { "opinions/politics", "world/rumors" } },
            new GameEventDefinition { Id = "trade_boom", Name = "A trade boom", Scope = EventScope.Region, Tags = new List<string> { "trade", "economy" }, BaseSentiment = 0.5f, Impact = 0.5f, Decay = 0.08f, AffectedNodeIds = new List<string> { "present/news", "knowledge/job" } },
            new GameEventDefinition { Id = "personal_loss", Name = "The death of someone close", Scope = EventScope.Personal, Tags = new List<string> { "death", "grief" }, BaseSentiment = -0.9f, Impact = 0.9f, Decay = 0.05f, AffectedNodeIds = new List<string> { "relationships/family" } },
            new GameEventDefinition { Id = "promotion", Name = "A promotion at work", Scope = EventScope.Personal, Tags = new List<string> { "work" }, BaseSentiment = 0.7f, Impact = 0.6f, Decay = 0.1f, AffectedNodeIds = new List<string> { "knowledge/job" } },
            new GameEventDefinition { Id = "new_friend", Name = "A new friendship", Scope = EventScope.Personal, Tags = new List<string> { "friendship" }, BaseSentiment = 0.6f, Impact = 0.5f, Decay = 0.06f, AffectedNodeIds = new List<string> { "relationships/friends" } }
        };
    }
}
