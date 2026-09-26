using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Serializable snapshot of the story's progression plus the Yarn variables (dialogue choice flags).
/// Definitions/graph stay in the asset; only this state is written into a save.
/// </summary>
// This DTO is round-tripped through Newtonsoft.Json (see Saver), never Unity's serializer, so the
// dictionary fields are intentionally not [SerializeField].
#pragma warning disable UAC1015
[Serializable]
public class StorySaveState
{
    public List<string> Completed = new List<string>();
    public List<string> Locked = new List<string>();
    public List<string> Pending = new List<string>();
    public string ActiveBeat = "";
    public Dictionary<string, float> YarnFloats = new Dictionary<string, float>();
    public Dictionary<string, string> YarnStrings = new Dictionary<string, string>();
    public Dictionary<string, bool> YarnBools = new Dictionary<string, bool>();
}
#pragma warning restore UAC1015

/// <summary>
/// Captures and restores story state, mirroring ThoughtPersistence. Plugged into Saver.
/// </summary>
public static class StoryPersistence
{
    public static string Capture()
    {
        var director = StoryDirector.Instance;
        var state = director != null ? director.CaptureState() : new StorySaveState();

        var runner = director != null ? director.GetRunner() : null;
        var storage = runner != null ? runner.VariableStorage : null;
        if (storage != null)
        {
            var (floats, strings, bools) = storage.GetAllVariables();
            state.YarnFloats = floats ?? new Dictionary<string, float>();
            state.YarnStrings = strings ?? new Dictionary<string, string>();
            state.YarnBools = bools ?? new Dictionary<string, bool>();
        }

        return JsonConvert.SerializeObject(state);
    }

    public static void Restore(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        StorySaveState state;
        try
        {
            state = JsonConvert.DeserializeObject<StorySaveState>(json);
        }
        catch (Exception exception)
        {
            Debug.LogError("[Story] Failed to restore story state: " + exception.Message);
            return;
        }
        if (state == null)
            return;

        var director = StoryDirector.Instance;
        var runner = director != null ? director.GetRunner() : null;
        var storage = runner != null ? runner.VariableStorage : null;
        if (storage != null)
        {
            storage.SetAllVariables(
                state.YarnFloats ?? new Dictionary<string, float>(),
                state.YarnStrings ?? new Dictionary<string, string>(),
                state.YarnBools ?? new Dictionary<string, bool>(),
                true);
        }

        director?.RestoreState(state);
    }
}
