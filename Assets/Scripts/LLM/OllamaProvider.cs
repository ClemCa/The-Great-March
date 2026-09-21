using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

/// <summary>
/// Local Ollama provider using the streaming NDJSON /api/chat endpoint. Thinking models stream
/// their reasoning in a separate field; we watch for it and stop asking models that never reply.
/// </summary>
public class OllamaProvider : ILLMProvider
{
    public IEnumerator Stream(LLMRequest request, Action<string> onToken, Action onComplete, Action<string> onError)
    {
        string baseUrl = LLMSettings.NormalizeBase(request.BaseUrl, LLMSettings.OllamaDefaultBase);
        string url = baseUrl + "/api/chat";

        string model = request.Model;
        bool noThink = LLMThinking.SupportsNoThink(model);

        var messages = new JArray();
        if (!string.IsNullOrEmpty(request.SystemPrompt))
            messages.Add(new JObject { ["role"] = "system", ["content"] = request.SystemPrompt });
        for (int i = 0; i < request.Messages.Count; i++)
        {
            string content = request.Messages[i].Content;
            if (noThink && i == request.Messages.Count - 1)
                content = LLMThinking.AppendNoThink(content);
            messages.Add(new JObject { ["role"] = request.Messages[i].Role, ["content"] = content });
        }

        var options = new JObject { ["temperature"] = request.Temperature, ["num_predict"] = request.MaxTokens };
        var body = new JObject
        {
            ["model"] = model,
            ["stream"] = true,
            ["think"] = !noThink,
            ["messages"] = messages,
            ["options"] = options
        };

        var handler = new LLMStreamHandler();
        var www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToString(Formatting.None)));
        www.downloadHandler = handler;
        www.timeout = request.TimeoutSeconds;
        www.SetRequestHeader("Content-Type", "application/json");

        bool sawThinking = false;
        bool sawContent = false;
        Func<string, string> parse = line =>
        {
            var token = LLMResponseParser.Ollama(line);
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
