using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ValueChangeButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int _direction = 1;
    public void OnPointerClick(PointerEventData eventData)
    {
        MenuAudioManager.Instance.PlayClick();
        ShippingSubMenu.Instance.UpdateValue(_direction);
    }

}
