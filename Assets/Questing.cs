using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Questing : MonoBehaviour
{
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
        if (_currentQuestline.current.Checker.Invoke())
        {
            _currentQuestline.NextQuest();
        }
    }

    public bool StartQuestLine(QuestLine questLine)
    {
        if (questLine == null || questLine.current == null)
            return false;
        _currentQuestline = questLine;
        return true;
    }

    public class Quest
    {
        public bool TriggerMode;
        public Button Trigger;
        public Func<bool> Checker;
        public string Dialogue;
        public Action Done;
        public Quest(Func<bool> checker, string dialogue, Action done = null)
        {
            TriggerMode = false;
            Checker = checker;
            Dialogue = dialogue;
            Done = done;
        }
        public Quest(Button trigger, string dialogue, Action done = null)
        {
            TriggerMode = true;
            Trigger = trigger;
            Dialogue = dialogue;
            Done = done;
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
            DialogDisplayer.Instance.StartDialogue(current.Dialogue);
        }

        public void NextQuest()
        {
            id++;
            if (Quests.Length > id)
            {
                current.Trigger?.onClick.RemoveListener(ReceiveTrigger);
                current = Quests[id];
                current.Trigger?.onClick.AddListener(ReceiveTrigger);
                current.Done?.Invoke();
                DialogDisplayer.Instance.StartDialogue(current.Dialogue);
            }
            else
                current = null;
        }

        public void ReceiveTrigger()
        {
            NextQuest();
        }
    }
}
