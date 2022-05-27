using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationRandomizer : MonoBehaviour
{
    void Awake()
    {
        transform.rotation = Quaternion.Euler(50 * Random.Range(0, 1f), 50 * Random.Range(0, 1f), 360 * Random.Range(0, 1f));
    }
}
