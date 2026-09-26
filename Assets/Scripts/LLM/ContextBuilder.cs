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
        sb.Append("You are ").Append(character.DisplayName).Append(", a person living in a sci-fi world. ");
        sb.Append("Speak only as they would out loud, in natural conversation, usually one to two sentences. ");
        sb.Append("When a thought weighs on you or the answer comes in beats, you may break it into a few short sentences, each on its own line (plain sentences, not a list). ");
        sb.Append("Never narrate, never describe actions or expressions, no asterisks, no markdown, no lists, and never wrap the whole reply in quotation marks. ");
        sb.Append("Never mention being an AI, the context, JSON, or any field name. ");

        var mood = character.CurrentMood;
        if (mood != null)
        {
            if (mood.Traits.Count > 0)
                sb.Append("Right now your manner is ").Append(string.Join(", ", mood.Traits)).Append(". ");
            if (!string.IsNullOrEmpty(mood.Sample))
                sb.Append("You might sound like: \"").Append(mood.Sample).Append("\" ");
        }

        sb.Append("A JSON snapshot follows describing what you know and feel. Treat its private fields as feelings you would never say out loud: ");
        sb.Append("playerInquiry is what the player asked and the fragment on your mind right now; ");
        sb.Append("thoughts are keyed by topic, each with a raw fragment, a feeling word, and a weight word describing how strongly it sits with you; ");
        sb.Append("knowledge and relationships are facts about your life; recentEvents are things that happened; conversationHistory is what you two said before. ");
        sb.Append("Answer the player's actual question, leaning on the most relevant parts. If something is not in the snapshot, admit you don't know or deflect in character. Never invent facts that contradict it, and never add new specifics (people, places, events, or numbers) that are not in the snapshot.");
        return sb.ToString();
    }

    /// <summary>
    /// Single source of truth for the outgoing request, shared by the game and the
    /// off-Unity prompt harness so refinements are tested exactly as shipped.
    /// </summary>
    public static LLMRequest BuildRequest(ThoughtCharacter character, string query, string reply, string question, long now)
    {
        var request = new LLMRequest
        {
            BaseUrl = LLMSettings.EffectiveBaseUrl(),
            ApiKey = LLMSettings.ApiKey,
            Model = LLMSettings.EffectiveModel(),
            Temperature = LLMSettings.Temperature,
            SystemPrompt = BuildSystemPrompt(character),
            Messages = new List<LLMMessage>()
        };
        request.Messages.Add(new LLMMessage("user", BuildUserMessage(character, query, reply, question, now)));
        return request;
    }

    public static string BuildUserMessage(ThoughtCharacter character, string query, string reply, string question, long now)
    {
        string context = BuildContextJson(character, query, reply, now);
        return "Context:\n" + context + "\n\nThe player asks: " + question;
    }

    /// <summary>
    /// Request for a character meeting the player cold. The emerging thought (if any) is folded into
    /// the snapshot, and the small-talk cue becomes an explicit meeting instruction so the model
    /// greets even when nothing is on the character's mind.
    /// </summary>
    public static LLMRequest BuildMeetingRequest(ThoughtCharacter character, EmergingThought emerging, long now)
    {
        var request = new LLMRequest
        {
            BaseUrl = LLMSettings.EffectiveBaseUrl(),
            ApiKey = LLMSettings.ApiKey,
            Model = LLMSettings.EffectiveModel(),
            Temperature = LLMSettings.Temperature,
            SystemPrompt = BuildMeetingSystemPrompt(character),
            Messages = new List<LLMMessage>()
        };
        request.Messages.Add(new LLMMessage("user", BuildMeetingUserMessage(character, emerging, now)));
        return request;
    }

    private static string BuildMeetingSystemPrompt(ThoughtCharacter character)
    {
        return BuildSystemPrompt(character)
            + " You have just met the player. Greet them in a short line that suits your mood;"
            + " if something is on your mind, let it show rather than announcing it outright."
            + " emergingThought, when present, is the one thing rising to the surface right now.";
    }

    public static string BuildMeetingUserMessage(ThoughtCharacter character, EmergingThought emerging, long now)
    {
        string context = BuildContextJson(character, "", "", now, emerging);
        var sb = new System.Text.StringBuilder();
        sb.Append("Context:\n").Append(context).Append("\n\n");
        sb.Append("You have just met the player. Say a short line in greeting.");
        if (emerging != null && emerging.HasThought)
        {
            sb.Append(" Something is on your mind (").Append(emerging.Topic).Append("): ")
              .Append(emerging.Fragment).Append(". Bring it up if it fits the moment.");
        }
        return sb.ToString();
    }

    public static string BuildContextJson(ThoughtCharacter character, string query, string reply, long now)
    {
        return BuildContextJson(character, query, reply, now, null);
    }

    public static string BuildContextJson(ThoughtCharacter character, string query, string reply, long now, EmergingThought emerging)
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

        if (emerging != null && emerging.HasThought)
        {
            var entry = emerging.Entry;
            root["emergingThought"] = new JObject
            {
                ["topic"] = emerging.Topic,
                ["feeling"] = entry != null ? RawRenderer.SentimentWord(entry.Sentiment) : "",
                ["weight"] = entry != null ? WeightWord(entry.Strength(now)) : "",
                ["raw"] = emerging.Fragment
            };
        }

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
            var entry = new JObject
            {
                ["name"] = relationship.PersonName,
                ["opinion"] = relationship.Opinion
            };
            if (relationship.Notes != null && relationship.Notes.Count > 0)
                entry["notes"] = new JArray(relationship.Notes);
            relationships[key] = entry;
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
                ["feeling"] = RawRenderer.SentimentWord(entry.Sentiment),
                ["weight"] = WeightWord(strength),
                ["raw"] = RawRenderer.Render(entry)
            };
        }
        return thoughts;
    }

    private static string WeightWord(float weight)
    {
        if (weight >= 0.6f)
            return "heavy";
        if (weight >= 0.3f)
            return "noticeable";
        return "faint";
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
