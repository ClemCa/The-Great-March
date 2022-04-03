using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectResourcesButton : MonoBehaviour, IPointerClickHandler
{
    private bool _unlocked;
    private bool _canConfirm;
    private PlanetRegistry.Resources _resource;

    public bool Unlocked { get => _unlocked; }
    public PlanetRegistry.Resources Resource { get => _resource; }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_canConfirm)
        {
            transform.GetComponentInChildren<TMPro.TMP_Text>().text = "(Planet)";
            Planet.Selected.EngageMoveSelectionMode(_resource, ShippingSubMenu.Instance.GetValue());
        }
        if (ResourcesSelectionSubMenu.Instance.Enabled)
        {
            ResourcesSelectionSubMenu.Hide(null);
            return;
        }
        ResourcesSelectionSubMenu.Show(transform, SetResource);
    }

    public void SetResource(PlanetRegistry.Resources? resource)
    {
        if (resource.HasValue)
        {     
            _resource = resource.Value;
            transform.GetComponentInChildren<Image>().sprite = PlanetRegistry.Instance.GetResourceSprite(resource.Value);
            _unlocked = true;
            ShippingSubMenu.Instance.Reset(1);
        }
        else
        {
            ShippingSubMenu.Instance.Reset(0);
            _unlocked = false;
            transform.GetComponentInChildren<TMPro.TMP_Text>().text = "(Select)";
        }
    }

    void Update()
    {
        _canConfirm = ShippingSubMenu.Instance.GetValue() != 0;
        if (_canConfirm)
        {
            transform.GetComponentInChildren<TMPro.TMP_Text>().text = "(Planet)";
        }
        else
        {
            transform.GetComponentInChildren<TMPro.TMP_Text>().text = "(Select)";
        }
    }
}
