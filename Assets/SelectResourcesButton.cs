using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectResourcesButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (ResourcesSelectionSubMenu.Instance.Enabled)
        {
            ResourcesSelectionSubMenu.Hide(null);
            return;
        }
        ResourcesSelectionSubMenu.Show(transform, SetResource);
    }

    public void SetResource(PlanetRegistry.Resources? resource)
    {
        transform.GetComponentInChildren<ResourcesChoicesLiveUpdate>().SetResource(resource);
    }
}
