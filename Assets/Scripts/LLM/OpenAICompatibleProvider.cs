using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

/// <summary>
/// Any OpenAI-compatible chat completions endpoint (OpenAI, DeepSeek, LM Studio, OpenRouter, ...)
/// using server-sent events. Reasoning models expose hidden thinking as delta.reasoning_content.
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
            ["max_tokens"] = request.MaxTokens,
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

        string model = request.Model;
        bool sawThinking = false;
        bool sawContent = false;
        Func<string, string> parse = line =>
        {
            var token = LLMResponseParser.OpenAI(line);
            if (!string.IsNullOrEmpty(token.Thinking))
                sawThinking = true;
            if (!string.IsNullOrEmpty(token.Content))
                sawContent = true;
            return token.Content;
        };

        yield return LLMStreamer.Stream(www, parse, onToken, onComplete, onError);

        LLMThinking.Observe(model, sawThinking, sawContent);
    }
}
