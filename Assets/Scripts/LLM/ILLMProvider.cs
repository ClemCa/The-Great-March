using System;
using System.Collections;

public class LLMMessage
{
    public string Role = "user";
    public string Content = "";

    public LLMMessage()
    {
    }

    public LLMMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }
}

public class LLMRequest
{
    public string BaseUrl = "";
    public string ApiKey = "";
    public string Model = "";
    public float Temperature = 0.85f;
    public int TimeoutSeconds = 180;
    public string SystemPrompt = "";
    public System.Collections.Generic.List<LLMMessage> Messages = new System.Collections.Generic.List<LLMMessage>();
}

/// <summary>
/// Streaming chat provider. Tokens are pushed as they arrive; done/error fire exactly once.
/// </summary>
public interface ILLMProvider
{
    IEnumerator Stream(LLMRequest request, Action<string> onToken, Action onComplete, Action<string> onError);
}

public static class LLMProviderFactory
{
    public static ILLMProvider Create(LLMProviderKind kind)
    {
        switch (kind)
        {
            case LLMProviderKind.OpenAICompatible:
                return new OpenAICompatibleProvider();
            default:
                return new OllamaProvider();
        }
    }
}
