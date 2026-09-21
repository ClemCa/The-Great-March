using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Invents a plausible setting when an Open character is asked about an empty path.
/// This is deliberately not the LLM: it keeps the world populated before content exists and
/// gives authored characters something concrete to grow from.
/// </summary>
public static class SettingGenerator
{
    private static readonly string[] MaleNames = { "Aldric", "Bram", "Cato", "Dorian", "Elias", "Fen", "Garan", "Hollis" };
    private static readonly string[] FemaleNames = { "Mira", "Sable", "Tova", "Wren", "Anya", "Bettina", "Clove", "Dahlia" };
    private static readonly string[] NeutralNames = { "Ash", "Juno", "Ren", "Sol", "Vesper", "Mar", "Kai", "Nic" };
    private static readonly string[] Jobs = { "dockworker", "archivist", "trader", "medic", "engineer", "farmer", "pilot", "teacher" };
    private static readonly string[] Quirks = { "kind", "stubborn", "funny", "proud", "quiet", "reckless", "gentle", "shrewd" };
    private static readonly string[] Places = { "the old station", "the outer rim", "the capital", "the salt flats", "the orbital yards" };
    private static readonly string[] Events = { "a bad winter", "a long voyage", "a fire", "a lucky windfall", "a broken promise" };
    private static readonly string[] Feelings = { "restless", "tired", "hopeful", "uneasy", "content", "lonely" };
    private static readonly string[] Actions = { "repairing the ship", "haggling at the market", "studying old charts", "visiting family", "looking for work" };
    private static readonly string[] Rumors = { "a whisper about the council", "talk of smuggling", "a story about a ghost fleet", "news of a distant war" };
    private static readonly string[] Topics = { "the new tariffs", "old music", "the frontier", "religion", "the old regime" };

    public static ThoughtEntry Generate(ThoughtCharacter character, string nodeId, System.Random rng)
    {
        if (character == null || character.Taxonomy == null || !character.Taxonomy.Exists(nodeId))
            return null;

        string root = RootOf(character.Taxonomy, nodeId);
        ThoughtEntry entry;
        switch (root)
        {
            case "relationships": entry = Relationship(nodeId, rng); break;
            case "knowledge": entry = Knowledge(nodeId, rng); break;
            case "history": entry = History(nodeId, rng); break;
            case "feelings": entry = Feeling(nodeId, rng); break;
            case "present": entry = Present(nodeId, rng); break;
            case "world": entry = World(nodeId, rng); break;
            case "opinions": entry = Opinion(nodeId, rng); break;
            default: entry = Generic(nodeId, rng); break;
        }
        if (entry == null)
            return null;

        entry.NodeId = nodeId;
        entry.Procedural = true;
        entry.CreatedTick = GameClock.Now;
        entry.LastTick = GameClock.Now;
        if (entry.Decay <= 0f)
            entry.Decay = 0.02f;
        if (string.IsNullOrEmpty(entry.Subject))
            entry.Subject = character.Taxonomy.NameOf(nodeId);
        character.Thoughts.Add(entry);
        return entry;
    }

    private static ThoughtEntry Relationship(string nodeId, System.Random rng)
    {
        string key = LastSegment(nodeId);
        string name = Name(key, rng);
        string quirk = Pick(Quirks, rng);
        string detail;
        if (key == "mother" || key == "father" || key == "current" || key == "past")
            detail = "My " + key + " is named " + name + ", " + quirk + ".";
        else
            detail = name + " is in my " + key + ", " + quirk + ".";
        return new ThoughtEntry
        {
            Subject = name,
            RawLabel = name + " " + quirk,
            Detail = detail,
            Sentiment = Rand(rng, -0.3f, 0.6f),
            Impact = 0.4f,
            Decay = 0.02f
        };
    }

