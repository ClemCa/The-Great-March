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

    private void ExecuteFlip(Transform target, PlanetRegistry.Resources resource)
    {
        _target = target;
        _enabled = !_enabled;
        if (_enabled)
        {
            _rectTransform.position = _target.position;
            GetComponentInChildren<FacilitiesSubMenuUpdater>().SelectResource(resource);
        }
    }
    public static void Flip(Transform target, PlanetRegistry.Resources resource)
    {
        _instance.ExecuteFlip(target, resource);
    }
    public static void Hide()
    {
        _instance._enabled = false;
    }
}
