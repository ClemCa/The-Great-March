using cakeslice;
using ClemCAddons;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Planet : MonoBehaviour
{
    #region localStorage
    private Dictionary<Registry.Resources, int> _resources = new Dictionary<Registry.Resources, int>();
    private Dictionary<Registry.AdvancedResources, int> _advancedResources = new Dictionary<Registry.AdvancedResources, int>();
    private int _people = 0;
    private Registry.Resources[] _availableResources;
    private List<Registry.Facilities> _facilities = new List<Registry.Facilities>();
    private List<float> _facilitiesProgression = new List<float>();
    private List<Registry.TransformationFacilities> _transformationFacilities = new List<Registry.TransformationFacilities>();
    private List<float> _transformationFacilitiesProgression = new List<float>();
    private string _name;
    private int _availableWildcards;
    private int _peopleFed = 0;
    private List<int> _peopleOverTime = new List<int>(new int[60]);
    private List<int> _resourcesOverTime = new List<int>(new int[60]);
    private List<int> _facilitiesOverTime = new List<int>(new int[60]);
    private float _consumption = 0;
    private static Planet selected;
    private static int moveSelection;
    private static bool moveSelectionAdv;
    private static int moveSelectionCount;
    private static int moveSelectionCountPeople;
    private static int moveSelectionShipID;
    private static Planet moveSelectionOrigin;
    private static bool moveSelectionType;
    private bool _hasPlayer = false;
    private static Planet _leaderPlanet;
    private int _roll = -1;
    private int _temperateType = -1;
    private List<Registry.Ship> _ships = new List<Registry.Ship>();
    private List<KeyValuePair<int, Registry.Ship>> _reservedShips = new List<KeyValuePair<int, Registry.Ship>>();
    private int _shipIDs = 0;
    private static Registry.Priorities globalPriorities = new Registry.Priorities();
    private Registry.Priorities localPriorities = new Registry.Priorities();
    #endregion localStorage
    #region Accessibility
    public Dictionary<Registry.Resources, int> Resources { get => _resources; set => _resources = value; }
    public Dictionary<Registry.AdvancedResources, int> AdvancedResources { get => _advancedResources; set => _advancedResources = value; }

    public List<Registry.Facilities> Facilities { get => _facilities; set => _facilities = value; }
    public List<Registry.TransformationFacilities> TransformationFacilities { get => _transformationFacilities; set => _transformationFacilities = value; }

    public static Planet Selected { get => selected;}

    public Registry.Resources[] AvailableResources { get => _availableResources;}

    public string Name { get => _name; }
    public bool HasPlayer { get => _hasPlayer; set => _hasPlayer = value; }
    public static Planet LeaderPlanet { get => _leaderPlanet;}
    public List<int> PeopleOverTime { get => _peopleOverTime; }
    public List<int> ResourcesOverTime { get => _resourcesOverTime; }
    public List<int> FacilitiesOverTime { get => _facilitiesOverTime; }
    public int TemperateType { get => _temperateType; set => _temperateType = value; }
    public int Roll { get => _roll; set => _roll = value; }
    public List<Registry.Ship> Ships { get => _ships; set => _ships = value; }
    public List<KeyValuePair<int, Registry.Ship>> ReservedShips { get => _reservedShips; set => _reservedShips = value; }
    public int ShipIDs { get => _shipIDs; set => _shipIDs = value; }
    

    public static void Select(Planet planet)
    {
        selected = planet;
    }

    public static void Unselect()
    {
        selected = null;
    }

    public static List<int> GetGlobalPriorities(int type)
    {
        switch (type)
        {
            case 0:
                if (globalPriorities.Food == null)
                    return Registry.Instance.DefaultPriorities.Food;
                return globalPriorities.Food;
            case 1:
                if (globalPriorities.Fuel == null)
                    return Registry.Instance.DefaultPriorities.Fuel;
                return globalPriorities.Fuel;
            default:
                if (globalPriorities.Food == null)
                    return Registry.Instance.DefaultPriorities.Food;
                return globalPriorities.Food;
        }
    }

    public List<int> GetLocalPriorities(int type)
    {
        switch (type)
        {
            case 0:
                if (localPriorities.Food == null)
                    return GetGlobalPriorities(type);
                return localPriorities.Food;
            case 1:
                if (localPriorities.Fuel == null)
                    return GetGlobalPriorities(type);
                return localPriorities.Fuel;
            default:
                if (globalPriorities.Food == null)
                    return GetGlobalPriorities(type);
                return localPriorities.Food;
        }
    }
    public static void SetGlobalPriorities(int type, List<int> priorities)
    {
        switch (type)
        {
            case 0:
                globalPriorities.Food = new List<int>(priorities);
                return;
            case 1:
                globalPriorities.Fuel = new List<int>(priorities);
                return;
            default:
                globalPriorities.Food = new List<int>(priorities);
                return;
        }
    }
    public void SetLocalPriorities(int type, List<int> priorities)
    {
        switch (type)
        {
            case 0:
                localPriorities.Food = new List<int>(priorities);
                return;
            case 1:
                localPriorities.Fuel = new List<int>(priorities);
                return;
            default:
                localPriorities.Food = new List<int>(priorities);
                return;
        }
    }
    public Saver.PlanetStorage GetPlanetStorage()
    {
        var storage = new Saver.PlanetStorage();
        storage._resources = JsonConvert.SerializeObject(_resources);
        storage._advancedResources = JsonConvert.SerializeObject(_advancedResources);
        storage._people = _people;
        storage._availableResources = _availableResources;
        storage._facilities = _facilities;
        storage._facilitiesProgression = _facilitiesProgression;
        storage._transformationFacilities = _transformationFacilities;
        storage._transformationFacilitiesProgression = _transformationFacilitiesProgression;
        storage._name = _name;
        storage._availableWildcards = _availableWildcards;
        storage._peopleFed = _peopleFed;
        storage._peopleOverTime = _peopleOverTime;
        storage._resourcesOverTime = _resourcesOverTime;
        storage._facilitiesOverTime = _facilitiesOverTime;
        storage._consumption = _consumption;
        storage._hasPlayer = _hasPlayer;
        storage._ships = _ships;
        storage._reservedShips = _reservedShips;
        storage._shipIDs = _shipIDs;
        storage._localPriorities = localPriorities;
        return storage;
    }

    public void SetPlanetStorage(Saver.PlanetStorage planetStorage)
    {
        _resources = JsonConvert.DeserializeObject<Dictionary<Registry.Resources, int>>(planetStorage._resources);
        _advancedResources = JsonConvert.DeserializeObject<Dictionary<Registry.AdvancedResources, int>>(planetStorage._advancedResources);
        _people = planetStorage._people;
        _availableResources = planetStorage._availableResources;
        _facilities = planetStorage._facilities;
        _facilitiesProgression = planetStorage._facilitiesProgression;
        _transformationFacilities = planetStorage._transformationFacilities;
        _transformationFacilitiesProgression = planetStorage._transformationFacilitiesProgression;
        _name = planetStorage._name;
        _availableWildcards = planetStorage._availableWildcards;
        _peopleFed = planetStorage._peopleFed;
        _peopleOverTime = planetStorage._peopleOverTime;
        _resourcesOverTime = planetStorage._resourcesOverTime;
        _facilitiesOverTime = planetStorage._facilitiesOverTime;
        _consumption = planetStorage._consumption;
        _hasPlayer = planetStorage._hasPlayer;
        _ships = planetStorage._ships;
        _reservedShips = planetStorage._reservedShips;
        _shipIDs = planetStorage._shipIDs;
        localPriorities = planetStorage._localPriorities;
    }

    public void SetWildcardSlots(int slots)
    {
        _availableWildcards = slots;
    }

    public void SetName(string name)
    {
        _name = name;
    }

    public bool GetSlot(Registry.Resources resource)
    {
        return _availableResources.FindIndex(resource) != -1;
    }

    public int GetWildcardSlots()
    {
        return _availableWildcards;
    }

    public int GetAvailableFuel()
    {
        int gas = GetResource(Registry.Resources.Gas);
        int oil = GetResource(Registry.Resources.Oil);
        int hydrogen = GetResource(Registry.Resources.Hydrogen);
        int highefficiencyfuel = GetResource(Registry.AdvancedResources.HighEfficiencyFuel);
        int hydrogenbattery = GetResource(Registry.AdvancedResources.HydrogenBattery);
        return gas + oil + hydrogen + highefficiencyfuel * 10 + hydrogenbattery * 10;
    }

    public int GetResource(Registry.Resources resoure)
    {
        return _resources[resoure];
    }

    public int GetResource(Registry.AdvancedResources resoure)
    {
        return _advancedResources[resoure];
    }

    public void AddResource(Registry.AdvancedResources resource, int count = 1)
    {
        Scoring.advancedResourcesUnits += count;
        _advancedResources[resource] += count;
    }

    public int[] SimulateFuel(int count)
    {
        var list = GetLocalPriorities(1);
        int gas = GetResource(Registry.Resources.Gas);
        int oil = GetResource(Registry.Resources.Oil);
        int hydrogen = GetResource(Registry.Resources.Hydrogen);
        int highefficiencyfuel = GetResource(Registry.AdvancedResources.HighEfficiencyFuel);
        int hydrogenbattery = GetResource(Registry.AdvancedResources.HydrogenBattery);

        int consumedGas = 0, consumedOil = 0, consumedHydrogen = 0, consumedHighFuel = 0, consumedHydrogenBat = 0;

        for (int i = 0; i < list.Count; i++)
        {
            dynamic resource;
            if (list[i] < Enum.GetNames(typeof(Registry.Resources)).Length)
                resource = (Registry.Resources)list[i];
            else
                resource = (Registry.AdvancedResources)list[i];
            var value = Registry.Instance.GetResourceValue(resource);
            while (count > 0)
            {
                switch (resource)
                {
                    case Registry.AdvancedResources.HydrogenBattery:

                        if (hydrogenbattery > 0)
                        {
                            count -= value;
                            hydrogenbattery--;
                            consumedHydrogenBat++;
                            continue;
                        }
                        break;
                    case Registry.AdvancedResources.HighEfficiencyFuel:
                        if (highefficiencyfuel > 0)
                        {
                            count -= value;
                            highefficiencyfuel--;
                            consumedHighFuel++;
                            continue;
                        }
                        break;
                    case Registry.Resources.Hydrogen:
                        if (hydrogen > 0)
                        {
                            count -= value;
                            hydrogen--;
                            consumedHydrogen++;
                            continue;
                        }
                        break;
                    case Registry.Resources.Oil:
                        if (oil > 0)
                        {
                            count -= value;
                            hydrogen--;
                            consumedOil++;
                            continue;
                        }
                        break;
                    case Registry.Resources.Gas:
                        if (gas > 0)
                        {
                            count -= value;
                            gas--;
                            consumedGas++;
                            continue;
                        }
                        break;
                }
                break;
            }
        }

        return new int[] { consumedGas, consumedOil, consumedHydrogen, consumedHighFuel, consumedHydrogenBat };
    }

    public void ConsumeFuel(int count)
    {
        var list = GetLocalPriorities(1);
        int gas = GetResource(Registry.Resources.Gas);
        int oil = GetResource(Registry.Resources.Oil);
        int hydrogen = GetResource(Registry.Resources.Hydrogen);
        int highefficiencyfuel = GetResource(Registry.AdvancedResources.HighEfficiencyFuel);
        int hydrogenbattery = GetResource(Registry.AdvancedResources.HydrogenBattery);
        for (int i = 0; i < list.Count; i++)
        {
            dynamic resource;
            if (list[i] < Enum.GetNames(typeof(Registry.Resources)).Length)
                resource = (Registry.Resources)list[i];
            else
                resource = (Registry.AdvancedResources)list[i];
            var value = Registry.Instance.GetResourceValue(resource);
            while (count > 0)
            {
                switch (resource)
                {
                    case Registry.AdvancedResources.HydrogenBattery:

                        if (hydrogenbattery > 0)
                        {
                            count -= value;
                            hydrogenbattery--;
                            TakeResource(Registry.AdvancedResources.HydrogenBattery);
                            continue;
                        }
                        break;
                    case Registry.AdvancedResources.HighEfficiencyFuel:
                        if (highefficiencyfuel > 0)
                        {
                            count -= value;
                            highefficiencyfuel--;
                            TakeResource(Registry.AdvancedResources.HighEfficiencyFuel);
                            continue;
                        }
                        break;
                    case Registry.Resources.Hydrogen:
                        if (hydrogen > 0)
                        {
                            count -= value;
                            hydrogen--;
                            TakeResource(Registry.Resources.Hydrogen);
                            continue;
                        }
                        break;
                    case Registry.Resources.Oil:
                        if (oil > 0)
                        {
                            count -= value;
                            hydrogen--;
                            TakeResource(Registry.Resources.Oil);
                            continue;
                        }
                        break;
                    case Registry.Resources.Gas:
                        if (gas > 0)
                        {
                            count -= value;
                            gas--;
                            TakeResource(Registry.Resources.Gas);
                            continue;
                        }
                        break;
                }
                break;
            }
        }
    }

    public void TakeResource(Registry.AdvancedResources resource, int count = 1)
    {
        _advancedResources[resource] -= count;
    }

    public void AddResource(Registry.Resources resource, int count = 1)
    {
        Scoring.naturalResourcesUnits += count;
        _resources[resource] += count;
    }

    public void TakeResource(Registry.Resources resource, int count = 1)
    {
        _resources[resource] -= count;
    }

    public int GetPeople()
    {
        return _people;
    }

    public void AddPeople(int count)
    {
        _people += count;
    }

    public void TakePeople(int count)
    {
        _people -= count;
    }

    public void RegisterBuiltFacility(Registry.Facilities facility)
    {
        Scoring.facilitiesCount++;
        _facilities.Add(facility);
        _facilitiesProgression.Add(0);
    }


    public void RegisterBuiltFacility(Registry.TransformationFacilities facility)
    {
        _availableWildcards--;
        Scoring.transformativeFacilitiesCount++;
        _transformationFacilities.Add(facility);
        _transformationFacilitiesProgression.Add(0);
    }

    public bool IsFacilityBuilt(Registry.Facilities facility)
    {
        return _facilities.FindIndex(t => t == facility) != -1;
    }

    public bool CanBuildFacility(Registry.Facilities facility)
    {
        return _availableResources.FindIndex(Registry.Instance.GetAssociatedResource(facility)) != -1;
    }


    public bool CanBuildFacility(Registry.TransformationFacilities facility)
    {
        return _availableWildcards > 0;
    }


    public bool HasFacility(Registry.Resources resource)
    {
        var r = _availableResources.FindIndex(resource);
        var t = Registry.Instance.GetAssociatedFacilities(resource);
        foreach (var facility in t)
        {
            var i = _facilities.FindIndex(t => t == facility);
            if (i != -1)
                return true;
        }
        return false;
    }
    public bool HasFacility(Registry.TransformationFacilities facility)
    {
        return _transformationFacilities.FindIndex(t => t == facility) != -1;
    }

    public Registry.Facilities GetFacility(Registry.Resources resource)
    {
        var r = _availableResources.FindIndex(resource);
        var t = Registry.Instance.GetAssociatedFacilities(resource);
        foreach (var facility in t)
        {
            var i = _facilities.FindIndex(t => t == facility);
            if (i != -1)
                return _facilities[i];
        }
        return _facilities[0];
    }

    public float GetFactoryProgression(Registry.Resources resource)
    {
        var r = _availableResources.FindIndex(resource);
        if (r == -1)
            return 0;
        var t = Registry.Instance.GetAssociatedFacilities(resource);
        int index = -1;
        foreach(var facility in t)
        {
            var i = _facilities.FindIndex(t => t == facility);
            if (i != -1)
                index = i;
        }
        if (index == -1)
            return 0;
        var info = Registry.Instance.GetFacilityInfo(_facilities[index]);
        return _facilitiesProgression[index] / info.Cooldown;
    }

    public float GetFactoryProgression(Registry.Facilities facility)
    {
        var index = _facilities.FindIndex(t => t == facility);
        if (index == -1)
            return 0;
        var info = Registry.Instance.GetFacilityInfo(_facilities[index]);
        return _facilitiesProgression[index] / info.Cooldown;
    }


    public float GetFactoryProgression(Registry.TransformationFacilities facility)
    {
        var index = _transformationFacilities.FindIndex(t => t == facility);
        if (index == -1)
            return 0;
        var info = Registry.Instance.GetFacilityInfo(_facilities[index]);
        return _transformationFacilitiesProgression[index] / info.Cooldown;
    }

    public void SetAvailableResources(Registry.Resources[] availableResources)
    {
        _availableResources = availableResources;
    }
    public void EngageMoveSelectionMode(Registry.Resources resource, int count, int shipID, int countPeople = 0)
    {
        Pausing.Block();
        moveSelection = (int)resource;
        moveSelectionAdv = false;
        moveSelectionCount = count;
        moveSelectionCountPeople = countPeople;
        moveSelectionShipID = shipID;
        moveSelectionOrigin = selected;
        moveSelectionType = true;
        selected = null;
        var planets = FindObjectsOfType<Planet>();
        foreach(var planet in planets)
        {
            planet.GetComponentInChildren<Outline>().color = 1;
        }
    }
    public void EngageMoveSelectionMode(Registry.AdvancedResources resource, int count, int shipID, int countPeople = 0)
    {
        Pausing.Block();
        moveSelection = (int)resource;
        moveSelectionAdv = true;
        moveSelectionCount = count;
        moveSelectionCountPeople = countPeople;
        moveSelectionShipID = shipID;
        moveSelectionOrigin = selected;
        moveSelectionType = true;
        selected = null;
        var planets = FindObjectsOfType<Planet>();
        foreach (var planet in planets)
        {
            planet.GetComponentInChildren<Outline>().color = 1;
        }
    }
    public void EngageMoveSelectionMode(int count, int shipID)
    {
        Pausing.Block();
        moveSelectionCount = count;
        moveSelectionOrigin = selected;
        moveSelectionShipID = shipID;
        moveSelectionType = false;
        selected = null;
    }
    public void EngageMoveSelectionMode(int shipID)
    {
        Pausing.Block();
        moveSelectionCount = -1;
        moveSelectionOrigin = selected;
        moveSelectionShipID = shipID;
        moveSelectionType = false;
        selected = null;
    }

    #endregion Accessibility
    #region Routines
    void Update()
    {
        if (_hasPlayer)
        {
            _leaderPlanet = this;

        }
        if (moveSelectionCount == 0)
            StandardUpdate();
        else
            MoveSelectionMode();
        UpdateGraph();
        RunBasicFacilities();
        RunTransformationFacilities();
        ConsumeFood();
    }

    private void UpdateGraph()
    {
        if (ClemCAddons.Utilities.Timer.MinimumDelay("UpdateGraph".GetHashCode(), 1000, true))
        {
            _peopleOverTime.Add(_people);
            while (_peopleOverTime.Count > 60)
            {
                _peopleOverTime.RemoveAt(0);
            }
            _resourcesOverTime.Add(_resources.Sum(t => t.Value) + _advancedResources.Sum(t => t.Value));
            while (_resourcesOverTime.Count > 60)
            {
                _resourcesOverTime.RemoveAt(0);
            }
            _facilitiesOverTime.Add(_facilities.Count() + _transformationFacilities.Count());
            while (_facilitiesOverTime.Count > 60)
            {
                _facilitiesOverTime.RemoveAt(0);
            }
        }
    }

    private void ConsumeFood()
    {
        if(_people == 0)
        {
            _consumption = 0;
            return;
        }
        _consumption += Time.deltaTime;
        if(_consumption > 60)
        {
            _peopleFed = (int)_peopleOverTime.Average();
            int needFood = (int)Mathf.Ceil(_peopleFed / 10f);
            int needWater = (int)Mathf.Ceil(_peopleFed / 10f);

            int preparedFood = _advancedResources[Registry.AdvancedResources.PreparedFood];
            int food = _resources[Registry.Resources.Food];
            int animals = _resources[Registry.Resources.Water];
            int plants = _resources[Registry.Resources.Plants];
            int water = _resources[Registry.Resources.Water];
            if ((needFood > 0 || needWater > 0) && preparedFood > 0)
            {
                if (needFood > 0)
                    needFood -= preparedFood * 10;
                else
                    needWater -= preparedFood * 10;
                preparedFood = -(int)Mathf.Ceil((needFood.Min(0) + needWater.Min(0)) / 10f); // setting to - negative or 0 value
                _advancedResources[Registry.AdvancedResources.PreparedFood] = preparedFood;
            }
            if (needFood > 0 && food > 0)
            {
                needFood -= food;
                food = -needFood.Min(0); // setting to - negative or 0 value
                _resources[Registry.Resources.Food] = food;
            }
            if (needFood > 0 && animals > 0)
            {
                needFood -= animals / 2;
                animals = -needFood.Min(0) * 2; // setting to - negative or 0 value
                _resources[Registry.Resources.Animals] = animals;
            }
            if (needFood > 0 && plants > 0)
            {
                needFood -= plants / 2;
                plants = -needFood.Min(0) * 2; // setting to - negative or 0 value
                _resources[Registry.Resources.Plants] = plants;
            }
            if (needWater > 0 && water > 0)
            {
                needWater -= water;
                water = -needWater.Min(0); // setting to - negative or 0 value
                _resources[Registry.Resources.Water] = water;
            }
            var deaths = (needWater / 10).Max(needFood / 10); // floored naturally
            _people -= deaths;
            _consumption = 0;
            _peopleFed = _people;
        }
    }

    private void RunTransformationFacilities()
    {
        for (int i = 0; i < _transformationFacilities.Count; i++)
        {
            var info = Registry.Instance.GetFacilityInfo(_transformationFacilities[i]);
            bool confirm = true;
            foreach (var resource in info.InputResources)
                if (GetResource(resource) < info.Cost)
                    confirm = false;
            if (!confirm)
                continue;
            _transformationFacilitiesProgression[i] += Time.deltaTime * (_people / 5f);
            if (_transformationFacilitiesProgression[i] > info.Cooldown)
            {
                _transformationFacilitiesProgression[i] = 0;
                foreach(var resource in info.InputResources)
                    TakeResource(resource, info.Cost);
                if (info.Advanced)
                    AddResource(info.OutputResourceTransformation, info.Production);
                else
                    AddResource(info.OutputResource, info.Production);
            }
        }
    }

    private void RunBasicFacilities()
    {
        for(int i = 0; i < _facilities.Count; i++)
        {
            var info = Registry.Instance.GetFacilityInfo(_facilities[i]);
            _facilitiesProgression[i] += Time.deltaTime * (_people / 5f);
            if(_facilitiesProgression[i] > info.Cooldown)
            {
                _facilitiesProgression[i] = 0;
                AddResource(info.AssociatedResource);
            }
        }
    }
    private void MoveSelectionMode()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
        };
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (moveSelectionOrigin == this)
        {
            GetComponentInChildren<Outline>().color = 0;
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            moveSelectionCount = 0;
            selected = moveSelectionOrigin;
            ShippingSubMenu.Show();
            ShippingSubMenu.ResetMenu();
            if (moveSelectionCount == -1)
                ShippingSubMenu.Instance.ShowPresidentSending();
            else
                ShippingSubMenu.Instance.ShowResourceSending();
            Pausing.Unblock();

        }
        if (Physics.Raycast(ray, out var hit, 100))
        {
            if (transform == hit.transform || transform.FindDeep(t => t == hit.transform))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    MenuAudioManager.Instance.PlayClick();
                    if(moveSelectionCount == -1)
                    {
                        int shipNum = moveSelectionShipID;
                        var order = new OrderHandler.Order(
                                OrderHandler.OrderType.WavingGoodbye,
                                10,
                                1f,
                                1,
                                new OrderHandler.OrderExec(moveSelectionOrigin, this, shipNum)
                            );
                        OrderHandler.Instance.Queue(order, moveSelectionOrigin);
                    }
                    else
                    {
                        if (moveSelectionType)
                        {
                            int count = moveSelectionCount;
                            int count2 = moveSelectionCountPeople;
                            int shipNum = moveSelectionShipID;
                            if (moveSelectionAdv)
                            {
                                Registry.AdvancedResources resource = (Registry.AdvancedResources)moveSelection;
                                var order = new OrderHandler.Order(
                                    OrderHandler.OrderType.PreparingCargo,
                                    20,
                                    0.5f,
                                    3,
                                    new OrderHandler.OrderExec(moveSelectionOrigin, this, resource, count, count2, shipNum)
                                );
                                OrderHandler.Instance.Queue(order, moveSelectionOrigin);
                            }
                            else
                            {
                                Registry.Resources resource = (Registry.Resources)moveSelection;
                                var order = new OrderHandler.Order(
                                    OrderHandler.OrderType.PreparingCargo,
                                    20,
                                    0.5f,
                                    3,
                                    new OrderHandler.OrderExec(moveSelectionOrigin, this, resource, count, count2, shipNum)
                                );
                                OrderHandler.Instance.Queue(order, moveSelectionOrigin);
                            }
                        }
                        else
                        {
                            int count = moveSelectionCount;
                            int shipNum = moveSelectionShipID;
                            var order = new OrderHandler.Order(
                                   OrderHandler.OrderType.PreparingForTrip,
                                   20,
                                   0.5f,
                                   15,
                                   new OrderHandler.OrderExec(moveSelectionOrigin, this, count, shipNum)
                               );
                            OrderHandler.Instance.Queue(order, moveSelectionOrigin);
                        }
                    }
                    moveSelectionCount = 0;
                    ShippingSubMenu.ResetMenu();
                    selected = moveSelectionOrigin;
                    Pausing.Unblock();
                }
                else
                {
                    GetComponentInChildren<Outline>().color = 2;
                }
            }
            else
            {
                GetComponentInChildren<Outline>().color = 1;
            }
        }
    }
    private void StandardUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
            };
            pointerData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerData, results);


            foreach (var r in results)
            {
                if (r.gameObject.name == "Menu" || r.gameObject.FindParentDeep("Menu") || r.gameObject.FindParentDeep("PauseCanvas") || r.gameObject.FindParentDeep("Dialogs"))
                    return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100))
            {
                if (transform == hit.transform || transform.FindDeep(t => t == hit.transform))
                {
                    if (selected == this)
                    {
                        selected = null;
                        return;
                    }
                    selected = this;
                    return;
                }
                if (!hit.transform.TryGetComponent<Planet>(out _) && hit.transform.FindParentWithComponent(typeof(Planet)) == null)
                {
                    selected = null;
                }
            }
            else
            {
                selected = null;
            }
        }
        GetComponentInChildren<Outline>().color = (selected == this).ToInt();
    }
    #endregion Routines
    #region Generation
    public void Initialize(Registry.SystemType systemType, Registry.Resources[] resources, bool _doNotRegenerate = false, int temperate = -1, int roll = 0)
    {

        for (int i = 0; i < System.Enum.GetNames(typeof(Registry.Resources)).Length; i++)
        {
            _resources.Add((Registry.Resources)i, 0);
        }
        for (int i = 0; i < System.Enum.GetNames(typeof(Registry.AdvancedResources)).Length; i++)
        {
            _advancedResources.Add((Registry.AdvancedResources)i, 0);
        }
        if (_doNotRegenerate)
            return;
        var rnd = new System.Random();
        char a = (char)rnd.Next('a', 'z');
        char b = (char)rnd.Next('a', 'z');
        int num = rnd.Next(100, 999);
        int maybe = rnd.Next(0, 100);
        if (maybe < 10) // critical role!
            _people = 1;
        _name = string.Concat(a, b, num);
        _availableResources = resources;
        Destroy(GetComponent<MeshRenderer>());
        for(int i = transform.childCount - 1; i >= 0 ; i--)
        {
            Destroy(transform.GetChild(i).gameObject); // might have cloned too late, after the original spawned a new planet inside
        }
        int r;
        _temperateType = -1;
        switch (systemType)
        {
            case Registry.SystemType.Alien:
                r = Registry.Instance.GetRandomPlanet(Registry.PlanetType.Alien);
                _roll = r;
                Instantiate(Registry.Instance.GetPlanet(Registry.PlanetType.Alien, r), transform)
                     .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
            case Registry.SystemType.Cold:
                r = Registry.Instance.GetRandomPlanet(Registry.PlanetType.Frozen);
                _roll = r;
                Instantiate(Registry.Instance.GetPlanet(Registry.PlanetType.Frozen, r), transform)
                     .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
            case Registry.SystemType.Hot:
                r = Registry.Instance.GetRandomPlanet(Registry.PlanetType.Desert);
                _roll = r;
                Instantiate(Registry.Instance.GetPlanet(Registry.PlanetType.Desert, r), transform)
                     .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
            case Registry.SystemType.Temperate:
                if (temperate == 0 || (temperate == -1 && Random.Range(0, 1) == 0))
                {
                    r = Registry.Instance.GetRandomPlanet(Registry.PlanetType.Temperate);
                    _roll = r;
                    Instantiate(Registry.Instance.GetPlanet(Registry.PlanetType.Temperate, r), transform)
                            .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                    _temperateType = 0;
                }
                else
                {
                    r = Registry.Instance.GetRandomPlanet(Registry.PlanetType.Earth);
                    _roll = r;
                    Instantiate(Registry.Instance.GetPlanet(Registry.PlanetType.Earth, r), transform)
                            .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                    _temperateType = 1;
                }
                break;
            default:
                if (temperate == -1)
                    break;
                _temperateType = temperate;
                _roll = roll;
                if (temperate == 0)
                    Instantiate(Registry.Instance.GetPlanet(Registry.PlanetType.Temperate, roll), transform)
                        .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                else
                    Instantiate(Registry.Instance.GetPlanet(Registry.PlanetType.Earth, roll), transform)
                        .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
        }
    }
    #endregion Generation
}