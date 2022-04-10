using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformationFacilitiesUpdater : MonoBehaviour
{
    [SerializeField] private GameObject _facilityPrefab;
    private Planet _target;
    private int _wildcards;

    void Update()
    {
        if (Planet.Selected != null && (Planet.Selected != _target || Planet.Selected.GetWildcardSlots() != _wildcards))
        {
            _target = Planet.Selected;
            _wildcards = Planet.Selected.GetWildcardSlots();
            Redraw();
        }
    }

    private void Redraw()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        var r = _target.TransformationFacilities;
        foreach(var facility in r)
        {
            var t = Instantiate(_facilityPrefab, transform);
            t.GetComponentInChildren<WildcardMenu>().SetFacility(facility);
        }
        var slots = _target.GetWildcardSlots();
        var count = 0;
        while (count < slots)
        {
            var t = Instantiate(_facilityPrefab, transform);
            t.GetComponentInChildren<WildcardMenu>().SetID(count);
            count++;
        }
    }
}