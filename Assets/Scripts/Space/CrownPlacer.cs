using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrownPlacer : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(Planet.LeaderPlanet != null)
        {
            transform.position = Planet.LeaderPlanet.transform.position;
        }
    }
}
