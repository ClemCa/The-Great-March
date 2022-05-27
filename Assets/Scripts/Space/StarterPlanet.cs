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
    [SerializeField] private StarterAdvancedResource[] starterAdvancedResources = new StarterAdvancedResource[] { };

    [SerializeField] private Registry.Facilities[] starterFacilities = new Registry.Facilities[] { };
    [SerializeField] private Registry.TransformationFacilities[] starterTransformationFacilities = new Registry.TransformationFacilities[] { };
    [SerializeField] private Registry.Resources[] starterSlots = new Registry.Resources[] { };
    [SerializeField] private int roll = 0;
    [SerializeField] private int temperateType = -1;

    [Serializable]
    public class StarterResource
    {
        public Registry.Resources Resource;
        public int Quantity;
    }

    [Serializable]
    public class StarterAdvancedResource
    {
        public Registry.AdvancedResources Resource;
        public int Quantity;
    }

    void Start()
    {
        var planet = GetComponent<Planet>();
        foreach(var starter in starterResources)
        {
            planet.AddResource(starter.Resource, starter.Quantity);
        }
        foreach (var starter in starterAdvancedResources)
        {
            planet.AddResource(starter.Resource, starter.Quantity);
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
        planet.HasPlayer = true;
        planet.TemperateType = temperateType;
        planet.Roll = roll;
    }

}
