using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

/// <summary>
/// Any OpenAI-compatible chat completions endpoint (OpenAI, DeepSeek, LM Studio, OpenRouter, ...)
/// using server-sent events.
/// </summary>
public class OpenAICompatibleProvider : ILLMProvider
{
    public IEnumerator Stream(LLMRequest request, Action<string> onToken, Action onComplete, Action<string> onError)
    {
        string baseUrl = LLMSettings.NormalizeBase(request.BaseUrl, LLMSettings.OpenAIDefaultBase);
        string url = baseUrl + "/chat/completions";

        var messages = new JArray();
        if (!string.IsNullOrEmpty(request.SystemPrompt))
            messages.Add(new JObject { ["role"] = "system", ["content"] = request.SystemPrompt });
        foreach (var message in request.Messages)
            messages.Add(new JObject { ["role"] = message.Role, ["content"] = message.Content });

        var body = new JObject
        {
            ["model"] = request.Model,
            ["stream"] = true,
            ["temperature"] = request.Temperature,
            ["messages"] = messages
        };

        var handler = new LLMStreamHandler();
        var www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToString(Formatting.None)));
        www.downloadHandler = handler;
        www.timeout = request.TimeoutSeconds;
        www.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(request.ApiKey))
            www.SetRequestHeader("Authorization", "Bearer " + request.ApiKey);

        yield return LLMStreamer.Stream(www, ParseLine, onToken, onComplete, onError);
    }

    private static string ParseLine(string line)
    {
        if (!line.StartsWith("data:"))
            return null;
        string payload = line.Substring(5).Trim();
        if (payload == "[DONE]")
            return null;
        var parsed = JObject.Parse(payload);
        var choices = parsed["choices"] as JArray;
        if (choices == null || choices.Count == 0)
            return null;
        return (string)choices[0]["delta"]?["content"];
    }
}
