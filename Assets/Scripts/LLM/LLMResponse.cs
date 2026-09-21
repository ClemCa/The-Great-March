using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

/// <summary>
/// One streamed chunk: the visible reply plus, for thinking models, hidden reasoning.
/// </summary>
public struct LLMToken
{
    public string Content;
    public string Thinking;
}

/// <summary>
/// Turns a provider stream line into content/thinking. Pure and Unity-free so it can be tested
/// off-editor; the providers only decide what to do with the thinking it reports.
/// </summary>
public static class LLMResponseParser
{
    public static LLMToken Ollama(string line)
    {
        var token = new LLMToken();
        var parsed = JObject.Parse(line);
        var message = parsed["message"];
        if (message != null)
        {
            token.Content = (string)message["content"];
            token.Thinking = (string)message["thinking"];
        }
        return token;
    }

    public static LLMToken OpenAI(string line)
    {
        var token = new LLMToken();
        if (!line.StartsWith("data:"))
            return token;
        string payload = line.Substring(5).Trim();
        if (payload == "[DONE]")
            return token;
        var parsed = JObject.Parse(payload);
        var choices = parsed["choices"] as JArray;
        if (choices == null || choices.Count == 0)
            return token;
        var delta = choices[0]["delta"];
        if (delta != null)
        {
            token.Content = (string)delta["content"];
            token.Thinking = (string)delta["reasoning_content"];
        }
        return token;
    }
}

/// <summary>
/// Watches models that emit hidden reasoning. Some (Qwen3-VL) reason without bound and never reach
/// a visible reply; once we see a turn that was thinking-only we stop asking that model and let the
/// Raw thought stand in, instead of paying for an empty call every time.
/// </summary>
public static class LLMThinking
{
    public const string NoThinkSwitch = "/no_think";

    private static readonly HashSet<string> Thinkers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> Unreliable = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public static bool IsKnown(string model)
    {
        return !string.IsNullOrEmpty(model) && Thinkers.Contains(model);
    }

    public static void MarkKnown(string model)
    {
        if (!string.IsNullOrEmpty(model))
            Thinkers.Add(model);
    }

    public static bool IsUnreliable(string model)
    {
        return !string.IsNullOrEmpty(model) && Unreliable.Contains(model);
    }

    public static void MarkUnreliable(string model)
    {
        if (!string.IsNullOrEmpty(model))
            Unreliable.Add(model);
    }

    /// <summary>Records the outcome of one turn; a thinking-only turn flags the model as unreliable.</summary>
    public static void Observe(string model, bool sawThinking, bool sawContent)
    {
        if (!sawThinking)
            return;
        MarkKnown(model);
        if (!sawContent)
            MarkUnreliable(model);
    }

    public static void Reset()
    {
        Thinkers.Clear();
        Unreliable.Clear();
    }

    /// <summary>Qwen models accept the /no_think soft switch; others would just see noise.</summary>
    public static bool SupportsNoThink(string model)
    {
        return !string.IsNullOrEmpty(model) && model.IndexOf("qwen", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>Appends the soft switch some Qwen models honour; safe to call twice.</summary>
    public static string AppendNoThink(string content)
    {
        if (string.IsNullOrEmpty(content))
            return NoThinkSwitch;
        if (content.Contains(NoThinkSwitch))
            return content;
        return content + "\n" + NoThinkSwitch;
    }
}
