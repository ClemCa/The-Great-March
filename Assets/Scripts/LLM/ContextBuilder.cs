using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Assembles the JSON context ingested by the LLM, mirroring the agreed structure:
/// playerInquiry / personality / recentEvents / knowledge / thoughts / conversationHistory.
/// The model is read-only with respect to game state.
/// </summary>
public static class ContextBuilder
{
    public static string BuildSystemPrompt(ThoughtCharacter character)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("You are ").Append(character.DisplayName).Append(", a character in a living sci-fi world. ");
        sb.Append("Always stay in character. Reply with natural spoken dialogue only: no narration, no stage directions, no markdown, no JSON. Keep replies to one to four short sentences. ");

        var mood = character.CurrentMood;
        if (mood != null)
        {
            if (mood.Traits.Count > 0)
                sb.Append("Right now your manner is ").Append(string.Join(", ", mood.Traits)).Append(". ");
            if (!string.IsNullOrEmpty(mood.Sample))
                sb.Append("An example of how you sound: \"").Append(mood.Sample).Append("\" ");
        }

        sb.Append("The JSON that follows describes what you know and feel. Draw on it to answer, but never read it aloud verbatim and never invent facts that contradict it.");
        return sb.ToString();
    }

    public static string BuildContextJson(ThoughtCharacter character, string query, string reply, long now)
    {
        var root = new JObject();

        root["playerInquiry"] = new JObject
        {
            ["query"] = query == null ? "" : query,
            ["reply"] = reply == null ? "" : reply
        };

        root["personality"] = BuildPersonality(character);
        root["recentEvents"] = BuildRecentEvents(character, now);
        root["knowledge"] = BuildKnowledge(character);
        root["relationships"] = BuildRelationships(character);
        root["thoughts"] = BuildThoughts(character, now);
        root["conversationHistory"] = BuildHistory(character);

        root["historySummary"] = new JObject
        {
            ["recent"] = character.History == null ? "" : character.History.RecentSummary,
            ["coarse"] = character.History == null ? "" : character.History.CoarseSummary
        };

        return root.ToString(Formatting.None);
    }

    private static JObject BuildPersonality(ThoughtCharacter character)
    {
        var personality = new JObject();
        for (int i = 0; i < character.Moods.Count; i++)
        {
            var mood = character.Moods[i];
            if (mood == null || string.IsNullOrEmpty(mood.Id))
                continue;
            personality[mood.Id] = new JObject
            {
                ["traits"] = new JArray(mood.Traits),
                ["sample"] = mood.Sample
            };
        }
        personality["current"] = character.CurrentMoodId;
        personality["notes"] = new JArray(character.AuthoredNotes);
        personality["examples"] = new JArray(character.ExampleSentences);
        return personality;
    }

    private static JObject BuildRecentEvents(ThoughtCharacter character, long now)
    {
        var personal = new JArray();
        var global = new JArray();

        var system = WorldEventSystem.Instance;
        if (system != null)
        {
            for (int i = 0; i < system.Live.Count; i++)
            {
                var ev = system.Live[i];
                var node = new JObject
                {
                    ["event"] = ev.Name,
                    ["scope"] = ev.Scope.ToString(),
                    ["ageDays"] = Mathf.Max(0f, GameClock.DaysBetween(ev.StartedTick, now))
                };
                if (ev.AffectedCharacterIds.Contains(character.Id))
                    personal.Add(node);
                else
                    global.Add(node);
            }
        }
        else
        {
            var seen = new HashSet<string>();
            for (int i = 0; i < character.Thoughts.Entries.Count; i++)
            {
                var entry = character.Thoughts.Entries[i];
                if (string.IsNullOrEmpty(entry.SourceEventId) || !seen.Add(entry.SourceEventId))
                    continue;
                global.Add(new JObject
                {
                    ["event"] = entry.Subject,
                    ["scope"] = "Unknown",
                    ["ageDays"] = Mathf.Max(0f, GameClock.DaysBetween(entry.CreatedTick, now))
                });
            }
        }

        return new JObject { ["personal"] = personal, ["global"] = global };
    }

    private static JObject BuildKnowledge(ThoughtCharacter character)
    {
        var knowledge = new JObject();
        for (int i = 0; i < character.Knowledge.Count; i++)
        {
            var item = character.Knowledge[i];
            if (!string.IsNullOrEmpty(item.Key))
                knowledge[item.Key] = item.Value;
        }
        return knowledge;
    }

    private static JObject BuildRelationships(ThoughtCharacter character)
    {
        var relationships = new JObject();
        for (int i = 0; i < character.Relationships.Count; i++)
        {
            var relationship = character.Relationships[i];
            string key = string.IsNullOrEmpty(relationship.Label) ? relationship.PersonName : relationship.Label;
            if (string.IsNullOrEmpty(key))
                continue;
            relationships[key] = new JObject
            {
                ["name"] = relationship.PersonName,
                ["opinion"] = relationship.Opinion
            };
        }
        return relationships;
    }

    private static JObject BuildThoughts(ThoughtCharacter character, long now)
    {
        var thoughts = new JObject();
        if (character.Taxonomy == null)
            return thoughts;

        for (int i = 0; i < character.Thoughts.Entries.Count; i++)
        {
            var entry = character.Thoughts.Entries[i];
            float strength = entry.Strength(now);
            if (strength <= 0.001f)
                continue;

            string path = character.Taxonomy.Exists(entry.NodeId)
                ? character.Taxonomy.DisplayPath(entry.NodeId)
                : entry.NodeId;

            var group = thoughts[path] as JObject;
            if (group == null)
            {
                group = new JObject();
                thoughts[path] = group;
            }

            group[entry.Subject] = new JObject
            {
                ["sentiment"] = entry.Sentiment,
                ["decay"] = entry.Decay,
                ["impact"] = strength,
                ["raw"] = RawRenderer.Render(entry)
            };
        }
        return thoughts;
    }

    private static JArray BuildHistory(ThoughtCharacter character)
    {
        var history = new JArray();
        if (character.History == null)
            return history;
        var recent = character.History.Recent(LLMSettings.VerbatimHistory);
        for (int i = 0; i < recent.Count; i++)
        {
            history.Add(new JObject
            {
                ["q"] = recent[i].Question,
                ["a"] = recent[i].Answer
            });
        }
        return history;
    }
}
