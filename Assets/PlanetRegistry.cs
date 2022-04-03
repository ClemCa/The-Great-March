using ClemCAddons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlanetRegistry : MonoBehaviour
{
    [SerializeField] private GameObject[] _alienPlanets;
    [SerializeField] private GameObject[] _desertPlanets;
    [SerializeField] private GameObject[] _earthLikePlanets;
    [SerializeField] private GameObject[] _frozenPlanets;
    [SerializeField] private GameObject[] _temperatePlanets;
    [SerializeField] private GameObject[] _tundraPlanets;
    [SerializeField] private ResourceSprite[] _resourceSprites;
    [SerializeField] private FacilityInfos[] _facilitiesInfo;

    private static PlanetRegistry _instance;

    public static PlanetRegistry Instance { get => _instance; set => _instance = value; }

    public static Resources[] GetResources()
    {
        return (Resources[])Enum.GetValues(typeof(Resources));
    }


    [Serializable]
    public class ResourceSprite
    {
        public Resources Resource;
        public Sprite Sprite;
    }

    [Serializable]
    public class FacilityInfos
    {
        public Facilities Facility;
        public Resources AssociatedResource;
        public Sprite Sprite;
    }

    public enum PlanetType
    {
        None,
        Alien,
        Desert,
        Earth,
        Frozen,
        Temperate,
        Tundra
    }

    public enum SystemType
    {
        None,
        Alien,
        Hot,
        Temperate,
        Cold
    }

    public enum Resources
    {
        Metal,
        Water,
        Food,
        Hydrogen,
        Oil,
        Gas,
        Animals,
        Plants
    }

    public enum Facilities
    {
        HydrogenExtractor,
        Mine,
        OilExtractor,
        GasExtractor,
        WaterPump,
        AnimalKidnappingCenter,
        SuperGrowthGreenhouse
    }

    void Awake()
    {
        _instance = this;
    }

    public Sprite GetResourceSprite(Resources resource)
    {
        return Array.Find(_resourceSprites, t => t.Resource == resource).Sprite;
    }


    public Sprite GetFacilitySprite(Facilities facility)
    {
        return Array.Find(_facilitiesInfo, t => t.Facility == facility).Sprite;
    }

    public Resources GetAssociatedResource(Facilities facility)
    {
        return Array.Find(_facilitiesInfo, t => t.Facility == facility).AssociatedResource;
    }

    public Facilities[] GetAssociatedFacilities(Resources resource)
    {
        return _facilitiesInfo.Where(t => t.AssociatedResource == resource).Select(t => t.Facility).ToArray();
    }


    public GameObject GetRandomPlanet(PlanetType planetType = PlanetType.None)
    {
        while (planetType == PlanetType.None)
            planetType = (PlanetType)Random.Range(0, Enum.GetNames(typeof(PlanetType)).Length);

        switch (planetType)
        {
            case PlanetType.Alien:
                return _alienPlanets[Random.Range(0, _alienPlanets.Length - 1)];
            case PlanetType.Desert:
                return _desertPlanets[Random.Range(0, _desertPlanets.Length - 1)];
            case PlanetType.Earth:
                return _earthLikePlanets[Random.Range(0, _earthLikePlanets.Length - 1)];
            case PlanetType.Frozen:
                return _frozenPlanets[Random.Range(0, _frozenPlanets.Length - 1)];
            case PlanetType.Temperate:
                return _temperatePlanets[Random.Range(0, _temperatePlanets.Length - 1)];
            case PlanetType.Tundra:
                return _tundraPlanets[Random.Range(0, _tundraPlanets.Length - 1)];
            default:
                Debug.LogError("A case is missing, returning starting planet instead");
                return _earthLikePlanets[0];
        }
    }
}
