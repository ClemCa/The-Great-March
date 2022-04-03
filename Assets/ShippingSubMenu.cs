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

    public static ShippingSubMenu Instance { get => _instance; }

    void Start()
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
        _rectTransform.position = Vector3.Lerp(_rectTransform.position, _target.position, Time.deltaTime * 5);
    }

    private void ExecuteFlip(Transform target)
    {
        _target = target;
        _enabled = !_enabled;
        if (_enabled)
        {
            _rectTransform.position = _target.position;
            FacilitySubMenu.Hide();
            ResourcesSelectionSubMenu.Hide(null);
        }
        else
        {
            ResetMenu();
        }
    }
    private void ExecuteResetMenu()
    {
        transform.FindDeep("ContentChoice").gameObject.SetActive(true);
        transform.FindDeep("PeopleChoice").gameObject.SetActive(false);
        transform.FindDeep("ResourcesChoice").gameObject.SetActive(false);
        ResourcesSelectionSubMenu.Hide(null);
        _currentValue = 0;
    }

    public void ShowPeopleChoice()
    {
        transform.FindDeep("ContentChoice").gameObject.SetActive(false);
        transform.FindDeep("PeopleChoice").gameObject.SetActive(true);
        var slider = transform.FindDeep("PeopleChoice").GetComponentInChildren<Slider>();
        slider.minValue = 0;
        slider.maxValue = Planet.Selected.GetPeople();
        slider.value = _currentValue;
    }
    public void ShowResourcesChoice()
    {
        transform.FindDeep("ContentChoice").gameObject.SetActive(false);
        transform.FindDeep("ResourcesChoice").gameObject.SetActive(true);
    }

    public void UpdateValue(float value)
    {
        _currentValue = (int)value;
    }

    public int GetValue()
    {
        return _currentValue;
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

    public static void ResetMenu()
    {
        _instance.ExecuteResetMenu();
    }
}
