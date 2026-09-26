using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEditor;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Yarn.Unity;
using Cysharp.Threading.Tasks;

public class DialogDisplayer : MonoBehaviour
{
    [SerializeField] private int _writingDelay = 100;
    [SerializeField] private int _choiceDelay = 250;
    [SerializeField] private DialogueRunner _runner;
    [SerializeField] private float _speed = 1;

    private float[] _speeds = new float[] { 1 };
    private int[] _delays = new int[0];
    private bool _visible = true;
    private string[] _choicesText;
    private static DialogDisplayer _instance;

    private RectTransform rectTransform;
    private TMPro.TMP_Text _contentText;
    private TMPro.TMP_Text _nameText;
    private Transform _buttons;
    private readonly List<GameObject> _choiceButtons = new List<GameObject>();

    private Action[] _choices;
    private Action _followUp;
    private bool _streaming;
    private string _streamRaw = "";
    private string[] _queuedLines;
    private int _lineIndex;

    public static DialogDisplayer Instance { get => _instance; }
    public DialogueRunner Runner { get => _runner; }


    [YarnCommand("SetSpeed")]
    public void SetSpeed(float speed, float speed2 = -1, float speed3 = -1, float speed4 = -1)
    {
        _speed = speed;
        var r = new float[] { speed };
        if (speed2 != -1)
            r = r.Add(speed2);
        if (speed3 != -1)
            r = r.Add(speed3);
        if (speed4 != -1)
            r = r.Add(speed4);
        _speeds = r;
    }

    [YarnCommand("SetDelay")]
    public void SetDelay(int delay, int delay2 = -1, int delay3 = -1, int delay4 = -1, int delay5 = -1)
    {
        var r = new int[] { delay };
        if (delay2 != -1)
            r = r.Add(delay2);
        if (delay3 != -1)
            r = r.Add(delay3);
        if (delay4 != -1)
            r = r.Add(delay4);
        if (delay5 != -1)
            r = r.Add(delay5);
        _delays = r;
    }

    void Awake()
    {
        _instance = this;
        rectTransform = GetComponent<RectTransform>();
        _nameText = transform.FindDeep("Name").GetComponentInChildren<TMPro.TMP_Text>();
        _contentText = transform.FindDeep("Content").GetComponentInChildren<TMPro.TMP_Text>();
        _buttons = transform.FindDeep("Buttons");
        Hide();
    }

    public void Initialize(string name, string text, Action followUp)
    {
        _followUp = followUp;
        Initialize(name, text);
    }

    public void Initialize(string name, string text)
    {
        _streaming = false;
        _queuedLines = null;
        _streamRaw = "";
        _nameText.text = name;
        _buttons.gameObject.SetActive(false);
        WriteOverTime(text, _contentText);
        Show();
    }

    /// <summary>
    /// Brain-exploration navigation: the speaker label names whose head we're inside, the
    /// content is a neutral path header, and the buttons are the next branches to dig into.
    /// </summary>
    public void ShowNavigation(string speaker, string header, string[] choicesText, Action[] choices)
    {
        _streaming = false;
        _queuedLines = null;
        _streamRaw = "";
        _followUp = null;
        _choicesText = choicesText;
        _choices = choices;
        _nameText.text = speaker;
        _contentText.text = header ?? "";
        _buttons.gameObject.SetActive(false);
        PresentChoices();
        Show();
    }

    #region Streaming (LLM mode)

    public void BeginStream(string name)
    {
        _streaming = true;
        _followUp = null;
        _choices = null;
        _choicesText = null;
        _queuedLines = null;
        _lineIndex = 0;
        _streamRaw = "";
        _nameText.text = name;
        _contentText.text = "";
        _buttons.gameObject.SetActive(false);
        Show();
    }

    public void AppendStream(string token)
    {
        if (string.IsNullOrEmpty(token) || !_streaming)
            return;
        _streamRaw += token;
        // Only the first line is revealed while the rest of the reply is still arriving;
        // any extra newline-separated lines are held back and spoken one at a time below.
        _contentText.text = FirstLine(_streamRaw);
    }

    public void EndStream()
    {
        if (!_streaming)
            return;
        _streaming = false;

        string[] lines = SplitLines(_streamRaw);
        if (lines.Length > 1)
        {
            _queuedLines = lines;
            _lineIndex = 0;
            _contentText.text = lines[0];
            PresentChoices();
            return;
        }

        if (lines.Length == 1)
            _contentText.text = lines[0];
        PresentChoices();
    }

    /// <summary>
    /// Splits a streamed reply into the separate lines a character speaks in one thought.
    /// Blank lines are dropped so an author-friendly trailing newline is harmless.
    /// </summary>
    private static string[] SplitLines(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new string[0];
        var parts = text.Replace("\r\n", "\n").Split('\n');
        var lines = new List<string>();
        for (int i = 0; i < parts.Length; i++)
        {
            string line = parts[i].Trim();
            if (!string.IsNullOrEmpty(line))
                lines.Add(line);
        }
        return lines.ToArray();
    }

