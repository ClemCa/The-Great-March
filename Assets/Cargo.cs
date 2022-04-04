using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class Cargo : MonoBehaviour
{
    public enum CargoType
    {
        Resource,
        People
    }

    [SerializeField] private float cruiseSpeed = 2.0f;
    [SerializeField] private float outerSystemCruiseSpeed = 1.0f;
    [SerializeField] private float blinkingSpeed = 0.5f;
    [SerializeField] private float downTime = 1;

    public CargoType Type { get; private set; }

    public int Amount { get; private set; }
    public Registry.Resources? Resource { get; private set; }

    public Planet Origin { get; private set; }
    public Planet Destination { get; private set; }


    public Vector3 GetMargin(Planet origin, Planet destination)
    {
        var margin = origin.GetComponent<Collider>().bounds.extents.x;

        // The spawn margin ensures the cargo doesn't spawn on the planet
        return origin.transform.position.Direction(destination.transform.position) * margin;
    }

    public bool HasArrived()
    {
        Bounds bounds = Destination.GetComponent<Collider>().bounds;
        return gameObject.GetComponent<Collider>().bounds.Intersects(bounds);
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
            GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.red * (_blink * 20 - 10));
            if (!_blink.IsBetween(0, 1))
            {
                _blink = 0;
                _blinkStep = !_blinkStep;
                _downTime = true;
            }
        }



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
                    var stageWorld = (Vector3.Lerp(Origin.transform.position + GetMargin(Origin, Destination), Destination.transform.position, 0.8f));
                    _stage1 = stageWorld - Origin.transform.parent.position;
                    _travel += Time.deltaTime / Origin.transform.localPosition.Distance(_stage1) * outerSystemCruiseSpeed;
                    _position = Origin.transform.parent.position + (Vector3.Slerp(Origin.transform.localPosition + GetMargin(Origin, Destination), _stage1, _travel));
                    if (_travel >= 0.5f || transform.position.Distance(Destination.transform.position) < transform.position.Distance(Origin.transform.position))
                    {
                        _stage1 = _position - Origin.transform.parent.position;
                        NextStage();
                    }
                    break;
                case 2:
                    _travel += Time.deltaTime / _stage1.Distance(Destination.transform.localPosition) * outerSystemCruiseSpeed;
                    _position = Destination.transform.parent.position + (Vector3.Slerp(_stage1, Destination.transform.localPosition, _travel));
                    break;
            }
            transform.position = Vector3.Lerp(transform.position, _position, Time.deltaTime * 3);
        }
        if (HasArrived())
        {
            
            if (Type == CargoType.Resource)
            {
                var order = new OrderHandler.Order(
                    OrderHandler.OrderType.UnpackingCargo,
                    20,
                    0.5f,
                    3,
                    () => {
                        Destination.AddResource(Resource.Value, Amount);
                    });
                OrderHandler.Instance.Queue(order, Destination);
            }
            else
            {
                Destination.AddPeople(Amount);
            }
            Destroy(gameObject);
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
                _stage1 = Origin.transform.parent.position + _stage1;
                _stage1 = _stage1 - Destination.transform.parent.position;
                break;
            default:
                break;
        }
        _stage++;
        _travel = 0;
    }
}
