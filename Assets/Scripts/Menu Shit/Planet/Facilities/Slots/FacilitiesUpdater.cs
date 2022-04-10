using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacilitiesUpdater : MonoBehaviour
{
    [SerializeField] private GameObject _facilityPrefab;
    private Planet _target;

    void Update()
    {
        if(Planet.Selected != null && Planet.Selected != _target)
        {
            _target = Planet.Selected;
            Redraw();
        }
    }

    private void Redraw()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        var r = Registry.GetResources();
        foreach (var resource in r)
        {
            if (_target.GetSlot(resource))
            {
                var t = Instantiate(_facilityPrefab, transform);
                t.GetComponentInChildren<SlotMenu>().SetResource(resource);
            }
        }
    }
}
