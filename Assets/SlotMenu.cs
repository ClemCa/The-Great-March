using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotMenu : MonoBehaviour, IPointerClickHandler
{
    private PlanetRegistry.Resources _resourceType;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Planet.Selected.HasFacility(_resourceType))
        {
            FacilitySubMenu.Flip(transform, _resourceType);
        }
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
        if (Planet.Selected.HasFacility(_resourceType))
        {
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(1).GetComponent<Image>().sprite = PlanetRegistry.Instance.GetFacilitySprite(Planet.Selected.GetFacility(_resourceType));
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
