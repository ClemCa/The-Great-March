using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformationFacilitiesSubMenuUpdater : MonoBehaviour
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
        var possibilities = (Registry.TransformationFacilities[])Enum.GetValues(typeof(Registry.TransformationFacilities));
        var r = _target.TransformationFacilities;
        foreach(var possibility in possibilities)
        {
            if (!r.Contains(possibility))
            {
                Instantiate(_facilityPrefab, transform).GetComponentInChildren<TransformationFacilityMenu>().SetFacility(possibility);
            }
        }
    }
}
