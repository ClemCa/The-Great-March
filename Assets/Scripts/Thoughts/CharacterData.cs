using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Editor-authored character file. The matching JSON Schema is character.schema.json and is
/// referenced through "$schema" so editors give autocomplete and catch mistakes.
/// </summary>
[Serializable]
public class CharacterData
{
    [JsonProperty("id")] public string Id = "";
    [JsonProperty("displayName")] public string DisplayName = "";
    [JsonProperty("gender")] public string Gender = "neutral";
    [JsonProperty("traits")] public List<string> Traits = new List<string>();
    [JsonProperty("moods")] public List<MoodData> Moods = new List<MoodData>();
    [JsonProperty("talkPolicies")] public List<TalkPolicyData> TalkPolicies = new List<TalkPolicyData>();
    [JsonProperty("relationships")] public List<RelationshipInfo> Relationships = new List<RelationshipInfo>();
    [JsonProperty("knowledge")] public List<KnowledgeInfo> Knowledge = new List<KnowledgeInfo>();
    [JsonProperty("notes")] public List<string> Notes = new List<string>();
    [JsonProperty("examples")] public List<string> Examples = new List<string>();
    [JsonProperty("thoughts")] public List<ThoughtData> Thoughts = new List<ThoughtData>();

    public ThoughtCharacter ToCharacter(ThoughtTaxonomy taxonomy, TraitLibrary traits)
    {
        var character = new ThoughtCharacter(Id, DisplayName, taxonomy, traits);
        character.Gender = Gender;
        character.TraitIds = new List<string>(Traits);

        character.Moods = new List<MoodInfo>();
        for (int i = 0; i < Moods.Count; i++)
        {
            character.Moods.Add(new MoodInfo
            {
                Id = Moods[i].Id,
                Traits = new List<string>(Moods[i].Traits),
                Sample = Moods[i].Sample,
                ValenceCenter = Moods[i].ValenceCenter,
                Authored = true
            });
        }
        if (character.Moods.Count == 0)
            character.Moods = MoodResolver.CreateStarter();

        for (int i = 0; i < TalkPolicies.Count; i++)
            character.TalkPolicies.Add(new TalkPolicyRule { NodeId = TalkPolicies[i].NodeId, Policy = TalkPolicies[i].Policy });

        character.Relationships = new List<RelationshipInfo>(Relationships);
        character.Knowledge = new List<KnowledgeInfo>(Knowledge);
        character.AuthoredNotes = new List<string>(Notes);
        character.ExampleSentences = new List<string>(Examples);
        character.CurrentMoodId = character.Moods[0].Id;

        long now = GameClock.Now;
        for (int i = 0; i < Thoughts.Count; i++)
        {
            var entry = Thoughts[i].ToEntry();
            entry.Authored = true;
            entry.CreatedTick = now - (long)(Thoughts[i].AgeDays * GameClock.MinutesPerDay);
            entry.LastTick = now;
            character.Thoughts.Add(entry);
        }
        return character;
    }
}

[Serializable]
public class MoodData
{
    [JsonProperty("id")] public string Id = "";
    [JsonProperty("valenceCenter")] public float ValenceCenter = 0.5f;
    [JsonProperty("traits")] public List<string> Traits = new List<string>();
    [JsonProperty("sample")] public string Sample = "";
}

[Serializable]
public class TalkPolicyData
{
    [JsonProperty("nodeId")] public string NodeId = "";
    [JsonProperty("policy")] public TalkPolicy Policy = TalkPolicy.Open;
}

[Serializable]
public class ThoughtData
{
    [JsonProperty("nodeId")] public string NodeId = "";
    [JsonProperty("subject")] public string Subject = "";
    [JsonProperty("sentiment")] public float Sentiment = 0f;
    [JsonProperty("impact")] public float Impact = 0.5f;
    [JsonProperty("decay")] public float Decay = 0.02f;
    [JsonProperty("ageDays")] public float AgeDays = 0f;
    [JsonProperty("rawLabel")] public string RawLabel = "";
    [JsonProperty("detail")] public string Detail = "";
    [JsonProperty("tags")] public List<string> Tags = new List<string>();

    public ThoughtEntry ToEntry()
    {
        return new ThoughtEntry
        {
            NodeId = NodeId,
            Subject = Subject,
            Sentiment = Sentiment,
            Impact = Impact,
            Decay = Decay,
            RawLabel = RawLabel,
            Detail = Detail,
            Tags = new List<string>(Tags)
        };
    }
}

[Serializable]
public class EventDataFile
{
    [JsonProperty("events")] public List<GameEventDefinition> Events = new List<GameEventDefinition>();
}
