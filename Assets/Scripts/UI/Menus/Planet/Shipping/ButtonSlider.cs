using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClemCAddons;
using ClemCAddons.Utilities;
using UnityEngine.SceneManagement;

public class ButtonSlider : MonoBehaviour
{
    [SerializeField] private Button _extensionButton;
    [SerializeField] private Button[] _buttons;
    [SerializeField] private GameObject _saveSlots;
    [SerializeField] private GameObject _loadSlots;

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
        {
            _extensionButton.GetComponentInChildren<TMPro.TMP_Text>().text = "<";
            _saveSlots.SetActive(false);
            _loadSlots.SetActive(false);
        }
        foreach (var button in _buttons)
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
        {
            _extensionButton.GetComponentInChildren<TMPro.TMP_Text>().text = "<";
            _saveSlots.SetActive(false);
            _loadSlots.SetActive(false);
        }
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
        SceneManager.LoadScene("MainMenu");
    }

    public void Save(int slot = -1)
    {
        if(slot == -1)
        {
            _saveSlots.SetActive(!_saveSlots.activeSelf);
            _loadSlots.SetActive(false);
            return;
        }
        _saveSlots.SetActive(false);
        Saver.Instance.Save(slot);
    }

    public void Load(int slot = -1)
    {
        if (slot == -1)
        {
            _loadSlots.SetActive(!_loadSlots.activeSelf);
            _saveSlots.SetActive(false);
            for(int i = 0; i < _loadSlots.transform.childCount; i++)
            {
                _loadSlots.transform.GetChild(i).GetComponentInChildren<Button>().interactable = Saver.Instance.SlotUsed(i+1);
            }
            return;
        }
        _loadSlots.SetActive(false);
        Saver.Instance.Load(slot);
    }
}
