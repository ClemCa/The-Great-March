using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResourceSelection : MonoBehaviour, IPointerClickHandler
{
    private Registry.Resources _resourceType;

    public void OnPointerClick(PointerEventData eventData)
    {
        ResourcesSelectionSubMenu.Hide(_resourceType);
    }
    public Registry.Resources GetResource()
    {
        return _resourceType;
    }
    public void SetResource(Registry.Resources resourceType)
    {
        _resourceType = resourceType;
    }

    void Start()
    {
        GetComponentInChildren<Image>().sprite = Registry.Instance.GetResourceSprite(_resourceType);
    }

    void Update()
    {
        if (Planet.Selected == null)
            return;
        if(ClemCAddons.Utilities.Timer.MinimumDelay("nospamtext".GetHashCode(),100,true))
            transform.GetComponentInChildren<TMPro.TMP_Text>().text = Planet.Selected.GetResource(_resourceType).ToString();
    }
}
