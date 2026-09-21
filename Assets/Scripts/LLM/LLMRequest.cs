using System.Collections.Generic;

/// <summary>
/// Provider-agnostic request/response DTOs. Kept free of Unity dependencies so tooling
/// outside the editor can build the exact request the game sends.
/// </summary>
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
    public int MaxTokens = 400;
    public int TimeoutSeconds = 180;
    public string SystemPrompt = "";
    public List<LLMMessage> Messages = new List<LLMMessage>();
}
