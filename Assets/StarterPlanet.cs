using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterPlanet : MonoBehaviour
{
    [SerializeField] private int starterPeople = 0;
    [SerializeField] private StarterResource[] starterResources = new StarterResource[] { };
    [SerializeField] private PlanetRegistry.Facilities[] starterFacilities = new PlanetRegistry.Facilities[] { };
    [SerializeField] private PlanetRegistry.Resources[] starterSlots = new PlanetRegistry.Resources[] { };

    [Serializable]
    public class StarterResource
    {
        public PlanetRegistry.Resources Ressource;
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
        planet.SetAvailableResources(starterSlots);
    }

}
