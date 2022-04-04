using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterPlanet : MonoBehaviour
{
    [SerializeField] private string starterName = "";
    [SerializeField] private int starterPeople = 0;
    [SerializeField] private int starterWildcards = 0;
    [SerializeField] private StarterResource[] starterResources = new StarterResource[] { };
    [SerializeField] private Registry.Facilities[] starterFacilities = new Registry.Facilities[] { };
    [SerializeField] private Registry.TransformationFacilities[] starterTransformationFacilities = new Registry.TransformationFacilities[] { };
    [SerializeField] private Registry.Resources[] starterSlots = new Registry.Resources[] { };

    [Serializable]
    public class StarterResource
    {
        public Registry.Resources Ressource;
        public int Quantity;
    }


    void Start()
    {
        var planet = GetComponent<Planet>();
        foreach(var starter in starterResources)
        {
            planet.AddResource(starter.Ressource, starter.Quantity);
        }
        planet.AddPeople(starterPeople);
        foreach(var facility in starterFacilities)
        {
            planet.RegisterBuiltFacility(facility);
        }
        foreach (var facility in starterTransformationFacilities)
        {
            planet.RegisterBuiltFacility(facility);
        }
        planet.SetAvailableResources(starterSlots);
        planet.SetName(starterName);
        planet.SetWildcardSlots(starterWildcards);
    }

}
