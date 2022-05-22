using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ClemCAddons;

public class OrderHandler : MonoBehaviour
{
    private static OrderHandler _instance;
    private Dictionary<string, List<Order>> _queue = new Dictionary<string, List<Order>>();

    public static OrderHandler Instance { get => _instance; }
    public Dictionary<string, List<Order>> QueueData { get => _queue; set => _queue = value; }

    [Serializable]
    public class Order
    {
        public string Planet;
        public int Assigned;
        public OrderType Type;
        public float Length;
        public float LengthLeft;
        public float SpeedPerPerson;
        public int MaxPeople;
        public OrderExec Execution;
        public Order(OrderType type, float length, float speedPerPerson, int maxPeople, OrderExec execution)
        {
            Assigned = 0;
            Type = type;
            Length = length;
            LengthLeft = length;
            SpeedPerPerson = speedPerPerson;
            MaxPeople = maxPeople;
            Execution = execution;
        }
    }

    [Serializable]
    public class OrderExec
    {
        public string Planet;
        public ActionType Type;
        public string Destination;
        public int Count;
        public Registry.Resources Resource;
        public Registry.AdvancedResources AdvancedResource;
        public Registry.Facilities Facility;
        public Registry.TransformationFacilities TransformationFacility;


        public OrderExec(Planet planet, Planet destination)
        {
            Planet = planet.Name;
            Destination = destination.Name;
            Type = ActionType.CargoLeader;
        }
        public OrderExec(Planet planet, Planet destination, int count)
        {
            Planet = planet.Name;
            Destination = destination.Name;
            Count = count;
            Type = ActionType.CargoPeople;
        }
        public OrderExec(Planet planet, Planet destination, Registry.Resources resource, int count)
        {
            Planet = planet.Name;
            Destination = destination.Name;
            Count = count;
            Resource = resource;
            Type = ActionType.CargoResources;
        }
        public OrderExec(Planet planet, Registry.Resources resource, int count)
        {
            Planet = planet.Name;
            Resource = resource;
            Count = count;
            Type = ActionType.Resources;
        }

        public OrderExec(Planet planet, Planet destination, Registry.AdvancedResources resource, int count)
        {
            Planet = planet.Name;
            Destination = destination.Name;
            Count = count;
            AdvancedResource = resource;
            Type = ActionType.CargoAdvancedResources;
        }

        public OrderExec(Planet planet, Registry.AdvancedResources resource, int count)
        {
            Planet = planet.Name;
            AdvancedResource = resource;
            Count = count;
            Type = ActionType.AdvancedResources;
        }

        public OrderExec(Planet planet, Registry.Facilities facility)
        {
            Planet = planet.Name;
            Facility = facility;    
            Type = ActionType.Facility;
        }

        public OrderExec(Planet planet, Registry.TransformationFacilities facility)
        {
            Planet = planet.Name;
            TransformationFacility = facility;
            Type = ActionType.TransformationFacility;
        }

        public OrderExec() { }

