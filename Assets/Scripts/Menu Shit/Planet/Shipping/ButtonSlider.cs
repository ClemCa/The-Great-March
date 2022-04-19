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
    private bool _extendedSave;

    void Awake()
    {
        transform.Find("Prompt").GetChild(0).GetComponent<TMPro.TMP_Text>().text = Settings.ShowPrompt ? "Hide Help" : "Show Help";
    }

    // Update is called once per frame
    void Update()
    {
        if (Pausing.Paused.OnceIfTrueGate("ButtonSlider".GetHashCode()))
        {
            _extensionButton.gameObject.SetActive(true);
            _extended = _extendedSave;
            SetExtended(_extended);
        }
        if ((!Pausing.Paused).OnceIfTrueGate("ButtonSliderReverse".GetHashCode()))
        {
            _extensionButton.gameObject.SetActive(false);
            _extendedSave = _extended;
            SetExtended(false);
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

    private void SetExtended(bool extended)
    {
        _extended = extended;
        if (_extended)
            _extensionButton.GetComponentInChildren<TMPro.TMP_Text>().text = ">";
        else
            _extensionButton.GetComponentInChildren<TMPro.TMP_Text>().text = "<";
        foreach (var button in _buttons)
        {
            button.gameObject.SetActive(_extended);
        }
    }

    public void ShowPrompt(TMPro.TMP_Text text)
    {
        Settings.ShowPrompt = !Settings.ShowPrompt;
        if (Settings.ShowPrompt == true)
            text.text = "Hide Help";
        else
            text.text = "Show Help";
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Save()
    {
        Saver.Instance.Save();
    }

    public void Load()
    {
        Saver.Instance.Load();
    }
}
