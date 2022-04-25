using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DottedLineRenderer : MonoBehaviour
{
    [SerializeField] private GameObject _dotPrefab;
    private Vector3[] _line = new Vector3[] { };
    private GameObject[] _dots = new GameObject[] { };


    void Update()
    {
        while(_dots.Length < _line.Length)
        {
            var r = Instantiate(_dotPrefab, transform);
            _dots = _dots.Add(r);
        }
        for(int i = 0; i < _dots.Length; i++)
        {
            _dots[i].transform.position = _line[i];
        }
    }

    public void SetDottedLine(Vector3[] points)
    {
        _line = points;
    }
}
