using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Planet : MonoBehaviour
{
    #region localStorage
    private Dictionary<PlanetRegistry.Ressources, int> _ressources = new Dictionary<PlanetRegistry.Ressources, int>();
    private int _people = 0;
    private List<PlanetRegistry.Facilities> _facilities = new List<PlanetRegistry.Facilities>();
    #endregion localStorage
    #region Accessibility
    public Dictionary<PlanetRegistry.Ressources, int> Ressources { get => _ressources; set => _ressources = value; }
    public List<PlanetRegistry.Facilities> Facilities { get => _facilities; set => _facilities = value; }


    public int GetRessource(PlanetRegistry.Ressources ressoure)
    {
        return _ressources[ressoure];
    }

    public void AddRessource(PlanetRegistry.Ressources ressource, int count = 1)
    {
        _ressources[ressource] += count;
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
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100))
            {
                if (hit.transform != transform)
                    return;
                Debug.Log("Selected " + gameObject);
            }
        }
    }
    #endregion Routines
    #region Generation
    public void Initialize(PlanetRegistry.SystemType systemType, PlanetRegistry.Ressources[] ressources, int[] ressourcesCount)
    {
        for (int i = 0; i < System.Enum.GetNames(typeof(PlanetRegistry.Ressources)).Length; i++)
        {
            int r = System.Array.FindIndex(ressources, t => t == (PlanetRegistry.Ressources)i);
            _ressources.Add((PlanetRegistry.Ressources)i, r == -1 ? 0 : ressourcesCount[r]);
        }
        Destroy(GetComponent<MeshRenderer>());
        switch (systemType)
        {
            case PlanetRegistry.SystemType.Alien:
                Instantiate(PlanetRegistry.Instance.GetRandomPlanet(PlanetRegistry.PlanetType.Alien), transform);
                break;
            case PlanetRegistry.SystemType.Cold:
                Instantiate(PlanetRegistry.Instance.GetRandomPlanet(PlanetRegistry.PlanetType.Frozen), transform);
                break;
            case PlanetRegistry.SystemType.Hot:
                Instantiate(PlanetRegistry.Instance.GetRandomPlanet(PlanetRegistry.PlanetType.Desert), transform);
                break;
            case PlanetRegistry.SystemType.Temperate:
                if (Random.Range(0, 1) == 0)
                    Instantiate(PlanetRegistry.Instance.GetRandomPlanet(PlanetRegistry.PlanetType.Temperate), transform);
                else
                    Instantiate(PlanetRegistry.Instance.GetRandomPlanet(PlanetRegistry.PlanetType.Earth), transform);
                break;
            default:
                Debug.Log(systemType);
                break;
        }
    }
    #endregion Generation
}