using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

/// <summary>
/// Local Ollama provider using the streaming NDJSON /api/chat endpoint.
/// </summary>
public class OllamaProvider : ILLMProvider
{
    public IEnumerator Stream(LLMRequest request, Action<string> onToken, Action onComplete, Action<string> onError)
    {
        string baseUrl = LLMSettings.NormalizeBase(request.BaseUrl, LLMSettings.OllamaDefaultBase);
        string url = baseUrl + "/api/chat";

        var messages = new JArray();
        if (!string.IsNullOrEmpty(request.SystemPrompt))
            messages.Add(new JObject { ["role"] = "system", ["content"] = request.SystemPrompt });
        foreach (var message in request.Messages)
            messages.Add(new JObject { ["role"] = message.Role, ["content"] = message.Content });

        var body = new JObject
        {
            ["model"] = request.Model,
            ["stream"] = true,
            ["messages"] = messages,
            ["options"] = new JObject { ["temperature"] = request.Temperature }
        };

        var handler = new LLMStreamHandler();
        var www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToString(Formatting.None)));
        www.downloadHandler = handler;
        www.timeout = request.TimeoutSeconds;
        www.SetRequestHeader("Content-Type", "application/json");

        yield return LLMStreamer.Stream(www, ParseLine, onToken, onComplete, onError);
    }

    private static string ParseLine(string line)
    {
        JObject parsed = JObject.Parse(line);
        var content = (string)parsed["message"]?["content"];
        return content;
    }
}
