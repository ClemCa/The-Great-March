using System;
using System.Linq;
using UnityEngine;
using Yarn.Unity;

public class Story : DialoguePresenterBase
{
    [SerializeField] private DialogDisplayer _displayer;
    [SerializeField] private DialogueRunner _runner;
    private bool _dialogueRunning = false;
    private bool _skipNext = false;

    public bool DialogueRunning { get => _dialogueRunning; }

    [YarnCommand("SkipAhead")]
    public void SkipAhead()
    {
        _skipNext = true;
    }

    public override YarnTask OnDialogueStartedAsync()
    {
        Pausing.Block();
        Pausing.InstantPause();
        _dialogueRunning = true;
        return YarnTask.CompletedTask;
    }

    public override async YarnTask RunLineAsync(LocalizedLine dialogueLine, LineCancellationToken token)
    {
        bool skip = _skipNext;
        _skipNext = false;

        var completionSource = new YarnTaskCompletionSource();
        _displayer.Initialize(dialogueLine.CharacterName, dialogueLine.TextWithoutCharacterName.Text, () => completionSource.TrySetResult());

        // SkipAhead advances to the next content immediately instead of waiting for player input.
        if (skip)
        {
            completionSource.TrySetResult();
            return;
        }

        using (token.NextContentToken.Register(() => completionSource.TrySetResult()))
        {
            await completionSource.Task;
        }
    }

    public override async YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions, LineCancellationToken token)
    {
        var completionSource = new YarnTaskCompletionSource<DialogueOption>();

        Action[] actions = dialogueOptions.Select<DialogueOption, Action>(option =>
        {
            if (option.IsAvailable)
            {
                return () => completionSource.TrySetResult(option);
            }
            return () => { };
        }).ToArray();

        _displayer.SetOptions(dialogueOptions.Select(option => option.Line.TextWithoutCharacterName.Text).ToArray(), actions);

        using (token.NextContentToken.Register(() => completionSource.TrySetResult(null)))
        {
            return await completionSource.Task;
        }
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        Pausing.Unblock();
        Pausing.Unpause();
        _dialogueRunning = false;
        _displayer.Hide();
        return YarnTask.CompletedTask;
    }
}
