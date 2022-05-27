using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ClemCAddons;
using UnityEngine.UI;
using System;

public class ShipChoice : MonoBehaviour
{
    [SerializeField] private Transform _shipParent;
    [SerializeField] private TMPro.TMP_Text _selectedText;
    [SerializeField] private TMPro.TMP_Text _refuelText;
    [SerializeField] private Button _refuelButton;

    private List<Registry.ShipType> _ships = new List<Registry.ShipType>();

    private int _selected = -1;
    private bool _launchStage = false;


    void OnEnable()
    {
        if (Planet.Selected != null && _selected < Planet.Selected.Ships.Count)
            return;
        _selected = -1;
        _selectedText.text = "";
        _refuelButton.interactable = false;
        _refuelText.text = "Select a ship first";
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy || Planet.Selected == null)
            return;
        var ships = Planet.Selected.Ships.Select(t => t.Type).ToList();

        if (!ships.SequenceEqual(_ships))
        {
            _ships = ships;
            for (int i = _shipParent.childCount - 1; i >= 0; i--)
                Destroy(_shipParent.GetChild(i).gameObject);
            for(int i = 0; i < ships.Count; i++)
            {
                var ship = Registry.Instance.GetShipPrefab(ships[i]);
                var id = i; // to use a copy instead of a reference in the lambda
                Instantiate(ship, _shipParent).GetComponent<Button>().onClick.AddListener(() => Click(id));
            }
        }

        if(_selected != -1)
        {
            var ship = Planet.Selected.Ships[_selected];
            var requiredFuel = Registry.Instance.GetRequiredFuel(ship.Type);
            if (ship.Fuel >= requiredFuel)
            {
                _refuelButton.interactable = true;
                _launchStage = true;
                _refuelText.text = "Launch";
            }
            else
            {
                var f = requiredFuel - ship.Fuel;
                _refuelText.text = "Refuel (" + f + ")";
                if (f < Planet.Selected.GetAvailableFuel())
                {
                    _refuelButton.interactable = true;
                    _refuelText.color = Color.white;
                }
                else
                {
                    _refuelButton.interactable = false;
                    _refuelText.color = Color.red;
                }
            }
        }
    }

    public void Click(int i)
    {
        _selected = i;
        _selectedText.text = "Ship n°"+(i+1)+": "+_ships[i].ToString();
        MenuAudioManager.Instance.PlayClick();
    }

    public void Return()
    {
        MenuAudioManager.Instance.PlayClick();
        ShippingSubMenu.Flip(transform);
    }

    public void Refuel()
    {
        MenuAudioManager.Instance.PlayClick();
        if (_launchStage)
        {
            var r = Planet.Selected.Ships[_selected];
            ShippingSubMenu.Instance.Ship = _selected;
            if (r.Type == Registry.ShipType.Presidential)
                ShippingSubMenu.Instance.ShowPresidentSending();
            else
                ShippingSubMenu.Instance.ShowResourceSending();
        }
        else
        {
            var ship = Planet.Selected.Ships[_selected];
            var requiredFuel = Registry.Instance.GetRequiredFuel(ship.Type);
            Planet.Selected.ConsumeFuel(requiredFuel - ship.Fuel);
        }
    }
}
