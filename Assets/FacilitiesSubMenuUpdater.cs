using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacilitiesSubMenuUpdater : MonoBehaviour
{
    [SerializeField] private GameObject _facilityPrefab;
    

    public void SelectResource(PlanetRegistry.Resources resource)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        foreach(var facility in PlanetRegistry.Instance.GetAssociatedFacilities(resource))
        {
            var t = Instantiate(_facilityPrefab, transform);
            t.GetComponentInChildren<FacilityMenu>().SetFacility(facility);
        }
    }
}
