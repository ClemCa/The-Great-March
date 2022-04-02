using cakeslice;
using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Planet : MonoBehaviour
{
    #region localStorage
    private Dictionary<PlanetRegistry.Resources, int> _resources = new Dictionary<PlanetRegistry.Resources, int>();
    private int _people = 0;
    private PlanetRegistry.Resources[] _availableResources;
    private List<PlanetRegistry.Facilities> _facilities = new List<PlanetRegistry.Facilities>();
    private static Planet selected;
    #endregion localStorage
    #region Accessibility
    public Dictionary<PlanetRegistry.Resources, int> Resources { get => _resources; set => _resources = value; }
    public List<PlanetRegistry.Facilities> Facilities { get => _facilities; set => _facilities = value; }
    public static Planet Selected { get => selected;}
    public PlanetRegistry.Resources[] AvailableResources { get => _availableResources;}

    public int GetResource(PlanetRegistry.Resources resoure)
    {
        return _resources[resoure];
    }

    public void AddResource(PlanetRegistry.Resources resource, int count = 1)
    {
        _resources[resource] += count;
    }

    public void TakeResource(PlanetRegistry.Resources resource, int count = 1)
    {
        _resources[resource] -= count;
    }

    public int GetPeople()
    {
        return _people;
    }

    public void AddPeople(int count)
    {
        _people += count;
    }

    public void TakePeople(int count)
    {
        _people -= count;
    }

    public void FacilityBuilt(PlanetRegistry.Facilities facility)
    {
        _facilities.Add(facility);
    }

    public bool IsFacilityBuild(PlanetRegistry.Facilities facility)
    {
        return _facilities.FindIndex(t => t == facility) != -1;
    }

    #endregion Accessibility
    #region Routines
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
            };
            pointerData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerData, results);

            foreach(var r in results)
            {
                if (r.gameObject.name == "PlanetMenu" || r.gameObject.FindParentDeep("PlanetMenu"))
                    return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100))
            {                if(transform == hit.transform || transform.FindDeep(t => t == hit.transform))
                {
                    if(selected == this)
                    {
                        selected = null;
                        return;
                    }
                    selected = this;
                    return;
                }
                if (!hit.transform.TryGetComponent<Planet>(out _) && hit.transform.FindParentWithComponent(typeof(Planet)) == null)
                {
                    selected = null;
                }
            }
            else
            {
                selected = null;
            }
        }
        GetComponentInChildren<Outline>().color = (selected == this).ToInt();
    }
    #endregion Routines
    #region Generation
    public void Initialize(PlanetRegistry.SystemType systemType, PlanetRegistry.Resources[] resources)
    {
        for (int i = 0; i < System.Enum.GetNames(typeof(PlanetRegistry.Resources)).Length; i++)
        {
            _resources.Add((PlanetRegistry.Resources)i, 0);
        }
        _availableResources = resources;
        Destroy(GetComponent<MeshRenderer>());
        for(int i = transform.childCount - 1; i >= 0 ; i--)
        {
            Destroy(transform.GetChild(i).gameObject); // might have cloned too late, after the original spawned a new planet inside
        }
        switch (systemType)
        {
            case PlanetRegistry.SystemType.Alien:
                Instantiate(PlanetRegistry.Instance.GetRandomPlanet(PlanetRegistry.PlanetType.Alien), transform)
                     .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
            case PlanetRegistry.SystemType.Cold:
                Instantiate(PlanetRegistry.Instance.GetRandomPlanet(PlanetRegistry.PlanetType.Frozen), transform)
                     .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
            case PlanetRegistry.SystemType.Hot:
                Instantiate(PlanetRegistry.Instance.GetRandomPlanet(PlanetRegistry.PlanetType.Desert), transform)
                     .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
            case PlanetRegistry.SystemType.Temperate:
                if (Random.Range(0, 1) == 0)
                    Instantiate(PlanetRegistry.Instance.GetRandomPlanet(PlanetRegistry.PlanetType.Temperate), transform)
                        .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                else
                    Instantiate(PlanetRegistry.Instance.GetRandomPlanet(PlanetRegistry.PlanetType.Earth), transform)
                        .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
            default:
                Debug.Log(systemType);
                break;
        }

    }
    #endregion Generation
}