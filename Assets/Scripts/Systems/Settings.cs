using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum VolumeLayer
{
    Master,
    UI,
    SFX,
    Music
}

public enum Setting
{
    MasterVolume,
    UIVolume,
    SFXVolume,
    MusicVolume,
    BackgroundSound,
    ShowPrompt,
    VSync,
    Fullscreen,
    Resolution,
    Quality,
    AntialiasingMode,
    AntialiasingQuality
}

public static class SettingExtensions
{
    public static Setting<T> GetTyped<T>(this Dictionary<Setting, ISetting> settings, Setting key)
    {
        if (settings.TryGetValue(key, out var setting))
        {
            if (setting is Setting<T> typedSetting)
            {
                return typedSetting;
            }
            throw new Exception("Setting " + key.ToString() + " is not of type " + typeof(T).ToString() + ": actual type is " + setting.GetType().ToString());
        }
        throw new Exception("Setting not found: " + key.ToString());
    }
}

public interface ISetting
{
    object Value { get; set; }
    object DefaultValue { get; }
    event Action<object> OnValueChanged;
    void Invoke();
}

public sealed class Setting<T> : ISetting
{
    public T value;
    public T defaultValue;

    private event Action<T> _typedChanged;
    private event Action<object> _objectChanged;

    public object Value
    {
        get => value;
        set
        {
            this.value = (T)value;
            Invoke();
        }
    }

    public object DefaultValue => defaultValue;

    public event Action<T> OnValueChanged
    {
        add { _typedChanged += value; }
        remove { _typedChanged -= value; }
    }

    event Action<object> ISetting.OnValueChanged
    {
        add { _objectChanged += value; }
        remove { _objectChanged -= value; }
    }

    public void Invoke()
    {
        _typedChanged?.Invoke(value);
        _objectChanged?.Invoke(value);
    }
}

public class Settings : MonoBehaviour
{
    private static Settings instance;
    private Dictionary<Setting, ISetting> settings = new Dictionary<Setting, ISetting>()
    {
        { Setting.MasterVolume, new Setting<float> { value = 0.6f, defaultValue = 0.6f } },
        { Setting.UIVolume, new Setting<float> { value = 0.6f, defaultValue = 0.6f } },
        { Setting.SFXVolume, new Setting<float> { value = 0.6f, defaultValue = 0.6f } },
        { Setting.MusicVolume, new Setting<float> { value = 0.6f, defaultValue = 0.6f } },
        { Setting.BackgroundSound, new Setting<bool> { value = false, defaultValue = false } },
        { Setting.ShowPrompt, new Setting<bool> { value = true, defaultValue = true } },
        { Setting.VSync, new Setting<bool> { value = false, defaultValue = false } },
        { Setting.Fullscreen, new Setting<bool> { value = true, defaultValue = true } },
        { Setting.Resolution, new Setting<int> { value = -1, defaultValue = -1 } },
        { Setting.Quality, new Setting<int> { value = -1, defaultValue = -1 } },
        { Setting.AntialiasingMode, new Setting<AntialiasingMode> { value = AntialiasingMode.SubpixelMorphologicalAntiAliasing, defaultValue = AntialiasingMode.SubpixelMorphologicalAntiAliasing } },
        { Setting.AntialiasingQuality, new Setting<AntialiasingQuality> { value = AntialiasingQuality.High, defaultValue = AntialiasingQuality.High } },
    };

    public static float MasterVolume
    {
        get => instance.GetSetting<float>(Setting.MasterVolume);
        set => instance.SetSetting(Setting.MasterVolume, value);
    }

    public static float UIVolume
    {
        get => instance.GetSetting<float>(Setting.UIVolume);
        set => instance.SetSetting(Setting.UIVolume, value);
    }

    public static float SFXVolume
    {
        get => instance.GetSetting<float>(Setting.SFXVolume);
        set => instance.SetSetting(Setting.SFXVolume, value);
    }

