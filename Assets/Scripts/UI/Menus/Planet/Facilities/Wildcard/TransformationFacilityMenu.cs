using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TransformationFacilityMenu : MonoBehaviour, IPointerClickHandler
{
    private static List<KeyValuePair<Planet, string>> _orderedFacilities = new List<KeyValuePair<Planet, string>>();
    private string _facility;

    public static List<KeyValuePair<Planet, string>> OrderedFacilities { get => _orderedFacilities; }
    public string Facility { get => _facility; }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(_facility))
            return;
        if (_orderedFacilities.FindIndex(t => t.Key == Planet.Selected && t.Value == _facility) != -1)
            return;
        MenuAudioManager.Instance.PlayClick();
        var selected = Planet.Selected;
        OrderHandler.Instance.Queue(
            new OrderHandler.Order(
                OrderHandler.OrderType.Building,
                180,
                0.75f,
                10,
                new OrderHandler.OrderExec(selected, _facility)), selected);
        _orderedFacilities.Add(new KeyValuePair<Planet, string>(Planet.Selected, _facility));
        SubMenu.HideActive();
    }

    public void SetFacility(string facility)
    {
        _facility = facility;
    }

     
    void Update()
    {
        if (Planet.Selected == null)
            return;
        if (string.IsNullOrEmpty(_facility))
            return;
        float f = Planet.Selected.CanBuildFacility(_facility) ? 1 : 0.5f;
        GetComponentInChildren<Image>().sprite = Registry.Instance.GetFacilitySprite(_facility);
        GetComponentInChildren<Image>().color = new Color(f,f,f, 1);
    }
}
