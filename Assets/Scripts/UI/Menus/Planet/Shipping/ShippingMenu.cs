using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShippingMenu : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ShippingMenuContent contentType;

    public enum ShippingMenuContent
    {
        Ships,
        Player,
        People,
        Resources,
        Return
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (contentType)
        {
            case ShippingMenuContent.Player:
                if (!Planet.Selected.HasPlayer || Cargo.LeaderInTransit)
                    return;
                Planet.Selected.EngageMoveSelectionMode(0);
                break;
            case ShippingMenuContent.People:
                ShippingSubMenu.Instance.ShowPeopleChoice();
                break;
            case ShippingMenuContent.Resources:
                ShippingSubMenu.Instance.ShowResourcesChoice();
                break;
            case ShippingMenuContent.Ships:
                ShippingSubMenu.Instance.ShowShipChoice();
                break;
            case ShippingMenuContent.Return:
                ShippingSubMenu.Flip(transform);
                break;
        }
        MenuAudioManager.Instance.PlayClick();
    }
}
