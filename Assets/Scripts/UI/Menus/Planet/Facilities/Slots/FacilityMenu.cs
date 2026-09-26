using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FacilityMenu : MonoBehaviour, IPointerClickHandler
{
    private static List<KeyValuePair<Planet, string>> _orderedFacilities = new List<KeyValuePair<Planet, string>>();
    private string _facility;

    public static List<KeyValuePair<Planet, string>> OrderedFacilities { get => _orderedFacilities; set => _orderedFacilities = value; }

    public string Facility { get => _facility; }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_orderedFacilities.FindIndex(t => t.Key == Planet.Selected && t.Value == _facility) != -1)
            return;
        MenuAudioManager.Instance.PlayClick();
        var selected = Planet.Selected;
        OrderHandler.Instance.Queue(
            new OrderHandler.Order(
                OrderHandler.OrderType.Building,
                140,
                1.5f,
                5,
                new OrderHandler.OrderExec(selected, _facility)),
            Planet.Selected);
        _orderedFacilities.Add(new KeyValuePair<Planet, string>(Planet.Selected, _facility));
        SubMenu.HideActive();
    }

    public void SetFacility(string facility)
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
