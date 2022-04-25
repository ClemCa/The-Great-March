using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ClemCAddons;

public class SlotMenu : MonoBehaviour, IPointerClickHandler
{
    private Registry.Resources _resourceType;

    public Registry.Resources ResourceType { get => _resourceType; }

    public bool HasFacility()
    {
        return Planet.Selected.HasFacility(_resourceType);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Planet.Selected.HasFacility(_resourceType))
        {
            MenuAudioManager.Instance.PlayClick();
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
            GetComponentInChildren<Image>().sprite = Registry.Instance.GetFacilitySprite(Planet.Selected.GetFacility(_resourceType));
        }
        transform.GetChild(1).GetComponent<RectTransform>().sizeDelta =
                    new Vector2(transform.GetChild(1).GetComponent<RectTransform>().sizeDelta.x,
                    GetComponent<RectTransform>().rect.height * Planet.Selected.GetFactoryProgression(_resourceType));
    }
}
