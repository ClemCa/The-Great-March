using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoringWriter : MonoBehaviour
{
    void Start()
    {
        GetComponent<TMPro.TMP_Text>().text = "As a Leader, you helped your people"
            + "survive { XXXXXX}"
            + "minutes.\n"
            + "You went through { XX}"
            + "systems.\n"
            + "You extracted { XXXXX}"
            + "unit of natural\n"
            + "resources, and produced { XXXX}\n"
            + "advanced resources.\n"
            + "You made { XXX}"
            + "facilities and\n"
            + "{ XXXX}"
            + "transformative facilities.\n"
            +"And all of that, over {XXXX}\n"
            + "minutes";
    }
}
