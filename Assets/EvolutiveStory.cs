using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using static GreatMarchOverloads;

public class EvolutiveStory : MonoBehaviour
{
    [Serializable, Visualizable]
    public struct Character
    {
        // Name
        [Visualizable]
        public NameInfo Name;
        // Gender
        [Visualizable]
        public Gender Gender;
        [Visualizable]
        public OpinionInfo SelfWorth;
        // Knowledge
        [Visualizable]
        public Knowledge Knowledge;
        // Personality
        [Visualizable]
        public Personality Personality;
        // Memory
        [Visualizable]
        public Memory Memory;
        // Relationships
        [Visualizable]
        public Relationships Relationships;
        // Present
        [Visualizable]
        public Present Present;
        // Memory-building
            // What was just said
            // What just happened
            // When the player is gone (role)
        // Quest-building
            // Using present data
            // Using relationship data (if someone they like is in trouble, if someone they hate could get into troubles)
    }

    #region Present
    public struct Present
    {
        // Present
            // Actions
                // What they are doing
            // Situation
                // Based on troubles or lackthereof
            // News
                // Weaknesses or troubles being resolved either positively or negatively
            // Their troubles
                // Transformed from weaknesses
            // Their weaknesses (potential troubles)
                // Based on roles, hobby, job, background, ongoing events
        public List<PresentExpiringFact> Actions; // Current actions
        public List<PresentExpiringFact> News; // Good or bad things that happened, without resolution, expire after a while
        public List<PresentFact> Troubles;
        public List<PresentFact> Weaknesses;
        public void DiscoverWeaknesses()
        {
            // go through roles, hobbies, jobs, background, ongoing events
            // figure out of a weakness associated with either of them can be added
            throw new NotImplementedException();
        }
        public void UpdateTroubles()
        {
            // go through every weakness, if good resolution remove, if bad fetch matching trouble
            // set trouble & matching action
            // go through every trouble, create news or add siutation accordingly to resolution
            throw new NotImplementedException();
        }
        public void UpdateActions()
        {
            // if current action expired, find a new one to work on
            // based on job, hobby, role & ongoing events
            throw new NotImplementedException();
        }
    }

    public struct PresentExpiringFact
    {
        public Fact Fact;
        public EventLimit EventLimit;
        public OpinionInfo Opinion;
        public bool CheckExpiration()
        {
            throw new NotImplementedException();
        }
    }

    public struct PresentFact
    {
        public Fact Fact;
        public string PositiveCondition; // Encode differently later on
        public string NegativeCondition; // Encode differently later on
        public string PositiveEnd;
        public string NegativeEnd;
        public void CheckConditions()
        {
            throw new NotImplementedException();
        }
    }
    #endregion Present

    #region Relationships
    [Serializable]
    public struct Relationships
    {
        // Relationships
        // Characters
            // Name of relationship ("they are my X")
            // Level of relationship
            // Trend (past changes in said relationship, begins at 0)
        public List<Relationship> All;
        public Relationship GetClosest(float value)
        {
            return All.ClosestWhere(t => t.Opinion.Opinion, value);
        }
        public Relationship Friendliest
        {
            get
            {
                return All.MaxWhere(t => t.Opinion.Opinion);
            }
        }
        public Relationship Muddiest
        {
            get
            {
                return All.MinWhere(t => t.Opinion.Opinion);
            }
        }
        public Relationship GetByName(string name)
        {
            return All.Find(t => t.Name.Name == name);
        }
        public bool HasRelationship(string name)
        {
            return All.FindIndex(t => t.Name.Name == name) != -1;
        }
    }

    #endregion Relationships

    #region Memory

    [Serializable]
    public struct Memory
    {
        // Memory
            // Past opinions
                // By sub-topic
                // Access to average by topic
            // Past facts (small talk)
                // Mother's name
                // What they did at a date
                // etc...
            // Past events
                // If was rolled present / mentioned it in conversation once
            // Past relationships
                // Who they knew
                // What level it stopped
                // What trend it stopped
                // Reason why (death, cut ties, etc)
        public TopicMemory Topic;
        public FactMemory Fact;
        public EventMemory Event;
        public RelationshipMemory Relationships;
    }

    public struct RelationshipMemory
    {
        public List<PastRelationship> Memory;
    }

