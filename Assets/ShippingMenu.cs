using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShippingMenu : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool isPeople;

    public void OnPointerClick(PointerEventData eventData)
    {
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
