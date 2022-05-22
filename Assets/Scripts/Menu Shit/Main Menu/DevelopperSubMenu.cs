using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System;
using UnityEngine.UI;

public class DevelopperSubMenu : MonoBehaviour
{
    [SerializeField] private bool _targetMode;
    [SerializeField] private Transform _target;

    private static DevelopperSubMenu _instance;
    private RectTransform _rectTransform;
    private bool _enabled;

    void Awake()
    {
        _instance = this;
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (_targetMode && _enabled.OnceIfTrueGate("DevelopperMenu".GetHashCode()))
        {
            _rectTransform.position = Camera.main.WorldToScreenPoint(Planet.Selected.transform.position);
        }
        if (!_enabled)
        {
            _rectTransform.anchoredPosition = new Vector3(0, 0);
            return;
        }
        if (_targetMode)
        {
            _rectTransform.position = Vector3.Lerp(_rectTransform.position, Camera.main.WorldToScreenPoint(_target.position), Time.deltaTime * 5);
        }
        else
        {
            _rectTransform.anchoredPosition = Vector3.Lerp(_rectTransform.anchoredPosition, new Vector3(_rectTransform.sizeDelta.x, 0), Time.deltaTime * 5);
        }
    }

    public static void Flip()
    {
        _instance._enabled = !_instance._enabled;
    }
    public static void Hide()
    {
        _instance._enabled = false;
    }
}
