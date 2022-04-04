using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectResourcesButton : MonoBehaviour, IPointerClickHandler
{
    private bool _unlocked;
    private bool _canConfirm;
    private Registry.Resources _resource;
    private Registry.AdvancedResources _advancedResource;
    private bool _isAdvanced;

    public bool Unlocked { get => _unlocked; }
    public Registry.Resources Resource { get => _resource; }
    public Registry.AdvancedResources AdvancedResource { get => _advancedResource; }
    public bool IsAdvanced { get => _isAdvanced; }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_canConfirm)
        {
            transform.GetComponentInChildren<TMPro.TMP_Text>().text = "(Planet)";
            Planet.Selected.EngageMoveSelectionMode(_resource, ShippingSubMenu.Instance.GetValue());
        }
        if (ResourcesSelectionSubMenu.Instance.Enabled)
        {
            ResourcesSelectionSubMenu.Hide();
            return;
        }
        ResourcesSelectionSubMenu.Show(transform, SetResource);
    }

    public void SetResource(Registry.Resources? resource, Registry.AdvancedResources? advancedResource)
    {
        if (resource.HasValue)
        {     
            _resource = resource.Value;
            _isAdvanced = false;
            transform.GetComponentInChildren<Image>().sprite = Registry.Instance.GetResourceSprite(resource.Value);
            _unlocked = true;
            ShippingSubMenu.Instance.Reset(1);
        }
        else if (advancedResource.HasValue)
        {
            _advancedResource = advancedResource.Value;
            _isAdvanced = true;
            transform.GetComponentInChildren<Image>().sprite = Registry.Instance.GetAdvancedResourceSprite(advancedResource.Value);
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
