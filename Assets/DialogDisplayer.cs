using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEditor;
using System;
using UnityEngine.EventSystems;
using Yarn.Unity;

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

    
    private Action[] _choices;
    private Action _followUp;

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
        Hide();
    }
    public void Initialize(string name, string text, Action followUp)
    {
        _followUp = followUp;
        Initialize(name, text);
    }
    public void Initialize(string name, string text)
    {
        transform.FindDeep("Name").GetComponentInChildren<TMPro.TMP_Text>().text = name;
        transform.FindDeep("Buttons").gameObject.SetActive(false);
        WriteOverTime(text, transform.FindDeep("Content").GetComponentInChildren<TMPro.TMP_Text>());
        Show();
    }

    public void SetOptions(string[] choicesText, Action[] choices)
    {
        _choicesText = choicesText;
        _choices = choices;
    }

    public void Show()
    {
        _visible = true;
        transform.position = transform.position.SetY(0);
    }

    public void Hide()
    {
        _visible = false;
        transform.position = transform.position.SetY(-GetComponent<RectTransform>().sizeDelta.y);
    }

    public void Select(int index)
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (_choices != null)
        {
            _choices[index].Invoke();
            _choices = null;
            return;
        }
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

    private async void WriteOverTime(string text, TMPro.TMP_Text target)
    {
        string current = "";
        int _speedID = 0;
        int delayID = 0;
        target.text = current;
        if(_delays.Length > 0)
        {
            await System.Threading.Tasks.Task.Delay(_delays[0]);
        }
        for (int i = 0; i < text.Length; i++)
        {
            await System.Threading.Tasks.Task.Delay((_writingDelay / _speed).Round());
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
                        await System.Threading.Tasks.Task.Delay(_delays[_speedID]);
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
                        await System.Threading.Tasks.Task.Delay(_delays[_speedID]);
                    continue;
                }
            }
            current += text[i];
            target.text = current;
        }
        _delays = new int[0]; // reset delays
        await System.Threading.Tasks.Task.Delay(_choiceDelay);
        var buttons = transform.FindDeep("Buttons");
        if (_choices != null)
        {
            buttons.gameObject.SetActive(true);
            buttons.GetChild(0).gameObject.SetActive(true);
            buttons.GetChild(0).GetComponentInChildren<TMPro.TMP_Text>().text = _choicesText[0];
            buttons.GetChild(1).GetComponentInChildren<TMPro.TMP_Text>().text = _choicesText[1];
        }
        else
        {
            buttons.GetChild(0).gameObject.SetActive(false);
            buttons.gameObject.SetActive(true);
            buttons.GetChild(1).GetComponentInChildren<TMPro.TMP_Text>().text = "Continue";
        }
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