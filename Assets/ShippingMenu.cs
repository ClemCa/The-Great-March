using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShippingMenu : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool isPlayer;
    [SerializeField] private bool isPeople;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPlayer)
        {
            if (!Planet.Selected.HasPlayer || Cargo.LeaderInTransit)
                return;
            Planet.Selected.EngageMoveSelectionMode();
            MenuAudioManager.Instance.PlayClick();
            return;
        }
        MenuAudioManager.Instance.PlayClick();
        if (isPeople)
        {
            ShippingSubMenu.Instance.ShowPeopleChoice();
        }
        else
        {
            ShippingSubMenu.Instance.ShowResourcesChoice();
        }
    }
}
