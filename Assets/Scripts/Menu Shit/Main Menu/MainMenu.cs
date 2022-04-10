using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        _ = SceneManager.LoadSceneAsync("Game");
        MenuAudioManager.Instance.PlayClick();
    }

    public void Credits()
    {
        CreditsSubMenu.Flip();
        DevelopperSubMenu.Hide();
        MenuAudioManager.Instance.PlayClick();
    }

    public void Developper()
    {

        MenuAudioManager.Instance.PlayClick();
        CreditsSubMenu.Hide();
        DevelopperSubMenu.Flip();
    }


    public void Exit()
    {
        MenuAudioManager.Instance.PlayClick();
        Application.Quit();
    }
}
