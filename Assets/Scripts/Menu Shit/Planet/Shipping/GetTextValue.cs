using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GetTextValue : MonoBehaviour
{
    [SerializeField] private TextCallback _getText;

    [Serializable]
    public class TextCallback : SerializableCallback<string> { };

    void Update()
    {
        GetComponent<TMPro.TMP_Text>().text = _getText.Invoke();
    }
}
