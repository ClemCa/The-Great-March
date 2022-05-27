using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CargoGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _cargoPrefab;
    private static CargoGenerator _instance;

    void Awake()
    {
        _instance = this;
    }

    private void CreateCargo(Registry.Resources resource, int quantity, Planet origin, Planet destination, int people, Registry.Ship ship)
    {
        var r = Instantiate(_cargoPrefab);
        r.GetComponent<Cargo>().Initialize(origin, destination, quantity, resource, people, ship);
    }
    private void CreateCargo(Registry.AdvancedResources resource, int quantity, Planet origin, Planet destination, int people, Registry.Ship ship)
    {
        var r = Instantiate(_cargoPrefab);
        r.GetComponent<Cargo>().Initialize(origin, destination, quantity, resource, people, ship);
    }
    private void CreateCargoPeople(int quantity, Planet origin, Planet destination, Registry.Ship ship)
    {
        var r = Instantiate(_cargoPrefab);
        r.GetComponent<Cargo>().Initialize(origin, destination, quantity, ship);
    }
    private void CreateCargoLeader(Planet origin, Planet destination, Registry.Ship ship)
    {
        var r = Instantiate(_cargoPrefab);
        r.GetComponent<Cargo>().Initialize(origin, destination, ship);
    }
    private Cargo CreateCargo()
    {
        return Instantiate(_cargoPrefab).GetComponent<Cargo>();
    }
    public static void GenerateCargo(Planet origin, Planet destination, Registry.Resources resource, int quantity, int people, Registry.Ship ship)
    {
        _instance.CreateCargo(resource, quantity, origin, destination, people, ship);
    }
    public static void GenerateCargo(Planet origin, Planet destination, Registry.AdvancedResources resource, int quantity, int people, Registry.Ship ship)
    {
        _instance.CreateCargo(resource, quantity, origin, destination, people, ship);
    }
    public static void GenerateCargo(Planet origin, Planet destination, int quantity, Registry.Ship ship)
    {
        _instance.CreateCargoPeople(quantity, origin, destination, ship);
    }
    public static void GenerateCargo(Planet origin, Planet destination, Registry.Ship ship)
    {
        _instance.CreateCargoLeader(origin, destination, ship);
    }

    public static Cargo GenerateCargo()
    {
        return _instance.CreateCargo();
    }
}
