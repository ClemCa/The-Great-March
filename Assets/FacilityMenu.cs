using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FacilityMenu : MonoBehaviour, IPointerClickHandler
{
    private PlanetRegistry.Facilities _facility;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(_facility + " clicked");
        // temporary:
        Planet.Selected.RegisterBuiltFacility(_facility);
        FacilitySubMenu.Hide();
        Debug.Log("temporarily creating instantly");
    }

    public void SetFacility(PlanetRegistry.Facilities facility)
    {
        _facility = facility;
    }

    void Start()
    {
        GetComponentInChildren<Image>().sprite = PlanetRegistry.Instance.GetFacilitySprite(_facility);
    }

    void Update()
    {
        if (Planet.Selected == null)
            return;
        float f = Planet.Selected.CanBuildFacility(_facility) ? 1 : 0.5f;
        GetComponentInChildren<Image>().color = new Color(f,f,f, 1);
    }
}
