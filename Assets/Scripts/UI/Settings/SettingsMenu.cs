using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Real settings menu with Audio / Display / Gameplay / Dialogue sections.
/// Dialogue holds the LLM configuration. Built as a prefab by SettingsMenuBuilder; controls are
/// resolved by name so the visual hierarchy can be restyled without touching this script.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    public Action Closed;

    private bool _updating;
    private bool _initialized;
    private int _section;

    private Button[] _tabs;
    private GameObject[] _panels;

    private Slider _master, _ui, _sfx, _music;
    private TextMeshProUGUI _masterV, _uiV, _sfxV, _musicV;

    private Toggle _fullscreen, _vsync;
    private Button _resolution, _quality, _aa, _aaQuality;
    private TextMeshProUGUI _resolutionV, _qualityV, _aaV, _aaQualityV;

    private Toggle _showPrompt, _backgroundSound;

    private Button _mode, _provider;
    private TMP_InputField _baseUrl, _model, _apiKey;
    private Slider _temperature, _verbatim, _summarized;
    private TextMeshProUGUI _temperatureV, _verbatimV, _summarizedV;
    private Toggle _thoughts;
    private TextMeshProUGUI _webglNote;

    public const string WebGlOllamaNote =
        "WebGL limitation: browsers block direct requests to a local Ollama server. LLM mode with " +
        "Ollama only works if it is started with CORS enabled (set OLLAMA_ORIGINS to this page's " +
        "origin); otherwise use an OpenAI-compatible endpoint.";

    private Button _close;

    private static readonly string[] SectionNames = { "Audio", "Display", "Gameplay", "Dialogue" };

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_initialized)
            return;
        _initialized = true;

        // Tabs
        _tabs = new Button[SectionNames.Length];
        _panels = new GameObject[SectionNames.Length];
        for (int i = 0; i < SectionNames.Length; i++)
        {
            int index = i;
            _tabs[i] = Find<Button>("Tab_" + SectionNames[i]);
            _panels[i] = FindGo("Panel_" + SectionNames[i]);
            if (_tabs[i] != null)
                _tabs[i].onClick.AddListener(() => SelectSection(index));
        }

        // Audio
        _master = Find<Slider>("Master_Control"); _masterV = Find<TextMeshProUGUI>("Master_Value");
        _ui = Find<Slider>("UI_Control"); _uiV = Find<TextMeshProUGUI>("UI_Value");
        _sfx = Find<Slider>("SFX_Control"); _sfxV = Find<TextMeshProUGUI>("SFX_Value");
        _music = Find<Slider>("Music_Control"); _musicV = Find<TextMeshProUGUI>("Music_Value");

        // Display
        _fullscreen = Find<Toggle>("Fullscreen_Control");
        _vsync = Find<Toggle>("VSync_Control");
        _resolution = Find<Button>("Resolution_Control"); _resolutionV = Find<TextMeshProUGUI>("Resolution_Value");
        _quality = Find<Button>("Quality_Control"); _qualityV = Find<TextMeshProUGUI>("Quality_Value");
        _aa = Find<Button>("Antialiasing_Control"); _aaV = Find<TextMeshProUGUI>("Antialiasing_Value");
        _aaQuality = Find<Button>("AntialiasingQuality_Control"); _aaQualityV = Find<TextMeshProUGUI>("AntialiasingQuality_Value");

        // Gameplay
        _showPrompt = Find<Toggle>("ShowPrompt_Control");
        _backgroundSound = Find<Toggle>("BackgroundSound_Control");

        // Dialogue
        _mode = Find<Button>("Mode_Control");
        _provider = Find<Button>("Provider_Control");
        _baseUrl = Find<TMP_InputField>("BaseUrl_Control");
        _model = Find<TMP_InputField>("Model_Control");
        _apiKey = Find<TMP_InputField>("ApiKey_Control");
        _temperature = Find<Slider>("Temperature_Control"); _temperatureV = Find<TextMeshProUGUI>("Temperature_Value");
        _verbatim = Find<Slider>("Verbatim_Control"); _verbatimV = Find<TextMeshProUGUI>("Verbatim_Value");
        _summarized = Find<Slider>("Summarized_Control"); _summarizedV = Find<TextMeshProUGUI>("Summarized_Value");
        _thoughts = Find<Toggle>("Thoughts_Control");
        _webglNote = Find<TextMeshProUGUI>("WebGL_Note");
        if (_webglNote != null)
            _webglNote.text = WebGlOllamaNote;

        _close = Find<Button>("Close");
        if (_close != null)
            _close.onClick.AddListener(Hide);

        WireListeners();
        WarnMissing();
    }

    private void WarnMissing()
    {
        var missing = new List<string>();
        if (_tabs[0] == null) missing.Add("Tab_Audio");
        if (_panels[0] == null) missing.Add("Panel_Audio");
        if (_master == null) missing.Add("Master_Control");
        if (_fullscreen == null) missing.Add("Fullscreen_Control");
        if (_mode == null) missing.Add("Mode_Control");
        if (_baseUrl == null) missing.Add("BaseUrl_Control");
        if (_temperature == null) missing.Add("Temperature_Control");
        if (_close == null) missing.Add("Close");
        if (missing.Count > 0)
            Debug.LogWarning("SettingsMenu prefab is missing controls: " + string.Join(", ", missing));
    }

    private void OnEnable()
    {
        RefreshAll();
    }

    private void WireListeners()
    {
        Add(_master, v => { Settings.MasterVolume = v; RefreshValueTexts(masterV: v); });
        Add(_ui, v => { Settings.UIVolume = v; RefreshValueTexts(uiV: v); });
        Add(_sfx, v => { Settings.SFXVolume = v; RefreshValueTexts(sfxV: v); });
        Add(_music, v => { Settings.MusicVolume = v; RefreshValueTexts(musicV: v); });

        AddToggle(_fullscreen, v => Settings.Fullscreen = v);
        AddToggle(_vsync, v => Settings.VSync = v);
        AddButton(_resolution, CycleResolution);
        AddButton(_quality, CycleQuality);
        AddButton(_aa, CycleAntialiasing);
        AddButton(_aaQuality, CycleAntialiasingQuality);

        AddToggle(_showPrompt, v => Settings.ShowPrompt = v);
        AddToggle(_backgroundSound, v => Settings.BackgroundSound = v);

        AddButton(_mode, CycleMode);
        AddButton(_provider, CycleProvider);
        AddInput(_baseUrl, v => { LLMSettings.BaseUrl = v; LLMSettings.Save(); });
        AddInput(_model, v => { LLMSettings.Model = v; LLMSettings.Save(); });
        AddInput(_apiKey, v => { LLMSettings.ApiKey = v; LLMSettings.Save(); });
        Add(_temperature, v => { LLMSettings.Temperature = v; LLMSettings.Save(); RefreshValueTexts(temperatureV: v); });
        Add(_verbatim, v => { LLMSettings.VerbatimHistory = Mathf.RoundToInt(v); LLMSettings.Save(); RefreshValueTexts(verbatimV: v); });
        Add(_summarized, v => { LLMSettings.SummarizedHistory = Mathf.RoundToInt(v); LLMSettings.Save(); RefreshValueTexts(summarizedV: v); });
        AddToggle(_thoughts, v => { LLMSettings.IncludeThoughtsJson = v; LLMSettings.Save(); });
    }

    public void Show()
    {
        gameObject.SetActive(true);
        RefreshAll();
    }

    public void Hide()
    {
        if (Closed != null)
            Closed.Invoke();
        gameObject.SetActive(false);
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
            Hide();
        else
            Show();
    }

    public static SettingsMenu FindAny()
    {
        return UnityEngine.Object.FindAnyObjectByType<SettingsMenu>(FindObjectsInactive.Include);
    }

    private void SelectSection(int index)
    {
        _section = index;
        for (int i = 0; i < _panels.Length; i++)
        {
            if (_panels[i] != null)
                _panels[i].SetActive(i == index);
            if (_tabs[i] != null)
            {
                var colors = _tabs[i].colors;
                colors.normalColor = i == index ? new Color(0.35f, 0.55f, 0.85f) : new Color(0.22f, 0.22f, 0.26f);
                _tabs[i].colors = colors;
            }
        }
    }

    private void RefreshAll()
    {
        _updating = true;

        SetSlider(_master, _masterV, Settings.MasterVolume);
        SetSlider(_ui, _uiV, Settings.UIVolume);
        SetSlider(_sfx, _sfxV, Settings.SFXVolume);
        SetSlider(_music, _musicV, Settings.MusicVolume);

        if (_fullscreen != null) _fullscreen.isOn = Settings.Fullscreen;
        if (_vsync != null) _vsync.isOn = Settings.VSync;
        SetCycle(_resolutionV, ResolutionLabel());
        SetCycle(_qualityV, QualityLabel());
        SetCycle(_aaV, Settings.AntialiasingMode.ToString());
        SetCycle(_aaQualityV, Settings.AntialiasingQuality.ToString());

        if (_showPrompt != null) _showPrompt.isOn = Settings.ShowPrompt;
        if (_backgroundSound != null) _backgroundSound.isOn = Settings.BackgroundSound;

        SetCycle(Find<TextMeshProUGUI>("Mode_Value"), LLMSettings.Mode.ToString());
        SetCycle(Find<TextMeshProUGUI>("Provider_Value"), LLMSettings.Provider.ToString());
        if (_baseUrl != null) _baseUrl.SetTextWithoutNotify(LLMSettings.BaseUrl);
        if (_model != null) _model.SetTextWithoutNotify(LLMSettings.Model);
        if (_apiKey != null) _apiKey.SetTextWithoutNotify(LLMSettings.ApiKey);
        SetSlider(_temperature, _temperatureV, LLMSettings.Temperature, "0.00");
        SetSlider(_verbatim, _verbatimV, LLMSettings.VerbatimHistory);
        SetSlider(_summarized, _summarizedV, LLMSettings.SummarizedHistory);
        if (_thoughts != null) _thoughts.isOn = LLMSettings.IncludeThoughtsJson;

        _updating = false;
        SelectSection(_section);
        RefreshDialogueInteractable();
    }

    private void CycleResolution()
    {
        var resolutions = Screen.resolutions;
        int count = resolutions != null ? resolutions.Length : 0;
        int next = Settings.ResolutionIndex + 1;
        if (next >= count)
            next = -1; // -1 = native
        Settings.ResolutionIndex = next;
        SetCycle(_resolutionV, ResolutionLabel());
    }

    private void CycleQuality()
    {
        int count = QualitySettings.names != null ? QualitySettings.names.Length : 0;
        if (count == 0)
            return;
        int next = Settings.QualityLevel + 1;
        if (next >= count)
            next = 0;
        Settings.QualityLevel = next;
        SetCycle(_qualityV, QualityLabel());
    }

    private void CycleAntialiasing()
    {
        var values = (AntialiasingMode[])Enum.GetValues(typeof(AntialiasingMode));
        int index = Array.IndexOf(values, Settings.AntialiasingMode);
        Settings.AntialiasingMode = values[(index + 1) % values.Length];
        SetCycle(_aaV, Settings.AntialiasingMode.ToString());
    }

    private void CycleAntialiasingQuality()
    {
        var values = (AntialiasingQuality[])Enum.GetValues(typeof(AntialiasingQuality));
        int index = Array.IndexOf(values, Settings.AntialiasingQuality);
        Settings.AntialiasingQuality = values[(index + 1) % values.Length];
        SetCycle(_aaQualityV, Settings.AntialiasingQuality.ToString());
    }

    private void CycleMode()
    {
        LLMSettings.Mode = LLMSettings.Mode == LLMMode.Raw ? LLMMode.LLM : LLMMode.Raw;
        SetCycle(Find<TextMeshProUGUI>("Mode_Value"), LLMSettings.Mode.ToString());
        RefreshDialogueInteractable();
    }

    private void CycleProvider()
    {
        LLMSettings.Provider = LLMSettings.Provider == LLMProviderKind.Ollama ? LLMProviderKind.OpenAICompatible : LLMProviderKind.Ollama;
        SetCycle(Find<TextMeshProUGUI>("Provider_Value"), LLMSettings.Provider.ToString());
        RefreshDialogueInteractable();
    }

    private void RefreshDialogueInteractable()
    {
        bool online = LLMSettings.Mode == LLMMode.LLM;
        if (_provider != null) _provider.interactable = online;
        if (_baseUrl != null) _baseUrl.interactable = online;
        if (_model != null) _model.interactable = online;
        if (_apiKey != null) _apiKey.interactable = online;
        if (_temperature != null) _temperature.interactable = online;
        RefreshWebGlNote();
    }

    private void RefreshWebGlNote()
    {
        if (_webglNote == null)
            return;

        bool ollamaLimited = Application.platform == RuntimePlatform.WebGLPlayer
            && LLMSettings.Provider == LLMProviderKind.Ollama;
        _webglNote.gameObject.SetActive(ollamaLimited);
    }

    private string ResolutionLabel()
    {
        var resolutions = Screen.resolutions;
        if (resolutions == null || resolutions.Length == 0)
            return "Native";
        int index = Settings.ResolutionIndex;
        if (index < 0 || index >= resolutions.Length)
            return "Native (" + Screen.currentResolution.width + "x" + Screen.currentResolution.height + ")";
        var r = resolutions[index];
        return r.width + "x" + r.height + " @" + Mathf.RoundToInt((float)r.refreshRateRatio.value) + "Hz";
    }

    private string QualityLabel()
    {
        var names = QualitySettings.names;
        if (names == null || names.Length == 0)
            return "Default";
        int index = Settings.QualityLevel;
        if (index < 0 || index >= names.Length)
            return names[names.Length - 1];
        return names[index];
    }

    private void SetSlider(Slider slider, TextMeshProUGUI value, float v, string format = "0.00")
    {
        if (slider != null)
            slider.SetValueWithoutNotify(v);
        if (value != null)
            value.text = v.ToString(format);
    }

    private void SetCycle(TextMeshProUGUI target, string text)
    {
        if (target != null)
            target.text = text;
    }

    private void RefreshValueTexts(float? masterV = null, float? uiV = null, float? sfxV = null, float? musicV = null, float? temperatureV = null, float? verbatimV = null, float? summarizedV = null)
    {
        if (_updating)
            return;
        if (masterV.HasValue && _masterV != null) _masterV.text = masterV.Value.ToString("0.00");
        if (uiV.HasValue && _uiV != null) _uiV.text = uiV.Value.ToString("0.00");
        if (sfxV.HasValue && _sfxV != null) _sfxV.text = sfxV.Value.ToString("0.00");
        if (musicV.HasValue && _musicV != null) _musicV.text = musicV.Value.ToString("0.00");
        if (temperatureV.HasValue && _temperatureV != null) _temperatureV.text = temperatureV.Value.ToString("0.00");
        if (verbatimV.HasValue && _verbatimV != null) _verbatimV.text = Mathf.RoundToInt(verbatimV.Value).ToString();
        if (summarizedV.HasValue && _summarizedV != null) _summarizedV.text = Mathf.RoundToInt(summarizedV.Value).ToString();
    }

    private void Add(Slider slider, Action<float> action)
    {
        if (slider != null)
            slider.onValueChanged.AddListener(v => { if (!_updating) action(v); });
    }

    private void AddToggle(Toggle toggle, Action<bool> action)
    {
        if (toggle != null)
            toggle.onValueChanged.AddListener(v => { if (!_updating) action(v); });
    }

    private void AddButton(Button button, Action action)
    {
        if (button != null)
            button.onClick.AddListener(() => { if (!_updating) action(); });
    }

    private void AddInput(TMP_InputField input, Action<string> action)
    {
        if (input != null)
            input.onEndEdit.AddListener(v => { if (!_updating) action(v); });
    }

    private T Find<T>(string name) where T : Component
    {
        var t = FindTransform(transform, name);
        return t == null ? null : t.GetComponent<T>();
    }

    private GameObject FindGo(string name)
    {
        var t = FindTransform(transform, name);
        return t == null ? null : t.gameObject;
    }

    private static Transform FindTransform(Transform root, string name)
    {
        if (root.name == name)
            return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var result = FindTransform(root.GetChild(i), name);
            if (result != null)
                return result;
        }
        return null;
    }
}
