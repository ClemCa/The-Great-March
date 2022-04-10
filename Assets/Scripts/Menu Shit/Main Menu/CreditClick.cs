using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditClick : MonoBehaviour
{
    public string link;
    public void Click()
    {
        MenuAudioManager.Instance.PlayClick();
        Application.OpenURL(link);
    }
}
