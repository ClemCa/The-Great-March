using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInfo : MonoBehaviour
{
    void Update()
    {
        if (Planet.Selected == null)
            return;
        transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = "Planet: " + Planet.Selected.Name;
        transform.GetChild(2).GetComponent<TMPro.TMP_Text>().text = "People: " + Planet.Selected.GetPeople();
    }
}
