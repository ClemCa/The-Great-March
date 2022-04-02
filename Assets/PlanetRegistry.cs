using System;
using System.Collections;
using System.Collections.Generic;
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

    private static PlanetRegistry _instance;

    public static PlanetRegistry Instance { get => _instance; set => _instance = value; }

    public static Ressources[] GetRessources()
    {
        return (Ressources[])Enum.GetValues(typeof(Ressources));
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

    public enum Ressources
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
        AnimalKidnappingCenter
    }

    void Awake()
    {
        _instance = this;
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
