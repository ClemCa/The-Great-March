using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    [SerializeField] private float _speed = 1;
    void Update()
    {
        transform.position += Vector3.right * Time.deltaTime * _speed;
    }
}