    public struct EventMemory
    {
        public List<EventFacts> Memory;
    }

    public struct TopicMemory
    {
        public List<Topic> Memory;
        public bool HasMentioned(string topic)
        {
            return Memory.FindIndex(t => t.Name.Name == topic) != -1;
        }
        public bool HasTalked(string topic, string subtopic)
        {
            return Memory.FindIndex(t => t.Name.Name == topic && t.Subtopics.FindIndex(s => s.Name.Name == subtopic) != -1) != -1;
        }
        public float GetPastExpressedOpinion(string topic, string subtopic)
        {
            return Memory.First(t => t.Name.Name == topic).Subtopics.First(s => s.Name.Name == subtopic).Opinion.Opinion;
        }
        public float GetPastAverageOpinion(string topic)
        {
            return Memory.First(t => t.Name.Name == topic).OpinionAverage;
        }
    }
    [Serializable]
    public struct FactMemory
    {
        public List<Fact> Memory;
        public bool HasMentioned(string subject)
        {
            return Memory.FindIndex(t => t.Name.Name == subject) != -1;
        }
        public bool HasTalked(string subject, string flag)
        {
            return Memory.FindIndex(t => t.Name.Name == subject && t.Flag == flag) != -1;
        }
        public string GetPastExpressedFact(string topic, string subtopic)
        {
            return Memory.First(t => t.Name.Name == topic && t.Flag == subtopic).Data;
        }
        public Context GetPastExpressedContext(string topic, string subtopic)
        {
            return Memory.First(t => t.Name.Name == topic && t.Flag == subtopic).Context;
        }
        public bool HasMentionedDate(string topic, string subtopic)
        {
            return Memory.First(t => t.Name.Name == topic && t.Flag == subtopic).Context.hasDate;
        }
        public bool HasMentionedPlace(string topic, string subtopic)
        {
            return Memory.First(t => t.Name.Name == topic && t.Flag == subtopic).Context.hasPlace;
        }
    }

    [Serializable]
    public struct PastRelationship
    {
        public Relationship Relationship;
        public NameInfo Reason;
    }

    public struct EventFacts
    {
        public Event Event;
        public FactMemory Facts;
    }

    [Serializable]
    public struct Fact
    {
        public NameInfo Name; // ex: my mother's name
        public string Subject; // ex: Mother
        public string Flag; // ex: Name
        public string Data; // ex: Mary
        public Context Context; // ex: her parents decided on her name "2 days prior"
    }

    [Serializable]
    public struct Context
    {
        public bool hasDate;
        public bool hasPlace;
        public NameInfo Date;
        public NameInfo Place;
    }

    #endregion Memory

    #region Knowledge
    public struct Knowledge
    {
        // Knowledge
            // Ongoing events
            // Job
            // Hobbies
            // Background
                // Random (pre-rolled)
            // Role (in story terms)
        [Visualizable]
        public List<Event> OngoingEvents;
        [Visualizable]
        public Job Job;
        [Visualizable]
        public List<Hobby> Hobbies;
        [Visualizable]
        public Background Background;
        public StoryRole Role;
    }

    public struct StoryRole
    {
        public string RoleName;
    }

    public struct Job
    {
        public NameInfo Name;
        public string[] SpecializedKnowledge;
    }

    public struct Hobby
    {
        // Defined from a job, has random knowledge, from beginner level up to that of a job
        public NameInfo Name;
        public string[] SpecializedKnowledge;
        public float KnowledgeConfidence;
        public Hobby(NameInfo name, Job job)
        {
            Name = name;
            var r = UnityEngine.Random.Range(1, job.SpecializedKnowledge.Length);
            KnowledgeConfidence = r / job.SpecializedKnowledge.Length;
            var knowledge = new List<string>();
            var left = job.SpecializedKnowledge.ToList();
            while(r > 0)
            {
                var rand = UnityEngine.Random.Range(0, left.Count());
                knowledge.Add(left[rand]);
                left.RemoveAt(rand);
                r--;
            }
            SpecializedKnowledge = knowledge.ToArray();
        }
    }

    public struct Background
    {
        public List<AdditionalKnowledge> AdditionalKnowledge;
    }

    public struct AdditionalKnowledge
    {
        public string SpecializedKnowledge;
        public string Source;
    }

