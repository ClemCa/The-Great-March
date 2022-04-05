using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoringWriter : MonoBehaviour
{
    void Start()
    {
        GetComponent<TMPro.TMP_Text>().text = "As a leader, you helped your people"
            + "survive " + Scoring.survivalTime
            + " minutes.\n"
            + "You went through " + Scoring.systems
            + " systems.\n"
            + "You extracted " + Scoring.naturalResourcesUnits
            + " units of natural"
            + " resources, and produced " + Scoring.advancedResourcesUnits
            + " advanced resources.\n"
            + "You made " + Scoring.facilitiesCount
            + " facilities and "
            + Scoring.transformativeFacilitiesCount
            + " transformative facilities.\n"
            + "And all of that, over " + Scoring.totalTime
            + " minutes";
    }
}
