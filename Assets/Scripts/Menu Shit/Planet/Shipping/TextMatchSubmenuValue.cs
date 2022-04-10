using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMatchSubmenuValue : MonoBehaviour
{
    void Update()
    {
        GetComponent<TMPro.TMP_Text>().text = ShippingSubMenu.Instance.GetValue().ToString();
    }
}
