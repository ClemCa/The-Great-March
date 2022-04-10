using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShippingCancelButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        MenuAudioManager.Instance.PlayClick();
        ShippingSubMenu.ResetMenu();
    }
}