using ClemCAddons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Registry : MonoBehaviour
{
    [SerializeField] private Sprite _wildcardSprite;
    [SerializeField] private GameObject[] _alienPlanets;
    [SerializeField] private GameObject[] _desertPlanets;
    [SerializeField] private GameObject[] _earthLikePlanets;
    [SerializeField] private GameObject[] _frozenPlanets;
    [SerializeField] private GameObject[] _temperatePlanets;
    [SerializeField] private GameObject[] _tundraPlanets;
    [SerializeField] private ResourceSprite[] _resourceSprites;
    [SerializeField] private AdvancedResourceSprite[] _advancedResourceSprites;
    [SerializeField] private FacilityData[] _facilities;
    [SerializeField] private ResourceInfo[] _resourcesNames;
    [SerializeField] private AdvancedResourceInfo[] _advancedResourcesNames;
    [SerializeField] private ShipInfos[] _shipInfos;
    [SerializeField] private Priorities _defaultPriorities;


    private static Registry _instance;

    public static Registry Instance { get => _instance; set => _instance = value; }
    public Priorities DefaultPriorities { get => _defaultPriorities; set => _defaultPriorities = value; }

    public static Resources[] GetResources()
    {
        return (Resources[])Enum.GetValues(typeof(Resources));
    }
    [Serializable]
    public class Priorities
    {
        public List<int> Food;
        public List<int> Fuel;
        public Priorities()
        {
        }
        public Priorities(List<int> food, List<int> fuel)
        {
            Food = food;
            Fuel = fuel;
        }
    }

    [Serializable]
    public class ResourceInfo
    {
        public Resources Resource;
        public string Name;
        public string Description;
        public int Value;
    }

    [Serializable]
    public class AdvancedResourceInfo
    {
        public AdvancedResources Resource;
        public string Name;
        public string Description;
        public int Value;
    }

    [Serializable]
    public class ResourceSprite
    {
        public Resources Resource;
        public Sprite Sprite;
    }

    [Serializable]
    public class AdvancedResourceSprite
    {
        public AdvancedResources Resource;
        public Sprite Sprite;
    }

    public enum FacilityEffectType
    {
        Consume,
        Produce
    }

    [Serializable]
    public class FacilityEffect
    {
        public FacilityEffectType Type;
        public bool Advanced;
        public Resources Resource;
        public AdvancedResources AdvancedResource;
        public int Amount = 1;

        public FacilityEffect() { }

        public FacilityEffect(FacilityEffectType type, Resources resource, int amount = 1)
        {
            Type = type;
            Resource = resource;
            Amount = amount;
        }

        public FacilityEffect(FacilityEffectType type, AdvancedResources resource, int amount = 1)
        {
            Type = type;
            Advanced = true;
            AdvancedResource = resource;
            Amount = amount;
        }
    }

    [Serializable]
    public class FacilityData
    {
        public string Id;
        public string Name;
        public string Description;
        public Sprite Sprite;
        public float Cooldown = 30;
        public bool Wildcard;
        public Resources SlotResource;
        public bool ExtendedTooltip;
        public FacilityEffect[] Effects;

        public FacilityEffect[] GetEffects(FacilityEffectType type)
        {
            if (Effects == null)
                return new FacilityEffect[0];
            var list = new List<FacilityEffect>();
            foreach (var effect in Effects)
                if (effect.Type == type)
                    list.Add(effect);
            return list.ToArray();
        }
    }

    [Serializable]
    public class ShipInfos
    {
        public ShipType Type;
        public Sprite Sprite;
        public GameObject Prefab;
        public int RequiredFuel;
    }


    [Serializable]
    public class Ship
    {
        public ShipType Type;
        public int Fuel;
    }

    public enum ShipType
    {
        Cargo,
        Passenger,
        Fighter,
        Presidential
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

    public enum AdvancedResources
    {
        HighEfficiencyFuel,
        HydrogenBattery,
        PreparedFood
    }

    void Awake()
    {
        _instance = this;
    }

    public int GetRequiredFuel(ShipType ship)
    {
        return Array.Find(_shipInfos, t => t.Type == ship).RequiredFuel;
    }
    

    public Resources? GetResourceFromName(string name)
    {
        return _resourcesNames.First(t => t.Name == name)?.Resource;
    }

    public AdvancedResources? GetAdvancedResourceFromName(string name)
    {
        return _advancedResourcesNames.First(t => t.Name == name)?.Resource;
    }

    public string GetResourceName(Resources resource)
    {
        return _resourcesNames.First(t => t.Resource == resource).Name;
    }

    public string GetResourceDescription(Resources resource)
    {
        return _resourcesNames.First(t => t.Resource == resource).Description;
    }

    public string GetResourceName(AdvancedResources resource)
    {
        return _advancedResourcesNames.First(t => t.Resource == resource).Name;
    }

    public int GetResourceValue(Resources resource)
    {
        return _resourcesNames.First(t => t.Resource == resource).Value;
    }

    public int GetResourceValue(AdvancedResources resource)
    {
        return _advancedResourcesNames.First(t => t.Resource == resource).Value;
    }

    public string GetResourceDescription(AdvancedResources resource)
    {
        return _advancedResourcesNames.First(t => t.Resource == resource).Description;
    }

    public FacilityData GetFacilityInfo(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;
        return _facilities.FirstOrDefault(t => t.Id == id);
    }

    public FacilityData[] GetAllFacilities()
    {
        return _facilities;
    }

    public FacilityData[] GetWildcardFacilities()
    {
        return _facilities.Where(t => t.Wildcard).ToArray();
    }

    public Sprite GetResourceSprite(Resources resource)
    {
        return Array.Find(_resourceSprites, t => t.Resource == resource).Sprite;
    }

    public Sprite GetAdvancedResourceSprite(AdvancedResources resource)
    {
        return Array.Find(_advancedResourceSprites, t => t.Resource == resource).Sprite;
    }

    public Sprite GetFacilitySprite(string id)
    {
        var info = GetFacilityInfo(id);
        return info == null ? null : info.Sprite;
    }

    public Sprite GetShipSprite(ShipType ship)
    {
        return Array.Find(_shipInfos, t => t.Type == ship).Sprite;
    }

    public GameObject GetShipPrefab(ShipType ship)
    {
        return Array.Find(_shipInfos, t => t.Type == ship).Prefab;
    }

    public Sprite GetWildcardSprite()
    {
        return _wildcardSprite;
    }
    public Resources GetAssociatedResource(string id)
    {
        var info = GetFacilityInfo(id);
        return info == null ? default : info.SlotResource;
    }

    public FacilityData[] GetAssociatedFacilities(Resources resource)
    {
        return _facilities.Where(t => !t.Wildcard && t.SlotResource == resource).ToArray();
    }

    public GameObject GetPlanet(PlanetType planetType, int id)
    {
        switch (planetType)
        {
            case PlanetType.Alien:
                return _alienPlanets[id];
            case PlanetType.Desert:
                return _desertPlanets[id];
            case PlanetType.Earth:
                return _earthLikePlanets[id];
            case PlanetType.Frozen:
                return _frozenPlanets[id];
            case PlanetType.Temperate:
                return _temperatePlanets[id];
            case PlanetType.Tundra:
                return _tundraPlanets[id];
            default:
                Debug.LogError("A case is missing, returning starting planet instead");
                return _earthLikePlanets[0];
        }
    }

    public int GetRandomPlanet(PlanetType planetType = PlanetType.None)
    {
        while (planetType == PlanetType.None)
            planetType = (PlanetType)Random.Range(0, Enum.GetNames(typeof(PlanetType)).Length);

        switch (planetType)
        {
            case PlanetType.Alien:
                return Random.Range(0, _alienPlanets.Length - 1);
            case PlanetType.Desert:
                return Random.Range(0, _desertPlanets.Length - 1);
            case PlanetType.Earth:
                return Random.Range(0, _earthLikePlanets.Length - 1);
            case PlanetType.Frozen:
                return Random.Range(0, _frozenPlanets.Length - 1);
            case PlanetType.Temperate:
                return Random.Range(0, _temperatePlanets.Length - 1);
            case PlanetType.Tundra:
                return Random.Range(0, _tundraPlanets.Length - 1);
            default:
                return 0;
        }
    }
}
