using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResourceMenu : MonoBehaviour, IPointerClickHandler
{
    private PlanetRegistry.Resources _resourceType;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(_resourceType + " clicked");
    }

    public void SetResource(PlanetRegistry.Resources resourceType)
    {
        _resourceType = resourceType;
    }

    void Start()
    {
        GetComponentInChildren<Image>().sprite = PlanetRegistry.Instance.GetRessourceSprite(_resourceType);
    }

    void Update()
    {
        if (Planet.Selected == null)
            return;
        transform.GetComponentInChildren<TMPro.TMP_Text>().text = Planet.Selected.GetResource(_resourceType).ToString();
    }
}
