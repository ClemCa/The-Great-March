using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System.Linq;
using System;

public class TutorialQuests : MonoBehaviour
{
    private Steps _steps;
    private bool _lock;

    private enum Steps
    {
        ProcedureExplanation,
        SelectMainPlanet,
        BuildFacilities,
        Queue,
        BuildTransformationFacilities,
        MoveResources,
        LastExplanations
    }
    void Update()
    {
        if (_lock)
            return;
        switch (_steps)
        {
            case Steps.ProcedureExplanation:
                DialogDisplayer.Instance.SetSpeed(5);
                DialogDisplayer.Instance.StartDialogue("Tutorial_Start");
                _steps++;
                break;
            case Steps.SelectMainPlanet:
                if (DialogDisplayer.Instance.Runner.IsDialogueRunning || Questing.Instance.IsRunningQuestline)
                    break;
                var questline = new Questing.QuestLine(
                    new Questing.Quest(MainPlanetSelected, "", "Select your planet", () => { _steps++; }),
                    new Questing.Quest(QueueBuilding, "Tutorial_Opened", "Build a facility", () => { _steps++; }),
                    new Questing.Quest(QueueTransformationBuilding, "Tutorial_QueueExplanation", "Build a transformation facility", () => { _steps++; }),
                    new Questing.Quest(QueueMoveResources, "Tutorial_TransformationBuilt", "Send resources towards another planet", () => { _steps++; }),
                    new Questing.Quest(DialogueFinished, "Tutorial_ResourcesMoved", "", () => { _steps++; Questing.Instance.HideUI(); }),
                    new Questing.Quest(DialogueFinished, "", "", () => { _steps++; })
                    );
                Questing.Instance.StartQuestLine(questline);
                break;
            case Steps.BuildFacilities:
                break;
            case Steps.Queue:
                break;
            case Steps.BuildTransformationFacilities:
                break;
            case Steps.MoveResources:
                break;
            case Steps.LastExplanations:
                ShowTutorialCompletion();
                _lock = true;
                break;
        }
    }

    private void ShowTutorialCompletion()
    {
        Questing.Instance.StartQuestLine(new Questing.QuestLine(new Questing.Quest(ReturnFalse, "Tutorial_Completion", "Tutorial completed.\nPress Escape and use the pause menu to leave.")));
    }

    private bool DialogueFinished()
    {
        return !DialogDisplayer.Instance.Runner.IsDialogueRunning;
    }

    private bool ReturnFalse()
    {
        return false;
    }

    private bool MainPlanetSelected()
    {
        return Planet.Selected != null && Planet.Selected.HasPlayer;
    }
    private bool QueueBuilding()
    {
        foreach (var r in OrderHandler.Instance.QueueData)
        {
            if (r.Value.FindIndex(t => (t.Execution.Type == OrderHandler.ActionType.Facility && t.LengthLeft.Round() == 1)) != -1)
                return true;
        }
        return false;
    }
    private bool QueueTransformationBuilding()
    {
        foreach (var r in OrderHandler.Instance.QueueData)
        {
            if (r.Value.FindIndex( t => (t.Execution.Type == OrderHandler.ActionType.TransformationFacility && t.LengthLeft.Round() == 1)) != -1)
                return true;
        }
        return false;
    }
    private bool QueueMoveResources()
    {
        foreach (var r in OrderHandler.Instance.QueueData)
        {
            if (r.Value.FindIndex(t => (t.Execution.Type == OrderHandler.ActionType.CargoResources && t.LengthLeft.Round() == 1)) != -1)
                return true;
        }
        return false;
    }
}
