using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ClemCAddons;
using System.Linq;

public class ResourceMenu : MonoBehaviour, IPointerClickHandler
{
    private PlanetRegistry.Resources _resourceType;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(_resourceType + " clicked");
        var r = Instantiate(PlanetRegistry.Instance.CargoPrefab);
        var planets = FindObjectsOfType<Planet>().ToList();
        planets.Remove(Planet.Selected);
        var destination = planets[Random.Range(0, planets.Count)];
        r.GetComponent<Cargo>().Initialize(Planet.Selected, destination, 1, _resourceType);
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
        transform.GetComponentInChildren<TMPro.TMP_Text>().text = Planet.Selected.GetResource(_resourceType).ToString();
    }
}
