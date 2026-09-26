using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacilitySubMenu : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Transform _target;
    private bool _enabled;
    private static FacilitySubMenu _instance;
    private Registry.Resources? _resource;

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

    private void ExecuteFlip(Transform target, Registry.Resources resource)
    {
        if (_resource.HasValue && _resource.Value != resource)
            _enabled = true;
        else
            _enabled = !_enabled;
        _target = target;
        _resource = resource;
        if (_enabled)
        {
            _rectTransform.position = _target.position;
            GetComponentInChildren<Updater>().SelectResource(resource);
            ShippingSubMenu.Hide();
            SubMenu.HideActive();
            ResourcesSelectionSubMenu.Hide();
        }
    }
    public static void Flip(Transform target, Registry.Resources resource)
    {
        _instance.ExecuteFlip(target, resource);
    }
    public static void Hide()
    {
        _instance._enabled = false;
    }
}
