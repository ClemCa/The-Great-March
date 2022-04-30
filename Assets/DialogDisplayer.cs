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
    private bool _visible = true;
    private string[] _choicesText;
    
    private Action[] _choices;
    private Action _followUp;

    [YarnCommand("SetSpeed")]
    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    void Awake()
    {
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

    public void StartDialogue()
    {
        _runner.StartDialogue("Intro_Start");
    }

    private async void WriteOverTime(string text, TMPro.TMP_Text target)
    {
        string current = "";
        target.text = current;
        for(int i = 0; i < text.Length; i++)
        {
            await System.Threading.Tasks.Task.Delay((_writingDelay / _speed).Round());
            if(text.Length > current.Length + 4)
            {
                var t = "";
                t += text[i];
                t += text[i + 1];
                t += text[i + 2];
                t += text[i + 3];
                if(t == "<br>")
                {
                    current += t;
                    i += 3;
                    target.text = current;
                    continue;
                }
            }
            current += text[i];
            target.text = current;
        }
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