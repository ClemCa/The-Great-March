using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        _ = SceneManager.LoadSceneAsync("Game");
    }

    public void Credits()
    {
        CreditsSubMenu.Flip();
    }

    public void Exit()
    {
        Application.Quit();
    }
}
