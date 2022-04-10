using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{
    [SerializeField] private Transform _sun;
    [SerializeField] private float _speed = 2;
    [SerializeField] private float _rotationSpeed = 1;

    void Update()
    {
        transform.RotateAround(_sun.position, Vector3.forward, Time.deltaTime * _speed);
        transform.Rotate((Vector3.forward * 0.8f + Vector3.right * 0.2f), Time.deltaTime * _rotationSpeed);
    }
}
