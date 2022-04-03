using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConfirmPeopleChoice : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if(ShippingSubMenu.Instance.GetValue() > 0)
            Planet.Selected.EngageMoveSelectionMode(ShippingSubMenu.Instance.GetValue());
    }
}