    [Serializable]
    public struct Relationship
    {
        public NameInfo Name;
        public OpinionInfo Opinion;
    }

    public struct Event
    {
        // Event
            // Name
            // Limit
                // Permanent
                // Time
                // Area (distance from X value)
            // Affected Topics
                // Flags
        public NameInfo Name;
        public EventLimit Limit;
        public List<EventAffectedTopic> AffectedTopics;
    }

    public struct EventLimit
    {
        public EventLimitType Type;
        public float Expiration;
    }

    public enum EventLimitType
    {
        Permanent,
        TimeLimited,
        AreaLimited
    }

    public struct EventAffectedTopic
    {
        public NameInfo Name;
        public string[] Flags;
    }

    #endregion Knowledge

    #region Personality & Topics

    public struct Personality
    {
        // Personality
            // Topics
            // Prefered topics, disliked topics
            // Opinions
            // Opinions within said topics
        [Visualizable]
        public List<Topic> Topics;
        public List<Topic> TopicsByLove
        {
            get
            {
                return Topics.OrderBy(t => t.OpinionAverage).ToList();
            }
        }
        public List<Topic> TopicsByHate
        {
            get
            {
                return Topics.OrderByDescending(t => t.OpinionAverage).ToList();
            }
        }
        [Visualizable]
        public Topic PreferedTopic
        {
            get
            {
                return Topics.MaxWhere(t => t.OpinionAverage);
            }
        }
        [Visualizable]
        public Topic HatedTopic
        {
            get
            {
                return Topics.MinWhere(t => t.OpinionAverage);
            }
        }
    }

    public struct Topic
    {
        public NameInfo Name;
        public float LikeRatio;
        public float OpinionAverage
        {
            get
            {
                return Subtopics.Average(t => t.Opinion.Opinion);
            }
        }
        public float TrendAverage
        {
            get
            {
                return Subtopics.Average(t => t.Opinion.Trend);
            }
        }
        public List<Subtopic> Subtopics;
    }

    public struct Subtopic
    {
        public NameInfo Name;
        public OpinionInfo Opinion;
        public List<FlaggedSentence> FlaggedSentences;
        public FlaggedSentence GetFlaggedSentence(string flag)
        {
            return FlaggedSentences.Find(t => t.Flag == flag);
        }
        public bool HasFlag(string flag)
        {
            return FlaggedSentences.FindIndex(t => t.Flag == flag) != -1;
        }
    }

    public struct FlaggedSentence
    {
        public string Flag;
        public string Sentence;
    }

    #endregion Personality & Topics

    #region General Data

    public enum DataType
    {
        Are,
        Have,
        Own,
        Do
    }

    [Serializable]
    public struct OpinionInfo
    {
        public float Opinion;
        public float Trend;
    }

    public enum Gender
    {
        Object,
        Neutral,
        Male,
        Female
    }

    [Serializable]
    public struct NameInfo
    {
        public string Name;
        public OpiniatedName[] OpiniatedNames;
        public OpiniatedName GetOpiniatedName(float opinion)
        {
            if (OpiniatedNames == null || OpiniatedNames.Length == 0)
                return new OpiniatedName() { Name = Name };
            return OpiniatedNames.ClosestWhere(t => t.Opinion.Opinion, opinion);
        }
    }

    [Serializable]
    public struct OpiniatedName
    {
        public string Name;
        public List<string> AltNames;
        public OpinionInfo Opinion;
        public string RandomAltName
        {
            get
            {
                if (AltNames == null || AltNames.Count() == 0)
                    return Name;
                return AltNames[UnityEngine.Random.Range(0, AltNames.Count())];
            }
        }
    }

    [System.Flags]
    public enum ConjugationMask
    {
        Present = (1 << 0),
        Present3 = (1 << 1),
        PresentParticiple = (1 << 2),
        SimplePast = (1 << 3),
        SimplePastPlural = (1 << 4),
        PastParticiple = (1 << 5),
        PassivePresent = (1 << 6),
        PassivePresent3 = (1 << 7)
    }

    public enum Conjugation
    {
        Present,
        Present3,
        PresentParticiple,
        SimplePast,
        SimplePastPlural,
        PastParticiple,
        PassivePresent,
        PassivePresent3
    }
    #endregion General Data

}
public static class GreatMarchOverloads
{

