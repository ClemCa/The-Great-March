using System.Collections.Generic;
using System.Text;

/// <summary>
/// Renders a request into the Qwen3 ChatML prompt the bundled local model expects.
///
/// LLamaSharp's own template renderer returns an empty string for this model, and it does not
/// expose the enable_thinking kwarg, so the format is built explicitly here. The structure
/// mirrors the model's chat_template.jinja, including the empty thinking block that the template
/// emits when enable_thinking is false (parity with Ollama's think=false).
///
/// The non-thinking form also appends the "/no_think" switch to the final user turn, exactly as
/// the SFT training data (see Tools/Distillation/train_sft.py) and the game's OllamaProvider do.
/// The bundled student was trained with that switch present, so omitting it here degrades replies.
/// </summary>
public static class Qwen3Prompt
{
    public const string ImStart = "<|im_start|>";
    public const string ImEnd = "<|im_end|>";
    public const string ThinkOpen = "<think>";
    public const string ThinkClose = "</think>";

    public static string Build(LLMRequest request, bool enableThinking = false)
    {
        return Build(request.SystemPrompt, request.Messages, enableThinking);
    }

    public static string Build(string systemPrompt, IList<LLMMessage> messages, bool enableThinking = false)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(systemPrompt))
            sb.Append(ImStart).Append("system\n").Append(systemPrompt).Append(ImEnd).Append('\n');

        if (messages != null)
        {
            int last = messages.Count - 1;
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message == null)
                    continue;

                string content = message.Content;
                if (!enableThinking && i == last && message.Role == "user")
                    content = LLMThinking.AppendNoThink(content);

                sb.Append(ImStart).Append(message.Role).Append('\n').Append(content).Append(ImEnd).Append('\n');
            }
        }

        sb.Append(ImStart).Append("assistant\n");
        if (!enableThinking)
            sb.Append(ThinkOpen).Append("\n\n").Append(ThinkClose).Append("\n\n");
        return sb.ToString();
    }
}
