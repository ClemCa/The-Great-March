using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GetTextValue : MonoBehaviour
{
    [SerializeField] private TextCallback _getText;

    // Base classes in the Siccity.SerializableCallback package are not [Serializable],
    // so Unity's serialization analyzer flags this hierarchy. Suppress it locally.
#pragma warning disable UAC1002
    [Serializable]
    public class TextCallback : SerializableCallback<string> { };
#pragma warning restore UAC1002

    void Update()
    {
        GetComponent<TMPro.TMP_Text>().text = _getText.Invoke();
    }
}
