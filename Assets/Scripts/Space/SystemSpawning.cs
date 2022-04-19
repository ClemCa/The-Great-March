using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class SystemSpawning : MonoBehaviour
{
    [SerializeField] private GameObject _systemPrefab;
    [SerializeField] private float _every;
    [SerializeField] private int _howManyBeforehand = 2;
    [SerializeField] private int _maxSystems = 10;
    [SerializeField] private List<GameObject> _spawned = new List<GameObject>();

    private float _last = 0;

    private static SystemSpawning _instance;

    public List<GameObject> Spawned { get => _spawned; }
    public static SystemSpawning Instance { get => _instance; }

    void Start()
    {
        _instance = this;
    }

    void Update()
    {
        if(transform.position.x + _every * _howManyBeforehand > _last)
        {
            _last += _every;
            _spawned.Add(Instantiate(_systemPrefab, new Vector3(_last, 0), Quaternion.identity));
        }
        if(_spawned.Count > _maxSystems)
        {
            Destroy(_spawned[0]);
            _spawned.RemoveAt(0);
        }
        if (Planet.LeaderPlanet != null)
            transform.position = transform.position.SetX(Mathf.Lerp(transform.position.x, Planet.LeaderPlanet.transform.parent.position.x, Time.deltaTime));
    }
}
