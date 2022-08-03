using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json;

public class PriorityUpdater : MonoBehaviour
{
    [SerializeField] private bool _global;
    [SerializeField] private int _currentDisplay;
    [SerializeField] private GameObject _prioritiesPrefab;
    [SerializeField] private TMPro.TMP_Text _switch;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private TMPro.TMP_Text _globalText;

    private int _currentlyDisplaying = -1;
    private List<int> _actuallydisplaying;

    public void SwitchDisplay()
    {
        ChangeDisplay(_currentDisplay == 0 ? 1 : 0);
    }

    public void SwitchMode()
    {
        _global = !_global;
        ChangeDisplay(_currentDisplay);
    }

    private void ChangeDisplay(int display)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var target = transform.GetChild(i);
            target.SetParent(null);
            Destroy(target.gameObject);
        }
        _currentDisplay = display;
        _actuallydisplaying = null;
        _switch.text = display == 0 ? "Fuel" : "Food";
        _title.text = display == 0 ? "Food Consumption" : "Fuel Consumption";
    }

    void Start()
    {
        SwitchMode();
    }

    void Update()
    {
        if (_currentlyDisplaying != _currentDisplay || CheckCache())
        {
            Check();
            _currentlyDisplaying = _currentDisplay;
            return;
        }
    }

    private bool CheckCache()
    {
        if (_actuallydisplaying == null)
            return false;
        if (_actuallydisplaying.Count != transform.childCount)
        {
            _actuallydisplaying = null;
            return false;
        }
        bool modified = false;
        for (int i = 0; i < _actuallydisplaying.Count; i++)
        {
            var child = transform.GetChild(i);
            var name = child.GetChild(1).GetComponent<TMPro.TMP_Text>().text;
            bool invalid = false;
            try
            {
                var n = Registry.Instance.GetResourceFromName(name);
                if (n.HasValue && (_actuallydisplaying[i] != ((int)n.Value)))
                {
                    _actuallydisplaying[i] = (int)n.Value;
                    modified = true;
                }
                else if (!n.HasValue)
                    invalid = true;
            }
            catch
            {
                var n = Registry.Instance.GetAdvancedResourceFromName(name);
                if (n.HasValue && (_actuallydisplaying[i] != ((int)n.Value) + Enum.GetNames(typeof(Registry.Resources)).Length))
                {
                    _actuallydisplaying[i] = (int)n.Value + Enum.GetNames(typeof(Registry.Resources)).Length;
                    modified = true;
                }
                else if (!n.HasValue)
                    invalid = true;
            }
            if (invalid)
            {
                Debug.LogError("Invalid error in priority cache check");
                return true;
            }
        }
        return modified;
    }

    private void Check()
    {
        _globalText.text = _global ? "Global" : "Local";
        List<int> priorities;
        if (_global)
        {
            priorities = new List<int>(Planet.GetGlobalPriorities(_currentDisplay));
        }
        else
        {
            if (Planet.Selected == null)
                return;
            priorities = new List<int>(Planet.Selected.GetLocalPriorities(_currentDisplay));
        }
        if (_actuallydisplaying == null)
            _actuallydisplaying = new List<int>();
        if (priorities.SequenceEqual(_actuallydisplaying))
        {
            return;
        }
        if(PermutationCheck(priorities, _actuallydisplaying))
        {
            UpdatePriorities(_actuallydisplaying);
            return;
        }
        Refresh(priorities);
    }

    private bool PermutationCheck(List<int> _target, List<int> _actual)
    {
        if (_target.Count != _actual.Count)
            return false;
        if (_target.OrderBy(x => x).SequenceEqual(_actual.OrderBy(x => x)))
            return true;
        return false;
    }

    private void Refresh(List<int> list)
    {
        int i = 0;
        for(_ = i; i < transform.childCount; i++)
        {
            if(i >= list.Count)
            {
                ClearExcess(i);
                _actuallydisplaying = list;
                return;
            }
            var display = transform.GetChild(i);
            display.GetChild(1).GetComponent<TMPro.TMP_Text>().text = GetNameByID(list[i]);
            display.GetChild(2).GetComponent<TMPro.TMP_Text>().text = (_currentDisplay == 0 ? "Feeding power: " : "Fueling power: ") + GetValueByID(list[i]);
        }
        for (_ = i; i < list.Count; i++)
        {
            var display = Instantiate(_prioritiesPrefab, transform).transform;
            display.GetChild(1).GetComponent<TMPro.TMP_Text>().text = GetNameByID(list[i]);
            display.GetChild(2).GetComponent<TMPro.TMP_Text>().text = (_currentDisplay == 0 ? "Feeding power: " : "Fueling power: ") + GetValueByID(list[i]);
        }
        _actuallydisplaying = new List<int>(list);
    }
    private string GetNameByID(int id)
    {
        if (id < Enum.GetNames(typeof(Registry.Resources)).Length)
            return Registry.Instance.GetResourceName((Registry.Resources)id);
        else
            return Registry.Instance.GetResourceName((Registry.AdvancedResources)(id - Enum.GetNames(typeof(Registry.Resources)).Length));
    }

    private int GetValueByID(int id)
    {
        if (id < Enum.GetNames(typeof(Registry.Resources)).Length)
            return Registry.Instance.GetResourceValue((Registry.Resources)id);
        else
            return Registry.Instance.GetResourceValue((Registry.AdvancedResources)(id - Enum.GetNames(typeof(Registry.Resources)).Length));
    }

    private void ClearExcess(int target)
    {
        for (int i = transform.childCount; i >= target; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    private void UpdatePriorities(List<int> newPriorities)
    {
        if (_global)
        {
            Planet.SetGlobalPriorities(_currentDisplay, newPriorities);
        }
        else
        {
            Planet.Selected.SetLocalPriorities(_currentDisplay, newPriorities);
        }
    }
}