    public static float MusicVolume
    {
        get => instance.GetSetting<float>(Setting.MusicVolume);
        set => instance.SetSetting(Setting.MusicVolume, value);
    }

    public static bool BackgroundSound
    {
        get => instance.GetSetting<bool>(Setting.BackgroundSound);
        set => instance.SetSetting(Setting.BackgroundSound, value);
    }

    public static bool ShowPrompt
    {
        get => instance.GetSetting<bool>(Setting.ShowPrompt);
        set => instance.SetSetting(Setting.ShowPrompt, value);
    }

    public static bool VSync
    {
        get => instance.GetSetting<bool>(Setting.VSync);
        set => instance.SetSetting(Setting.VSync, value);
    }

    public static bool Fullscreen
    {
        get => instance.GetSetting<bool>(Setting.Fullscreen);
        set => instance.SetSetting(Setting.Fullscreen, value);
    }

    public static int ResolutionIndex
    {
        get => instance.GetSetting<int>(Setting.Resolution);
        set => instance.SetSetting(Setting.Resolution, value);
    }

    public static int QualityLevel
    {
        get => instance.GetSetting<int>(Setting.Quality);
        set => instance.SetSetting(Setting.Quality, value);
    }

    public static AntialiasingMode AntialiasingMode
    {
        get => instance.GetSetting<AntialiasingMode>(Setting.AntialiasingMode);
        set => instance.SetSetting(Setting.AntialiasingMode, value);
    }

    public static AntialiasingQuality AntialiasingQuality
    {
        get => instance.GetSetting<AntialiasingQuality>(Setting.AntialiasingQuality);
        set => instance.SetSetting(Setting.AntialiasingQuality, value);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
        LoadSettings();
    }

    public static bool Loaded => instance != null;

    private void LoadSettings()
    {
        foreach (var key in settings.Keys)
        {
            settings[key].Value = LoadSetting(key, settings[key].DefaultValue);
        }
        settings[Setting.MasterVolume].OnValueChanged += _ => UpdateVolumes(VolumeLayer.Master);
        settings[Setting.UIVolume].OnValueChanged += _ => UpdateVolumes(VolumeLayer.UI);
        settings[Setting.SFXVolume].OnValueChanged += _ => UpdateVolumes(VolumeLayer.SFX);
        settings[Setting.MusicVolume].OnValueChanged += _ => UpdateVolumes(VolumeLayer.Music);
        settings.GetTyped<bool>(Setting.BackgroundSound).OnValueChanged += value =>
        {
            foreach (var audioSource in EnumerateSources())
            {
                audioSource.ignoreListenerPause = value;
            }
        };
        settings.GetTyped<bool>(Setting.VSync).OnValueChanged += value => QualitySettings.vSyncCount = value ? 1 : 0;
        settings.GetTyped<bool>(Setting.Fullscreen).OnValueChanged += value =>
        {
            Screen.fullScreen = value;
            ApplyResolution();
        };
        settings.GetTyped<int>(Setting.Resolution).OnValueChanged += _ => ApplyResolution();
        settings.GetTyped<int>(Setting.Quality).OnValueChanged += value =>
        {
            if (value >= 0 && value < QualitySettings.count)
            {
                QualitySettings.SetQualityLevel(value, true);
            }
        };
        settings.GetTyped<AntialiasingMode>(Setting.AntialiasingMode).OnValueChanged += value =>
        {
            if (Camera.main != null)
            {
                Camera.main.GetUniversalAdditionalCameraData().antialiasing = value;
            }
        };
        settings.GetTyped<AntialiasingQuality>(Setting.AntialiasingQuality).OnValueChanged += value =>
        {
            if (Camera.main != null)
            {
                Camera.main.GetUniversalAdditionalCameraData().antialiasingQuality = value;
            }
        };
        MassInvoke(Setting.VSync, Setting.Fullscreen, Setting.Resolution, Setting.Quality, Setting.AntialiasingMode, Setting.AntialiasingQuality);
    }

