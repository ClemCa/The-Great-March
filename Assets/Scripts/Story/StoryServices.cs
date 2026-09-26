using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Named gameplay hooks for story beats. Graph assets stay decoupled from scene objects: a beat
/// names a trigger key, and any system (or a scene component on Awake) registers a handler.
/// </summary>
public static class StoryTriggerRegistry
{
    private static readonly Dictionary<string, Action<string>> Handlers = new Dictionary<string, Action<string>>();

    public static void Register(string key, Action<string> handler)
    {
        if (string.IsNullOrEmpty(key) || handler == null)
            return;
        Handlers[key] = handler;
    }

    public static void Unregister(string key)
    {
        if (!string.IsNullOrEmpty(key))
            Handlers.Remove(key);
    }

    public static bool Invoke(string key, string payload)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        if (Handlers.TryGetValue(key, out var handler))
        {
            handler?.Invoke(payload);
            return true;
        }
        Debug.LogWarning("[Story] No handler registered for trigger '" + key + "'.");
        return false;
    }

    public static void Clear()
    {
        Handlers.Clear();
    }
}

/// <summary>
/// Character appearance requests raised by beats. A scene-side listener (portrait, spawner, camera
/// rig, ...) subscribes and decides what "Slot"/"SpawnPoint" mean for it.
/// </summary>
public static class StoryAppearanceService
{
    public static event Action<StoryAppearance> Requested;

    public static void Apply(StoryAppearance appearance)
    {
        if (appearance == null || string.IsNullOrEmpty(appearance.CharacterId))
            return;
        if (Requested == null)
        {
            Debug.LogWarning("[Story] Appearance requested for '" + appearance.CharacterId + "' but no listener is registered.");
            return;
        }
        Requested.Invoke(appearance);
    }
}
