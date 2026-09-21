using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class QAPair
{
    public string Question = "";
    public string Answer = "";
    public long Tick = 0L;
}

/// <summary>
/// Chronological question/answer pairs plus two rolling summary tiers:
/// the last N pairs are kept verbatim, the next M are summarized, everything older is coarse.
/// The LLM never edits knowledge; it only remembers what it said here.
/// </summary>
[Serializable]
public class ConversationHistory
{
    public List<QAPair> Pairs = new List<QAPair>();
    public string RecentSummary = "";
    public string CoarseSummary = "";

    public void Add(string question, string answer)
    {
        Pairs.Add(new QAPair { Question = question, Answer = answer, Tick = GameClock.Now });
    }

    public List<QAPair> Recent(int cap)
    {
        if (cap <= 0)
            return new List<QAPair>();
        int start = Mathf.Max(0, Pairs.Count - cap);
        return Pairs.GetRange(start, Pairs.Count - start);
    }

    public List<QAPair> Middle(int recentCap, int summaryCap)
    {
        int end = Mathf.Max(0, Pairs.Count - recentCap);
        int start = Mathf.Max(0, end - summaryCap);
        if (end <= start)
            return new List<QAPair>();
        return Pairs.GetRange(start, end - start);
    }

    public List<QAPair> Older(int recentCap, int summaryCap)
    {
        int end = Mathf.Max(0, Pairs.Count - recentCap - summaryCap);
        if (end <= 0)
            return new List<QAPair>();
        return Pairs.GetRange(0, end);
    }

    /// <summary>
    /// Rebuilds both summary tiers. digest may be a local truncation or an LLM summarizer.
    /// </summary>
    public void UpdateSummaries(int recentCap, int summaryCap, Func<List<QAPair>, string, string> digest)
    {
        if (digest == null)
            digest = NaiveDigest;

        var middle = Middle(recentCap, summaryCap);
        if (middle.Count > 0)
            RecentSummary = digest(middle, RecentSummary);

        var older = Older(recentCap, summaryCap);
        if (older.Count > 0)
            CoarseSummary = digest(older, CoarseSummary);
    }

    public static string NaiveDigest(List<QAPair> pairs, string previous)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(previous))
            sb.Append(previous).Append(' ');
        int start = Mathf.Max(0, pairs.Count - 12);
        for (int i = start; i < pairs.Count; i++)
            sb.Append("Q:").Append(Trim(pairs[i].Question)).Append(" A:").Append(Trim(pairs[i].Answer)).Append(' ');
        return sb.ToString().Trim();
    }

    private static string Trim(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";
        text = text.Replace("\n", " ");
        return text.Length > 120 ? text.Substring(0, 120) : text;
    }
}
