using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Updater : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private UpdaterMode _mode;
    private Planet _target;
    private int _wildcards;
    private int _building;

    public enum UpdaterMode
    {
        Facility,
        SubFacility,
        TransformationFacility,
        SubTransformationFacility
    }

    void Update()
    {
        if (Planet.Selected != null
            && (Planet.Selected != _target
            || ((_mode == UpdaterMode.TransformationFacility || _mode == UpdaterMode.SubTransformationFacility)
                && Planet.Selected.GetWildcardSlots() != _wildcards)
            || (_mode == UpdaterMode.SubTransformationFacility && _building != TransformationFacilityMenu.OrderedFacilities.Count))
            )
        {
            _target = Planet.Selected;
            if(_mode == UpdaterMode.TransformationFacility || _mode == UpdaterMode.SubTransformationFacility)
                _wildcards = Planet.Selected.GetWildcardSlots();
            if(_mode == UpdaterMode.SubTransformationFacility)
                _building = TransformationFacilityMenu.OrderedFacilities.Count;
            Redraw();
        }
    }

    private void Redraw()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        switch (_mode)
        {
            case UpdaterMode.Facility:
                { // isolate context from other cases, so can use the same variable names
                    var r = Registry.GetResources();
                    foreach (var resource in r)
                    {
                        if (_target.GetSlot(resource))
                        {
                            var t = Instantiate(_prefab, transform);
                            t.GetComponentInChildren<SlotMenu>().SetResource(resource);
                        }
                    }
                }
                break;
            case UpdaterMode.TransformationFacility:
                {
                    var r = _target.TransformationFacilities;
                    foreach (var facility in r)
                    {
                        var t = Instantiate(_prefab, transform);
                        t.GetComponentInChildren<WildcardMenu>().SetFacility(facility);
                    }
                    var slots = _target.GetWildcardSlots();
                    var count = 0;
                    while (count < slots)
                    {
                        var t = Instantiate(_prefab, transform);
                        t.GetComponentInChildren<WildcardMenu>().SetID(count);
                        count++;
                    }
                }
                break;
            case UpdaterMode.SubTransformationFacility:
                {
                    for (int i = transform.childCount - 1; i >= 0; i--)
                        Destroy(transform.GetChild(i).gameObject);
                    var possibilities = (Registry.TransformationFacilities[])Enum.GetValues(typeof(Registry.TransformationFacilities));
                    var r = _target.TransformationFacilities;
                    foreach (var possibility in possibilities)
                    {
                        if (!r.Contains(possibility) && TransformationFacilityMenu.OrderedFacilities.FindIndex(t => t.Value == possibility) == -1)
                        {
                            Instantiate(_prefab, transform).GetComponentInChildren<TransformationFacilityMenu>().SetFacility(possibility);
                        }
                    }
                }
                break;
        }
    }

    public void SelectResource(Registry.Resources resource)
    {
        if (_mode != UpdaterMode.SubFacility)
            return;
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        foreach (var facility in Registry.Instance.GetAssociatedFacilities(resource))
        {
            var t = Instantiate(_prefab, transform);
            t.GetComponentInChildren<FacilityMenu>().SetFacility(facility);
        }
    }
}