    public static List<int> FindAllIndexes<T>(this List<T> source, Predicate<T> match)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < source.Count; i++)
        {
            if (match(source[i]))
            {
                list.Add(i);
            }
        }

        return list;
    }

    public static void AddUnique<T>(ref List<T> source, T value)
    {
        if (!source.Contains(value))
            source.Add(value);
    }

    public static T MaxWhere<T>(this IEnumerable<T> source, Func<T, long> selector)
    {
        int highestID = -1;
        long highest = long.MinValue;
        for (int i = 0; i < source.Count(); i++)
        {
            var l = selector.Invoke(source.ElementAt(i));
            if (l > highest)
            {
                highestID = i;
                highest = l;
            }
        }
        return source.ElementAt(highestID);
    }
    public static T MaxWhere<T>(this IEnumerable<T> source, Func<T, float> selector)
    {
        int highestID = -1;
        float highest = float.MinValue;
        for (int i = 0; i < source.Count(); i++)
        {
            var l = selector.Invoke(source.ElementAt(i));
            if (l > highest)
            {
                highestID = i;
                highest = l;
            }
        }
        return source.ElementAt(highestID);
    }
    public static T MinWhere<T>(this IEnumerable<T> source, Func<T, long> selector)
    {
        int lowestID = -1;
        long lowest = long.MaxValue;
        for (int i = 0; i < source.Count(); i++)
        {
            var l = selector.Invoke(source.ElementAt(i));
            if (l < lowest)
            {
                lowestID = i;
                lowest = l;
            }
        }
        return source.ElementAt(lowestID);
    }
    public static T MinWhere<T>(this IEnumerable<T> source, Func<T, float> selector)
    {
        int lowestID = -1;
        float lowest = float.MaxValue;
        for (int i = 0; i < source.Count(); i++)
        {
            var l = selector.Invoke(source.ElementAt(i));
            if (l < lowest)
            {
                lowestID = i;
                lowest = l;
            }
        }
        return source.ElementAt(lowestID);
    }

    public static T ClosestWhere<T>(this IEnumerable<T> source, Func<T, long> selector, long target)
    {
        int closestID = -1;
        long closest = long.MaxValue;
        for (int i = 0; i < source.Count(); i++)
        {
            var l = Math.Abs(selector.Invoke(source.ElementAt(i)) - target);
            if (l < closest)
            {
                closestID = i;
                closest = l;
            }
        }
        return source.ElementAt(closestID);
    }
    public static T ClosestWhere<T>(this IEnumerable<T> source, Func<T, float> selector, float target)
    {
        int closestID = -1;
        float closest = float.MaxValue;
        for (int i = 0; i < source.Count(); i++)
        {
            var l = Math.Abs(selector.Invoke(source.ElementAt(i)) - target);
            if (l < closest)
            {
                closestID = i;
                closest = l;
            }
        }
        return source.ElementAt(closestID);
    }
#nullable enable
    public static Nullable<T> ClosestOrNullWhere<T>(this IEnumerable<T> source, Func<T, long> selector, long target) where T : struct
    {
        int closestID = -1;
        long closest = long.MaxValue;
        for (int i = 0; i < source.Count(); i++)
        {
            var l = Math.Abs(selector.Invoke(source.ElementAt(i)) - target);
            if (l < closest)
            {
                closestID = i;
                closest = l;
            }
        }
        if (closestID == -1)
            return null;
        return source.ElementAt(closestID);
    }
    public static Nullable<T> ClosestOrNullWhere<T>(this IEnumerable<T> source, Func<T, float> selector, float target) where T : struct
    {
        int closestID = -1;
        float closest = float.MaxValue;
        for (int i = 0; i < source.Count(); i++)
        {
            var l = Math.Abs(selector.Invoke(source.ElementAt(i)) - target);
            if (l < closest)
            {
                closestID = i;
                closest = l;
            }
        }
        if (closestID == -1)
            return null;
        return source.ElementAt(closestID);
    }
#nullable disable
    public static T Random<T>(this IEnumerable<T> source)
    {
        return source.ElementAt(UnityEngine.Random.Range(0, source.Count()));
    }
}
