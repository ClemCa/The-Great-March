using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Invents a plausible setting when an Open character is asked about an empty path.
/// Every leaf has its own process, and each process is biased by the character's current
/// <see cref="ThoughtState"/> (live mood, traits, and the world events they still feel).
/// This is deliberately not the LLM: it keeps the world populated before content exists.
/// </summary>
public static class SettingGenerator
{
    private static readonly string[] MaleNames = { "Aldric", "Bram", "Cato", "Dorian", "Elias", "Fen", "Garan", "Hollis" };
    private static readonly string[] FemaleNames = { "Mira", "Sable", "Tova", "Wren", "Anya", "Bettina", "Clove", "Dahlia" };
    private static readonly string[] NeutralNames = { "Ash", "Juno", "Ren", "Sol", "Vesper", "Mar", "Kai", "Nic" };
    private static readonly string[] Jobs = { "dockworker", "archivist", "trader", "medic", "engineer", "farmer", "pilot", "teacher" };
    private static readonly string[] Quirks = { "kind", "stubborn", "funny", "proud", "quiet", "reckless", "gentle", "shrewd" };
    private static readonly string[] Places = { "the old station", "the outer rim", "the capital", "the salt flats", "the orbital yards" };
    private static readonly string[] Skills = { "welding", "navigation", "first aid", "haggling", "engine repair", "cooking", "reading old charts", "staying calm", "picking locks" };
    private static readonly string[] Studies = { "the orbital academy", "a trade school", "night classes", "an apprenticeship", "the archive program", "self-teaching" };
    private static readonly string[] Fields = { "history", "mathematics", "engineering", "medicine", "law", "astronomy", "old languages" };

    private static readonly string[] PositiveFeelings = { "hopeful", "content", "excited", "grateful", "proud", "cheerful" };
    private static readonly string[] NeutralFeelings = { "tired", "calm", "distracted", "restless", "thoughtful" };
    private static readonly string[] NegativeFeelings = { "uneasy", "lonely", "anxious", "frustrated", "sad", "angry" };

    private static readonly string[] ShortTermLowWants = { "to be left alone", "to get through the day", "to curl up and sleep", "to talk to someone", "to stop worrying" };
    private static readonly string[] ShortTermHighWants = { "to keep this feeling going", "to celebrate", "to tell everyone", "to do something reckless", "to share the good news" };
    private static readonly string[] LongTermWants = { "to matter", "to be more than a test subject", "to see the frontier", "to build a place of my own", "to prove them wrong", "to be remembered" };

    private static readonly string[] Fears = { "being replaced", "dying alone", "being forgotten", "losing the people I love", "failing when it counts", "running out of time" };
    private static readonly string[] Hopes = { "to pass the test", "to be more than a test subject", "to start over somewhere new", "to make something that lasts", "to be understood", "to see the frontier" };

    private static readonly string[] DoingNormal = { "keeping busy", "working the same job", "getting by", "staying out of trouble" };
    private static readonly string[] DoingLow = { "keeping my head down", "lying low", "going through the motions", "avoiding people" };
    private static readonly string[] DoingHigh = { "riding the good mood", "making the most of it", "taking big swings", "saying yes to everything" };

    private static readonly string[] PlansShort = { "to sleep early", "to finish this today", "to lay low for a while", "to check on a friend tonight" };
    private static readonly string[] PlansLong = { "to leave this place", "to save enough to buy my way out", "to start over somewhere new", "to build something that outlasts me" };

    private static readonly string[] Troubles = { "money", "the leak in the roof", "a rival at work", "bad news from home", "a debt I can't pay" };

    private static readonly string[] News = { "a festival is coming", "prices are up again", "a new ship docked this morning", "the council is meeting again" };
    private static readonly string[] Rumors = { "a whisper about the council", "talk of smuggling", "a story about a ghost fleet", "news of a distant war" };
    private static readonly string[] WorldPeople = { "a wandering preacher", "a famous general", "a smuggler I know", "the new governor", "an off-world trader" };
    private static readonly string[] Factions = { "the council", "the guild", "the navy", "the free traders", "the archive order" };

