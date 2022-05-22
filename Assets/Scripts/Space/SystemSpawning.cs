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

    void Awake()
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

    public void Respawn(Saver.SystemSave[] systems, string selectedPlanet)
    {
        while (_spawned.Count > 0)
        {
            Destroy(_spawned[0]);
            _spawned.RemoveAt(0);
        }
        foreach (var system in systems)
        {
            var t = Instantiate(_systemPrefab);
            _spawned.Add(t);
            var sun = t.GetComponent<StellarSystem>().Sun;
            JsonUtility.FromJsonOverwrite(system.System, t.GetComponent<StellarSystem>());
            t.GetComponent<StellarSystem>().SystemType = system.SystemType;
            t.GetComponent<StellarSystem>().Block = true;
            t.GetComponent<StellarSystem>().FirstPlanet = t.transform.GetComponentInChildren<Planet>().gameObject;
            t.GetComponent<StellarSystem>().Sun = sun;

            t.transform.position = JsonUtility.FromJson<SerializableVector3>(system.Position).Value;

            for (int i = 0; i < system.Planets.Length; i++)
            {
                var planetSave = JsonUtility.FromJson<Saver.PlanetSave>(system.Planets[i]);
                Planet r;
                if (i == 0)
                {
                    r = t.GetComponent<StellarSystem>().FirstPlanet.GetComponent<Planet>();
                }
                else
                {
                    r = Instantiate(t.GetComponent<StellarSystem>().FirstPlanet, t.transform).GetComponent<Planet>();
                }
                r.Initialize(system.SystemType, new Registry.Resources[0], false, system.TemperateType[i], system.Roll[i]);
                r.transform.localPosition = JsonUtility.FromJson<SerializableVector3>(system.PlanetPositions[i]).Value;
                r.SetPlanetStorage(JsonUtility.FromJson<Saver.PlanetStorage>(planetSave.Planet));
                if (r.HasPlayer)
                {
                    _last = t.transform.position.x + _every * _howManyBeforehand;
                }
                if (selectedPlanet != "" && selectedPlanet == r.Name)
                    Planet.Select(r);
            }
            if (selectedPlanet == "")
                Planet.Unselect();
        }
    }
}
