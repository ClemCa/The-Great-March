using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesChoicesLiveUpdate : MonoBehaviour
{
    private PlanetRegistry.Resources? _resourceType;

    void Update()
    {
        var slider = transform.parent.parent.GetComponentInChildren<Slider>();
        if (!_resourceType.HasValue)
        {
            slider.minValue = 0;
            slider.maxValue = 0;
        }
        else
        {
            slider.minValue = 0;
            slider.maxValue = Planet.Selected.GetResource(_resourceType.Value);
        }
    }

    public void SetResource(PlanetRegistry.Resources? resource)
    {
        _resourceType = resource;
    }
}
