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
    private PlanetRegistry.Resources? _ressource;

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
        if (_ressource.HasValue && _ressource.Value != resource)
            _enabled = true;
        else
            _enabled = !_enabled;
        _target = target;
        _ressource = resource;
        if (_enabled)
        {
            _rectTransform.position = _target.position;
            GetComponentInChildren<FacilitiesSubMenuUpdater>().SelectResource(resource);
            ShippingSubMenu.Hide();
            ResourcesSelectionSubMenu.Hide(null);
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
