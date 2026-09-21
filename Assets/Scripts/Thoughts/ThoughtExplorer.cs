using System;
using UnityEngine;

/// <summary>
/// Raw-mode exploration: walks the character's thought tree (A -> B -> C) using the shared
/// dialogue UI. Resolving a node either shows the raw fragment or, in LLM mode, streams a reply.
/// </summary>
public class ThoughtExplorer : MonoBehaviour
{
    [SerializeField] private string _characterId = "anne";
    [SerializeField] private DialogDisplayer _displayer;
    [SerializeField] private bool _autoStart = false;

    private ThoughtCharacter _character;

    /// <summary>Raised when the player backs out of the character's head entirely.</summary>
    public event Action Exited;

    private DialogDisplayer Displayer
    {
        get { return _displayer != null ? _displayer : DialogDisplayer.Instance; }
    }

    private void Start()
    {
        if (_autoStart)
            Begin();
    }

    [ContextMenu("Begin Exploration")]
    public void Begin()
    {
        _character = ThoughtCharacterRegistry.Get(_characterId);
        if (_character == null)
        {
            Debug.LogWarning("ThoughtExplorer: no character registered with id '" + _characterId + "'.");
            return;
        }
        ShowOptions("");
    }

    public void ShowOptions(string pathId)
    {
        if (_character == null || Displayer == null)
            return;

        var options = ThoughtResolver.Options(_character, pathId);

        // No deeper categories to drill into: this is where the thought surfaces.
        if (options.Count == 0)
        {
            Ask(pathId, ParentOf(pathId));
            return;
        }

        bool atRoot = string.IsNullOrEmpty(pathId);
        var labels = new string[options.Count + 1];
        var actions = new Action[options.Count + 1];
        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            labels[i] = option.Label;
            actions[i] = () => ShowOptions(option.NodeId);
        }

        labels[options.Count] = atRoot ? "Leave" : "Back";
        if (atRoot)
            actions[options.Count] = Exit;
        else
            actions[options.Count] = () => ShowOptions(ParentOf(pathId));

        Displayer.ShowNavigation(_character.DisplayName, Header(pathId), labels, actions);
    }

    /// <summary>
    /// Where we are inside the character's head, shown alongside the speaker label.
    /// </summary>
    private string Header(string pathId)
    {
        if (string.IsNullOrEmpty(pathId))
            return "";
        return _character.Taxonomy.DisplayPath(pathId).Replace("/", " / ");
    }

    private string ParentOf(string pathId)
    {
        var node = _character.Taxonomy.Get(pathId);
        return node == null ? "" : node.ParentId;
    }

    /// <summary>Steps out of the character's head entirely.</summary>
    public void Exit()
    {
        if (Displayer != null)
            Displayer.Hide();
        if (Exited != null)
            Exited();
    }

    private void Ask(string nodeId, string returnPath)
    {
        if (LLMDialogueManager.Instance != null)
        {
            LLMDialogueManager.Instance.Ask(_character, nodeId, _character.Taxonomy.NameOf(nodeId), (resolution, text) =>
            {
                if (Displayer != null)
                    Displayer.SetFollowUp(() => ShowOptions(returnPath));
            });
            return;
        }

        var result = ThoughtResolver.Resolve(_character, nodeId, new System.Random());
        Displayer.SetFollowUp(() => ShowOptions(returnPath));
        Displayer.Initialize(_character.DisplayName, result.RawText);
    }
}
