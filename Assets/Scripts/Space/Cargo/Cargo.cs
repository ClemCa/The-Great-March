using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System;
using System.Linq;

public class Cargo : MonoBehaviour
{
    public enum CargoType
    {
        Resource,
        AdvancedResource,
        People,
        Leader
    }
    
    [SerializeField] private float cruiseSpeed = 2.0f;
    [SerializeField] private float outerSystemCruiseSpeed = 1.0f;
    [SerializeField] private float blinkingSpeed = 0.5f;
    [SerializeField] private float downTime = 1;

    private Vector3[] _path = new Vector3[] { };


    public CargoType Type { get; private set; }

    public int Amount { get; private set; }
    public Registry.Resources? Resource { get; private set; }
    public Registry.AdvancedResources? AdvancedResource { get; private set; }


    public Planet Origin { get; private set; }
    public Planet Destination { get; private set; }


    public static bool LeaderInTransit = false;

    [Serializable]
    public class CargoSave
    {
        public SerializableVector3 Position;
        public string Origin;
        public string Destination;
        public int Stage;
        public SerializableVector3 Stage1;
        public float Travel;
        public bool InnerTravel;
        public CargoType Type;
        public int Amount;
        public Registry.Resources? Resource;
        public Registry.AdvancedResources? AdvancedResource;
        public SerializableVector3[] Path;
    }

    void Awake()
    {
        CargoSound.StartSound();
    }

    void OnDestroy()
    {
        CargoSound.StopSound();
    }

    public CargoSave GetSave()
    {
        var save = new CargoSave();

        save.Position = new SerializableVector3(transform.position);
        save.Origin = Origin.Name;
        save.Destination = Destination.Name;
        save.Stage = _stage;
        save.Stage1 = new SerializableVector3(_stage1);
        save.Travel = _travel;
        save.InnerTravel = _innerTravel;
        save.Type = Type;
        save.Amount = Amount;
        save.Resource = Resource;
        save.AdvancedResource = AdvancedResource;
        save.Path = _path.Select(t => new SerializableVector3(t)).ToArray();

        return save;
    }

    public void LoadSave(CargoSave save)
    {
        transform.position = save.Position.Value;
        var planets = FindObjectsOfType<Planet>(false);
        Origin = Array.Find(planets, t => t.Name == save.Origin);
        Destination = Array.Find(planets, t => t.Name == save.Destination);
        _stage = save.Stage;
        _stage1 = save.Stage1.Value;
        _travel = save.Travel;
        _innerTravel = save.InnerTravel;
        Type = save.Type;
        Amount = save.Amount;
        Resource = save.Resource;
        AdvancedResource = save.AdvancedResource;
        _path = save.Path.Select(t => t.Value).ToArray();

    }

    public Vector3 GetMargin(Planet origin, Planet destination)
    {
        var margin = origin.GetComponent<Collider>().bounds.extents.x;

        // The spawn margin ensures the cargo doesn't spawn on the planet
        return origin.transform.position.Direction(destination.transform.position) * margin;
    }

    public bool HasArrived()
    {
        Bounds bounds = Destination.GetComponent<Collider>().bounds;
        return gameObject.GetComponent<Collider>().bounds.Intersects(bounds) || (!_innerTravel && _stage == 2 && _travel >= 1);
    }

    public void Initialize(Planet origin, Planet destination)
    {
        LeaderInTransit = true;

        Type = CargoType.Leader;
        Origin = origin;

        Origin.HasPlayer = false;

        Destination = destination;
        _innerTravel = origin.transform.parent.GetComponent<StellarSystem>() == destination.transform.parent.GetComponent<StellarSystem>();
        // smh comparing transform doesn't work
    }

    public void Initialize(Planet origin, Planet destination, int amount)
    {
        Type = CargoType.People;

        origin.TakePeople(amount);
        Amount = amount;

        Origin = origin;
        Destination = destination;
        _innerTravel = origin.transform.parent.GetComponent<StellarSystem>() == destination.transform.parent.GetComponent<StellarSystem>();
        // smh comparing transform doesn't work
    }

    public void Initialize(Planet origin, Planet destination, int amount, Registry.Resources resource)
    {
        Type = CargoType.Resource;

        origin.TakeResource(resource, amount);
        Amount = amount;
        Resource = resource;

        Origin = origin;
        Destination = destination;
        _innerTravel = origin.transform.parent.GetComponent<StellarSystem>() == destination.transform.parent.GetComponent<StellarSystem>();
        // smh comparing transform doesn't work
    }
    public void Initialize(Planet origin, Planet destination, int amount, Registry.AdvancedResources resource)
    {
        Type = CargoType.AdvancedResource;

        origin.TakeResource(resource, amount);
        Amount = amount;
        AdvancedResource = resource;

        Origin = origin;
        Destination = destination;
        _innerTravel = origin.transform.parent.GetComponent<StellarSystem>() == destination.transform.parent.GetComponent<StellarSystem>();
        // smh comparing transform doesn't work
    }

    private bool _innerTravel;
    private int _stage;
    private float _travel = 0;
    private Vector3 _stage1;
    private Vector3 _position;

    private float _blink = 1;
    private bool _downTime;
    private bool _blinkStep = false;

