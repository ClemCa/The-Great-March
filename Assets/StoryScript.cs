using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryScript : MonoBehaviour
{
    public static int SlotLoader = -1;
    public static int StoryStage = 0;

    void Start()
    {
        if(SlotLoader != -1)
        {
            Saver.Instance.Load(SlotLoader);
            return;
        }
        CheckStage();
    }

    public static void CheckStage()
    {
        if (StoryStage == 0)
        {
            DialogDisplayer.Instance?.SetSpeed(1);
            DialogDisplayer.Instance?.StartDialogue("Intro_Start");
            StoryStage++;
        }
    }
}
