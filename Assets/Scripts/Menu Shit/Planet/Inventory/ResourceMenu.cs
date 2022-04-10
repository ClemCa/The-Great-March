using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ClemCAddons;
using System.Linq;

public class ResourceMenu : MonoBehaviour, IPointerClickHandler
{
    private Registry.Resources _resourceType;
    private Registry.AdvancedResources _advancedResourceType;
    private bool _advancedResources;

    public bool AdvancedResources { get => _advancedResources; }
    public Registry.Resources ResourceType { get => _resourceType; }
    public Registry.AdvancedResources AdvancedResourceType { get => _advancedResourceType;  }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Nothing to do on resource click");
    }

    public void SetResource(Registry.Resources resourceType)
    {
        _resourceType = resourceType;
    }

    public void SetResource(Registry.AdvancedResources resourceType)
    {
        _advancedResourceType = resourceType;
        _advancedResources = true;
    }

    void Start()
    {
        if(_advancedResources)
            GetComponentInChildren<Image>().sprite = Registry.Instance.GetAdvancedResourceSprite(_advancedResourceType);
        else
            GetComponentInChildren<Image>().sprite = Registry.Instance.GetResourceSprite(_resourceType);
    }

    void Update()
    {
        if (Planet.Selected == null)
            return;
        if(_advancedResources)
            transform.GetComponentInChildren<TMPro.TMP_Text>().text = Planet.Selected.GetResource(_advancedResourceType).ToString();
        else
            transform.GetComponentInChildren<TMPro.TMP_Text>().text = Planet.Selected.GetResource(_resourceType).ToString();
    }
}