    void Update()
    {
        if (_downTime)
        {
            _blink += Time.deltaTime;
            if(_blink >= downTime)
            {
                _blink = (!_blinkStep).ToInt();
                _downTime = false;
            }
        }
        else
        {
            if (_blinkStep)
            {
                _blink += Time.unscaledDeltaTime * blinkingSpeed;
            }
            else
            {
                _blink -= Time.unscaledDeltaTime * blinkingSpeed;
            }
            if (!_blink.IsBetween(0, 1))
            {
                _blink = 0;
                _blinkStep = !_blinkStep;
                _downTime = true;
            }
        }
        GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.red * (_blink * 20 - 10));
        if (_innerTravel)
        {
            _travel += Time.deltaTime / Origin.transform.Distance(Destination.transform) * cruiseSpeed;
            transform.parent = Origin.transform.parent;
            transform.localPosition = Vector3.Slerp(Origin.transform.localPosition + GetMargin(Origin, Destination), Destination.transform.position, _travel);
        }
        else
        {
            switch (_stage)
            {
                case 0:
                    NextStage();
                    break;
                case 1:
                    _travel += Time.deltaTime
                        / Origin.transform.position.Distance(Destination.transform.position)
                        * outerSystemCruiseSpeed;
                    _position = ExitingPath();
                    if (_travel >= 0.5f || transform.position.Distance(Origin.transform.position) > 3 || transform.position.Distance(Destination.transform.position) < transform.position.Distance(Origin.transform.position))
                    {
                        _stage1 = _position;
                        NextStage();
                    }
                    break;
                case 2:
                    _travel += Time.deltaTime / _stage1.Distance(Destination.transform.position) * outerSystemCruiseSpeed;
                    _position = Vector3.Lerp(_stage1, Destination.transform.position, _travel);
                    if(transform.position.Distance(Destination.transform.parent.position) < 5 || transform.position.Distance(Destination.transform.position) < 3)
                    {
                        _stage1 = _position - Origin.transform.parent.position;
                        NextStage();
                    }
                    break;
                case 3:
                    _travel += Time.deltaTime / _stage1.Distance(Destination.transform.localPosition) * outerSystemCruiseSpeed;
                    _position = EnteringPath();
                    break;
            }
            transform.position = Vector3.Lerp(transform.position, _position, Time.deltaTime * 3);
        }
        if (HasArrived())
        {
            switch (Type)
            {
                case CargoType.Leader:
                    LeaderInTransit = false;
                    Destination.HasPlayer = true;
                    if (!_innerTravel)
                    {
                        Scoring.systems++;
                    }
                    var orderL = new OrderHandler.Order(
                        OrderHandler.OrderType.TriumphantArrival,
                        60,
                        0.5f,
                        5,
                        null);
                    OrderHandler.Instance.Queue(orderL, Destination);
                    break;
                case CargoType.Resource:
                    var orderR = new OrderHandler.Order(
                    OrderHandler.OrderType.UnpackingCargo,
                    20,
                    0.5f,
                    3,
                    new OrderHandler.OrderExec(Destination, Resource.Value, Amount));
                    OrderHandler.Instance.Queue(orderR, Destination);
                    break;
                case CargoType.AdvancedResource:
                    var orderAR = new OrderHandler.Order(
                    OrderHandler.OrderType.UnpackingCargo,
                    20,
                    0.5f,
                    3,
                    new OrderHandler.OrderExec(Destination, AdvancedResource.Value, Amount));
                    OrderHandler.Instance.Queue(orderAR, Destination);
                    break;
                case CargoType.People:
                    Destination.AddPeople(Amount);
                    break;
            }
            Destroy(gameObject);
        }

        if (_path.Length == 0 || _path[_path.Length - 1].Distance(transform.position) >= 0.5f)
        {
            _path = _path.Add(transform.position);
            GetComponentInChildren<DottedLineRenderer>().SetDottedLine(_path);
        }
    }

    private void NextStage()
    {
        switch (_stage)
        {
            case 0:
                transform.position = Origin.transform.position + GetMargin(Origin, Destination);
                _position = transform.position;
                break;
            case 1:
                break;
            case 2:
                _stage1 = Origin.transform.parent.position + _stage1;
                _stage1 = _stage1 - Destination.transform.parent.position;
                break;
            default:
                break;
        }
        _stage++;
        _travel = 0;
    }

    private Vector3 ExitingPath()
    {
        Vector3 point = Origin.transform.position;
        Vector3 nextPoint;
        bool done = false;
        for (int i = 0; i < 100; i++)
        {
            nextPoint = Origin.transform.parent.position + Vector3.Slerp(Origin.transform.localPosition + GetMargin(Origin, Destination), Destination.transform.position - Origin.transform.parent.position, i/100f);
            Debug.DrawLine(point, nextPoint, Color.red);
            if (!done && (i / 100f >= 0.5f || point.Distance(Origin.transform.position) > 3 || point.Distance(Destination.transform.position) < point.Distance(Origin.transform.position)))
            {
                _stage1 = point;
                Debug.DrawLine(transform.position, _stage1, Color.green);
                done = true;
            }
            point = nextPoint;

        }
        return Origin.transform.parent.position + Vector3.Slerp(Origin.transform.localPosition + GetMargin(Origin, Destination), Destination.transform.position - Origin.transform.parent.position, _travel);
    }

    private Vector3 EnteringPath()
    {
        var stageWorld = Destination.transform.position - Origin.transform.parent.position;
        Vector3 point = Origin.transform.position;
        Vector3 nextPoint;
        for (int i = 0; i < 100; i++)
        {
            nextPoint = Origin.transform.parent.position + Vector3.Slerp(Origin.transform.localPosition + GetMargin(Origin, Destination), stageWorld, i / 100f);
            Debug.DrawLine(point, nextPoint, Color.red);
            point = nextPoint;
        }
        point = Destination.transform.parent.position + _stage1;
        for (int i = 0; i < 100; i++)
        {
            nextPoint = Destination.transform.parent.position + (Vector3.Slerp(_stage1, Destination.transform.localPosition, i / 100f));
            Debug.DrawLine(point, nextPoint, Color.blue);
            point = nextPoint;
        }
        return Destination.transform.parent.position + (Vector3.Slerp(_stage1, Destination.transform.localPosition, _travel));
    }
}
