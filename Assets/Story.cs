using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;

public class Story : DialogueViewBase
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

    public override void DialogueStarted()
    {
        Pausing.Block();
        Pausing.InstantPause();
        _dialogueRunning = true;
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
        if (_skipNext)
        {
            _skipNext = false;
            onDialogueLineFinished.Invoke();
        }
        if (onDialogueLineFinished != null)
            _displayer.Initialize(dialogueLine.CharacterName, dialogueLine.TextWithoutCharacterName.Text, onDialogueLineFinished);
    }

    public override void InterruptLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
        onDialogueLineFinished?.Invoke();
    }

    public override void DismissLine(Action onDismissalComplete)
    {
        onDismissalComplete?.Invoke();
    }

    public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
    {
        Action[] actions = dialogueOptions.Select<DialogueOption, Action>(t => () => { onOptionSelected.Invoke(t.DialogueOptionID); } ).ToArray();
        _displayer.SetOptions(dialogueOptions.Select(t => t.Line.TextWithoutCharacterName.Text).ToArray(), actions);
    }

    public override void DialogueComplete()
    {
        Pausing.Unblock();
        Pausing.Unpause();
        _dialogueRunning = false;
        _displayer.Hide();
    }

    public override void UserRequestedViewAdvancement()
    {
    }
}
