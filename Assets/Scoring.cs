using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scoring : MonoBehaviour
{
    private static Scoring _instance;

    public int survivalTime;
    public float totalTime;

    private bool checkFirst = true;

    private System.DateTime startTime;

    void Start()
    {
        if (_instance == null)
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
            }
            survivalTime = (int)Time.timeSinceLevelLoad / 60;
            totalTime = (int)System.DateTime.Now.Subtract(startTime).TotalMinutes;
        }
        else
        {
            checkFirst = false;
        }
    }
}
