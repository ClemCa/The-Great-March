using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ClemCAddons;

public class CargoChoice : MonoBehaviour
{
    bool once = false;
    [SerializeField] private TMPro.TMP_Dropdown _dropdown;
    [SerializeField] private TMPro.TMP_Text _resourceText;
    [SerializeField] private TMPro.TMP_Text _peopleText;
    [SerializeField] private TMPro.TMP_Text _launchText;
    [SerializeField] private Button _launchButton;
    [SerializeField] private Slider _sliderResource;
    [SerializeField] private Slider _sliderPeople;

    private Registry.Resources _resource;
    private Registry.AdvancedResources _advResource;

    private bool _resourceSelected;
    private bool _advResourceSelected;


    void OnEnable()
    {
        _resourceSelected = false;
        _advResourceSelected = false;
        _dropdown.value = 0;
        _sliderResource.value = 0;
        _sliderPeople.value = 0;
    }

    void Update()
    {
        if (Planet.Selected == null || ShippingSubMenu.Instance == null)
            return;
        var ship = Planet.Selected.Ships[ShippingSubMenu.Instance.Ship];
        if (_resourceSelected)
        {
            _sliderResource.interactable = true;
            switch (ship.Type)
            {
                case Registry.ShipType.Cargo:
                    _sliderResource.maxValue = Planet.Selected.GetResource(_resource).Min(10);
                    break;
                case Registry.ShipType.Fighter:
                    _sliderResource.maxValue = 0;
                    break;
                case Registry.ShipType.Passenger:
                    _sliderResource.maxValue = Planet.Selected.GetResource(_resource).Min(5);
                    break;
            }
            _resourceText.text = _sliderResource.value + " " + Registry.Instance.GetResourceName(_resource);
        } else if (_advResourceSelected)
        {
            _sliderResource.interactable = true;
            switch (ship.Type)
            {
                case Registry.ShipType.Cargo:
                    _sliderResource.maxValue = Planet.Selected.GetResource(_advResource).Min(10);
                    break;
                case Registry.ShipType.Fighter:
                    _sliderResource.maxValue = 0;
                    break;
                case Registry.ShipType.Passenger:
                    _sliderResource.maxValue = Planet.Selected.GetResource(_advResource).Min(5);
                    break;
            }
            _resourceText.text = _sliderResource.value + " " + Registry.Instance.GetResourceName(_advResource);
        }
        else
        {
            _sliderResource.interactable = false;
            _resourceText.text = "Select a resource first";
        }
        switch (ship.Type)
        {
            case Registry.ShipType.Cargo:
                _sliderPeople.maxValue = Planet.Selected.GetPeople().Min(5);
                break;
            case Registry.ShipType.Fighter:
                _sliderResource.maxValue = 0;
                break;
            case Registry.ShipType.Passenger:
                _sliderPeople.maxValue = Planet.Selected.GetPeople().Min(15);
                break;
        }
        _peopleText.text = _sliderPeople.value + " People";
        _launchButton.interactable = CheckLaunchCondition();
        if (once)
            return;
        _dropdown.ClearOptions();
        var resources = Enum.GetValues(typeof(Registry.Resources)).Cast<Registry.Resources>();
        var advresources = Enum.GetValues(typeof(Registry.AdvancedResources)).Cast<Registry.AdvancedResources>();
        var list = new List<TMPro.TMP_Dropdown.OptionData>();
        list.Add(new TMPro.TMP_Dropdown.OptionData("None"));
        foreach (var resource in resources)
        {
            list.Add(new TMPro.TMP_Dropdown.OptionData(Registry.Instance.GetResourceName(resource), Registry.Instance.GetResourceSprite(resource)));
        }
        foreach (var resource in advresources)
        {
            list.Add(new TMPro.TMP_Dropdown.OptionData(Registry.Instance.GetResourceName(resource), Registry.Instance.GetAdvancedResourceSprite(resource)));
        }
        _dropdown.AddOptions(list);
        once = true;
    }

    public void SelectedResource(int resource)
    {
        if(resource == 0)
        {
            _advResourceSelected = false;
            _resourceSelected = false;
            return;
        }
        resource--;
        if (resource < Enum.GetNames(typeof(Registry.Resources)).Count())
        {
            var r = (Registry.Resources) resource;
            _advResourceSelected = false;
            _resourceSelected = true;
            _resource = r;
        }
        else
        {
            var r = (Registry.AdvancedResources)resource - Enum.GetNames(typeof(Registry.Resources)).Count();
            _resourceSelected = false;
            _advResourceSelected = true;
            _advResource = r;
            return;
        }
    }

    public void Launch()
    {
        if(_dropdown.value == 0)
        {
            Planet.Selected.EngageMoveSelectionMode(_sliderPeople.value.Round(), shipID:ShippingSubMenu.Instance.Ship);
            return;
        }
        if (_advResourceSelected)
        {
            Planet.Selected.EngageMoveSelectionMode(_advResource, _sliderResource.value.Round(), countPeople: _sliderPeople.value.Round(), shipID:ShippingSubMenu.Instance.Ship);
        }
        else
        {
            Planet.Selected.EngageMoveSelectionMode(_resource, _sliderResource.value.Round(), countPeople: _sliderPeople.value.Round(), shipID: ShippingSubMenu.Instance.Ship);
        }
    }

    public void Return()
    {
        ShippingSubMenu.ResetMenu();
    }

    private bool CheckLaunchCondition()
    {
        var ship = Planet.Selected.Ships[ShippingSubMenu.Instance.Ship];
        switch (ship.Type)
        {
            case Registry.ShipType.Cargo:
                return _sliderPeople.value > 0;
            case Registry.ShipType.Fighter:
                return _sliderPeople.value > 0;
            case Registry.ShipType.Passenger:
                return _sliderPeople.value > 0;
            default:
                return false;
        }
    }

    public void ChangePeopleValue(float value)
    {
        if (ShippingSubMenu.Instance == null)
            return;
        var ship = Planet.Selected.Ships[ShippingSubMenu.Instance.Ship];
        int resourceCount = 0;
        if (_advResourceSelected)
            resourceCount = Planet.Selected.GetResource(_advResource);
        else if (_resourceSelected)
            resourceCount = Planet.Selected.GetResource(_resource);
        switch (ship.Type)
        {
            case Registry.ShipType.Cargo:
                _sliderResource.maxValue = resourceCount.Min(11 - value.Round());
                break;
            case Registry.ShipType.Fighter:
                break;
            case Registry.ShipType.Passenger:
                _sliderResource.maxValue = resourceCount.Min(15 - value.Round(), 5);
                break;
        }
    }

    public void ChangeResourceValue(float value)
    {
        if (ShippingSubMenu.Instance == null)
            return;
        var ship = Planet.Selected.Ships[ShippingSubMenu.Instance.Ship];
        switch (ship.Type)
        {
            case Registry.ShipType.Cargo:
                _sliderPeople.maxValue = Planet.Selected.GetPeople().Min(11 - value.Round(), 5);
                break;
            case Registry.ShipType.Fighter:
                break;
            case Registry.ShipType.Passenger:
                _sliderPeople.maxValue = Planet.Selected.GetPeople().Min(15 - value.Round());
                break;
        }
    }
}
