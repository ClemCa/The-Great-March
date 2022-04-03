using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class InventoryUpdater : MonoBehaviour
{
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private bool _showEmptyResources = true;
    void Awake()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        var r = PlanetRegistry.GetResources();
        foreach(var resource in r)
        {
            var t = Instantiate(_itemPrefab, transform);
            if(t.TryGetComponent<ResourceMenu>(out var res))
                res.SetResource(resource);
            if (t.TryGetComponent<ResourceSelection>(out var result))
                result.SetResource(resource);
        }
    }
    void Update()
    {
        if (_showEmptyResources || Planet.Selected == null)
            return;
        if (ClemCAddons.Utilities.Timer.MinimumDelay("gateUpdate".GetHashCode(), 100, true))
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            var r = PlanetRegistry.GetResources();
            foreach (var resource in r)
            {
                if(Planet.Selected.GetResource(resource) > 0)
                {
                    var t = Instantiate(_itemPrefab, transform);
                    if (t.TryGetComponent<ResourceMenu>(out var res))
                        res.SetResource(resource);
                    if (t.TryGetComponent<ResourceSelection>(out var result))
                        result.SetResource(resource);
                }
            }
        }
    }
}
