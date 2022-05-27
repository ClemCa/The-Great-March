using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeRandomizer : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float _minRange;
    [SerializeField, Range(0, 2)] private float _maxRange;

    void Awake()
    {
        transform.localScale = Random.Range(_minRange, _maxRange) * Vector3.one;
    }

}
