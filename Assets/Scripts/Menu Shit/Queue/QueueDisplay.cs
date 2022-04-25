using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System.Text.RegularExpressions;

public class QueueDisplay : MonoBehaviour
{
    [SerializeField] private GameObject _cancelObject;
    private OrderHandler.Order _order;

    public OrderHandler.Order Order { get => _order; set => _order = value; }

    void Start()
    {
        if (_order != null && (_order.Type == OrderHandler.OrderType.UnpackingCargo || _order.Type == OrderHandler.OrderType.WavingGoodbye))
            _cancelObject.SetActive(false);
    }

    void Update()
    {
        if (!Planet.Selected || Order == null)
            return;
        var rect = transform.Find("Progress").GetComponent<RectTransform>();
        var v = rect.sizeDelta;
        v.x = (1 - _order.LengthLeft / _order.Length) * 200;
        rect.sizeDelta = v;
        transform.Find("OrderType").GetComponent<TMPro.TMP_Text>().text = Regex.Replace(_order.Type.ToString(), "[A-Z]", " $0");
        transform.Find("Assigned").GetComponent<TMPro.TMP_Text>().text = "Assigned: " + _order.Assigned + "/"+_order.MaxPeople+" people";
        transform.Find("Speed").GetComponent<TMPro.TMP_Text>().text = "Speed: x"+_order.SpeedPerPerson * _order.Assigned;
        transform.Find("Difficulty").GetComponent<TMPro.TMP_Text>().text = "Tasks: " + _order.LengthLeft.Round() +"/"+_order.Length;
    }

    public void Clear()
    {
        OrderHandler.Instance.Clear(_order);
    }
}
