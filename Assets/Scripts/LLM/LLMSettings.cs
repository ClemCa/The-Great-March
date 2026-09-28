using UnityEngine;

public enum LLMMode
{
    Raw,
    LLM
}

public enum LLMProviderKind
{
    Local,
    Ollama,
    OpenAICompatible
}

/// <summary>
/// Runtime-editable LLM configuration, persisted in PlayerPrefs alongside the save keys.
/// </summary>
public static class LLMSettings
{
    private const string Prefix = "llm.";

    public const string OllamaDefaultBase = "http://localhost:11434";
    public const string OpenAIDefaultBase = "https://api.openai.com/v1";
    public const string DefaultOllamaModel = "qwen3-0.6b-npc";
    public const string DefaultOpenAIModel = "gpt-4o-mini";
    public const string DefaultLocalModelFile = "qwen3-0.6b-npc-v4-q4_k_m.gguf";

    public static LLMMode Mode
    {
        get { return (LLMMode)PlayerPrefs.GetInt(Prefix + "mode", (int)LLMMode.LLM); }
        set { PlayerPrefs.SetInt(Prefix + "mode", (int)value); }
    }

    public static LLMProviderKind Provider
    {
        get { return (LLMProviderKind)PlayerPrefs.GetInt(Prefix + "provider", (int)LLMProviderKind.Local); }
        set { PlayerPrefs.SetInt(Prefix + "provider", (int)value); }
    }

    public static string BaseUrl
    {
        get { return PlayerPrefs.GetString(Prefix + "baseUrl", ""); }
        set { PlayerPrefs.SetString(Prefix + "baseUrl", value == null ? "" : value.Trim()); }
    }

    public static string Model
    {
        get { return PlayerPrefs.GetString(Prefix + "model", ""); }
        set { PlayerPrefs.SetString(Prefix + "model", value == null ? "" : value.Trim()); }
    }

    /// <summary>Filename (or overridable path) of the GGUF bundled for the in-process Local provider.</summary>
    public static string LocalModelFile
    {
        get { return PlayerPrefs.GetString(Prefix + "localModelFile", DefaultLocalModelFile); }
        set { PlayerPrefs.SetString(Prefix + "localModelFile", value == null ? "" : value.Trim()); }
    }

    public static string ApiKey
    {
        get { return PlayerPrefs.GetString(Prefix + "apiKey", ""); }
        set { PlayerPrefs.SetString(Prefix + "apiKey", value == null ? "" : value.Trim()); }
    }

    public static float Temperature
    {
        get { return PlayerPrefs.GetFloat(Prefix + "temperature", 0.85f); }
        set { PlayerPrefs.SetFloat(Prefix + "temperature", Mathf.Clamp(value, 0f, 2f)); }
    }

    public static int VerbatimHistory
    {
        get { return PlayerPrefs.GetInt(Prefix + "verbatim", 10); }
        set { PlayerPrefs.SetInt(Prefix + "verbatim", Mathf.Max(0, value)); }
    }

    public static int SummarizedHistory
    {
        get { return PlayerPrefs.GetInt(Prefix + "summarized", 50); }
        set { PlayerPrefs.SetInt(Prefix + "summarized", Mathf.Max(0, value)); }
    }

    public static bool IncludeThoughtsJson
    {
        get { return PlayerPrefs.GetInt(Prefix + "thoughtsJson", 1) == 1; }
        set { PlayerPrefs.SetInt(Prefix + "thoughtsJson", value ? 1 : 0); }
    }

    public static string EffectiveBaseUrl()
    {
        if (Provider == LLMProviderKind.Local)
            return "";
        if (!string.IsNullOrEmpty(BaseUrl))
            return NormalizeBase(BaseUrl, ProviderDefaultBase(Provider));
        return ProviderDefaultBase(Provider);
    }

    public static string EffectiveModel()
    {
        if (Provider == LLMProviderKind.Local)
            return LocalModelFile;
        if (!string.IsNullOrEmpty(Model))
            return Model;
        return Provider == LLMProviderKind.Ollama ? DefaultOllamaModel : DefaultOpenAIModel;
    }

    public static string ProviderDefaultBase(LLMProviderKind provider)
    {
        switch (provider)
        {
            case LLMProviderKind.Local:
                return "";
            case LLMProviderKind.OpenAICompatible:
                return OpenAIDefaultBase;
            default:
                return OllamaDefaultBase;
        }
    }

    public static string NormalizeBase(string url, string fallback)
    {
        if (string.IsNullOrEmpty(url))
            url = fallback;
        url = url.Trim().TrimEnd('/');
        if (url.IndexOf("://") < 0)
            url = "http://" + url;
        return url;
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}
