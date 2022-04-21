using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System.Linq;

public class StellarSystem : MonoBehaviour
{
    [SerializeField] private GameObject _sun;
    [SerializeField] private GameObject _firstPlanet;
    [SerializeField] private float _distancePlanets = 3;
    [SerializeField] private float _minimumDistance = 2;
    [SerializeField] private Registry.SystemType _systemType;
    [SerializeField] private bool _doNotRegenerate = false;
    private Lane[] _lanes;

    private bool _block;
    public bool Block { get => _block; set => _block = value; }
    public bool RanStart = false;
    public GameObject FirstPlanet { get => _firstPlanet; set => _firstPlanet = value; }
    public Registry.SystemType SystemType { get => _systemType; set => _systemType = value; }
    public GameObject Sun { get => _sun; set => _sun = value; }

    private struct Lane
    {
        public GameObject Planet;
        public float Distance;
        public Lane(GameObject planet, float distance)
        {
            Planet = planet;
            Distance = distance;
        }
    }

    public void Setup(Registry.SystemType systemType)
    {
        _systemType = systemType;
    }

    void Start()
    {
        if (Block)
        {
            return;
        }
        while (_systemType == Registry.SystemType.None && !_doNotRegenerate)
            _systemType = (Registry.SystemType)Random.Range(0, System.Enum.GetNames(typeof(Registry.SystemType)).Length - 1);
        int planets = Random.Range(2,5);
        int resources = Random.Range(5, 8);
        var r = Registry.GetResources();
        r = r.RemoveAll(Registry.Resources.Food);
        if (_doNotRegenerate)
        {
            _firstPlanet.GetComponent<Planet>().Initialize(_systemType, new Registry.Resources[] { }, true);
            return;
        }
        while(r.Length > resources)
        {
            r = r.RemoveAt(Random.Range(0, r.Length));
        }
        var ratios = GetRandomRatio(r.Length);
        _lanes = new Lane[planets];
        _lanes[1] = new Lane(_firstPlanet, _distancePlanets * 2 + _minimumDistance);
        for(int i = 0; i < _lanes.Length; i++)
        {
            if(_lanes[i].Planet == null)
            {
                _lanes[i] = new Lane(Instantiate(_firstPlanet, transform), _distancePlanets * (i+1) + _minimumDistance);
            }
            _lanes[i].Planet.transform.localPosition = Random.insideUnitCircle.normalized * _lanes[i].Distance;
            
            var resourcesToDistribute = new List<Registry.Resources>();
            for (int t = 0; t < r.Length; t++)
            {
                if (Mathf.CeilToInt(ratios[t] * Mathf.Floor(resources / (float)planets)) > 0)
                    resourcesToDistribute.Add(r[t]);
            }
            _lanes[i].Planet.GetComponent<Planet>().Initialize(_systemType, r);
        }
    }

    private void PrintChildInventory()
    {
        var r = Registry.GetResources();
        var count = new int[r.Length];
        var children = GetComponentsInChildren<Planet>();
        for(int i = 0; i < children.Length; i++)
        {
            for(int t = 0; t < count.Length; t++)
            {
                count[t] += children[i].Resources[r[t]];
            }
        }
        for(int i = 0; i < r.Length; i++)
        {
            Debug.Log(r[i] + ": " + count[i]);
        }
    }

    private float[] GetRandomRatio(int length)
    {
        var result = new float[length];
        int total = 0;
        while(total == 0) // just to be sure there isn't one rare case where all random values roll 0
            for(int i = 0; i < result.Length; i++)
            {
                var v = Random.Range(1, 100);
                total += v;
                result[i] = v;
            }
        for(int i = 0; i < result.Length; i++)
        {
            result[i] = result[i] / total;
        }
        return result;
    }
}
