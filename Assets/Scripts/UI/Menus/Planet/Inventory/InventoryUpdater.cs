using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System;

public class InventoryUpdater : MonoBehaviour
{
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private bool _showEmptyResources = true;
    private bool _safe;

    public bool Safe { get => _safe; set => _safe = value; }

    void Awake()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        var r = Registry.GetResources();
        
        foreach (var resource in r)
        {
            var t = Instantiate(_itemPrefab, transform);
            if(t.TryGetComponent<ResourceMenu>(out var res))
                res.SetResource(resource);
            if (t.TryGetComponent<ResourceSelection>(out var result))
                result.SetResource(resource);
        }
        if (_showEmptyResources)
        {
            var r2 = Enum.GetNames(typeof(Registry.AdvancedResources)).Length;
            for(int i = 0; i < r2; i++)
            {
                var t = Instantiate(_itemPrefab, transform);
                if (t.TryGetComponent<ResourceMenu>(out var res))
                    res.SetResource((Registry.AdvancedResources)i);
                if (t.TryGetComponent<ResourceSelection>(out var result))
                    result.SetResource((Registry.AdvancedResources)i);
            }
        }
    }
    void Update()
    {
        if (_showEmptyResources || Planet.Selected == null)
            return;

        if (_safe.OnceIfTrueGate("oneAndNoMore".GetHashCode()))
            ClearUpdate();
        if (_safe)
            return;
        if (ClemCAddons.Utilities.Timer.MinimumDelay("updateDelayInv".GetHashCode(), 1000, true))
            ClearUpdate();
    }

    private void ClearUpdate()
    {
        Transform[] children = transform.GetChildrenWithComponent(typeof(ResourceSelection));
        var r = Registry.GetResources();
        var r2 = Planet.Selected.AdvancedResources;
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        foreach (var resource in r)
        {
            if (Planet.Selected.GetResource(resource) > 0)
            {
                var t = Instantiate(_itemPrefab, transform);
                if (t.TryGetComponent<ResourceMenu>(out var res))
                    res.SetResource(resource);
                if (t.TryGetComponent<ResourceSelection>(out var result))
                    result.SetResource(resource);
            }
        }
        foreach(var advancedResource in r2)
        {
            if (advancedResource.Value > 0)
            {
                var t = Instantiate(_itemPrefab, transform);
                if (t.TryGetComponent<ResourceMenu>(out var res))
                    res.SetResource(advancedResource.Key);
                if (t.TryGetComponent<ResourceSelection>(out var result))
                    result.SetResource(advancedResource.Key);
            }
        }
    }
}