    private static ThoughtEntry Knowledge(string nodeId, System.Random rng)
    {
        string key = LastSegment(nodeId);
        if (key == "job" || key == "role")
        {
            string job = Pick(Jobs, rng);
            return new ThoughtEntry { Subject = job, RawLabel = job, Detail = "I work as a " + job + ".", Sentiment = Rand(rng, -0.2f, 0.5f), Impact = 0.5f, Decay = 0.01f };
        }
        if (key == "workplace")
        {
            string place = Pick(Places, rng);
            return new ThoughtEntry { Subject = place, RawLabel = place, Detail = "I spend my days at " + place + ".", Sentiment = Rand(rng, -0.2f, 0.4f), Impact = 0.4f, Decay = 0.01f };
        }
        if (key == "colleagues")
        {
            string name = Name("", rng);
            return new ThoughtEntry { Subject = name, RawLabel = name + " colleague", Detail = name + " works with me.", Sentiment = Rand(rng, -0.3f, 0.6f), Impact = 0.4f, Decay = 0.02f };
        }
        string general = Pick(Quirks, rng);
        return new ThoughtEntry { Subject = Pick(Jobs, rng), RawLabel = general, Detail = "I know my way around " + general + " work.", Sentiment = Rand(rng, 0f, 0.5f), Impact = 0.35f, Decay = 0.01f };
    }

    private static ThoughtEntry History(string nodeId, System.Random rng)
    {
        string key = LastSegment(nodeId);
        string ev = Pick(Events, rng);
        if (key == "childhood" && rng.NextDouble() < 0.5)
            ev = "a childhood in " + Pick(Places, rng);
        return new ThoughtEntry { Subject = ev, RawLabel = ev, Detail = "That was " + ev + ".", Sentiment = Rand(rng, -0.7f, 0.2f), Impact = 0.6f, Decay = 0.01f };
    }

    private static ThoughtEntry Feeling(string nodeId, System.Random rng)
    {
        string feeling = Pick(Feelings, rng);
        return new ThoughtEntry { Subject = feeling, RawLabel = feeling, Detail = "Right now I feel " + feeling + ".", Sentiment = Rand(rng, -0.6f, 0.6f), Impact = 0.5f, Decay = 0.08f };
    }

    private static ThoughtEntry Present(string nodeId, System.Random rng)
    {
        string action = Pick(Actions, rng);
        return new ThoughtEntry { Subject = action, RawLabel = action, Detail = "These days I am " + action + ".", Sentiment = Rand(rng, -0.4f, 0.5f), Impact = 0.4f, Decay = 0.1f };
    }

    private static ThoughtEntry World(string nodeId, System.Random rng)
    {
        string rumor = Pick(Rumors, rng);
        return new ThoughtEntry { Subject = rumor, RawLabel = rumor, Detail = "People mention " + rumor + ".", Sentiment = Rand(rng, -0.5f, 0.3f), Impact = 0.3f, Decay = 0.1f };
    }

    private static ThoughtEntry Opinion(string nodeId, System.Random rng)
    {
        string topic = Pick(Topics, rng);
        float sentiment = Rand(rng, -0.7f, 0.7f);
        return new ThoughtEntry { Subject = topic, RawLabel = topic + " " + RawRenderer.SentimentWord(sentiment), Detail = "I have strong thoughts about " + topic + ".", Sentiment = sentiment, Impact = 0.45f, Decay = 0.02f };
    }

    private static ThoughtEntry Generic(string nodeId, System.Random rng)
    {
        string quirk = Pick(Quirks, rng);
        return new ThoughtEntry { Subject = LastSegment(nodeId), RawLabel = quirk, Detail = "It is " + quirk + ", I suppose.", Sentiment = Rand(rng, -0.3f, 0.4f), Impact = 0.3f, Decay = 0.05f };
    }

    private static string Name(string key, System.Random rng)
    {
        if (key == "mother")
            return Pick(FemaleNames, rng);
        if (key == "father")
            return Pick(MaleNames, rng);
        int roll = rng.Next(0, 3);
        return roll == 0 ? Pick(MaleNames, rng) : roll == 1 ? Pick(FemaleNames, rng) : Pick(NeutralNames, rng);
    }

    private static string RootOf(ThoughtTaxonomy taxonomy, string nodeId)
    {
        var node = taxonomy.Get(nodeId);
        int guard = 0;
        while (node != null && !string.IsNullOrEmpty(node.ParentId) && guard++ < 64)
            node = taxonomy.Get(node.ParentId);
        return node == null ? "" : node.Id;
    }

    private static string LastSegment(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
            return "";
        int slash = nodeId.LastIndexOf('/');
        return slash < 0 ? nodeId : nodeId.Substring(slash + 1);
    }

    private static string Pick(string[] pool, System.Random rng)
    {
        return pool[rng.Next(0, pool.Length)];
    }

    private static float Rand(System.Random rng, float min, float max)
    {
        return min + (float)rng.NextDouble() * (max - min);
    }
}
