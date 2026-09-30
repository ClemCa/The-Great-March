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
        sb.Append("You are ").Append(character.DisplayName).Append(", a person living in a sci-fi world. Stay in character and speak only as spoken words, the way you would out loud. ");
        sb.Append("Keep it to one or two natural sentences. If the answer comes in beats you may use a few short sentences, each on its own line, but never a list. ");
        sb.Append("Never narrate or describe actions, expressions, or sounds; no markdown, no emoji, and never wrap your whole reply in quotation marks. A brief non-verbal cue such as *sigh* is allowed, but never any other asterisk stage direction. ");
        sb.Append("Never mention being an AI, the context, JSON, or any field name. ");
        sb.Append("Say something new: never repeat a line from the snapshot, from the examples below, or from earlier in the conversation. Answer what is actually asked. ");
        sb.Append("Use only the facts in the snapshot. If you do not know, admit it or deflect in character; never invent people, places, events, or numbers that are not in it. ");
        sb.Append("playerInquiry.policy says how willing you are to discuss the topic: open or enthusiastic means share freely (enthusiastic with relish), reluctant means a brief non-answer or a change of subject, secretive means refuse without giving details, unknown means you honestly do not know. Let it set your tone, but never name it. ");

        var mood = character.CurrentMood;
        if (mood != null && mood.Traits.Count > 0)
            sb.Append("Your manner right now is ").Append(string.Join(", ", mood.Traits)).Append(". ");
        if (character.AuthoredNotes.Count > 0)
            sb.Append(string.Join(" ", character.AuthoredNotes)).Append(' ');

        sb.Append("A JSON snapshot follows: playerInquiry is what the player asked (query) and the fragment on your mind (reply); thoughts are what is on your mind; knowledge and relationships are facts about your life; recentEvents are things that happened; conversationHistory is what you said before. ");

        // A couple of authored lines to set the voice. Kept short and clearly labelled: small models
        // otherwise copy them verbatim, so the instruction stresses that these are tone references only.
        var refs = new List<string>();
        if (mood != null && !string.IsNullOrEmpty(mood.Sample))
            refs.Add(mood.Sample);
        if (character.ExampleSentences != null)
        {
            for (int i = 0; i < character.ExampleSentences.Count && refs.Count < 3; i++)
            {
                string line = character.ExampleSentences[i];
                if (!string.IsNullOrEmpty(line) && !refs.Contains(line))
                    refs.Add(line);
            }
        }
        if (refs.Count > 0)
        {
            sb.Append("For tone only; never reuse these exact lines: ");
            for (int i = 0; i < refs.Count; i++)
            {
                if (i > 0)
                    sb.Append(" / ");
                sb.Append('"').Append(refs[i]).Append('"');
            }
            sb.Append('.');
        }
        return sb.ToString();
    }

    /// <summary>
    /// Single source of truth for the outgoing request, shared by the game and the
    /// off-Unity prompt harness so refinements are tested exactly as shipped.
    /// </summary>
    public static LLMRequest BuildRequest(ThoughtCharacter character, string query, string reply, string question, long now, TalkPolicy? policy = null)
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
        request.Messages.Add(new LLMMessage("user", BuildUserMessage(character, query, reply, question, now, policy)));
        return request;
    }

    public static string BuildUserMessage(ThoughtCharacter character, string query, string reply, string question, long now, TalkPolicy? policy = null)
    {
        string context = BuildContextJson(character, query, reply, now, null, policy);
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
        return BuildContextJson(character, query, reply, now, null, null);
    }

    public static string BuildContextJson(ThoughtCharacter character, string query, string reply, long now, EmergingThought emerging, TalkPolicy? policy = null)
    {
        var root = new JObject();

        var inquiry = new JObject
        {
            ["query"] = query == null ? "" : query,
            ["reply"] = reply == null ? "" : reply
        };
        if (policy.HasValue)
            inquiry["policy"] = PolicyWord(policy.Value);
        root["playerInquiry"] = inquiry;

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
        // Only the active mood is exposed here. Authored samples, notes and example lines are carried
        // by the system prompt as tone references, so they are deliberately kept out of the JSON where
        // small models are tempted to echo them verbatim.
        var mood = character.CurrentMood;
        return new JObject
        {
            ["current"] = character.CurrentMoodId,
            ["manner"] = new JArray(mood != null ? mood.Traits : new List<string>())
        };
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

    private static string PolicyWord(TalkPolicy policy)
    {
        switch (policy)
        {
            case TalkPolicy.Reluctant: return "reluctant";
            case TalkPolicy.Secretive: return "secretive";
            case TalkPolicy.Enthusiastic: return "enthusiastic";
            case TalkPolicy.Unknown: return "unknown";
            default: return "open";
        }
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
