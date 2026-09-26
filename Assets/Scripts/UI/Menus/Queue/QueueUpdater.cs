using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QueueUpdater : MonoBehaviour
{
    [SerializeField] private GameObject _displayOrderPrefab;
    private Dictionary<int, QueueDisplay> instanceMapping = new Dictionary<int, QueueDisplay>();

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
            if(orders.Length <= i)
            {
                if (instanceMapping.ContainsKey(i))
                {
                    Destroy(instanceMapping[i].gameObject);
                    instanceMapping.Remove(i);
                }
                else
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }
            else if (instanceMapping.ContainsKey(i))
            {
                instanceMapping[i].Order = orders[i];
                instanceMapping[i].Init();
            }
            else
            {
                var display = Instantiate(_displayOrderPrefab, transform).GetComponent<QueueDisplay>();
                display.Order = orders[i];
                instanceMapping.Add(i, display);
            }
        }
        for (_ = i; i < orders.Length; i++)
        {
            var display = Instantiate(_displayOrderPrefab, transform).GetComponent<QueueDisplay>();
            display.Order = orders[i];
            instanceMapping.Add(i, display);
        }
        UpdateOriginalQueue();
    }

    private void UpdateOriginalQueue()
    {
        List<int> ban = new List<int>();
        var enumerationDictionary = instanceMapping.ToDictionary(t => t.Key, t => t.Value);
        foreach(var display in enumerationDictionary)
        {
            var index = display.Value.transform.GetSiblingIndex();
            if (index != display.Key && !ban.Contains(index))
            {
                var save = instanceMapping[index];
                instanceMapping[index] = display.Value;
                instanceMapping[display.Key] = save;
                OrderHandler.Instance.PermutateOrders(Planet.Selected, display.Key, index);
                ban.Add(index);
                ban.Add(display.Key);
            }
        }
    }
}
