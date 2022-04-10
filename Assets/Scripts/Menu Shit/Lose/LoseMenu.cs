using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseMenu : MonoBehaviour
{
    public void StartGame()
    {
        _ = SceneManager.LoadSceneAsync("Game");
    }

    public void MainMenu()
    {
        _ = SceneManager.LoadSceneAsync("MainMenu");
    }


    public void Exit()
    {
        Application.Quit();
    }
}
