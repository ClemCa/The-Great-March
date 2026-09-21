using System;
using System.Collections;

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
