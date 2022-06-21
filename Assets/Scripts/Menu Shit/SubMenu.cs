using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubMenu : MonoBehaviour
{
    [SerializeField] private SubMenuMode _mode;
    private RectTransform _rectTransform;
    private Transform _target;
    private bool _enabled;
    private int _slotID;
    private static SubMenu _activeInstance;
    private static List<SubMenu> _instances = new List<SubMenu>();

    public enum SubMenuMode
    {
        TransformationFacilitySubMenu,
        FacilitySubMenu,
        DeveloperSubMenu,
        CreditsSubMenu
    }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _instances.Add(this);
    }

    void Update()
    {
        if(_mode == SubMenuMode.DeveloperSubMenu || _mode == SubMenuMode.CreditsSubMenu)
        {
            if (!_enabled)
            {
                _rectTransform.anchoredPosition = new Vector3(0, 0);
                return;
            }
            _rectTransform.anchoredPosition = Vector3.Lerp(_rectTransform.anchoredPosition, new Vector3(_rectTransform.sizeDelta.x, 0), Time.deltaTime * 5);
        }
        else
        {
            if (!_enabled)
            {
                _rectTransform.anchoredPosition = new Vector3(1920, 0);
                return;
            }
            if (Planet.Selected == null)
            {
                _enabled = false;
                return;
            }
        }
        _rectTransform.position = Vector3.Lerp(_rectTransform.position, _target.position, Time.deltaTime * 5);
    }

    private void ExecuteFlip(Transform target, int slotID)
    {
        if (_activeInstance != null)
        {
            _activeInstance.Hide();
            return;
        }

        if (_slotID != slotID && slotID != -1)
            _enabled = true;
        else
            _enabled = !_enabled;
        _slotID = slotID;
        _target = target;
        if (_enabled)
        {
            _activeInstance = this;
            _rectTransform.position = _target.position;
        }
    }
    private void ExecuteFlip(Transform target, Registry.Resources resource)
    {
        if (_activeInstance != null)
        {
            _activeInstance.Hide();
            return;
        }

        _enabled = !_enabled;
        

        _target = target;
        if (_enabled)
        {
            GetComponentInChildren<Updater>().SelectResource(resource);
            ShippingSubMenu.Hide();
            ResourcesSelectionSubMenu.Hide();
            _activeInstance = this;
            _rectTransform.position = _target.position;
        }
    }

    private void ExecuteFlip()
    {
        if (_activeInstance != null)
        {
            _activeInstance.Hide();
            return;
        }

        _enabled = !_enabled;

        if (_enabled)
        {
            _activeInstance = this;
        }
    }

    public void Flip()
    {
        ExecuteFlip();
    }

    public void Flip(Transform target, Registry.Resources resource)
    {
        ExecuteFlip(target, resource);
    }

    public void Flip(Transform target, int slotID = -1)
    {
        ExecuteFlip(target, slotID);
    }

    public void Hide()
    {
        _enabled = false;
        _activeInstance = null;
    }

    public static void HideActive()
    {
        if (_activeInstance != null)
            _activeInstance.Hide();
    }

    public static SubMenu GetInstance(SubMenuMode mode)
    {
        return _instances.Find(t => t._mode == mode);
    }


}
