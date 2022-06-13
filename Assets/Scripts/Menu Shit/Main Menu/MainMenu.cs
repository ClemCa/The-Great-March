using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public void StartGame()
    {
        StoryScript.SlotLoader = -1;
        _ = SceneManager.LoadSceneAsync("Game");
        MenuAudioManager.Instance.PlayClick();
    }

    public void LoadSlot(int slot)
    {
        StoryScript.SlotLoader = slot;
        _ = SceneManager.LoadSceneAsync("Game");
        MenuAudioManager.Instance.PlayClick();
    }

    public void Tutorial()
    {
        _ = SceneManager.LoadSceneAsync("Tutorial");
        MenuAudioManager.Instance.PlayClick();
    }

    public void Credits()
    {
        SubMenu.GetInstance(SubMenu.SubMenuMode.CreditsSubMenu).Flip();
        MenuAudioManager.Instance.PlayClick();
    }

    public void Developper()
    {

        MenuAudioManager.Instance.PlayClick();
        SubMenu.GetInstance(SubMenu.SubMenuMode.DeveloperSubMenu).Flip();
    }


    public void Exit()
    {
        MenuAudioManager.Instance.PlayClick();
        Application.Quit();
    }

    public void FlipEnabled(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void UpdateSlots(Transform transform)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            child.GetComponent<Button>().interactable = Saver.Instance.SlotUsed(i+1);
        }
    }
}