    private T GetSetting<T>(Setting key)
    {
        if (settings.TryGetValue(key, out var setting))
        {
            return (T)setting.Value;
        }
        throw new Exception("Setting not found: " + key);
    }

    private T LoadSetting<T>(Setting key, T defaultValue)
    {
        if (PlayerPrefs.HasKey(key.ToString()))
        {
            switch (defaultValue)
            {
                case float f:
                    return (T)(object)PlayerPrefs.GetFloat(key.ToString(), f);
                case int i:
                    return (T)(object)PlayerPrefs.GetInt(key.ToString(), i);
                case bool b:
                    return (T)(object)bool.Parse(PlayerPrefs.GetString(key.ToString(), b.ToString()));
                case Enum e:
                    return (T)Enum.Parse(typeof(T), PlayerPrefs.GetInt(key.ToString(), Convert.ToInt32(e)).ToString());
                default:
                    return (T)(object)PlayerPrefs.GetString(key.ToString(), defaultValue.ToString());
            }
        }
        return defaultValue;
    }

    private void SetSetting<T>(Setting key, T value)
    {
        if (settings.TryGetValue(key, out var setting))
        {
            setting.Value = value;
        }
        switch (value)
        {
            case float f:
                PlayerPrefs.SetFloat(key.ToString(), f);
                break;
            case int i:
                PlayerPrefs.SetInt(key.ToString(), i);
                break;
            case bool b:
                PlayerPrefs.SetString(key.ToString(), b.ToString());
                break;
            case Enum e:
                PlayerPrefs.SetInt(key.ToString(), Convert.ToInt32(e));
                break;
            default:
                PlayerPrefs.SetString(key.ToString(), value.ToString());
                break;
        }
        PlayerPrefs.Save();
    }

    private static void ApplyResolution()
    {
        if (instance == null)
        {
            return;
        }
        bool fullscreen = instance.GetSetting<bool>(Setting.Fullscreen);
        int index = instance.GetSetting<int>(Setting.Resolution);
        var resolutions = Screen.resolutions;
        if (index < 0 || resolutions == null || resolutions.Length == 0)
        {
            var current = Screen.currentResolution;
            Screen.SetResolution(current.width, current.height, fullscreen);
            return;
        }
        index = Mathf.Clamp(index, 0, resolutions.Length - 1);
        var resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, fullscreen);
    }

    private static IEnumerable<AudioSource> EnumerateSources()
    {
        var seen = new HashSet<AudioSource>();
        foreach (var source in global::Sound.AllAudioSources)
        {
            if (source != null && seen.Add(source))
            {
                yield return source;
            }
        }
        foreach (var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include))
        {
            if (source != null && seen.Add(source))
            {
                yield return source;
            }
        }
    }

    private static void UpdateVolumes(VolumeLayer layer)
    {
        if (layer == VolumeLayer.Master)
        {
            UpdateVolumes(VolumeLayer.UI);
            UpdateVolumes(VolumeLayer.SFX);
            UpdateVolumes(VolumeLayer.Music);
        }
        float computedVolume = ComputedVolume(layer);
        string tag = layer.ToString();
        foreach (var audioSource in EnumerateSources())
        {
            if (audioSource.CompareTag(tag))
            {
                audioSource.volume = computedVolume;
            }
        }
    }

    public static float ComputedVolume(VolumeLayer layer)
    {
        return layer switch
        {
            VolumeLayer.Master => MasterVolume,
            VolumeLayer.UI => MasterVolume * UIVolume,
            VolumeLayer.SFX => MasterVolume * SFXVolume,
            VolumeLayer.Music => MasterVolume * MusicVolume,
            _ => throw new System.Exception("Unknown volume layer " + layer.ToString()),
        };
    }

    private static void MassInvoke(params Setting[] keys)
    {
        foreach (var key in keys)
        {
            if (instance.settings.TryGetValue(key, out var setting))
            {
                setting.Invoke();
            }
        }
    }
}
