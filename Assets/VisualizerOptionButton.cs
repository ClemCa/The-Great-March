using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizerOptionButton : MonoBehaviour
{
    public void Option()
    {
        GetComponentInParent<MindVisualizerOptions>().CallOption(GetComponentInChildren<TMPro.TMP_Text>().text);
    }
}
