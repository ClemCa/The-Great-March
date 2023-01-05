using ClemCAddons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using static DialogueGenerator;
using static EvolutiveStory;

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

    }

    public string GenerateFromField(EvolutiveStory.Character origin, object obj)
    {
        // To note cases where lists are empty are unhandled, will result in an error due to Random.Range(0,0) being invalid.
        switch (obj)
        {
            case EvolutiveStory.NameInfo o:
                return GenerateSentenceFacts(_testCharacter, new EvolutiveStory.Fact[] { new EvolutiveStory.Fact() { Name = new EvolutiveStory.NameInfo() { Name = "name" }, Subject = origin.Name.Name, Data = origin.Name.Name, Flag = "Name" } }, Intent.SeriousAnnecdote, 0.5f);
            case EvolutiveStory.Gender o:
                return GenerateSentenceFacts(_testCharacter, new EvolutiveStory.Fact[] { new EvolutiveStory.Fact() { Name = new EvolutiveStory.NameInfo() { Name = "gender" }, Subject = origin.Name.Name, Data = o switch
                {
                    EvolutiveStory.Gender.Male => "male",
                    EvolutiveStory.Gender.Female => "female",
                    EvolutiveStory.Gender.Neutral => "non-binary",
                    EvolutiveStory.Gender.Object => "undefined",
                    _ => "unknown"
                }, Flag = "Gender" } }, Intent.SeriousAnnecdote, 0.5f);
            case EvolutiveStory.Knowledge o:
                {
                    // Call the same function, but with the topic just a bit more defined
                    var hasOngoingEvents = o.OngoingEvents.Count > 0;
                    var randomField = UnityEngine.Random.Range(0, hasOngoingEvents ? 4 : 3);
                    switch (randomField)
                    {
                        case 0:
                            return GenerateFromField(origin, o.Job);
                        case 1:
                            return GenerateFromField(origin, o.Hobbies.Random());
                        case 2:
                            return GenerateFromField(origin, o.Role);
                        case 3:
                            return GenerateFromField(origin, o.OngoingEvents.Random());
                        default:
                            Debug.LogError("Something is wrong");
                            return "";
                    }
                }
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
                            return GenerateFromField(origin, o.PreferedTopic);
                        case 1:
                        case 2:
                            return GenerateFromField(origin, o.Topics.Where(t => t.LikeRatio > 0.5f).Random());
                        case 4:
                        case 5:
                        case 6:
                            return GenerateFromField(origin, o.Topics.Random());
                        case 7:
                        case 8:
                            return GenerateFromField(origin, o.Topics.Where(t => t.LikeRatio < 0.5f).Random());
                        case 9:
                            return GenerateFromField(origin, o.HatedTopic);
                        default:
                            Debug.LogError("Something is wrong");
                            return "";
                    }
                }
            case EvolutiveStory.Memory o:
                {
                    // Call the same function, but with the topic just a bit more defined
                    var hasEvents = o.Event.Memory.Count > 0;
                    var randomField = UnityEngine.Random.Range(0, hasEvents ? 3 : 2);
                    switch (randomField)
                    {
                        case 0:
                            return GenerateFromField(origin, o.Fact.Memory.Random());
                        case 1:
                            return GenerateFromField(origin, o.Relationships.Memory.Random());
                        case 2:
                            return GenerateFromField(origin, o.Event.Memory.Random());
                        default:
                            Debug.LogError("Something is wrong");
                            return "";
                    }
                }
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
                            return GenerateFromField(origin, o.Friendliest);
                        case 1:
                        case 2:
                            return GenerateFromField(origin, o.All.Where(t => t.Opinion.Opinion > 0.5f).Random());
                        case 4:
                        case 5:
                        case 6:
                            return GenerateFromField(origin, o.All.Random());
                        case 7:
                        case 8:
                            return GenerateFromField(origin, o.All.Where(t => t.Opinion.Opinion < 0.5f).Random());
                        case 9:
                            return GenerateFromField(origin, o.Muddiest);
                        default:
                            Debug.LogError("Something is wrong");
                            return "";
                    }
                }
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
                        return "";
                    }
                    var randomField = UnityEngine.Random.Range(0, count);
                    switch (randomField)
                    {
                        case 0:
                            if (hasActions)
                                return GenerateFromField(origin, o.Actions.Random());
                            else if (hasTroubles)
                                return GenerateFromField(origin, o.Troubles.Random());
                            else
                                return GenerateFromField(origin, o.Weaknesses.Random());
                        case 1:
                            if (hasActions)
                            {
                                if(hasTroubles)
                                    return GenerateFromField(origin, o.Troubles);
                                else
                                    return GenerateFromField(origin, o.Weaknesses);
                            }
                            else
                                return GenerateFromField(origin, o.Weaknesses);
                        case 2:
                            return GenerateFromField(origin, o.Weaknesses);
                        default:
                            Debug.LogError("Something is wrong");
                            return "";
                    }
                }
            case EvolutiveStory.OpinionInfo: // No origin, can only be self
                {
                    return GenerateSentenceOpinion(_testCharacter, _testCharacter.Name.Name, _testCharacter.Name, (OpinionInfo)obj);
                }
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
                return "";
            default:
                Debug.LogError("Field can't be generated from");
                return "";
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
            EvolutiveStory.Conjugation.Present => v == "be" ? (number == 0 ? "am" : "is") : v, // literally the only special case
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

    [Serializable]
    public struct ExpressableDataIntermediary
    {
        public ExpressableDataPiece Piece;
        public bool MentionSubject;
        public int PropertyID;
        public int SubjectID;
        public int MaxProperty;
        public int MaxSubject;
    }

    [Serializable]
    public struct ExpressableData
    {
        public List<ExpressableDataPiece> Pieces;

        // I know this one is really bad, but it only runs once, even if it is O(n^4) and attributes memory it doesn't matter
        public ExpressableData(List<ExpressableDataPiece> pieces)
        {
            // EXTRACT PIECES TO GET SIMILAR PIECES OF DATA TOGETHER, SAME OVERALL ORDER
            // ELIMINATE SIMILAR PIECES OF DATA
            // PRIORITY:
            // 1) PUT DATA WITH THE SAME SUBJECT TOGETHER
            // 2) PUT DATA WITH THE SAME PROPERTY TOGETHER
            // 3) PUT DATA WITH THE SAME DATA TYPE TOGETHER
            //
            var dp = new List<ExpressableDataPiece>();
            string subject = "";
            while(pieces.Count > 0) // Same subject together, by order of first appearance
            {
                var currentSubject = pieces[0].Subject;
                // doing the check even though it is always true, to keep in mind the assumption that is made
                // when I need to reread it
                if (currentSubject != subject)
                {
                    subject = currentSubject;
                    var indexes = pieces.FindAllIndexes(t => t.Subject == subject);
                    var tmp = new List<ExpressableDataPiece>();
                    indexes.ForEach(i => tmp.Add(pieces[i]));
                    for (int i = indexes.Count - 1; i >= 0; i--)
                    {
                        pieces.RemoveAt(indexes[i]);
                    }
                    string property = "";
                    while (tmp.Count > 0)
                    {
                        var currentProperty = tmp[0].Property;
                        if (property != currentProperty)
                        {
                            var tmp2 = new List<ExpressableDataPiece>();
                            property = currentProperty;
                            indexes = tmp.FindAllIndexes(t => t.Property == currentProperty);
                            indexes.ForEach(i => tmp2.Add(tmp[i]));
                            for (int i = indexes.Count - 1; i >= 0; i--)
                            {
                                tmp.RemoveAt(indexes[i]);
                            }
                            EvolutiveStory.DataType dataType = (EvolutiveStory.DataType)(-1);
                            while (tmp2.Count > 0)
                            {
                                var currentDataType = tmp2[0].DataType;
                                if (dataType != currentDataType)
                                {
                                    dataType = currentDataType;
                                    indexes = tmp2.FindAllIndexes(t => t.DataType == currentDataType);
                                    indexes.ForEach(i => dp.Add(tmp2[i]));
                                    for (int i = indexes.Count - 1; i >= 0; i--)
                                    {
                                        tmp2.RemoveAt(indexes[i]);
                                    }
                                }
                                else
                                {
                                    throw new Exception("Something has gone terribly wrong");
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Something has gone terribly wrong");
                        }
                    }
                }
                else
                {
                    throw new Exception("Something has gone terribly wrong");
                }
            }
            Pieces = dp;
        }
    }

    [Serializable]
    public struct ExpressableDataPiece
    {
        public string Owner;
        public string Subject;
        public string Property;
        public string Value;
        public EvolutiveStory.DataType DataType;
    }


    private EvolutiveStory.Character GetCharacterByName(string name)
    {
        // FOR TEST PURPOSES RN
        return new EvolutiveStory.Character { Gender = EvolutiveStory.Gender.Female };
    }

    private EvolutiveStory.Character GetSelf()
    {
        // FOR TEST PURPOSES RN
        return _testCharacter;
    }

    private bool IsCharacter(string name)
    {
        return true;
    }

    private string BuildSubject(EvolutiveStory.Character character, string subject, string target, string opiniatedName, float tone, bool isGivenCharacterSelf, int mentionID = 0, int maxMention = 0)
    {
        if (character.Name.Name == target)
            return "I";
        var relationship = character.Relationships.GetByName(subject);
        float relationshipScore = relationship.Opinion.Opinion;
        var owner = relationship.Name.GetOpiniatedName(relationshipScore).RandomAltName;
        var targetChar = GetCharacterByName(subject);
        string toUse;
        if (isGivenCharacterSelf && subject == character.Name.Name)
            return "my " + opiniatedName;
        if (mentionID == 0)
        {
            toUse = (isGivenCharacterSelf ? "my " : character.Name + "'s ") + owner;
        }
        else if (mentionID < maxMention)
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
        }
        else
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
        return toUse + "'s " + opiniatedName;
    }

    public string GenerateSentence(EvolutiveStory.Character self, ExpressableData data)
    {
        EvolutiveStory.DataType dataType = (EvolutiveStory.DataType)(-1);
        List<string> subjects = new List<string>();
        List<string> properties = new List<string>();
        ExpressableDataIntermediary[] pieceSave = data.Pieces.Select(t => new ExpressableDataIntermediary() { Piece = t, MentionSubject = true, PropertyID = 0 }).ToArray();
        List<int> sentences = new List<int>();
        int count = 0;

        // Split and process our data
        while (data.Pieces.Count > 0)
        {
            var piece = data.Pieces[0];
            if (dataType != piece.DataType)
            {
                // RESET
                subjects.Clear();
                properties.Clear();
                dataType = piece.DataType;
                sentences.Add(count);
            }
            GreatMarchOverloads.AddUnique(ref subjects, piece.Subject);
            GreatMarchOverloads.AddUnique(ref properties, piece.Property);
            // We want:
            // Unlimited: same subject & same property
            // Max 3: same subject different property
            // Max 3: different subjects same property

            // Rules:
            // 1) Can't have both several subjects and several properties
            // 2) Can't have over 3 subjects
            // 3) Can't have over 3 properties

            if ((subjects.Count > 1 && properties.Count > 1) || subjects.Count > 3 || properties.Count > 3)
            {
                // RESET
                subjects.Clear();
                properties.Clear();
                dataType = piece.DataType;
                subjects.Add(piece.Subject);
                properties.Add(piece.Property);
                sentences.Add(count);
            }

            if (properties.Count > 1)
                pieceSave[count].MentionSubject = false; // Would be weird to repeat the subject instead of using a pronoun

            // This will give us info on what needs to be mentioned and formatted in what way, and save on future logic
            pieceSave[count].PropertyID = properties.Count - 1;
            pieceSave[count].SubjectID = subjects.Count - 1;

            count++;
        }

        // Build each sentence individually
        for (int i = 0; i < sentences.Count; i++)
        {
            var sentencePieces = pieceSave.ToList().GetRange(sentences[i], (i < sentences.Count - 1 ? sentences[i + 1] : sentences[sentences.Count - 1]) - i);
            var pieces = new List<string>();
            // Build each part of the sentence
            foreach(var piece in sentencePieces)
            {
                pieces.Add(GenerateFromExpressableData(piece));
            }
        }
        return "";
    }

    private string GenerateFromExpressableData(ExpressableDataIntermediary expressableData)
    {
        string result = "";
        //  public ExpressableDataPiece Piece;
        //  public bool MentionSubject;
        //  public int PropertyID;
        //  public int SubjectID;
        if (expressableData.MentionSubject)
        {
            result = BuildSubject(expressableData);
        }

        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // STOPPED HERE
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************
        // *******************************************************************************************************************

        return result;
    }

    private string BuildSubject(ExpressableDataIntermediary data)
    {
        // data type doesn't matter for subjects
        EvolutiveStory.Character self = GetSelf();
        bool isOwner = data.Piece.Owner == self.Name.Name;
        if (isOwner)
        {
            bool isSelf = data.Piece.Subject == data.Piece.Owner;
            if (isSelf) // talking about yourself directly
            {
                if (!data.MentionSubject)
                {
                    return "and";
                }
                if (self.SelfWorth.Opinion > 0.95)
                    return "yours truly";
                else if (self.SelfWorth.Opinion > 0.8)
                    return "we";
                else if (self.SelfWorth.Opinion > 0.05)
                    return "I";
                else
                    return "this pitiful self";
            } else if (IsCharacter(data.Piece.Subject)) // talking about another NPC
            {
                var character = GetCharacterByName(data.Piece.Subject);
                string toUse;
                var gender = character.Gender;
                var relationship = self.Relationships.GetByName(data.Piece.Subject);
                var name = data.MaxSubject == 1 ? relationship.Name.GetOpiniatedName(relationship.Opinion.Opinion).RandomAltName : character.Name.Name;
                if (data.MaxProperty > 1) // We're going to list properties
                {
                    if (data.PropertyID == 0)
                    {
                        toUse = name + "'s";
                    }
                    else if (data.PropertyID < data.MaxProperty)
                    {
                        toUse = gender switch
                        {
                            EvolutiveStory.Gender.Object => ", its",
                            EvolutiveStory.Gender.Neutral => ", their",
                            EvolutiveStory.Gender.Male => ", his",
                            EvolutiveStory.Gender.Female => ", her",
                            _ => ", its"
                        };
                    }
                    else if (data.PropertyID == 1) // and superior or equal to max property
                    {
                        toUse = gender switch
                        {
                            EvolutiveStory.Gender.Object => " and its",
                            EvolutiveStory.Gender.Neutral => " and their",
                            EvolutiveStory.Gender.Male => " and his",
                            EvolutiveStory.Gender.Female => " and her",
                            _ => " and its"
                        };
                    }
                    else // simply superior or equal to max property
                    {
                        toUse = gender switch
                        {
                            EvolutiveStory.Gender.Object => ", and its",
                            EvolutiveStory.Gender.Neutral => ", and their",
                            EvolutiveStory.Gender.Male => ", and his",
                            EvolutiveStory.Gender.Female => ", and her",
                            _ => ", and its"
                        };
                    }
                    return toUse + " " + data.Piece.Property;
                }
                else // we're going to list subjects
                {
                    if (data.SubjectID == 0)
                    {
                        toUse = name;
                    }
                    else if (data.SubjectID < data.MaxSubject)
                    {
                        toUse = ", " + name;
                    }
                    else if (data.PropertyID == 1) // and superior or equal to max property
                    {
                        toUse = " and " + name;
                    }
                    else // simply superior or equal to max property
                    {
                        toUse = " and " + name + "'s " + ToPlural(data.Piece.Property);
                    }
                    return toUse;
                }
            } else // talking about something or someone related to you, called by their relationship to you (ex: my x)
            {
                var isCharacter = self.Relationships.HasRelationship(data.Piece.Subject);
                string toUse;
                string name;
                EvolutiveStory.Gender gender;
                if (isCharacter)
                {
                    var character = GetCharacterByName(data.Piece.Subject);
                    gender = character.Gender;
                    var relationship = self.Relationships.GetByName(data.Piece.Subject);
                    name = data.MaxSubject == 1 ? relationship.Name.GetOpiniatedName(relationship.Opinion.Opinion).RandomAltName : character.Name.Name;
                }
                else
                {
                    gender = EvolutiveStory.Gender.Object;
                    name = data.Piece.Subject;
                }
                if (data.PropertyID == 0)
                {
                    toUse = "my " + name + "'s";
                }
                else if (data.PropertyID < data.MaxProperty)
                {
                    toUse = gender switch
                    {
                        EvolutiveStory.Gender.Object => ", its",
                        EvolutiveStory.Gender.Neutral => ", their",
                        EvolutiveStory.Gender.Male => ", his",
                        EvolutiveStory.Gender.Female => ", her",
                        _ => ", its"
                    };
                }
                else if (data.PropertyID == 1) // and superior or equal to max property
                {
                    toUse = gender switch
                    {
                        EvolutiveStory.Gender.Object => " and its",
                        EvolutiveStory.Gender.Neutral => " and their",
                        EvolutiveStory.Gender.Male => " and his",
                        EvolutiveStory.Gender.Female => " and her",
                        _ => " and its"
                    };
                }
                else // simply superior or equal to max property
                {
                    toUse = gender switch
                    {
                        EvolutiveStory.Gender.Object => ", and its",
                        EvolutiveStory.Gender.Neutral => ", and their",
                        EvolutiveStory.Gender.Male => ", and his",
                        EvolutiveStory.Gender.Female => ", and her",
                        _ => ", and its"
                    };
                }
                return toUse + " " + data.Piece.Property;
            }
        } else // is not owner, there is a layer of abstraction between me and the subject
        {      // we don't check for self, any mention of self would be a mistake
            var isCharacter = IsCharacter(data.Piece.Subject);
            string toUse;
            string name;
            EvolutiveStory.Gender gender;
            if (isCharacter)
            {
                var character = GetCharacterByName(data.Piece.Subject);
                gender = character.Gender;
                name = character.Name.Name;
            }
            else
            {
                gender = EvolutiveStory.Gender.Object;
                name = data.Piece.Subject;
            }
            if (data.MaxProperty > 1) // We're going to list properties
            {
                if (data.PropertyID == 0)
                {
                    toUse = name + "'s";
                }
                else if (data.PropertyID < data.MaxProperty)
                {
                    toUse = gender switch
                    {
                        EvolutiveStory.Gender.Object => ", its",
                        EvolutiveStory.Gender.Neutral => ", their",
                        EvolutiveStory.Gender.Male => ", his",
                        EvolutiveStory.Gender.Female => ", her",
                        _ => ", its"
                    };
                }
                else if (data.PropertyID == 1) // and superior or equal to max property
                {
                    toUse = gender switch
                    {
                        EvolutiveStory.Gender.Object => " and its",
                        EvolutiveStory.Gender.Neutral => " and their",
                        EvolutiveStory.Gender.Male => " and his",
                        EvolutiveStory.Gender.Female => " and her",
                        _ => " and its"
                    };
                }
                else // simply superior or equal to max property
                {
                    toUse = gender switch
                    {
                        EvolutiveStory.Gender.Object => ", and its",
                        EvolutiveStory.Gender.Neutral => ", and their",
                        EvolutiveStory.Gender.Male => ", and his",
                        EvolutiveStory.Gender.Female => ", and her",
                        _ => ", and its"
                    };
                }
                return toUse + " " + data.Piece.Property;
            }
            else // we're going to list subjects
            {
                if (data.SubjectID == 0)
                {
                    toUse = name;
                }
                else if (data.SubjectID < data.MaxSubject)
                {
                    toUse = ", " + name;
                }
                else if (data.PropertyID == 1) // and superior or equal to max property
                {
                    toUse = " and " + name;
                }
                else // simply superior or equal to max property
                {
                    toUse = " and " + name + "'s " + ToPlural(data.Piece.Property);
                }
                return toUse;
            }
        }
    }

    private string ToPlural(string singular)
    {
        if (singular.Last() == 's')
            return singular;
        return string.Concat(singular.SkipLast(1)) + 's';
    }

    private string GenerateSentenceOpinion(EvolutiveStory.Character self, string subject, NameInfo nameInfo, EvolutiveStory.OpinionInfo opinion)
    {
        var structure = FetchStructure(1, 0, Intent.SeriousAnnecdote, 0.5f);
        string change = "";
        string changeEnd = "";
        if(Mathf.Abs(opinion.Trend) > 0.1)
        {
            change = "lately, ";
        } else if (Mathf.Abs(opinion.Trend) > 0.25)
        {
            change = "recently, ";
        }
        string flag;
        if (opinion.Opinion <= 0.25f)
        {
            flag = "Hate";
        }
        else if (opinion.Opinion <= 0.4f)
        {
            flag = "Dislike";
        }
        else if (opinion.Opinion <= 0.6f)
        {
            Debug.LogError("add a way to have more variety to that");
            return "I don't know how I feel about " + nameInfo.Name;
        }
        else if (opinion.Opinion <= 0.75f)
        {
            flag = "Like";
        }
        else
        {
            flag = "Love";
        }
        SubjectData subjectData;
        if (_testCharacter.Name.Name == nameInfo.Name)
        {
            subjectData = new SubjectData(subject, nameInfo, new string[] { "myself" }, new string[] { flag });
        }
        else
        {
            subjectData = new SubjectData(subject, nameInfo, new string[] { nameInfo.Name }, new string[] { flag });
        }
        return BuildSentence(self, change+structure+changeEnd, new SentenceData(new SubjectData[] { subjectData }, new NameInfo[0], Conjugation.Present, 0));
    }

    private string GenerateSentenceFacts(EvolutiveStory.Character self, EvolutiveStory.Fact[] facts, Intent intent, float tone)
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

        var subjectData = new SubjectData[subjects.Length];
        for (int i = 0; i < subjects.Length; i++)
        {
            var concernedFacts = facts.Where(t => t.Subject == subjects[i]);
            subjectData[i] = new SubjectData(subjects[i], facts.First(t => t.Subject == subjects[i]).Name, concernedFacts.Select(t => t.Data).ToArray(), concernedFacts.Select(t => t.Flag).ToArray());
        }

        return BuildSentence(self, structure, new SentenceData(subjectData, contexts, Conjugation.Present, tone));
    }

    public struct SentenceData
    {
        public SubjectData[] SubjectData;
        public EvolutiveStory.NameInfo[] ContextData;
        public Conjugation TargetTense;
        public float Tone;

        public SentenceData(SubjectData[] subjectData, NameInfo[] contextData, Conjugation targetTense, float tone)
        {
            SubjectData = subjectData;
            ContextData = contextData;
            TargetTense = targetTense;
            Tone = tone;

            Debug.LogError("Missing:\n" +
                "Limits on alternative names that can be used\n" +
                "ARE / HAVE / OWN / DO distinction");
        }
    }

    public struct SubjectData
    {
        public string Subject;
        public int Number;
        public int Gender;
        public NameInfo Name;
        public string[] Data;
        public string[] Flags;

        public SubjectData(string subject, NameInfo name, string[] data, string[] flags, int number = 1, int gender = 0)
        {
            Subject = subject;
            Name = name;
            Data = data;
            Flags = flags;
            Number = number;
            Gender = gender;
        }
    }

    public string BuildSentence(EvolutiveStory.Character self, string structure, SentenceData sentenceData)
    {
        var regex = new Regex("VBH");
        string res = "";
        for (int i = 0; i < sentenceData.SubjectData.Length; i++)
        {
            var subject = sentenceData.SubjectData[i];
            int number;
            if(subject.Name.Name == self.Name.Name)
            {
                number = 0;
            }
            else
            {
                number = 1;
                Debug.LogWarning("Right now, singular and plural are both singular");
            }
            for (int d = 0; d < subject.Data.Length; d++)
            {
                var content = BuildSubject(self, subject.Subject, subject.Name.Name, subject.Name.GetOpiniatedName(sentenceData.Tone).RandomAltName, sentenceData.Tone, true, d, subject.Data.Length);
                var verb = GetConjugation(ChooseVerb(subject.Flags[d], EvolutiveStory.Conjugation.Present), EvolutiveStory.Conjugation.Present, number, 0); // NUMBER => number of subject (0 if self), GENDER => gender of subject
                content += " " + verb;
                content += " " + subject.Data[d];
                res += content;
            }
            structure = regex.Replace(structure, res, 1);

        }
        structure = regex.Replace(structure, res, 1);
        for (int i = 0; i < sentenceData.ContextData.Length; i++)
        {
            // we don't care for the subject here because context only exists if it matches the number of facts in the first place
            // we don't care about which subject is which
            var context = sentenceData.ContextData[i];
            structure = structure.Replace("Contextual" + i, context.GetOpiniatedName(sentenceData.Tone).RandomAltName);
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

