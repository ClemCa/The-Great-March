using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ClemCAddons;
using System.Linq;

public class ResourceMenu : MonoBehaviour, IPointerClickHandler
{
    private Registry.Resources _resourceType;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Nothing to do on resource click");
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
        transform.GetComponentInChildren<TMPro.TMP_Text>().text = Planet.Selected.GetResource(_resourceType).ToString();
    }
}
