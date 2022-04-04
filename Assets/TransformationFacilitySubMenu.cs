using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformationFacilitySubMenu : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Transform _target;
    private bool _enabled;
    private static TransformationFacilitySubMenu _instance;
    private int _slotID;

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
        _rectTransform.position = Vector3.Lerp(_rectTransform.position, _target.position, Time.deltaTime * 5);
    }

    private void ExecuteFlip(Transform target, int slotID)
    {
        if (_slotID != slotID)
            _enabled = true;
        else
            _enabled = !_enabled;
        _slotID = slotID;
        _target = target;
        if (_enabled)
        {
            _rectTransform.position = _target.position;
            ShippingSubMenu.Hide();
            FacilitySubMenu.Hide();
            ResourcesSelectionSubMenu.Hide(null);
        }
    }
    public static void Flip(Transform target, int slotID)
    {
        _instance.ExecuteFlip(target, slotID);
    }
    public static void Hide()
    {
        _instance._enabled = false;
    }
}
