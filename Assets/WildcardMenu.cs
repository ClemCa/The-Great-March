using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ClemCAddons;
using UnityEngine.UI;

public class WildcardMenu : MonoBehaviour, IPointerClickHandler
{
    private Registry.TransformationFacilities? _facility;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_facility.HasValue)
        {
            TransformationFacilitySubMenu.Flip(transform.FindParentDeep("PlanetMenu").Find("Inventory Section"), _facility.Value);
        }
    }
    void Start()
    {
        GetComponentInChildren<Image>().sprite = Registry.Instance.GetTransformationFacilitySprite(_facility.Value);
    }

    void Update()
    {
        if (Planet.Selected == null)
            return;
        if (_facility.HasValue)
        {
            // DRAW STUFF
        }
        else
        {
            // HIDE STUFF
        }
    }
    public void SetFacility(Registry.TransformationFacilities facility)
    {
        _facility = facility;
    }
}