    private static string FirstLine(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";
        int index = text.IndexOf('\n');
        return index < 0 ? text : text.Substring(0, index).TrimEnd('\r');
    }

    #endregion

    public void SetOptions(string[] choicesText, Action[] choices)
    {
        _choicesText = choicesText;
        _choices = choices;
    }

    public void SetFollowUp(Action followUp)
    {
        _followUp = followUp;
    }

    public void Show()
    {
        _visible = true;
        rectTransform.anchoredPosition = rectTransform.anchoredPosition.SetY(0);
    }

    public void Hide()
    {
        _visible = false;
        rectTransform.anchoredPosition = rectTransform.anchoredPosition.SetY(-rectTransform.rect.size.y);
    }

    public void Select(int index)
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (_choices != null)
        {
            Action action = (index >= 0 && index < _choices.Length) ? _choices[index] : null;
            _choices = null;
            _choicesText = null;
            if (action != null)
                action.Invoke();
            return;
        }
        // A thought can be spoken as several lines in a row; Continue reveals the next one
        // until the character is done, then the follow-up (e.g. return to the thought tree) runs.
        if (_queuedLines != null && _lineIndex < _queuedLines.Length - 1)
        {
            _lineIndex++;
            _buttons.gameObject.SetActive(false);
            WriteOverTime(_queuedLines[_lineIndex], _contentText);
            return;
        }
        _queuedLines = null;
        if (_followUp != null)
            _followUp.Invoke();
    }

    public void Flip()
    {
        if (_visible)
            Hide();
        else
            Show();
    }

    public void StartDialogue(string dialogue = "Intro_Start")
    {
        _runner.StartDialogue(dialogue);
    }

    private void PresentChoices()
    {
        int count = _choices != null ? _choicesText.Length : 1;
        if (count <= 0)
            count = 1;
        EnsureChoiceButtons(count);
        for (int i = 0; i < _buttons.childCount; i++)
        {
            var button = _buttons.GetChild(i).gameObject;
            bool used = i < count;
            button.SetActive(used);
            if (!used)
                continue;
            button.GetComponentInChildren<TMPro.TMP_Text>().text = _choices != null ? _choicesText[i] : "Continue";
            var uiButton = button.GetComponent<Button>();
            if (uiButton != null)
            {
                uiButton.onClick.RemoveAllListeners();
                int index = i;
                uiButton.onClick.AddListener(() => Select(index));
            }
        }
        _buttons.gameObject.SetActive(true);
    }

    /// <summary>
    /// The prefab ships with two buttons; extra choices are cloned from the second template.
    /// </summary>
    private void EnsureChoiceButtons(int count)
    {
        while (_buttons.childCount < count)
        {
            int templateIndex = Mathf.Clamp(1, 0, _buttons.childCount - 1);
            Instantiate(_buttons.GetChild(templateIndex).gameObject, _buttons);
        }
        _choiceButtons.Clear();
        for (int i = 0; i < count; i++)
            _choiceButtons.Add(_buttons.GetChild(i).gameObject);
    }

    private async UniTaskVoid WriteOverTime(string text, TMPro.TMP_Text target)
    {
        string current = "";
        int _speedID = 0;
        int delayID = 0;
        target.text = current;
        if(_delays.Length > 0)
        {
            await UniTask.Delay(_delays[0]);
        }
        for (int i = 0; i < text.Length; i++)
        {
            await UniTask.Delay((_writingDelay / _speed).Round());
            if(text.Length > i + 5) // 4+1
            {
                var t = "";
                t += text[i];
                t += text[i + 1];
                t += text[i + 2];
                t += text[i + 3];
                if (t == "<dl>")
                {
                    current += " ";  // replace command by space, and directly skip ahead
                    i += 3;
                    target.text = current;
                    delayID++;
                    if (delayID < _delays.Length)
                        await UniTask.Delay(_delays[_speedID]);
                    continue;
                }
                if (t == "<sd>")
                {
                    i += 3;
                    target.text = current;
                    _speedID++;
                    if (_speedID < _speeds.Length)
                        _speed = _speeds[_speedID];
                    continue;
                }
                if (t == "<br>")
                {
                    current += t;
                    i += 3;
                    target.text = current;
                    _speedID++;
                    delayID++;
                    if (_speedID < _speeds.Length)
                        _speed = _speeds[_speedID];
                    if (delayID < _delays.Length)
                        await UniTask.Delay(_delays[_speedID]);
                    continue;
                }
            }
            current += text[i];
            target.text = current;
        }
        _delays = new int[0]; // reset delays
        await UniTask.Delay(_choiceDelay);
        PresentChoices();
    }
}

#if(UNITY_EDITOR)
[CustomEditor(typeof(DialogDisplayer))]
public class DialogDisplayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Flip"))
        {
            foreach(var target in targets)
            {
                var display = (target as DialogDisplayer);
                display.Flip();
            }
        }
        if (GUILayout.Button("Start Dialogue"))
        {
            foreach (var target in targets)
            {
                var display = (target as DialogDisplayer);
                display.StartDialogue();
            }
        }
    }
}
#endif
