using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueUpdater : MonoBehaviour
{
    [SerializeField] private GameObject _displayOrderPrefab;
    void Update()
    {
        if (!Planet.Selected)
        {
            return;
        }
        var orders = OrderHandler.Instance.GetPlanetQueue(Planet.Selected);
        int i;
        for(i = 0; i < transform.childCount; i++)
        {
            if (i >= orders.Length)
            {
                Clear(i);
                return;
            }
            transform.GetChild(i).GetComponent<QueueDisplay>().Order = orders[i];
        }
#pragma warning disable CS1717 // Assignment made to same variable
        for (i = i; i < orders.Length; i++)
#pragma warning restore CS1717 // Assignment made to same variable
        {
            Instantiate(_displayOrderPrefab, transform).GetComponent<QueueDisplay>().Order = orders[i];
        }
    }
    
    private void Clear(int from)
    {
        for(int i = from; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
