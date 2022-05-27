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
        Planet.Selected.EngageMoveSelectionMode(ShippingSubMenu.Instance.Ship);
    }

    public void Return()
    {
        ShippingSubMenu.ResetMenu();
    }
}
