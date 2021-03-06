using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TransformationFacilityMenu : MonoBehaviour, IPointerClickHandler
{
    private static List<KeyValuePair<Planet, Registry.TransformationFacilities>> _orderedFacilities = new List<KeyValuePair<Planet, Registry.TransformationFacilities>>();
    private Registry.TransformationFacilities? _facility;

    public static List<KeyValuePair<Planet, Registry.TransformationFacilities>> OrderedFacilities { get => _orderedFacilities; }
    public Registry.TransformationFacilities? Facility { get => _facility; }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_facility.HasValue)
            return;
        if (_orderedFacilities.FindIndex(t => t.Key == Planet.Selected && t.Value == _facility.Value) != -1)
            return;
        MenuAudioManager.Instance.PlayClick();
        var selected = Planet.Selected;
        OrderHandler.Instance.Queue(
            new OrderHandler.Order(
                OrderHandler.OrderType.Building,
                180,
                0.75f,
                10,
                new OrderHandler.OrderExec(selected, _facility.Value)), selected);
        _orderedFacilities.Add(new KeyValuePair<Planet, Registry.TransformationFacilities>(Planet.Selected, _facility.Value));
        SubMenu.HideActive();
    }

    public void SetFacility(Registry.TransformationFacilities facility)
    {
        _facility = facility;
    }

     
    void Update()
    {
        if (Planet.Selected == null)
            return;
        if (!_facility.HasValue)
            return;
        float f = Planet.Selected.CanBuildFacility(_facility.Value) ? 1 : 0.5f;
        GetComponentInChildren<Image>().sprite = Registry.Instance.GetTransformationFacilitySprite(_facility.Value);
        GetComponentInChildren<Image>().color = new Color(f,f,f, 1);
    }
}
