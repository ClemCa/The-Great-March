using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Questing : MonoBehaviour
{
    [SerializeField] private RectTransform _display;
    private static Questing _instance;
    private QuestLine _currentQuestline;

    public bool IsRunningQuestline { get => _currentQuestline != null && _currentQuestline.current != null; }

    public static Questing Instance { get => _instance;}

    void Start()
    {
        _instance = this;
    }

    void Update()
    {
        if (_currentQuestline == null || _currentQuestline.current == null || _currentQuestline.current.Trigger)
            return;
        _display.GetComponentInChildren<TMPro.TMP_Text>().text = _currentQuestline.current.Objective;
        if (_currentQuestline.current.Checker.Invoke())
        {
            _currentQuestline.NextQuest(_display);
        }
    }

    public bool StartQuestLine(QuestLine questLine)
    {
        if (questLine == null || questLine.current == null)
            return false;
        _currentQuestline = questLine;
        _display.GetComponentInChildren<TMPro.TMP_Text>().text = _currentQuestline.current.Objective;
        ClemCAddons.Utilities.Lerper.ConstantLerp(_display.anchoredPosition, Vector2.zero, 2, (v) => _display.anchoredPosition = v);
        return true;
    }

    public void HideUI()
    {
        ClemCAddons.Utilities.Lerper.ConstantLerp(_display.anchoredPosition, new Vector2(0, 75), 1, (v) => _display.anchoredPosition = v);
    }

    public class Quest
    {
        public bool TriggerMode;
        public Button Trigger;
        public Func<bool> Checker;
        public string Dialogue;
        public Action Done;
        public string Objective;
        public Quest(Func<bool> checker, string dialogue, string description, Action done = null)
        {
            TriggerMode = false;
            Checker = checker;
            Dialogue = dialogue;
            Done = done;
            Objective = description;
        }
        public Quest(Button trigger, string dialogue, string description, Action done = null)
        {
            TriggerMode = true;
            Trigger = trigger;
            Dialogue = dialogue;
            Done = done;
            Objective = description;
        }
    }

    public class QuestLine
    {
        public Quest current;
        private Quest[] Quests;
        private int id;

        public QuestLine(params Quest[] quests)
        {
            Quests = quests;
            id = 0;
            current = quests[0];
            current.Trigger?.onClick.AddListener(ReceiveTrigger);
            if(current.Dialogue != "")
                DialogDisplayer.Instance.StartDialogue(current.Dialogue);
        }

        public void NextQuest(RectTransform display)
        {
            id++;
            if (Quests.Length > id)
            {
                current.Trigger?.onClick.RemoveListener(ReceiveTrigger);
                current = Quests[id];
                current.Trigger?.onClick.AddListener(ReceiveTrigger);
                current.Done?.Invoke();
                if(current.Dialogue != "")
                    DialogDisplayer.Instance.StartDialogue(current.Dialogue); 
            }
            else
            {
                current = null;
                display.GetComponentInChildren<TMPro.TMP_Text>().text = "";
                ClemCAddons.Utilities.Lerper.ConstantLerp(display.anchoredPosition, new Vector2(0, 75), 1, (v) => display.anchoredPosition = v);
            }
        }

        public void ReceiveTrigger()
        {
            NextQuest(Instance._display);
        }
    }
}
