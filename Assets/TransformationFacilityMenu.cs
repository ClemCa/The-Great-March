using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TransformationFacilityMenu : MonoBehaviour, IPointerClickHandler
{
    private Registry.TransformationFacilities _facility;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(_facility + " clicked");
        OrderHandler.Instance.Queue(
            new OrderHandler.Order(
                OrderHandler.OrderType.Building,
                30,
                5,
                6,
                () => {
                    Planet.Selected.RegisterBuiltFacility(_facility);
                }), Planet.Selected);

        FacilitySubMenu.Hide();
    }

    public void SetFacility(Registry.TransformationFacilities facility)
    {
        _facility = facility;
    }

    void Start()
    {
        GetComponentInChildren<Image>().sprite = Registry.Instance.GetTransformationFacilitySprite(_facility);
    }

    void Update()
    {
        if (Planet.Selected == null)
            return;
        float f = Planet.Selected.CanBuildFacility(_facility) ? 1 : 0.5f;
        GetComponentInChildren<Image>().color = new Color(f,f,f, 1);
    }
}
