using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class InventoryUpdater : MonoBehaviour
{
    [SerializeField] private GameObject _itemPrefab;
    void Start()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        var r = PlanetRegistry.GetResources();
        foreach(var resource in r)
        {
            var t = Instantiate(_itemPrefab, transform);
            t.GetComponentInChildren<ResourceMenu>().SetResource(resource);
        }
    }
}
