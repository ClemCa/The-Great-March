using UnityEngine;

/// <summary>
/// Renders a thought as an unstructured Raw-mode fragment, e.g. "Dad death" or "Event X sad".
/// Authored labels win; otherwise the label is generated from subject + sentiment.
/// </summary>
public static class RawRenderer
{
    public static string Render(ThoughtEntry entry)
    {
        if (entry == null)
            return "";
        if (!string.IsNullOrEmpty(entry.RawLabel))
            return entry.RawLabel;
        string word = SentimentWord(entry.Sentiment);
        if (string.IsNullOrEmpty(entry.Subject))
            return word;
        return entry.Subject + " " + word;
    }

    public static string SentimentWord(float sentiment)
    {
        if (sentiment >= 0.6f)
            return "joyful";
        if (sentiment >= 0.2f)
            return "good";
        if (sentiment > -0.2f)
            return "neutral";
        if (sentiment > -0.6f)
            return "sad";
        return "awful";
    }
}
