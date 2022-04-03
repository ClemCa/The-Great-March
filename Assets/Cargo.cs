using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cargo : MonoBehaviour
{
    public enum CargoType
    {
        Resource,
        People
    }

    [SerializeField] private float cruiseSpeed = 1.0f;
    [SerializeField] private float planetMargin = 3.0f;

    public CargoType Type { get; private set; }

    public int Amount { get; private set; }
    public PlanetRegistry.Resources? Resource { get; private set; }

    public Planet Origin { get; private set; }
    public Planet Destination { get; private set; }

    // Calculate the vector to start at by the planet since the start position needs
    // to be known before Initialize can be called
    public static Vector3 CalculatePlanetStart(Planet origin, Planet destination)
    {
        Vector3 direction = (destination.transform.position - origin.transform.position).normalized;
        Bounds bounds = origin.GetComponent<Collider>().bounds;

        // The spawn margin ensures the cargo doesn't spawn on the planet
        return Vector3.Scale(bounds.extents, direction) + planetMargin;
    }

    public bool HasArrived()
    {
        Bounds bounds = Destination.GetComponent<Collider>().bounds;
        // Expand mutates the Bounds, which we don't want of course.
        Bounds expanded = new Bounds(bounds.center, bounds.size);
        expanded.Expand(planetMargin);

        return gameObject.GetComponent<Collider>().bounds.Intersects(expanded);
    }

    public void Initialize(Planet origin, Planet destination, int amount)
    {
        Type = CargoType.People;

        origin.TakePeople(amount);
        Amount = amount;

        Origin = origin;
        Destination = destination;
    }

    public void Initialize(Planet origin, Planet destination, int amount, PlanetRegistry.Resources resource)
    {
        Type = CargoType.Resource;

        origin.TakeResource(resource, amount);
        Amount = amount;
        Resource = resource;

        Origin = origin;
        Destination = destination;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = (Destination.transform.position - Origin.transform.position).normalized;
        transform.position += direction * cruiseSpeed * Time.deltaTime;

        if (HasArrived())
        {
            if (Type == CargoType.Resource)
                Destination.AddResource(Resource.Value, Amount);
            else  // Type == CargoType.People
                Destination.AddPeople(Amount);

            Destroy(gameObject);
        }
    }
}