    private static readonly string[] RecentHistory = { "a rough week", "a small win", "a strange dream", "an argument I regret", "a favor owed" };
    private static readonly string[] PersonalHistory = { "a promise I broke", "a risk I took", "a debt I paid", "a lie I told", "a favor I owe" };
    private static readonly string[] WorldHistory = { "the old war", "the founding", "a lost colony", "the last coup" };
    private static readonly string[] ChildhoodHistory = { "a strict household", "moving too often", "early trials", "a lonely childhood" };
    private static readonly string[] TurningPoints = { "the day I left home", "the accident", "the day everything changed", "the night I almost died" };

    private static readonly string[] OpinionTopics = { "the new tariffs", "old music", "the frontier", "religion", "the old regime", "off-worlders" };
    private static readonly string[] PoliticsTopics = { "the council", "the elections", "the new law", "the war effort" };
    private static readonly string[] Values = { "honesty", "loyalty", "freedom", "family", "hard work" };
    private static readonly string[] Culture = { "the old songs", "the festivals", "the way we bury our dead", "the food", "the stories we tell" };

    public static ThoughtEntry Generate(ThoughtCharacter character, string nodeId, System.Random rng)
    {
        if (character == null || character.Taxonomy == null || !character.Taxonomy.Exists(nodeId))
            return null;

        var state = ThoughtState.From(character);
        var entry = Create(state, nodeId, rng);
        if (entry == null)
            entry = Generic(state, nodeId, rng);
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

    private static ThoughtEntry Create(ThoughtState s, string nodeId, System.Random rng)
    {
        // Authored knowledge wins: if the designer declared a fact for this node (even without a
        // hand-written thought), surface it verbatim instead of inventing something that could
        // contradict it.
        if (nodeId != null && nodeId.StartsWith("knowledge/"))
        {
            var authored = FindKnowledge(s, nodeId);
            if (authored != null && !string.IsNullOrEmpty(authored.Value))
                return FromKnowledge(s, authored, rng);
        }

        switch (nodeId)
        {
            // ----- Feelings -----
            case "feelings/current": return CurrentFeeling(s, rng);
            case "feelings/mood": return Mood(s, rng);
            case "feelings/wants": return Wants(s, rng);
            case "feelings/fears": return Fears_(s, rng);
            case "feelings/hopes": return Hopes_(s, rng);

            // ----- Present -----
            case "present/doing": return Doing(s, rng);
            case "present/plans": return Plans(s, rng);
            case "present/troubles": return Troubles_(s, rng);
            case "present/news": return News_(s, rng);

            // ----- World -----
            case "world/events": return WorldEvents_(s, rng);
            case "world/places": return Places_(s, rng);
            case "world/people": return WorldPeople_(s, rng);
            case "world/rumors": return Rumors_(s, rng);
            case "world/factions": return Factions_(s, rng);

            // ----- History -----
            case "history/recent": return Recent(s, rng);
            case "history/personal": return Personal_(s, rng);
            case "history/world": return WorldHistory_(s, rng);
            case "history/childhood": return Childhood(s, rng);
            case "history/turningpoints": return TurningPoints_(s, rng);

            // ----- Opinions -----
            case "opinions/people": return OpinionPeople(s, rng);
            case "opinions/topics": return OpinionTopics_(s, rng);
            case "opinions/politics": return OpinionPolitics(s, rng);
            case "opinions/values": return OpinionValues(s, rng);
            case "opinions/culture": return OpinionCulture(s, rng);

            // ----- Knowledge -----
            case "knowledge/job/role": return Job(s, rng);
            case "knowledge/job/colleagues": return Colleagues(s, rng);
            case "knowledge/job/skills": return Skills_(s, rng);
            case "knowledge/job/workplace": return Workplace(s, rng);
            case "knowledge/education": return Education(s, rng);
            case "knowledge/background": return Background(s, rng);

            // ----- Relationships -----
            case "relationships/family/mother": return Relative(s, rng, nodeId, "mother", FemaleNames, 0.5f);
            case "relationships/family/father": return Relative(s, rng, nodeId, "father", MaleNames, 0.5f);
            case "relationships/family/siblings": return Relative(s, rng, nodeId, "sibling", NeutralNames, 0.4f);
            case "relationships/family/children": return Relative(s, rng, nodeId, "child", NeutralNames, 0.6f);
            case "relationships/family/extended": return Relative(s, rng, nodeId, "extended family", NeutralNames, 0.4f);
            case "relationships/friends/close": return Relative(s, rng, nodeId, "close friend", NeutralNames, 0.6f);
            case "relationships/friends/acquaintances": return Relative(s, rng, nodeId, "acquaintance", NeutralNames, 0.3f);
            case "relationships/friends/lost": return Relative(s, rng, nodeId, "lost friend", NeutralNames, 0.1f);
            case "relationships/rivals/enemies": return Relative(s, rng, nodeId, "enemy", NeutralNames, -0.7f);
            case "relationships/rivals/competitors": return Relative(s, rng, nodeId, "competitor", NeutralNames, -0.3f);
            case "relationships/partners/current": return Relative(s, rng, nodeId, "partner", NeutralNames, 0.7f);
            case "relationships/partners/past": return Relative(s, rng, nodeId, "past partner", NeutralNames, 0.1f);
            case "relationships/self/selfworth": return SelfWorth(s, rng);
            case "relationships/self/body": return SelfBody(s, rng);
            case "relationships/self/past": return SelfPast(s, rng);
        }
        return null;
    }

    #region Feelings

    private static ThoughtEntry CurrentFeeling(ThoughtState s, System.Random rng)
    {
        string feeling;
        if (s.Low)
            feeling = Pick(NegativeFeelings, rng);
        else if (s.High)
            feeling = Pick(PositiveFeelings, rng);
        else
            feeling = Pick(NeutralFeelings, rng);

        return new ThoughtEntry
        {
            Subject = feeling,
            RawLabel = feeling,
            Detail = "Right now I feel " + feeling + ".",
            Sentiment = Mathf.Clamp(s.Signed + Rand(rng, -0.25f, 0.25f), -1f, 1f),
            Impact = 0.5f,
            Decay = 0.08f
        };
    }

    /// <summary>
    /// The standing mood. Driven by the events still being felt, weighted by how personally they land;
    /// only falls back to raw thought valence when nothing is going on.
    /// </summary>
    private static ThoughtEntry Mood(ThoughtState s, System.Random rng)
    {
        float valence = s.HasAnyEvent ? s.EventValence(true) : s.Valence;
        string mood = MoodWord(valence);
        return new ThoughtEntry
        {
            Subject = mood,
            RawLabel = mood,
            Detail = "Lately I've been feeling " + mood + ".",
            Sentiment = Mathf.Clamp(valence * 2f - 1f, -1f, 1f),
            Impact = 0.45f,
            Decay = 0.06f
        };
    }

    /// <summary>
    /// Short-term wants when the mood is running hot or cold; long-term wants when it is steady.
    /// Intensity is what decides the horizon.
    /// </summary>
    private static ThoughtEntry Wants(ThoughtState s, System.Random rng)
    {
        string want;
        string lead;
        float decay;
        if (s.Low)
        {
            want = Pick(ShortTermLowWants, rng);
            lead = "Right now I want ";
            decay = 0.12f;
        }
        else if (s.High)
        {
            want = Pick(ShortTermHighWants, rng);
            lead = "Right now I want ";
            decay = 0.12f;
        }
        else
        {
            want = Pick(LongTermWants, rng);
            lead = "More than anything, I want ";
            decay = 0.02f;
        }

        return new ThoughtEntry
        {
            Subject = want,
            RawLabel = want,
            Detail = lead + want + ".",
            Sentiment = Sentiment(s, rng, 0.1f, 0.6f, 0f),
            Impact = 0.5f,
            Decay = decay
        };
    }

    private static ThoughtEntry Fears_(ThoughtState s, System.Random rng)
    {
        string fear = Pick(Fears, rng);
        float bias = s.Low ? -0.2f : 0f;
        return new ThoughtEntry
        {
            Subject = fear,
            RawLabel = fear,
            Detail = "I'm afraid of " + fear + ".",
            Sentiment = Sentiment(s, rng, -0.8f, -0.2f, bias),
            Impact = 0.6f,
            Decay = 0.05f
        };
    }

    private static ThoughtEntry Hopes_(ThoughtState s, System.Random rng)
    {
        string hope = Pick(Hopes, rng);
        float bias = s.Low ? -0.15f : 0.1f;
        return new ThoughtEntry
        {
            Subject = hope,
            RawLabel = hope,
            Detail = "I hope " + hope + ".",
            Sentiment = Sentiment(s, rng, 0.2f, 0.8f, bias),
            Impact = 0.5f,
            Decay = 0.05f
        };
    }

    private static string MoodWord(float valence)
    {
        if (valence < 0.20f) return "miserable";
        if (valence < 0.38f) return "low";
        if (valence < 0.55f) return "steady";
        if (valence < 0.72f) return "good";
        return "great";
    }

    #endregion

    #region Present

    private static ThoughtEntry Doing(ThoughtState s, System.Random rng)
    {
        string action;
        if (s.Low)
            action = Pick(DoingLow, rng);
        else if (s.High)
            action = Pick(DoingHigh, rng);
        else
            action = Pick(DoingNormal, rng);

        return new ThoughtEntry
        {
            Subject = action,
            RawLabel = action,
            Detail = "These days I am " + action + ".",
            Sentiment = Sentiment(s, rng, -0.3f, 0.4f, 0f),
            Impact = 0.4f,
            Decay = 0.1f
        };
    }

    private static ThoughtEntry Plans(ThoughtState s, System.Random rng)
    {
        bool shortTerm = s.Low || s.High;
        string plan = shortTerm ? Pick(PlansShort, rng) : Pick(PlansLong, rng);
        return new ThoughtEntry
        {
            Subject = plan,
            RawLabel = plan,
            Detail = "I plan " + plan + ".",
            Sentiment = Sentiment(s, rng, 0f, 0.5f, 0f),
            Impact = 0.45f,
            Decay = shortTerm ? 0.12f : 0.02f
        };
    }

    private static ThoughtEntry Troubles_(ThoughtState s, System.Random rng)
    {
        var ev = s.BestEvent(true, false, null);
        if (ev != null)
            return new ThoughtEntry
            {
                Subject = ev.Name,
                RawLabel = ev.Name,
                Detail = ev.Name + ". It's still weighing on me.",
                Sentiment = Sentiment(s, rng, -0.8f, -0.3f, 0f),
                Impact = 0.6f,
                Decay = 0.08f
            };

        string trouble = Pick(Troubles, rng);
        return new ThoughtEntry
        {
            Subject = trouble,
            RawLabel = trouble,
            Detail = "I can't stop thinking about " + trouble + ".",
            Sentiment = Sentiment(s, rng, -0.7f, -0.2f, 0f),
            Impact = 0.5f,
            Decay = 0.08f
        };
    }

    private static ThoughtEntry News_(ThoughtState s, System.Random rng)
    {
        var ev = s.BestEvent(false, false, null, EventScope.Local, EventScope.Region);
        if (ev != null)
            return new ThoughtEntry
            {
                Subject = ev.Name,
                RawLabel = ev.Name,
                Detail = ev.Name + ". Everyone's talking about it.",
                Sentiment = Sentiment(s, rng, -0.4f, 0.4f, 0f),
                Impact = 0.35f,
                Decay = 0.1f
            };

        string news = Pick(News, rng);
        return new ThoughtEntry
        {
            Subject = news,
            RawLabel = news,
            Detail = "They say " + news + ".",
            Sentiment = Sentiment(s, rng, -0.3f, 0.4f, 0f),
            Impact = 0.3f,
            Decay = 0.1f
        };
    }

    #endregion

    #region World

    private static ThoughtEntry WorldEvents_(ThoughtState s, System.Random rng)
    {
        var ev = s.BestEvent(false, false, null, EventScope.World);
        if (ev != null)
            return new ThoughtEntry
            {
                Subject = ev.Name,
                RawLabel = ev.Name,
                Detail = "Word is out: " + ev.Name + ".",
                Sentiment = Sentiment(s, rng, -0.5f, 0.5f, 0f),
                Impact = 0.4f,
                Decay = 0.05f
            };

        var region = s.BestEvent(false, false, null, EventScope.Region);
        if (region != null)
            return new ThoughtEntry
            {
                Subject = region.Name,
                RawLabel = region.Name,
                Detail = "Word is out: " + region.Name + ".",
                Sentiment = Sentiment(s, rng, -0.5f, 0.5f, 0f),
                Impact = 0.4f,
                Decay = 0.05f
            };

        string rumor = Pick(Rumors, rng);
        return new ThoughtEntry
        {
            Subject = rumor,
            RawLabel = rumor,
            Detail = "People mention " + rumor + ".",
            Sentiment = Sentiment(s, rng, -0.5f, 0.3f, 0f),
            Impact = 0.3f,
            Decay = 0.1f
        };
    }

    private static ThoughtEntry Places_(ThoughtState s, System.Random rng)
    {
        string place = Pick(Places, rng);
        return new ThoughtEntry
        {
            Subject = place,
            RawLabel = place,
            Detail = "I know " + place + " better than most.",
            Sentiment = Sentiment(s, rng, -0.2f, 0.4f, 0f),
            Impact = 0.35f,
            Decay = 0.04f
        };
    }

    private static ThoughtEntry WorldPeople_(ThoughtState s, System.Random rng)
    {
        string person = Pick(WorldPeople, rng);
        return new ThoughtEntry
        {
            Subject = person,
            RawLabel = person,
            Detail = person + " has been making the rounds.",
            Sentiment = Sentiment(s, rng, -0.3f, 0.3f, 0f),
            Impact = 0.3f,
            Decay = 0.05f
        };
    }

    private static ThoughtEntry Rumors_(ThoughtState s, System.Random rng)
    {
        string rumor = Pick(Rumors, rng);
        return new ThoughtEntry
        {
            Subject = rumor,
            RawLabel = rumor,
            Detail = "There's " + rumor + " going around.",
            Sentiment = Sentiment(s, rng, -0.4f, 0.3f, 0f),
            Impact = 0.3f,
            Decay = 0.1f
        };
    }

    private static ThoughtEntry Factions_(ThoughtState s, System.Random rng)
    {
        string faction = Pick(Factions, rng);
        return new ThoughtEntry
        {
            Subject = faction,
            RawLabel = faction,
            Detail = faction + " has more pull than people admit.",
            Sentiment = Sentiment(s, rng, -0.4f, 0.3f, 0f),
            Impact = 0.3f,
            Decay = 0.03f
        };
    }

    #endregion

    #region History

    private static ThoughtEntry Recent(ThoughtState s, System.Random rng)
    {
        var ev = s.BestEvent(false, false, null);
        if (ev != null)
            return new ThoughtEntry
            {
                Subject = ev.Name,
                RawLabel = ev.Name,
                Detail = "Not long ago: " + ev.Name + ".",
                Sentiment = Sentiment(s, rng, -0.6f, 0.6f, 0f),
                Impact = 0.55f,
                Decay = 0.12f
            };

        string item = Pick(RecentHistory, rng);
        return new ThoughtEntry
        {
            Subject = item,
            RawLabel = item,
            Detail = "It's been " + item + " lately.",
            Sentiment = Sentiment(s, rng, -0.6f, 0.4f, 0f),
            Impact = 0.5f,
            Decay = 0.12f
        };
    }

    private static ThoughtEntry Personal_(ThoughtState s, System.Random rng)
    {
        string item = Pick(PersonalHistory, rng);
        return new ThoughtEntry
        {
            Subject = item,
            RawLabel = item,
            Detail = "I still think about " + item + ".",
            Sentiment = Sentiment(s, rng, -0.6f, 0.2f, 0f),
            Impact = 0.6f,
            Decay = 0.01f
        };
    }

    private static ThoughtEntry WorldHistory_(ThoughtState s, System.Random rng)
    {
        string item = Pick(WorldHistory, rng);
        return new ThoughtEntry
        {
            Subject = item,
            RawLabel = item,
            Detail = "People still argue about " + item + ".",
            Sentiment = Sentiment(s, rng, -0.5f, 0.2f, 0f),
            Impact = 0.5f,
            Decay = 0.01f
        };
    }

    private static ThoughtEntry Childhood(ThoughtState s, System.Random rng)
    {
        string item = rng.NextDouble() < 0.5 ? "a childhood in " + Pick(Places, rng) : Pick(ChildhoodHistory, rng);
        return new ThoughtEntry
        {
            Subject = item,
            RawLabel = item,
            Detail = "That was " + item + ".",
            Sentiment = Sentiment(s, rng, -0.6f, 0.2f, 0f),
            Impact = 0.55f,
            Decay = 0.01f
        };
    }

    private static ThoughtEntry TurningPoints_(ThoughtState s, System.Random rng)
    {
        string item = Pick(TurningPoints, rng);
        return new ThoughtEntry
        {
            Subject = item,
            RawLabel = item,
            Detail = "I keep coming back to " + item + ".",
            Sentiment = Sentiment(s, rng, -0.7f, 0.3f, 0f),
            Impact = 0.65f,
            Decay = 0.01f
        };
    }

    #endregion

    #region Opinions

    private static ThoughtEntry OpinionPeople(ThoughtState s, System.Random rng)
    {
        var relationship = FirstRelationship(s);
        string name = relationship != null && !string.IsNullOrEmpty(relationship.PersonName) ? relationship.PersonName : Pick(NeutralNames, rng);
        float opinion = relationship != null ? relationship.Opinion : Sentiment(s, rng, -0.3f, 0.6f, 0f);
        string line;
        if (opinion > 0.6f) line = "I'd trust " + name + " with anything.";
        else if (opinion > 0.2f) line = name + " is alright by me.";
        else if (opinion > -0.2f) line = "I don't have strong feelings about " + name + ".";
        else if (opinion > -0.6f) line = name + " rubs me the wrong way.";
        else line = "I can't stand " + name + ".";

        return new ThoughtEntry
        {
            Subject = name,
            RawLabel = name + " " + RawRenderer.SentimentWord(opinion),
            Detail = line,
            Sentiment = Mathf.Clamp(opinion, -1f, 1f),
            Impact = 0.45f,
            Decay = 0.02f
        };
    }

    private static ThoughtEntry OpinionTopics_(ThoughtState s, System.Random rng)
    {
        string topic = Pick(OpinionTopics, rng);
        float sentiment = Sentiment(s, rng, -0.7f, 0.7f, 0f);
        return new ThoughtEntry
        {
            Subject = topic,
            RawLabel = topic + " " + RawRenderer.SentimentWord(sentiment),
            Detail = "I have strong thoughts about " + topic + ".",
            Sentiment = sentiment,
            Impact = 0.45f,
            Decay = 0.02f
        };
    }

    private static ThoughtEntry OpinionPolitics(ThoughtState s, System.Random rng)
    {
        var ev = s.BestEvent(false, false, "politics");
        string topic = ev != null ? ev.Name : Pick(PoliticsTopics, rng);
        float sentiment = Sentiment(s, rng, -0.7f, 0.7f, 0f);
        return new ThoughtEntry
        {
            Subject = topic,
            RawLabel = topic + " " + RawRenderer.SentimentWord(sentiment),
            Detail = "I have strong opinions about " + topic + ".",
            Sentiment = sentiment,
            Impact = 0.45f,
            Decay = 0.02f
        };
    }

    private static ThoughtEntry OpinionValues(ThoughtState s, System.Random rng)
    {
        string value = Pick(Values, rng);
        float sentiment = Sentiment(s, rng, 0.3f, 0.8f, 0.1f);
        return new ThoughtEntry
        {
            Subject = value,
            RawLabel = value,
            Detail = "I believe in " + value + ".",
            Sentiment = sentiment,
            Impact = 0.5f,
            Decay = 0.01f
        };
    }

    private static ThoughtEntry OpinionCulture(ThoughtState s, System.Random rng)
    {
        string item = Pick(Culture, rng);
        float sentiment = Sentiment(s, rng, -0.4f, 0.7f, 0.1f);
        return new ThoughtEntry
        {
            Subject = item,
            RawLabel = item,
            Detail = "I care a lot about " + item + ".",
            Sentiment = sentiment,
            Impact = 0.4f,
            Decay = 0.02f
        };
    }

    #endregion

    #region Knowledge

    private static ThoughtEntry Job(ThoughtState s, System.Random rng)
    {
        string job = Pick(Jobs, rng);
        return new ThoughtEntry { Subject = job, RawLabel = job, Detail = "I work as a " + job + ".", Sentiment = Sentiment(s, rng, -0.2f, 0.5f, 0f), Impact = 0.5f, Decay = 0.01f };
    }

    private static ThoughtEntry Colleagues(ThoughtState s, System.Random rng)
    {
        string name = Name("", rng);
        return new ThoughtEntry { Subject = name, RawLabel = name, Detail = name + " is a colleague of mine.", Sentiment = Sentiment(s, rng, -0.3f, 0.6f, 0f), Impact = 0.4f, Decay = 0.02f };
    }

    private static ThoughtEntry Skills_(ThoughtState s, System.Random rng)
    {
        string skill = Pick(Skills, rng);
        return new ThoughtEntry { Subject = skill, RawLabel = skill, Detail = "I am good at " + skill + ".", Sentiment = Sentiment(s, rng, 0f, 0.6f, 0.1f), Impact = 0.4f, Decay = 0.02f };
    }

    private static ThoughtEntry Workplace(ThoughtState s, System.Random rng)
    {
        string place = Pick(Places, rng);
        return new ThoughtEntry { Subject = place, RawLabel = place, Detail = "I spend my days at " + place + ".", Sentiment = Sentiment(s, rng, -0.2f, 0.4f, 0f), Impact = 0.4f, Decay = 0.01f };
    }

    private static ThoughtEntry Education(ThoughtState s, System.Random rng)
    {
        string study = Pick(Studies, rng);
        return new ThoughtEntry { Subject = study, RawLabel = study, Detail = "I learned what I know through " + study + ".", Sentiment = Sentiment(s, rng, -0.2f, 0.5f, 0f), Impact = 0.4f, Decay = 0.01f };
    }

    private static ThoughtEntry Background(ThoughtState s, System.Random rng)
    {
        string home = Pick(Places, rng);
        return new ThoughtEntry { Subject = home, RawLabel = home, Detail = "I came up around " + home + ".", Sentiment = Sentiment(s, rng, -0.3f, 0.5f, 0f), Impact = 0.45f, Decay = 0.01f };
    }

    #endregion

    #region Relationships

    private static ThoughtEntry Relative(ThoughtState s, System.Random rng, string nodeId, string role, string[] names, float baseOpinion)
    {
        var authored = FindRelationship(s, nodeId);
        string name = authored != null && !string.IsNullOrEmpty(authored.PersonName) ? authored.PersonName : Pick(names, rng);
        float opinion = authored != null ? authored.Opinion : Sentiment(s, rng, baseOpinion - 0.3f, baseOpinion + 0.3f, 0f);
        string quirk = Pick(Quirks, rng);

        string detail;
        if (authored != null && authored.Notes != null && authored.Notes.Count > 0)
        {
            detail = string.Join(" ", authored.Notes);
        }
        else
        {
            switch (role)
            {
                case "mother": detail = "My mother is named " + name + ", " + quirk + "."; break;
                case "father": detail = "My father is named " + name + ", " + quirk + "."; break;
                case "sibling": detail = name + " is my sibling, " + quirk + "."; break;
                case "child": detail = name + " is my child, " + quirk + "."; break;
                case "extended family": detail = name + " is family, " + quirk + "."; break;
                case "close friend": detail = name + " is one of my closest friends, " + quirk + "."; break;
                case "acquaintance": detail = name + " is someone I know, " + quirk + "."; break;
                case "lost friend": detail = "I lost touch with " + name + " years ago."; break;
                case "enemy": detail = name + " has it out for me."; break;
                case "competitor": detail = name + " keeps beating me at everything."; break;
                case "partner": detail = name + " is my partner, " + quirk + "."; break;
                case "past partner": detail = name + " and I were together once."; break;
                default: detail = name + " is " + role + " to me."; break;
            }
        }

        return new ThoughtEntry
        {
            Subject = name,
            RawLabel = name + " " + quirk,
            Detail = detail,
            Sentiment = Mathf.Clamp(opinion, -1f, 1f),
            Impact = 0.5f,
            Decay = 0.02f
        };
    }

    private static ThoughtEntry SelfWorth(ThoughtState s, System.Random rng)
    {
        bool positive = s.Valence >= 0.5f || (s.Profile != null && s.Profile.ValenceBias > 0f);
        string detail = positive ? "I think I'm worth something." : "I'm not sure I'm worth much.";
        return new ThoughtEntry
        {
            Subject = "self worth",
            RawLabel = "self worth " + (positive ? "good" : "sad"),
            Detail = detail,
            Sentiment = Sentiment(s, rng, positive ? 0.2f : -0.6f, positive ? 0.6f : -0.1f, 0f),
            Impact = 0.55f,
            Decay = 0.03f
        };
    }

    private static ThoughtEntry SelfBody(ThoughtState s, System.Random rng)
    {
        bool positive = s.Valence >= 0.5f;
        string detail = positive ? "I feel good in my own skin." : "I don't like what I see in the mirror.";
        return new ThoughtEntry
        {
            Subject = "my body",
            RawLabel = "my body " + (positive ? "good" : "sad"),
            Detail = detail,
            Sentiment = Sentiment(s, rng, positive ? 0.1f : -0.6f, positive ? 0.5f : -0.1f, 0f),
            Impact = 0.45f,
            Decay = 0.04f
        };
    }

    private static ThoughtEntry SelfPast(ThoughtState s, System.Random rng)
    {
        string detail = s.Low ? "The person I used to be would hate who I've become." : "I'm not the person I used to be.";
        return new ThoughtEntry
        {
            Subject = "who I was",
            RawLabel = "who I was",
            Detail = detail,
            Sentiment = Sentiment(s, rng, -0.5f, 0.3f, 0f),
            Impact = 0.5f,
            Decay = 0.02f
        };
    }

    #endregion

    private static ThoughtEntry Generic(ThoughtState s, string nodeId, System.Random rng)
    {
        float sentiment = Sentiment(s, rng, -0.3f, 0.4f, 0f);
        string topic = Humanize(LastSegment(nodeId));
        return new ThoughtEntry
        {
            Subject = topic,
            RawLabel = topic + " " + RawRenderer.SentimentWord(sentiment),
            Detail = "I have feelings about " + topic + ".",
            Sentiment = sentiment,
            Impact = 0.3f,
            Decay = 0.05f
        };
    }

    private static KnowledgeInfo FindKnowledge(ThoughtState s, string nodeId)
    {
        if (s.Character == null || s.Character.Knowledge == null)
            return null;
        for (int i = 0; i < s.Character.Knowledge.Count; i++)
            if (s.Character.Knowledge[i].NodeId == nodeId)
                return s.Character.Knowledge[i];
        return null;
    }

    private static ThoughtEntry FromKnowledge(ThoughtState s, KnowledgeInfo info, System.Random rng)
    {
        string label = string.IsNullOrEmpty(info.Key) ? info.Value : info.Key;
        return new ThoughtEntry
        {
            Subject = label,
            RawLabel = label,
            Detail = info.Value,
            Sentiment = Sentiment(s, rng, 0f, 0.5f, 0.1f),
            Impact = 0.45f,
            Decay = 0.01f
        };
    }

    private static RelationshipInfo FindRelationship(ThoughtState s, string nodeId)
    {
        if (s.Character == null || s.Character.Relationships == null || s.Character.Taxonomy == null)
            return null;

        // Most specific authored relationship wins, so a rule on a branch (e.g. relationships/friends)
        // also covers its descendant leaves.
        var current = nodeId;
        int guard = 0;
        while (!string.IsNullOrEmpty(current) && guard++ < 64)
        {
            for (int i = 0; i < s.Character.Relationships.Count; i++)
                if (s.Character.Relationships[i].NodeId == current)
                    return s.Character.Relationships[i];
            var node = s.Character.Taxonomy.Get(current);
            current = node == null ? "" : node.ParentId;
        }
        return null;
    }

    private static RelationshipInfo FirstRelationship(ThoughtState s)
    {
        if (s.Character == null || s.Character.Relationships == null || s.Character.Relationships.Count == 0)
            return null;
        return s.Character.Relationships[0];
    }

    /// <summary>Base roll nudged by current mood and the trait's outlook bias.</summary>
    private static float Sentiment(ThoughtState s, System.Random rng, float min, float max, float bias)
    {
        float value = Rand(rng, min, max);
        value += (s.Valence - 0.5f) * 0.5f;
        if (s.Profile != null)
            value += s.Profile.ValenceBias * 0.25f;
        value += bias;
        return Mathf.Clamp(value, -1f, 1f);
    }

    private static string Humanize(string id)
    {
        if (string.IsNullOrEmpty(id))
            return "";
        return char.ToUpperInvariant(id[0]) + id.Substring(1);
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
