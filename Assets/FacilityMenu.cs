using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FacilityMenu : MonoBehaviour, IPointerClickHandler
{
    private static List<KeyValuePair<Planet, Registry.Facilities>> _orderedFacilities = new List<KeyValuePair<Planet, Registry.Facilities>>();
    private Registry.Facilities _facility;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (_orderedFacilities.FindIndex(t => t.Key == Planet.Selected && t.Value == _facility) != -1)
            return;
        var selected = Planet.Selected;
        OrderHandler.Instance.Queue(
            new OrderHandler.Order(
                OrderHandler.OrderType.Building,
                140,
                1.5f,
                5,
                () => {
                    _orderedFacilities.Remove(new KeyValuePair<Planet, Registry.Facilities>(selected, _facility));
                    selected.RegisterBuiltFacility(_facility);
                }),
            Planet.Selected);
        _orderedFacilities.Add(new KeyValuePair<Planet, Registry.Facilities>(Planet.Selected, _facility));
        FacilitySubMenu.Hide();
    }

    public void SetFacility(Registry.Facilities facility)
    {
        _facility = facility;
    }

    void Start()
    {
        GetComponentInChildren<Image>().sprite = Registry.Instance.GetFacilitySprite(_facility);
    }

    void Update()
    {
        if (Planet.Selected == null)
            return;
        float f = Planet.Selected.CanBuildFacility(_facility) ? 1 : 0.5f;
        GetComponentInChildren<Image>().color = new Color(f,f,f, 1);
    }
}
