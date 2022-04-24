using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _creditPrefab;
    [SerializeField] private Credit[] _credits;

    [Serializable]
    private class Credit
    {
        public string Name = "";
        public string Description = "";
        public string URL = "";

        public Credit(string name, string description, string url)
        {
            Name = name;
            Description = description;
            URL = url;
        }
    }

    void Start()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        foreach(var credit in _credits)
        {
            var c = Instantiate(_creditPrefab, transform);
            c.GetComponent<CreditClick>().link = credit.URL;
            c.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = credit.Name;
            c.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = credit.Description;
        }
    }
}
