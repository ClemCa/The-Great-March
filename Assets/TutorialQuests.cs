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
                    new Questing.Quest(MainPlanetSelected,"Tutorial_Opened", () => { _steps++; UpdateQuestUI(); }),
                    new Questing.Quest(QueueBuilding, "Tutorial_QueueExplanation", () => { _steps++; UpdateQuestUI(); }),
                    new Questing.Quest(QueueTransformationBuilding, "Tutorial_TransformationBuilt", () => { _steps++; UpdateQuestUI(); }),
                    new Questing.Quest(QueueMoveResources, "Tutorial_ResourcesMoved", () => { _steps++; UpdateQuestUI(); })
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
                DialogDisplayer.Instance.StartDialogue("Tutorial_Completion");
                _lock = true;
                break;
        }
    }

    private void ShowTutorialCompletion()
    {

    }

    private void UpdateQuestUI()
    {

    }

    private bool MainPlanetSelected()
    {
        return Planet.Selected != null && Planet.Selected.HasPlayer;
    }
    private bool QueueBuilding()
    {
        return Planet.Selected != null && Array.Find(OrderHandler.Instance.GetPlanetQueue(Planet.Selected), t => (t.Execution.Type == OrderHandler.ActionType.Facility && t.LengthLeft == 1)) != null;
    }
    private bool QueueTransformationBuilding()
    {
        return Planet.Selected != null && Array.Find(OrderHandler.Instance.GetPlanetQueue(Planet.Selected), t => (t.Execution.Type == OrderHandler.ActionType.TransformationFacility && t.LengthLeft == 1)) != null;
    }
    private bool QueueMoveResources()
    {
        return Planet.Selected != null && Array.Find(OrderHandler.Instance.GetPlanetQueue(Planet.Selected), t => (t.Execution.Type == OrderHandler.ActionType.CargoResources && t.LengthLeft / t.Length == 1)) != null;
    }
}
