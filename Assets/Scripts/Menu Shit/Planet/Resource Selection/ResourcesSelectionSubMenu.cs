using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System;
using UnityEngine.UI;

public class ResourcesSelectionSubMenu : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Transform _target;
    private bool _enabled;
    private static ResourcesSelectionSubMenu _instance;
    private static ResourceReturn _resourceReturn;

    public static ResourcesSelectionSubMenu Instance { get => _instance; }
    public bool Enabled { get => _enabled; set => _enabled = value; }

    public delegate void ResourceReturn(Registry.Resources? resource, Registry.AdvancedResources? advancedResource);

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

    public static void Hide()
    {
        if (_instance._enabled)
            _resourceReturn.Invoke(null, null);
        _instance._enabled = false;
        _instance.GetComponentInChildren<InventoryUpdater>().Safe = false;
    }

    public static void Hide(Registry.Resources? result)
    {
        if (_instance._enabled)
            _resourceReturn.Invoke(result, null);
        _instance._enabled = false;
        _instance.GetComponentInChildren<InventoryUpdater>().Safe = false;
    }

    public static void Hide(Registry.AdvancedResources? result)
    {
        if (_instance._enabled)
            _resourceReturn.Invoke(null, result);
        _instance._enabled = false;
        _instance.GetComponentInChildren<InventoryUpdater>().Safe = false;

    }

    public static void Show(Transform target, ResourceReturn resourceReturn)
    {
        _instance._enabled = true;
        _instance._target = target;
        _instance._rectTransform.position = target.position;
        _resourceReturn = resourceReturn;
        _instance.GetComponentInChildren<InventoryUpdater>().Safe = true;
    }
}
