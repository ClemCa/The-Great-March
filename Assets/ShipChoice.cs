using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipChoice : MonoBehaviour
{
    [SerializeField] private Transform _shipParent;
    [SerializeField] private GameObject _cargoShip;
    [SerializeField] private GameObject _passengerShip;
    [SerializeField] private GameObject _fighterShip;

    private List<Registry.ShipType> _ships = new List<Registry.ShipType>();


    void Update()
    {
        if (!gameObject.activeInHierarchy || Planet.Selected == null)
            return;
        var ships = Planet.Selected.Ships;
    }
}