        public void Invoke()
        {
            var planets = FindObjectsOfType<Planet>();
            var planet = Array.Find(planets, t => t.Name == Planet);
            if (planet == null)
                return;
            switch (Type)
            {
                case ActionType.CargoLeader:
                    var destination = Array.Find(planets, t => t.Name == Destination);
                    if (destination == null)
                        return;
                    CargoGenerator.GenerateCargo(planet, destination);
                    break;
                case ActionType.CargoPeople:
                    var destination3 = Array.Find(planets, t => t.Name == Destination);
                    if (destination3 == null)
                        return;
                    CargoGenerator.GenerateCargo(planet, destination3, Count);
                    break;
                case ActionType.CargoResources:
                    var destination2 = Array.Find(planets, t => t.Name == Destination);
                    if (destination2 == null)
                        return;
                    CargoGenerator.GenerateCargo(planet, destination2, Resource, Count);
                    break;
                case ActionType.Resources:
                    planet.AddResource(Resource, Count);
                    break;
                case ActionType.CargoAdvancedResources:
                    var destination4 = Array.Find(planets, t => t.Name == Destination);
                    if (destination4 == null)
                        return;
                    CargoGenerator.GenerateCargo(planet, destination4, AdvancedResource, Count);
                    break;
                case ActionType.AdvancedResources:
                    planet.AddResource(AdvancedResource, Count);
                    break;
                case ActionType.Facility:
                    FacilityMenu.OrderedFacilities.Remove(new KeyValuePair<Planet, Registry.Facilities>(planet, Facility));
                    planet.RegisterBuiltFacility(Facility);
                    break;
                case ActionType.TransformationFacility:
                    TransformationFacilityMenu.OrderedFacilities.Remove(new KeyValuePair<Planet, Registry.TransformationFacilities>(planet, TransformationFacility));
                    planet.RegisterBuiltFacility(TransformationFacility);
                    break;
                default:
                    break;
            }
        }
        public void Cancel()
        {
            var planets = FindObjectsOfType<Planet>();
            var planet = Array.Find(planets, t => t.Name == Planet);
            if (planet == null)
                return;
            switch (Type)
            {
                case ActionType.Facility:
                    FacilityMenu.OrderedFacilities.Remove(new KeyValuePair<Planet, Registry.Facilities>(planet, Facility));
                    break;
                case ActionType.TransformationFacility:
                    TransformationFacilityMenu.OrderedFacilities.Remove(new KeyValuePair<Planet, Registry.TransformationFacilities>(planet, TransformationFacility));
                    break;
                default:
                    break;
            }
        }
    }

    public enum ActionType
    {
        CargoLeader,
        CargoPeople,
        CargoResources,
        CargoAdvancedResources,
        Resources,
        AdvancedResources,
        Facility,
        TransformationFacility
    }

    public enum OrderType
    {
        Building,
        PreparingCargo,
        PreparingForTrip,
        WavingGoodbye,
        TriumphantArrival,
        UnpackingCargo
    }

    void Awake()
    {
        _instance = this;
    }

    void Update()
    {
        Process();
    }

    public void Queue(Order order, Planet planet)
    {
        order.Planet = planet.Name;
        if (_queue.ContainsKey(planet.Name))
            _queue[planet.Name].Add(order);
        else
            _queue.Add(planet.Name, new List<Order>() { order });
    }

    public Order[] GetPlanetQueue(Planet planet)
    {
        if (!_queue.ContainsKey(planet.Name))
            return new Order[0];
        return _queue[planet.Name].ToArray();
    }

    public void Clear(Order order)
    {
        var i = _queue.Values.ToList().FindIndex(t => t.FindIndex(r => r == order) != -1);
        if (i == -1)
            return;
        if(order.Execution != null)
            order.Execution.Cancel();
        var k = _queue.ElementAt(i).Key;
        var v = _queue.ElementAt(i).Value;
        v.Remove(order);
        _queue[k] = v;
    }

    private void Process()
    {
        ClearEmpty();
        Assignement();
        foreach(var p in _queue.Keys)
        {
            for (int i = _queue[p].Count - 1; i >= 0; i--)
            {
                var r = Countdown(_queue[p][i]);
                if (r != null)
                    _queue[p][i] = r;
                else
                    _queue[p].RemoveAt(i);
            }
        }
    }

    private void Assignement()
    {
        var array = _queue.Keys.ToArray();
        foreach (var r in array)
        {
            var planet = FindObjectsOfType<Planet>().First(t => t.Name == r);
            int distribution = planet.GetPeople();
            foreach(var t in _queue[r])
            {
                if(distribution > 0)
                {
                    int v = distribution.Min(t.MaxPeople);
                    t.Assigned = v;
                    distribution -= v;
                }
                else
                    t.Assigned = 0;
            }
        }
    }

    private void ClearEmpty()
    {
        var array = _queue.Keys.ToArray();
        foreach(var r in array)
        {
            if (r == null)
                _queue.Remove(r);
            for(int i = _queue[r].Count - 1; i >= 0; i--)
            {
                var order = _queue[r][i];
            }
        }
    }

    private Order Countdown(Order order)
    {
        order.LengthLeft -= Time.deltaTime * order.SpeedPerPerson * order.Assigned;
        if (order.LengthLeft <= 0)
        {
            Execute(order);
            return null;
        }
        return order;
    }

    private void Execute(Order order)
    {
        if(order != null)
            order.Execution.Invoke();
    }
}
