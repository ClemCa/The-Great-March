using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemSpawning : MonoBehaviour
{
    [SerializeField] private GameObject _systemPrefab;
    [SerializeField] private float _every;
    private float _last = 0;

    void Update()
    {
        if(transform.position.x > _last + _every)
        {
            _last = transform.position.x;
            Debug.Log("instantiate prefab now");
        }
    }
}
