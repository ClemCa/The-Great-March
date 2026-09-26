using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PresidentCargo : MonoBehaviour
{
    [SerializeField] private Button _launchButton;

    void Update()
    {
        _launchButton.interactable = Planet.Selected != null && Planet.Selected.HasPlayer;
    }

    public void Launch()
    {
        var id = Planet.Selected.ShipIDs; // don't wanna pass along a reference
        Planet.Selected.ShipIDs++;
        Planet.Selected.ReservedShips.Add(new KeyValuePair<int, Registry.Ship>(id, Planet.Selected.Ships[ShippingSubMenu.Instance.Ship]));
        Planet.Selected.Ships.RemoveAt(ShippingSubMenu.Instance.Ship);
        Planet.Selected.EngageMoveSelectionMode(id);
    }

    public void Return()
    {
        ShippingSubMenu.ResetMenu();
    }
}
