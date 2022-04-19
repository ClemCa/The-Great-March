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
        [NonSerialized]
        public Action Execution;
        public Order(OrderType type, float length, float speedPerPerson, int maxPeople, Action execution)
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

    public enum OrderType
    {
        Building,
        CargoResources,
        PreparingCargo,
        PreparingForTrip,
        WavingGoodbye,
        TriumphantArrival,
        UnpackingCargo
    }

    void Start()
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
        order.Execution.Invoke();
    }
}
