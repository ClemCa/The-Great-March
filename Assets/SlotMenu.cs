using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ClemCAddons;

public class SlotMenu : MonoBehaviour, IPointerClickHandler
{
    private Registry.Resources _resourceType;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Planet.Selected.HasFacility(_resourceType))
        {
            FacilitySubMenu.Flip(transform.FindParentDeep("PlanetMenu").Find("Inventory Section"), _resourceType);
        }
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
        if (Planet.Selected.HasFacility(_resourceType))
        {
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(1).GetComponent<Image>().sprite = Registry.Instance.GetFacilitySprite(Planet.Selected.GetFacility(_resourceType));
        }
        else
        {
            transform.GetChild(1).gameObject.SetActive(false);
        }
        transform.GetChild(2).GetComponent<RectTransform>().sizeDelta =
                    new Vector2(transform.GetChild(2).GetComponent<RectTransform>().sizeDelta.x,
                    GetComponent<RectTransform>().sizeDelta.y * Planet.Selected.GetFactoryProgression(_resourceType));
    }
}
