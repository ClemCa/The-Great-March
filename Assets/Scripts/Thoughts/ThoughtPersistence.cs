using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class ThoughtSaveData
{
    public List<ThoughtCharacter> Characters = new List<ThoughtCharacter>();
    public List<GameEvent> Events = new List<GameEvent>();
    public List<string> FiredCanonical = new List<string>();
    public long ClockMinutes = 0L;
}

/// <summary>
/// Captures and restores all thought-system state (characters, thoughts, conversation history,
/// live events, game clock). Taxonomy and trait libraries are runtime-only and re-bound on load.
/// </summary>
public static class ThoughtPersistence
{
    public static string Capture()
    {
        var data = new ThoughtSaveData();
        var characters = ThoughtCharacterRegistry.AllList();
        for (int i = 0; i < characters.Count; i++)
            data.Characters.Add(characters[i]);

        var system = WorldEventSystem.Instance;
        if (system != null)
        {
            data.Events.AddRange(system.Live);
            data.FiredCanonical.AddRange(system.FiredCanonical);
        }

        data.ClockMinutes = GameClock.Now;
        return JsonConvert.SerializeObject(data);
    }

    public static void Restore(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        ThoughtSaveData data;
        try
        {
            data = JsonConvert.DeserializeObject<ThoughtSaveData>(json);
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to restore thought state: " + exception.Message);
            return;
        }
        if (data == null)
            return;

        GameClock.SetNow(data.ClockMinutes);

        var bootstrap = ThoughtBootstrap.Instance;
        var taxonomy = bootstrap != null ? bootstrap.Taxonomy : ThoughtTaxonomy.CreateStarter();
        var traits = bootstrap != null ? bootstrap.Traits : TraitLibrary.CreateStarter();

        ThoughtCharacterRegistry.Clear();
        for (int i = 0; i < data.Characters.Count; i++)
        {
            var character = data.Characters[i];
            character.Bind(taxonomy, traits);
            ThoughtCharacterRegistry.Register(character);
        }

        var system = WorldEventSystem.Instance;
        if (system != null)
        {
            system.RestoreEvents(data.Events);
            system.RestoreCanonicalFired(data.FiredCanonical);
        }
    }
}
