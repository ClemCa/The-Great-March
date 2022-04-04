using cakeslice;
using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Planet : MonoBehaviour
{
    #region localStorage
    private Dictionary<Registry.Resources, int> _resources = new Dictionary<Registry.Resources, int>();
    private Dictionary<Registry.AdvancedResources, int> _advancedResources = new Dictionary<Registry.AdvancedResources, int>();
    private int _people = 0;
    private Registry.Resources[] _availableResources;
    private List<Registry.Facilities> _facilities = new List<Registry.Facilities>();
    private List<float> _facilitiesProgression = new List<float>();
    private List<Registry.TransformationFacilities> _transformationFacilities = new List<Registry.TransformationFacilities>();
    private List<float> _transformationFacilitiesProgression = new List<float>();
    private string _name;
    private int _availableWildcards;
    private static Planet selected;
    private static Registry.Resources moveSelection;
    private static int moveSelectionCount;
    private static Planet moveSelectionOrigin;
    private static bool moveSelectionType;
    #endregion localStorage
    #region Accessibility
    public Dictionary<Registry.Resources, int> Resources { get => _resources; set => _resources = value; }
    public Dictionary<Registry.AdvancedResources, int> AdvancedResources { get => _advancedResources; set => _advancedResources = value; }

    public List<Registry.Facilities> Facilities { get => _facilities; set => _facilities = value; }
    public List<Registry.TransformationFacilities> TransformationFacilities { get => _transformationFacilities; set => _transformationFacilities = value; }

    public static Planet Selected { get => selected;}

    public Registry.Resources[] AvailableResources { get => _availableResources;}

    public string Name { get => _name; }

    public void SetWildcardSlots(int slots)
    {
        _availableWildcards = slots;
    }

    public void SetName(string name)
    {
        _name = name;
    }

    public bool GetSlot(Registry.Resources resource)
    {
        return _availableResources.FindIndex(resource) != -1;
    }

    public int GetWildcardSlots()
    {
        return _availableWildcards;
    }


    public int GetResource(Registry.Resources resoure)
    {
        return _resources[resoure];
    }

    public int GetResource(Registry.AdvancedResources resoure)
    {
        return _advancedResources[resoure];
    }

    public void AddResource(Registry.AdvancedResources resource, int count = 1)
    {
        _advancedResources[resource] += count;
    }

    public void TakeResource(Registry.AdvancedResources resource, int count = 1)
    {
        _advancedResources[resource] -= count;
    }

    public void AddResource(Registry.Resources resource, int count = 1)
    {
        _resources[resource] += count;
    }

    public void TakeResource(Registry.Resources resource, int count = 1)
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

    public void RegisterBuiltFacility(Registry.Facilities facility)
    {
        _facilities.Add(facility);
        _facilitiesProgression.Add(0);
    }


    public void RegisterBuiltFacility(Registry.TransformationFacilities facility)
    {
        _transformationFacilities.Add(facility);
        _transformationFacilitiesProgression.Add(0);
    }

    public bool IsFacilityBuilt(Registry.Facilities facility)
    {
        return _facilities.FindIndex(t => t == facility) != -1;
    }

    public bool CanBuildFacility(Registry.Facilities facility)
    {
        return _availableResources.FindIndex(Registry.Instance.GetAssociatedResource(facility)) != -1;
    }


    public bool CanBuildFacility(Registry.TransformationFacilities facility)
    {
        return _availableWildcards > 0;
    }


    public bool HasFacility(Registry.Resources resource)
    {
        var r = _availableResources.FindIndex(resource);
        var t = Registry.Instance.GetAssociatedFacilities(resource);
        foreach (var facility in t)
        {
            var i = _facilities.FindIndex(t => t == facility);
            if (i != -1)
                return true;
        }
        return false;
    }
    public bool HasFacility(Registry.TransformationFacilities facility)
    {
        return _transformationFacilities.FindIndex(t => t == facility) != -1;
    }

    public Registry.Facilities GetFacility(Registry.Resources resource)
    {
        var r = _availableResources.FindIndex(resource);
        var t = Registry.Instance.GetAssociatedFacilities(resource);
        foreach (var facility in t)
        {
            var i = _facilities.FindIndex(t => t == facility);
            if (i != -1)
                return _facilities[i];
        }
        return _facilities[0];
    }

    public float GetFactoryProgression(Registry.Resources resource)
    {
        var r = _availableResources.FindIndex(resource);
        if (r == -1)
            return 0;
        var t = Registry.Instance.GetAssociatedFacilities(resource);
        int index = -1;
        foreach(var facility in t)
        {
            var i = _facilities.FindIndex(t => t == facility);
            if (i != -1)
                index = i;
        }
        if (index == -1)
            return 0;
        var info = Registry.Instance.GetFacilityInfo(_facilities[index]);
        return _facilitiesProgression[index] / info.Cooldown;
    }

    public float GetFactoryProgression(Registry.Facilities facility)
    {
        var index = _facilities.FindIndex(t => t == facility);
        if (index == -1)
            return 0;
        var info = Registry.Instance.GetFacilityInfo(_facilities[index]);
        return _facilitiesProgression[index] / info.Cooldown;
    }


    public float GetFactoryProgression(Registry.TransformationFacilities facility)
    {
        var index = _transformationFacilities.FindIndex(t => t == facility);
        if (index == -1)
            return 0;
        var info = Registry.Instance.GetFacilityInfo(_facilities[index]);
        return _transformationFacilitiesProgression[index] / info.Cooldown;
    }

    public void SetAvailableResources(Registry.Resources[] availableResources)
    {
        _availableResources = availableResources;
    }
    public void EngageMoveSelectionMode(Registry.Resources resource, int count)
    {
        Pausing.Block();
        moveSelection = resource;
        moveSelectionCount = count;
        moveSelectionOrigin = selected;
        moveSelectionType = true;
        selected = null;
        var planets = FindObjectsOfType<Planet>();
        foreach(var planet in planets)
        {
            planet.GetComponentInChildren<Outline>().color = 1;
        }
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
        RunBasicFacilities();
        RunTransformationFacilities(); ;
    }

    private void RunTransformationFacilities()
    {
        for (int i = 0; i < _transformationFacilities.Count; i++)
        {
            var info = Registry.Instance.GetFacilityInfo(_transformationFacilities[i]);
            bool confirm = true;
            foreach (var resource in info.InputResources)
                if (GetResource(resource) < info.Cost)
                    confirm = false;
            if (!confirm)
                continue;
            _transformationFacilitiesProgression[i] += Time.deltaTime * (_people / 5f);
            if (_transformationFacilitiesProgression[i] > info.Cooldown)
            {
                _transformationFacilitiesProgression[i] = 0;
                foreach(var resource in info.InputResources)
                    TakeResource(resource, info.Cost);
                if (info.Advanced)
                    AddResource(info.OutputResourceTransformation, info.Production);
                else
                    AddResource(info.OutputResource, info.Production);
            }
        }
    }

    private void RunBasicFacilities()
    {
        for(int i = 0; i < _facilities.Count; i++)
        {
            var info = Registry.Instance.GetFacilityInfo(_facilities[i]);
            _facilitiesProgression[i] += Time.deltaTime * (_people / 5f);
            if(_facilitiesProgression[i] > info.Cooldown)
            {
                _facilitiesProgression[i] = 0;
                AddResource(info.AssociatedResource);
            }
        }
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
    public void Initialize(Registry.SystemType systemType, Registry.Resources[] resources, bool _doNotRegenerate = false)
    {
        for (int i = 0; i < System.Enum.GetNames(typeof(Registry.Resources)).Length; i++)
        {
            _resources.Add((Registry.Resources)i, 0);
        }
        for (int i = 0; i < System.Enum.GetNames(typeof(Registry.AdvancedResources)).Length; i++)
        {
            _advancedResources.Add((Registry.AdvancedResources)i, 0);
        }
        if (_doNotRegenerate)
            return;
        var rnd = new System.Random();
        char a = (char)rnd.Next('a', 'z');
        char b = (char)rnd.Next('a', 'z');
        int num = rnd.Next(100, 9999);
        _name = string.Concat(a, b, num);
        _availableResources = resources;
        Destroy(GetComponent<MeshRenderer>());
        for(int i = transform.childCount - 1; i >= 0 ; i--)
        {
            Destroy(transform.GetChild(i).gameObject); // might have cloned too late, after the original spawned a new planet inside
        }
        switch (systemType)
        {
            case Registry.SystemType.Alien:
                Instantiate(Registry.Instance.GetRandomPlanet(Registry.PlanetType.Alien), transform)
                     .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
            case Registry.SystemType.Cold:
                Instantiate(Registry.Instance.GetRandomPlanet(Registry.PlanetType.Frozen), transform)
                     .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
            case Registry.SystemType.Hot:
                Instantiate(Registry.Instance.GetRandomPlanet(Registry.PlanetType.Desert), transform)
                     .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
            case Registry.SystemType.Temperate:
                if (Random.Range(0, 1) == 0)
                    Instantiate(Registry.Instance.GetRandomPlanet(Registry.PlanetType.Temperate), transform)
                        .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                else
                    Instantiate(Registry.Instance.GetRandomPlanet(Registry.PlanetType.Earth), transform)
                        .GetComponent<MeshRenderer>().gameObject.AddComponent<Outline>().color = 0;
                break;
            default:
                Debug.Log(systemType);
                break;
        }

    }
    #endregion Generation
}