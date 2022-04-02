using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlanetGenerator : MonoBehaviour
{
    private Dictionary<PlanetRegistry.Ressources, int> _ressources = new Dictionary<PlanetRegistry.Ressources, int>();

    public Dictionary<PlanetRegistry.Ressources, int> Ressources { get => _ressources; set => _ressources = value; }

    public void Initialize(PlanetRegistry.SystemType systemType, PlanetRegistry.Ressources[] ressources, int[] ressourcesCount)
    {
        for(int i = 0; i < System.Enum.GetNames(typeof(PlanetRegistry.Ressources)).Length; i++)
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
}