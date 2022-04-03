using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformationFacilitiesUpdater : MonoBehaviour
{
    [SerializeField] private GameObject _facilityPrefab;
    private Planet _target;

    void Update()
    {
        if (Planet.Selected != null && Planet.Selected != _target)
        {
            _target = Planet.Selected;
            Redraw();
        }
    }

    private void Redraw()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        var r = _target.TransformationFacilities;
        foreach (var resource in r)
        {
            var t = Instantiate(_facilityPrefab, transform);
            t.GetComponentInChildren<WildcardMenu>().SetFacility(resource);
        }
        var left = _target.GetWildcardSlots();
        while (left > 0)
        {
            var t = Instantiate(_facilityPrefab, transform);
            t.GetComponentInChildren<WildcardMenu>();
        }
    }
}