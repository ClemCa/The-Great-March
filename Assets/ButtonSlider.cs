using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClemCAddons;
using ClemCAddons.Utilities;

public class ButtonSlider : MonoBehaviour
{
    [SerializeField] private Button _extensionButton;
    [SerializeField] private Button[] _buttons;

    private bool _extended;

    // Update is called once per frame
    void Update()
    {
        if (Pausing.Paused.OnceIfTrueGate("ButtonSlider".GetHashCode()))
        {
            _extensionButton.gameObject.SetActive(true);
        }
        if ((!Pausing.Paused).OnceIfTrueGate("ButtonSliderReverse".GetHashCode()))
        {
            _extensionButton.gameObject.SetActive(false);
            if (_extended)
                Extend();
        }
    }

    public void Extend()
    {
        _extended = !_extended;
        if(_extended)
            _extensionButton.GetComponentInChildren<TMPro.TMP_Text>().text = ">";
        else
            _extensionButton.GetComponentInChildren<TMPro.TMP_Text>().text = "<";
        foreach(var button in _buttons)
        {
            button.gameObject.SetActive(_extended);
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
