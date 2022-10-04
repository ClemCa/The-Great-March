using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class DialogueGenerator : MonoBehaviour
{
    [SerializeField] private FlagDatabase _flagDatabase;
    [SerializeField] private StructureDatabase _structureDatabase;
    [SerializeField] private EvolutiveStory.Character _testCharacter;
    [SerializeField] private float _testOpinion = 0.5f;
    [SerializeField] private Intent _testIntent;
    [SerializeField] private string _verbDataSource;
    [SerializeField] private MindVisualizer _mindVisualizer;
    private Dictionary<string, VerbData> _verbData = new Dictionary<string, VerbData>();

    public EvolutiveStory.Character TestCharacter { get => _testCharacter; set => _testCharacter = value; }

    void Awake()
    {
        using (var reader = new StreamReader(Application.dataPath+"/"+_verbDataSource))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',', StringSplitOptions.None);
                _verbData.Add(values[0], new VerbData(values[0], values[1], values[2], values[3], values[4], values[5]));
            }
        }
        _mindVisualizer.GenerateFrom("test", _testCharacter, 10);
    }

    void Start()
    {
        var fact1 = _testCharacter.Memory.Fact.Memory[0];
        var fact2 = _testCharacter.Memory.Fact.Memory[1];
        Debug.Log(GenerateSentence(_testCharacter, new EvolutiveStory.Fact[] { fact1 }, _testIntent, _testOpinion));
        Debug.Log(GenerateSentence(_testCharacter, new EvolutiveStory.Fact[] { fact2 }, _testIntent, _testOpinion));
        Debug.Log(GenerateSentence(_testCharacter, new EvolutiveStory.Fact[] { fact1, fact2 }, _testIntent, _testOpinion));
    }

    public void GenerateFromField(EvolutiveStory.Character origin, object obj)
    {
        // To note cases where lists are empty are unhandled, will result in an error due to Random.Range(0,0) being invalid.
        switch (obj)
        {
            case EvolutiveStory.NameInfo o:
                Debug.Log(GenerateSentence(_testCharacter, new EvolutiveStory.Fact[] { new EvolutiveStory.Fact() { Name = new EvolutiveStory.NameInfo() { Name = "name" }, Subject = origin.Name.Name, Data = origin.Name.Name, Flag = "Name" } }, Intent.SeriousAnnecdote, 0.5f));
                break;
            case EvolutiveStory.Gender o:
                Debug.Log(GenerateSentence(_testCharacter, new EvolutiveStory.Fact[] { new EvolutiveStory.Fact() { Name = new EvolutiveStory.NameInfo() { Name = "gender" }, Subject = origin.Name.Name, Data = o switch
                {
                    EvolutiveStory.Gender.Male => "male",
                    EvolutiveStory.Gender.Female => "female",
                    EvolutiveStory.Gender.Neutral => "non-binary",
                    EvolutiveStory.Gender.Object => "undefined",
                    _ => "unknown"
                }, Flag = "Gender" } }, Intent.SeriousAnnecdote, 0.5f));
                break;
            case EvolutiveStory.Knowledge o:
                {
                    // Call the same function, but with the topic just a bit more defined
                    var hasOngoingEvents = o.OngoingEvents.Count > 0;
                    var randomField = UnityEngine.Random.Range(0, hasOngoingEvents ? 4 : 3);
                    switch (randomField)
                    {
                        case 0:
                            GenerateFromField(origin, o.Job);
                            break;
                        case 1:
                            GenerateFromField(origin, o.Hobbies.Random());
                            break;
                        case 2:
                            GenerateFromField(origin, o.Role);
                            break;
                        case 3:
                            GenerateFromField(origin, o.OngoingEvents.Random());
                            break;
                        default:
                            Debug.LogError("Something is wrong");
                            break;
                    }
                }
                break;
            case EvolutiveStory.Personality o:
                {
                    // Call the same function, but with the topic just a bit more defined
                    var randomField = UnityEngine.Random.Range(0, 10);
                    // 0 = specific topic loved
                    // 1 < 2 = topics loved (can end up on the favorite topic)
                    // 3 - 6 = random topic
                    // 7 - 8 = topics hated (can end up on the hated topic)
                    // 9 = specific topic hated
                    switch (randomField)
                    {
                        case 0:
                            GenerateFromField(origin, o.PreferedTopic);
                            break;
                        case 1:
                        case 2:
                            GenerateFromField(origin, o.Topics.Where(t => t.LikeRatio > 0.5f).Random());
                            break;
                        case 4:
                        case 5:
                        case 6:
                            GenerateFromField(origin, o.Topics.Random());
                            break;
                        case 7:
                        case 8:
                            GenerateFromField(origin, o.Topics.Where(t => t.LikeRatio < 0.5f).Random());
                            break;
                        case 9:
                            GenerateFromField(origin, o.HatedTopic);
                            break;
                        default:
                            Debug.LogError("Something is wrong");
                            break;
                    }
                }
                break;
            case EvolutiveStory.Memory o:
                {
                    // Call the same function, but with the topic just a bit more defined
                    var hasEvents = o.Event.Memory.Count > 0;
                    var randomField = UnityEngine.Random.Range(0, hasEvents ? 3 : 2);
                    switch (randomField)
                    {
                        case 0:
                            GenerateFromField(origin, o.Fact.Memory.Random());
                            break;
                        case 1:
                            GenerateFromField(origin, o.Relationships.Memory.Random());
                            break;
                        case 2:
                            GenerateFromField(origin, o.Event.Memory.Random());
                            break;
                        default:
                            Debug.LogError("Something is wrong");
                            break;
                    }
                }
                break;
            case EvolutiveStory.Relationships o:
                {
                    // Call the same function, but with the topic just a bit more defined
                    var randomField = UnityEngine.Random.Range(0, 10);
                    // 0 = specific person loved
                    // 1 < 2 = persons loved (can end up on the favorite topic)
                    // 3 - 6 = random person
                    // 7 - 8 = persons hated (can end up on the hated topic)
                    // 9 = specific person hated
                    switch (randomField)
                    {
                        case 0:
                            GenerateFromField(origin, o.Friendliest);
                            break;
                        case 1:
                        case 2:
                            GenerateFromField(origin, o.All.Where(t => t.Opinion.Opinion > 0.5f).Random());
                            break;
                        case 4:
                        case 5:
                        case 6:
                            GenerateFromField(origin, o.All.Random());
                            break;
                        case 7:
                        case 8:
                            GenerateFromField(origin, o.All.Where(t => t.Opinion.Opinion < 0.5f).Random());
                            break;
                        case 9:
                            GenerateFromField(origin, o.Muddiest);
                            break;
                        default:
                            Debug.LogError("Something is wrong");
                            break;
                    }
                }
                break;
            case EvolutiveStory.Present o:
                {
                    // Call the same function, but with the topic just a bit more defined
                    var hasActions = o.Actions.Count > 0;
                    var hasTroubles = o.Troubles.Count > 0;
                    var hasWeaknesses = o.Weaknesses.Count > 0;
                    var count = new[] { hasActions, hasTroubles, hasWeaknesses }.Count(t => t);
                    if (count == 0)
                    {
                        Debug.LogError("Unhandled for now: nothing to say on present");
                        return;
                    }
                    var randomField = UnityEngine.Random.Range(0, count);
                    switch (randomField)
                    {
                        case 0:
                            if (hasActions)
                                GenerateFromField(origin, o.Actions.Random());
                            else if (hasTroubles)
                                GenerateFromField(origin, o.Troubles.Random());
                            else
                                GenerateFromField(origin, o.Weaknesses.Random());
                            break;
                        case 1:
                            if (hasActions)
                            {
                                if(hasTroubles)
                                    GenerateFromField(origin, o.Troubles);
                                else
                                    GenerateFromField(origin, o.Weaknesses);
                            }
                            else
                                GenerateFromField(origin, o.Weaknesses);
                            break;
                        case 2:
                            GenerateFromField(origin, o.Weaknesses);
                            break;
                        default:
                            Debug.LogError("Something is wrong");
                            break;
                    }
                }
                break;
            case EvolutiveStory.Job:
            case EvolutiveStory.Hobby:
            case EvolutiveStory.StoryRole:
            case EvolutiveStory.Event:
            case EvolutiveStory.Topic:
            case EvolutiveStory.Fact:
            case EvolutiveStory.Relationship:
            case EvolutiveStory.PastRelationship:
            case EvolutiveStory.PresentFact:
            case EvolutiveStory.PresentExpiringFact:
                Debug.LogError("left to do");
                break;
            default:
                Debug.LogError("Field can't be generated from");
                break;
        }
    }

    private VerbInfo ChooseVerb(string flag, EvolutiveStory.Conjugation conjugation)
    {
        int id = (int)conjugation;
        EvolutiveStory.ConjugationMask conj = (EvolutiveStory.ConjugationMask)Enum.GetValues(typeof(EvolutiveStory.ConjugationMask)).GetValue(id);
        var choices = _flagDatabase.Flags.Find(t => t.Flag == flag).Verbs;
        choices = choices.Where(t => (t.ValidConjugations & conj) != 0).OrderBy(t => UnityEngine.Random.Range(0, 1f)).ToList();
        return choices[0];
    }

    private string GetConjugation(VerbInfo verb, EvolutiveStory.Conjugation conjugation, int number, int gender)
    {
        var v = verb.Verb;
        v = conjugation switch
        {
            EvolutiveStory.Conjugation.Present => v == "be" ? "is" : v, // literally the only special case
            EvolutiveStory.Conjugation.Present3 => _verbData[v].Singular3,
            EvolutiveStory.Conjugation.PresentParticiple => "is " + _verbData[v].PresentParticiple,
            EvolutiveStory.Conjugation.SimplePast => _verbData[v].SimplePast,
            EvolutiveStory.Conjugation.SimplePastPlural => _verbData[v].SimplePastPlural,
            EvolutiveStory.Conjugation.PastParticiple => "was "+_verbData[v].PastParticiple,
            EvolutiveStory.Conjugation.PassivePresent => "am "+_verbData[v].PastParticiple,
            EvolutiveStory.Conjugation.PassivePresent3 => "is "+_verbData[v].PastParticiple,
            _ => v,
        };
        v = string.Join(" ", verb.Prefix.Trim(), v, verb.Suffix.Trim()).Trim();
        v = v.Replace("SJ", number switch
        {
            0 => "me",
            1 => gender switch
            {
                0 => "it",
                1 => "him",
                2 => "her",
                _ => "them"
            },
            _ => "them"
        });
        return v;
    }

    public struct VerbData
    {
        public string Base;
        private string Singular;
        private string PresentParticip;
        private string Simple;
        private string SimplePlural;
        private string PastParticip;

        public VerbData(string baseVerb, string singular3, string presentParticiple, string simplePast, string simplePlural, string pastParticiple)
        {
            Base = baseVerb;
            Singular = singular3;
            PresentParticip = presentParticiple;
            Simple = simplePast;
            SimplePlural = simplePlural;
            PastParticip = pastParticiple;
        }

        public string Singular3
        {
            get
            {
                if (Singular == "" || Singular == "-")
                    return Base;
                return Singular;
            }
        }
        public string PresentParticiple
        {
            get
            {
                if (PresentParticip == "" || PresentParticip == "-")
                    return Singular3;
                return PresentParticip;
            }
        }
        public string SimplePast
        {
            get
            {
                if (Simple == "" || Simple == "-")
                    return Base;
                return Simple;
            }
        }
        public string SimplePastPlural
        {
            get
            {
                if (SimplePlural == "" || SimplePlural == "-")
                    return SimplePast;
                return SimplePlural;
            }
        }
        public string PastParticiple
        {
            get
            {
                if (PastParticip == "" || PastParticip == "-")
                    return SimplePast;
                return PastParticip;
            }
        }
    }

    // MAKE DIALOGUE
        // STRUCTURE
            // WHERE TO PLACE SUBJECT
            // WHERE TO PLACE DIRECT OBJECT COMPLEMENT
        // DATA
            // GET NAME
            // GET STRUCTURE TO USE


    public enum Intent
    {
        AngryProtest,
        SadDenial,
        FactualDenial,
        Sad,
        Explanation,
        LightAnnecdote,
        SeriousAnnecdote,
        HighSocietyAnnecdote,
        SurprisingAnnecdote
    }


    private EvolutiveStory.Character GetCharacterByName(string name)
    {
        // FOR TEST PURPOSES RN
        return new EvolutiveStory.Character { Gender = EvolutiveStory.Gender.Female };
    }

    private string BuildSubject(EvolutiveStory.Fact fact, EvolutiveStory.Character character, float tone, bool isGivenCharacterSelf, int mentionID = 0, int maxMention = 0)
    {
        var relationship = character.Relationships.GetByName(fact.Subject);
        float relationshipScore = relationship.Opinion.Opinion;
        var owner = relationship.Name.GetOpiniatedName(relationshipScore).RandomAltName;
        var target = fact.Name.GetOpiniatedName(tone).RandomAltName;
        var targetChar = GetCharacterByName(fact.Subject);
        string toUse;
        if (isGivenCharacterSelf && fact.Subject == character.Name.Name)
            return "my " + target;
        if(mentionID == 0)
        {
            toUse = (isGivenCharacterSelf ? "my " : character.Name + "'s ") + owner;
        } else if (mentionID < maxMention)
        {
            toUse = targetChar.Gender switch
            {
                EvolutiveStory.Gender.Object => ", its",
                EvolutiveStory.Gender.Neutral => ", their",
                EvolutiveStory.Gender.Male => ", his",
                EvolutiveStory.Gender.Female => ", her",
                _ => ", its"
            };
        }
        else if (mentionID == 1) // and superior or equal to max mention
        {
            toUse = targetChar.Gender switch
            {
                EvolutiveStory.Gender.Object => " and its",
                EvolutiveStory.Gender.Neutral => " and their",
                EvolutiveStory.Gender.Male => " and his",
                EvolutiveStory.Gender.Female => " and her",
                _ => " and its"
            };
        } else
        {
            toUse = targetChar.Gender switch
            {
                EvolutiveStory.Gender.Object => ", and its",
                EvolutiveStory.Gender.Neutral => ", and their",
                EvolutiveStory.Gender.Male => ", and his",
                EvolutiveStory.Gender.Female => ", and her",
                _ => ", and its"
            };
        }
        return toUse + "'s "+target;
    }

    public string GenerateSentence(EvolutiveStory.Character self, EvolutiveStory.Fact[] facts, Intent intent, float tone)
    {
        var subjects = facts.Select(t => t.Subject).Distinct().ToArray();
        var factsC = facts.Count();
        var contextual = facts.Select(t => t.Context).Where(t => t.hasDate || t.hasPlace).ToArray();
        if (contextual.Count() != factsC)
            contextual = new EvolutiveStory.Context[0]; // we don't want contextual data to be associated with the wrong fact
        var contexts = contextual.SelectMany(t =>
           (t.hasDate && t.hasPlace) ?
               new EvolutiveStory.NameInfo[] { t.Date, t.Place }
               : t.hasDate ? new EvolutiveStory.NameInfo[] { t.Date } :
               t.hasPlace ? new EvolutiveStory.NameInfo[] { t.Place } :
               new EvolutiveStory.NameInfo[0]
        ).Distinct().ToArray();
        // if there is no problem with the number of contextual data, we can eliminate redundancies
        // (the context is all about the same data)
        var structure = FetchStructure(subjects.Count(), contexts.Count(), intent, tone);
        return BuildSentence(self, structure, subjects, facts, contexts, tone);
    }

    public string BuildSentence(EvolutiveStory.Character self, string structure, string[] subjects, EvolutiveStory.Fact[] facts, EvolutiveStory.NameInfo[] contexts, float opinion)
    {
        var regex = new Regex("VBH");
        List<int> subjectMaximums = new List<int>(); // how many times do subjects repeat, it's inefficient but it's fine, we don't run this often
        string subject = facts[0].Subject;
        int last = 0;
        for (int i = 1; i < facts.Length; i++)
        {
            if(facts[i].Subject != subject)
            {
                subject = facts[i].Subject;
                subjectMaximums.Add(i - last);
                last = i;
                continue;
            }
            if (i == facts.Length - 1)
            {
                subjectMaximums.Add(i - last);
            }
        }
        if (facts.Length == 1)
            subjectMaximums.Add(1);
        last = 0;
        int id = -1;
        bool doRegex = false;
        string res = "";
        for (int i = 0; i < facts.Length; i++)
        {
            var fact = facts[i];
            var subjectID = Array.IndexOf(subjects, fact.Subject);
            if (subjectID != id)
            {
                id = subjectID;
                last = 0;
                if(res != "")
                {
                    structure = regex.Replace(structure, res, 1);
                    res = "";
                }
            }
            else
                last++;
            var content = BuildSubject(fact, self, opinion, true, last, subjectMaximums[id]);
            var verb = GetConjugation(ChooseVerb(fact.Flag, EvolutiveStory.Conjugation.Present), EvolutiveStory.Conjugation.Present, 0, 0); // NUMBER => number of subject (0 if self), GENDER => gender of subject
            content += " "+verb;
            content += " "+fact.Data;
            res += content;
        }
        structure = regex.Replace(structure, res, 1);
        for (int i = 0; i < contexts.Length; i++)
        {
            // we don't care for the subject here because context only exists if it matches the number of facts in the first place
            // we don't care about which subject is which
            var context = contexts[i];
            structure = structure.Replace("Contextual"+i, context.GetOpiniatedName(opinion).RandomAltName);
        }
        var s = new StringBuilder(structure);
        s[0] = char.ToUpper(structure[0]);
        // capitalize the first letter
        return s.ToString();
    }

    public string FetchStructure(int subjectCount, int contextualDataCount, Intent intent, float tone)
    {
        var all = _structureDatabase.GetAllMatchingStructures(subjectCount, contextualDataCount, intent, tone);
        if (all.Length > 0)
            return all.Random();
        all = _structureDatabase.GetAllMatchingStructureNoContext(subjectCount, intent, tone);
        if (all.Length > 0)
            return all.Random();
        var r = _structureDatabase.GetClosestMatchingStructure(subjectCount, contextualDataCount, intent, tone);
        if (r != null)
            return r;
        return _structureDatabase.GetClosestMatchingStructureNoContext(subjectCount, intent, tone);

        // return "You wouldn't believe it, Fact0 Data0 Contextual0";
    }

    [Serializable]
    public struct FlagDatabase
    {
        public List<FlagInfo> Flags;
    }

    [Serializable]
    public struct FlagInfo
    {
        public string Flag;
        public List<VerbInfo> Verbs;
    }

    [Serializable]
    public struct VerbInfo
    {
        public string Verb;
        public string Prefix; // ex: have to X
        public string Suffix; // ex: X to be
        [EnumMask] public EvolutiveStory.ConjugationMask ValidConjugations;
    }

    [Serializable]
    public struct StructureDatabase
    {
        public List<Structure> Structures;
        public string[] GetAllMatchingStructures(int subjects, int contextualData, Intent intent, float tone)
        {
            return Structures.Where(t => t.Matches(subjects, contextualData, intent, tone)).Select(t => t.Sentence).ToArray();
        }
        public string[] GetAllMatchingStructureNoContext(int subjects, Intent intent, float tone)
        {
            return Structures.Where(t => t.Matches(subjects, intent, tone)).Select(t => t.Sentence).ToArray();
        }
        public string GetClosestMatchingStructure(int subjects, int contextualData, Intent intent, float tone)
        {
            var c = Structures.Where(t => t.Matches(subjects, contextualData, intent)).ClosestOrNullWhere(t => t.Tone, tone);
            return c.HasValue ? c.Value.Sentence : null;
        }
        public string GetClosestMatchingStructureNoContext(int subjects, Intent intent, float tone)
        {
            return Structures.Where(t => t.Matches(subjects, intent)).ClosestWhere(t => t.Tone, tone).Sentence;
        }
    }

    [Serializable]
    public struct Structure
    {
        public int Subjects;
        public int ContextualData;
        public Intent Intent;
        public float Tone;
        public string Sentence;

        public bool Matches(int subjects, int contextualData, Intent intent, float tone)
        {
            return subjects == Subjects && contextualData == ContextualData && intent == Intent && tone == Tone;
        }
        public bool Matches(int subjects, Intent intent, float tone)
        {
            return subjects == Subjects && tone == Tone && intent == Intent;
        }
        public bool Matches(int subjects, int contextualData, Intent intent)
        {
            return subjects == Subjects && contextualData == ContextualData && intent == Intent;
        }
        public bool Matches(int subjects, Intent intent)
        {
            return subjects == Subjects && intent == Intent;
        }
    }
}
