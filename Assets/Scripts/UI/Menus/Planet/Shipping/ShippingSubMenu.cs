using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System;
using UnityEngine.UI;

public class ShippingSubMenu : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Transform _target;
    private bool _enabled;
    private static ShippingSubMenu _instance;
    private int _currentValue = 0;
    private MenuMode _mode;

    public int Ship;
    public static ShippingSubMenu Instance { get => _instance; }

    public enum MenuMode
    {
        People,
        Resources,
        Ship
    }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _instance = this;
    }

    void Update()
    {
        if (!_enabled)
        {
            _rectTransform.anchoredPosition = new Vector3(1920, 0);
            return;
        }
        if (Planet.Selected == null)
        {
            _instance._enabled = false;
            return;
        }
        _rectTransform.position = Vector3.Lerp(_rectTransform.position, _target.position, Time.unscaledDeltaTime * 5);
    }

    private void ExecuteFlip(Transform target)
    {
        _target = target;
        _enabled = !_enabled;
        if (_enabled)
        {
            _rectTransform.position = _target.position;
            SubMenu.HideActive();
            ResourcesSelectionSubMenu.Hide();
        }
        else
        {
            ResetMenu();
        }
    }
    private void SetState(bool value)
    {
        _enabled = value;
        if (_enabled)
        {
            _rectTransform.position = _target.position;
            SubMenu.HideActive();
            ResourcesSelectionSubMenu.Hide();
        }
        else
        {
            ResetMenu();
        }
    }
    private void ExecuteResetMenu()
    {
        transform.FindDeep("ContentChoice").gameObject.SetActive(false);
        transform.FindDeep("PeopleChoice").gameObject.SetActive(false);
        transform.FindDeep("ResourcesChoice").gameObject.SetActive(false);
        transform.FindDeep("ResourceSending").gameObject.SetActive(false);
        transform.FindDeep("PresidentSending").gameObject.SetActive(false);
        transform.FindDeep("ShipsChoice").gameObject.SetActive(true);
        transform.GetComponentInChildren<SelectResourcesButton>(true).SetResource(null, null);
        ResourcesSelectionSubMenu.Hide();
        _currentValue = 0;
    }
    public void ShowContentChoice()
    {
        _mode = MenuMode.People;
        transform.FindDeep("ShipsChoice").gameObject.SetActive(false);
        transform.FindDeep("ContentChoice").gameObject.SetActive(true);
        _currentValue = 0;
    }
    public void ShowPeopleChoice()
    {       
        _mode = MenuMode.People;
        transform.FindDeep("ContentChoice").gameObject.SetActive(false);
        transform.FindDeep("PeopleChoice").gameObject.SetActive(true);
        _currentValue = _currentValue.Min(Planet.Selected.GetPeople());
    }
    public void ShowResourcesChoice()
    {
        _mode = MenuMode.Resources;
        transform.FindDeep("ContentChoice").gameObject.SetActive(false);
        transform.FindDeep("ResourcesChoice").gameObject.SetActive(true);
        _currentValue = 0;
    }

    public void ShowResourceSending()
    {
        _mode = MenuMode.Resources;
        transform.FindDeep("ShipsChoice").gameObject.SetActive(false);
        transform.FindDeep("ResourceSending").gameObject.SetActive(true);
        _currentValue = 0;
    }

    public void ShowShipChoice()
    {
        _mode = MenuMode.Ship;
        transform.FindDeep("ContentChoice").gameObject.SetActive(false);
        transform.FindDeep("ShipsChoice").gameObject.SetActive(true);
        _currentValue = 0;
    }

    public void ShowPresidentSending()
    {
        _mode = MenuMode.Ship;
        transform.FindDeep("ShipsChoice").gameObject.SetActive(false);
        transform.FindDeep("PresidentSending").gameObject.SetActive(true);
        _currentValue = 0;
    }

    public void UpdateValue(int value)
    {
        if(_mode == MenuMode.People)
            _currentValue = Mathf.Clamp(value + _currentValue, 0, Planet.Selected.GetPeople().Min(5));
        var button = GetComponentInChildren<SelectResourcesButton>();
        if(_mode == MenuMode.Resources && button.Unlocked)
        {
            if(button.IsAdvanced)
                _currentValue = Mathf.Clamp(value + _currentValue, 0, Planet.Selected.GetResource(button.AdvancedResource).Min(10));
            else
                _currentValue = Mathf.Clamp(value + _currentValue, 0, Planet.Selected.GetResource(button.Resource).Min(10));

        }
    }

    public void Reset(int value)
    {
        _currentValue = value;
    }

    public int GetValue()
    {
        return _currentValue;
    }

    public string GetValueAsString()
    {
        return _currentValue.ToString();
    }

    public static void Flip(Transform target)
    {
        _instance.ExecuteFlip(target);
    }
    public static void Hide()
    {
        _instance._enabled = false;
        ResetMenu();
    }

    public static void Show()
    {
        _instance.SetState(true);
    }

    public static void ResetMenu()
    {
        _instance.ExecuteResetMenu();
    }
}
