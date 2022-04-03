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
    private List<float> _facilitiesProgression = new List<float>();
    private static Planet selected;
    private static PlanetRegistry.Resources moveSelection;
    private static int moveSelectionCount;
    private static Planet moveSelectionOrigin;
    private static bool moveSelectionType;
    #endregion localStorage
    #region Accessibility
    public Dictionary<PlanetRegistry.Resources, int> Resources { get => _resources; set => _resources = value; }
    public List<PlanetRegistry.Facilities> Facilities { get => _facilities; set => _facilities = value; }
    public static Planet Selected { get => selected;}
    public PlanetRegistry.Resources[] AvailableResources { get => _availableResources;}


    public bool GetSlot(PlanetRegistry.Resources resource)
    {
        return _availableResources.FindIndex(resource) != -1;
    }

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

    public void RegisterBuiltFacility(PlanetRegistry.Facilities facility)
    {
        _facilities.Add(facility);
        _facilitiesProgression.Add(0);
    }

    public bool IsFacilityBuilt(PlanetRegistry.Facilities facility)
    {
        return _facilities.FindIndex(t => t == facility) != -1;
    }

    public bool CanBuildFacility(PlanetRegistry.Facilities facility)
    {
        return _availableResources.FindIndex(PlanetRegistry.Instance.GetAssociatedResource(facility)) != -1;
    }

    public bool HasFacility(PlanetRegistry.Resources resource)
    {
        var r = _availableResources.FindIndex(resource);
        var t = PlanetRegistry.Instance.GetAssociatedFacilities(resource);
        foreach (var facility in t)
        {
            var i = _facilities.FindIndex(t => t == facility);
            if (i != -1)
                return true;
        }
        return false;
    }

    public PlanetRegistry.Facilities GetFacility(PlanetRegistry.Resources resource)
    {
        var r = _availableResources.FindIndex(resource);
        var t = PlanetRegistry.Instance.GetAssociatedFacilities(resource);
        foreach (var facility in t)
        {
            var i = _facilities.FindIndex(t => t == facility);
            if (i != -1)
                return _facilities[i];
        }
        return _facilities[0];
    }

    public float GetFactoryProgression(PlanetRegistry.Resources resource)
    {
        var r = _availableResources.FindIndex(resource);
        if (r == -1)
            return 0;
        var t = PlanetRegistry.Instance.GetAssociatedFacilities(resource);
        int index = -1;
        foreach(var facility in t)
        {
            var i = _facilities.FindIndex(t => t == facility);
            if (i != -1)
                index = i;
        }
        if (index == -1)
            return 0;
        return _facilitiesProgression[index];
    }

    public float GetFactoryProgression(PlanetRegistry.Facilities facility)
    {
        var index = _facilities.FindIndex(t => t == facility);
        if (index == -1)
            return 0;
        return _facilitiesProgression[index];
    }

    public void SetAvailableResources(PlanetRegistry.Resources[] availableResources)
    {
        _availableResources = availableResources;
    }
    public void EngageMoveSelectionMode(PlanetRegistry.Resources resource, int count)
    {
        Pausing.Block();
        moveSelection = resource;
        moveSelectionCount = count;
        moveSelectionOrigin = selected;
        moveSelectionType = true;
        selected = null;
    }
    public void EngageMoveSelectionMode(int count)
    {
        Pausing.Block();
        moveSelectionCount = count;
        moveSelectionOrigin = selected;
        moveSelectionType = false;
        selected = null;
    }
    #endregion Accessibility
    #region Routines
    void Update()
    {
        if (moveSelectionCount == 0)
            StandardUpdate();
        else
            MoveSelectionMode();
    }
    private void MoveSelectionMode()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
        };
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (moveSelectionOrigin == this)
        {
            GetComponentInChildren<Outline>().color = 0;
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            moveSelectionCount = 0;
            selected = moveSelectionOrigin;
            ShippingSubMenu.Show();
            ShippingSubMenu.ResetMenu();
            if (moveSelectionType)
                ShippingSubMenu.Instance.ShowResourcesChoice();
            else
                ShippingSubMenu.Instance.ShowPeopleChoice();
            Pausing.Unblock();

        }
        if (Physics.Raycast(ray, out var hit, 100))
        {
            if (transform == hit.transform || transform.FindDeep(t => t == hit.transform))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (moveSelectionType)
                        CargoGenerator.GenerateCargo(moveSelectionOrigin, this, moveSelection, moveSelectionCount);
                    else
                        CargoGenerator.GenerateCargo(moveSelectionOrigin, this, moveSelectionCount);
                    moveSelectionCount = 0;
                    ShippingSubMenu.ResetMenu();
                    Pausing.Unblock();
                }
                else
                {
                    GetComponentInChildren<Outline>().color = 2;
                }
            }
            else
            {
                GetComponentInChildren<Outline>().color = 1;
            }
        }
    }
    private void StandardUpdate()
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


            foreach (var r in results)
            {
                if (r.gameObject.name == "Menu" || r.gameObject.FindParentDeep("Menu"))
                    return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100))
            {
                if (transform == hit.transform || transform.FindDeep(t => t == hit.transform))
                {
                    if (selected == this)
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
    public void Initialize(PlanetRegistry.SystemType systemType, PlanetRegistry.Resources[] resources, bool _doNotRegenerate = false)
    {
        for (int i = 0; i < System.Enum.GetNames(typeof(PlanetRegistry.Resources)).Length; i++)
        {
            _resources.Add((PlanetRegistry.Resources)i, 0);
        }
        if (_doNotRegenerate)
            return;
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