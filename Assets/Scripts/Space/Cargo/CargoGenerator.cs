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

    private void CreateCargo(Registry.Resources resource, int quantity, Planet origin, Planet destination)
    {
        var r = Instantiate(_cargoPrefab);
        r.GetComponent<Cargo>().Initialize(origin, destination, quantity, resource);
    }
    private void CreateCargo(Registry.AdvancedResources resource, int quantity, Planet origin, Planet destination)
    {
        var r = Instantiate(_cargoPrefab);
        r.GetComponent<Cargo>().Initialize(origin, destination, quantity, resource);
    }
    private void CreateCargoPeople(int quantity, Planet origin, Planet destination)
    {
        var r = Instantiate(_cargoPrefab);
        r.GetComponent<Cargo>().Initialize(origin, destination, quantity);
    }
    private void CreateCargoLeader(Planet origin, Planet destination)
    {
        var r = Instantiate(_cargoPrefab);
        r.GetComponent<Cargo>().Initialize(origin, destination);
    }
    private Cargo CreateCargo()
    {
        return Instantiate(_cargoPrefab).GetComponent<Cargo>();
    }
    public static void GenerateCargo(Planet origin, Planet destination, Registry.Resources resource, int quantity)
    {
        _instance.CreateCargo(resource, quantity, origin, destination);
    }
    public static void GenerateCargo(Planet origin, Planet destination, Registry.AdvancedResources resource, int quantity)
    {
        _instance.CreateCargo(resource, quantity, origin, destination);
    }
    public static void GenerateCargo(Planet origin, Planet destination, int quantity)
    {
        _instance.CreateCargoPeople(quantity, origin, destination);
    }
    public static void GenerateCargo(Planet origin, Planet destination)
    {
        _instance.CreateCargoLeader(origin, destination);
    }

    public static Cargo GenerateCargo()
    {
        return _instance.CreateCargo();
    }
}
