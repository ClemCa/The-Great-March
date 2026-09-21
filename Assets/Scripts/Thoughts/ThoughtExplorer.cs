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
    private string _currentPath = "";

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

        _currentPath = pathId;
        var options = ThoughtResolver.Options(_character, pathId);
        var labels = new string[options.Count];
        var actions = new Action[options.Count];
        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            labels[i] = option.Label;
            actions[i] = () =>
            {
                if (option.IsResolve)
                    Ask(option.NodeId);
                else
                    ShowOptions(option.NodeId);
            };
        }

        Displayer.SetOptions(labels, actions);
        Displayer.Initialize(_character.DisplayName, Prompt(pathId));
    }

    private string Prompt(string pathId)
    {
        if (string.IsNullOrEmpty(pathId))
            return "What do you want to ask about?";
        return "What about " + _character.Taxonomy.NameOf(pathId) + "?";
    }

    private void Ask(string nodeId)
    {
        if (LLMDialogueManager.Instance != null)
        {
            LLMDialogueManager.Instance.Ask(_character, nodeId, _character.Taxonomy.NameOf(nodeId), (resolution, text) =>
            {
                if (Displayer != null)
                    Displayer.SetFollowUp(() => ShowOptions(_currentPath));
            });
            return;
        }

        var result = ThoughtResolver.Resolve(_character, nodeId, new System.Random());
        Displayer.SetFollowUp(() => ShowOptions(_currentPath));
        Displayer.Initialize(_character.DisplayName, result.RawText);
    }
}
