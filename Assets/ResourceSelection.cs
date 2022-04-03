using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResourceSelection : MonoBehaviour, IPointerClickHandler
{
    private PlanetRegistry.Resources _resourceType;

    public void OnPointerClick(PointerEventData eventData)
    {
        ResourcesSelectionSubMenu.Hide(_resourceType);
    }
    public PlanetRegistry.Resources GetResource()
    {
        return _resourceType;
    }
    public void SetResource(PlanetRegistry.Resources resourceType)
    {
        _resourceType = resourceType;
    }

    void Start()
    {
        GetComponentInChildren<Image>().sprite = PlanetRegistry.Instance.GetResourceSprite(_resourceType);
    }

    void Update()
    {
        if (Planet.Selected == null)
            return;
        if(ClemCAddons.Utilities.Timer.MinimumDelay("nospamtext".GetHashCode(),100,true))
            transform.GetComponentInChildren<TMPro.TMP_Text>().text = Planet.Selected.GetResource(_resourceType).ToString();
    }
}
