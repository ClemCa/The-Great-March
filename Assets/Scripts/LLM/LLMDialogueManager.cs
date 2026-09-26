using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates a character interaction. Raw mode shows the resolved thought fragment;
/// LLM mode streams a natural reply built from the full JSON context. Either way the
/// question/answer pair is remembered so the character grows over time.
/// </summary>
public class LLMDialogueManager : MonoBehaviour
{
    private static LLMDialogueManager _instance;

    [SerializeField] private DialogDisplayer _displayer;

    private readonly System.Random _rng = new System.Random();

    public static LLMDialogueManager Instance { get { return _instance; } }
    public DialogDisplayer Displayer { get { return _displayer != null ? _displayer : DialogDisplayer.Instance; } }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Ask(ThoughtCharacter character, string nodeId, string playerQuestion, Action<ThoughtResolution, string> onFinished = null)
    {
        StartCoroutine(AskRoutine(character, nodeId, playerQuestion, onFinished));
    }

    public void Ask(ThoughtCharacter character, string nodeId, Action<ThoughtResolution, string> onFinished = null)
    {
        Ask(character, nodeId, null, onFinished);
    }

    /// <summary>
    /// LLM-mode meeting line: surfaces the character's most fitting thought (if one is alive) and
    /// greets the player. Deliberately inert outside LLM mode (Raw mode has no notion of meeting)
    /// and a no-op for a provider that only reasons, so it never burns a silent turn.
    /// </summary>
    public void Meet(ThoughtCharacter character, Action<EmergingThought, string> onFinished = null)
    {
        if (LLMSettings.Mode != LLMMode.LLM || character == null)
            return;
        if (LLMThinking.IsUnreliable(LLMSettings.EffectiveModel()))
            return;
        StartCoroutine(MeetRoutine(character, onFinished));
    }

    private IEnumerator MeetRoutine(ThoughtCharacter character, Action<EmergingThought, string> onFinished)
    {
        var emerging = EmergingThoughtResolver.MostFitting(character, _rng);
        character.RefreshMood(GameClock.Now);
        character.History.UpdateSummaries(LLMSettings.VerbatimHistory, LLMSettings.SummarizedHistory, null);

        var displayer = Displayer;
        if (displayer != null)
            displayer.BeginStream(character.DisplayName);

        var request = ContextBuilder.BuildMeetingRequest(character, emerging, GameClock.Now);
        var provider = LLMProviderFactory.Create(LLMSettings.Provider);

        string streamed = "";
        string error = null;

        yield return provider.Stream(request,
            token =>
            {
                streamed += token;
                if (displayer != null)
                    displayer.AppendStream(token);
            },
            () => { },
            err => { error = err; });

        if (string.IsNullOrEmpty(streamed))
        {
            // A raw fragment is not a greeting, so there is nothing sensible to show. Leave the
            // thought un-surfaced; the next meeting can try again.
            if (displayer != null)
                displayer.EndStream();
            if (onFinished != null)
                onFinished(emerging, error);
            yield break;
        }

        if (displayer != null)
            displayer.EndStream();

        if (emerging.HasThought)
            emerging.MarkSurfaced(GameClock.Now);
        character.History.Add("", streamed);

        if (onFinished != null)
            onFinished(emerging, streamed);
    }

    private IEnumerator AskRoutine(ThoughtCharacter character, string nodeId, string playerQuestion, Action<ThoughtResolution, string> onFinished)
    {
        var resolution = ThoughtResolver.Resolve(character, nodeId, _rng);
        string query = character.Taxonomy != null ? character.Taxonomy.DisplayPath(nodeId) : nodeId;
        string question = string.IsNullOrEmpty(playerQuestion) ? query : playerQuestion;
        string finalText = resolution.RawText;

        bool useLlm = LLMSettings.Mode == LLMMode.LLM
            && !LLMThinking.IsUnreliable(LLMSettings.EffectiveModel());

        if (useLlm)
        {
            character.RefreshMood(GameClock.Now);
            character.History.UpdateSummaries(LLMSettings.VerbatimHistory, LLMSettings.SummarizedHistory, null);

            var displayer = Displayer;
            if (displayer != null)
                displayer.BeginStream(character.DisplayName);

            var request = BuildRequest(character, query, resolution.RawText, question);
            var provider = LLMProviderFactory.Create(LLMSettings.Provider);

            string streamed = "";
            string error = null;

            yield return provider.Stream(request,
                token =>
                {
                    streamed += token;
                    if (displayer != null)
                        displayer.AppendStream(token);
                },
                () => { },
                err => { error = err; });

            if (string.IsNullOrEmpty(streamed))
                streamed = error == null ? resolution.RawText : "(unavailable) " + resolution.RawText;

            finalText = streamed;
            if (displayer != null)
                displayer.EndStream();

            character.History.Add(question, finalText);
        }
        else
        {
            // Raw mode, or a thinking model that never reaches a visible reply: let the Raw thought
            // stand in rather than paying for an empty call every turn.
            var displayer = Displayer;
            if (displayer != null)
                displayer.Initialize(character.DisplayName, resolution.RawText);
        }

        if (onFinished != null)
            onFinished(resolution, finalText);
    }

    private LLMRequest BuildRequest(ThoughtCharacter character, string query, string rawReply, string question)
    {
        return ContextBuilder.BuildRequest(character, query, rawReply, question, GameClock.Now);
    }
}
