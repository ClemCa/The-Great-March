using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scoring : MonoBehaviour
{
    private static Scoring _instance;

    public static int survivalTime = 0;
    public static int totalTime = 0;
    public static int systems = 1;
    public static long naturalResourcesUnits = 0;
    public static long advancedResourcesUnits = 0;
    public static long facilitiesCount = 0;
    public static long transformativeFacilitiesCount = 0;



    private bool checkFirst = true;

    private System.DateTime startTime;
    private System.DateTime lastTime;

    public static Scoring Instance { get => _instance; }
    public DateTime StartTime { get => startTime; set => startTime = value; }
    public DateTime LastTime { get => lastTime; set => lastTime = value; }

    public void Loading()
    {
        startTime = startTime.Add(System.DateTime.Now.Subtract(lastTime));
        lastTime = System.DateTime.Now;
        // update time to match ours
    }

    void Start()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this);
    }

    void Update()
    {
        if(SceneManager.GetActiveScene().name == "Game")
        {
            if (checkFirst)
            {
                startTime = System.DateTime.Now;
                naturalResourcesUnits = advancedResourcesUnits = facilitiesCount = transformativeFacilitiesCount = 0;
                systems = 1;
                checkFirst = false;
            }
            survivalTime = (int)Time.timeSinceLevelLoad / 60;
            lastTime = System.DateTime.Now;
            totalTime = (int)(lastTime.Subtract(startTime).TotalMinutes);
        }
        else
        {
            checkFirst = true;
        }
    }
}
