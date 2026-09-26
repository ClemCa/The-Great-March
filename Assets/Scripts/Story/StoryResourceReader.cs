using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reads resource amounts for conditions. Scope decides where the number comes from: the summed
/// total of every planet, the player's leader planet, or a planet addressed by name.
/// </summary>
public static class StoryResourceReader
{
    public static float Read(StoryCondition condition)
    {
        if (condition == null)
            return 0f;

        switch (condition.Scope)
        {
            case StoryResourceScope.PlayerPlanet:
                return ReadPlanet(Planet.LeaderPlanet != null ? Planet.LeaderPlanet : Planet.Selected, condition);
            case StoryResourceScope.NamedPlanet:
                return ReadPlanet(FindByName(condition.PlanetName), condition);
            case StoryResourceScope.Global:
            default:
                return SumGlobal(condition);
        }
    }

    private static float ReadPlanet(Planet planet, StoryCondition condition)
    {
        if (planet == null)
            return 0f;
        if (condition.Advanced)
            return planet.AdvancedResources.TryGetValue((Registry.AdvancedResources)condition.ResourceIndex, out int advanced) ? advanced : 0;
        return planet.Resources.TryGetValue((Registry.Resources)condition.ResourceIndex, out int basic) ? basic : 0;
    }

    private static float SumGlobal(StoryCondition condition)
    {
        var planets = Object.FindObjectsByType<Planet>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        float total = 0f;
        for (int i = 0; i < planets.Length; i++)
        {
            if (planets[i] == null)
                continue;
            if (condition.Advanced)
            {
                if (planets[i].AdvancedResources.TryGetValue((Registry.AdvancedResources)condition.ResourceIndex, out int advanced))
                    total += advanced;
            }
            else if (planets[i].Resources.TryGetValue((Registry.Resources)condition.ResourceIndex, out int basic))
            {
                total += basic;
            }
        }
        return total;
    }

    private static Planet FindByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        var planets = Object.FindObjectsByType<Planet>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < planets.Length; i++)
            if (planets[i] != null && planets[i].Name == name)
                return planets[i];
        return null;
    }
}
