using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResourceSelection : MonoBehaviour, IPointerClickHandler
{
    private Registry.Resources _resourceType;
    private Registry.AdvancedResources _advancedResourceType;
    private bool _advancedResources;

    public void OnPointerClick(PointerEventData eventData)
    {
        MenuAudioManager.Instance.PlayClick();
        if(_advancedResources)
            ResourcesSelectionSubMenu.Hide(_advancedResourceType);
        else
            ResourcesSelectionSubMenu.Hide(_resourceType);
        PromptMenu.Hide();
    }
    public Registry.Resources GetResource()
    {
        return _resourceType;
    }
    public bool GetResourceType()
    {
        return _advancedResources;
    }
    public Registry.AdvancedResources GetAdvancedResource()
    {
        return _advancedResourceType;
    }
    public void SetResource(Registry.Resources resourceType)
    {
        _resourceType = resourceType;
        _advancedResources = false;
    }

    public void SetResource(Registry.AdvancedResources resourceType)
    {
        _advancedResourceType = resourceType;
        _advancedResources = true;
    }

    void Start()
    {
        if (_advancedResources)
            GetComponentInChildren<Image>().sprite = Registry.Instance.GetAdvancedResourceSprite(_advancedResourceType);
        else
            GetComponentInChildren<Image>().sprite = Registry.Instance.GetResourceSprite(_resourceType);
    }

    void Update()
    {
        if (Planet.Selected == null)
            return;
        if (_advancedResources)
            transform.GetComponentInChildren<TMPro.TMP_Text>().text = Planet.Selected.GetResource(_advancedResourceType).ToString();
        else
            transform.GetComponentInChildren<TMPro.TMP_Text>().text = Planet.Selected.GetResource(_resourceType).ToString();
    }
}
