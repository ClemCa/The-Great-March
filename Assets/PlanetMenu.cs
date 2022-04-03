using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class PlanetMenu : MonoBehaviour
{
    [SerializeField] private bool _targetMode;

    private RectTransform _rectTransform;
    private Transform _target;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if(_targetMode && (Planet.Selected != null).OnceIfTrueGate("PlanetMenu".GetHashCode()))
        {
            _target = Planet.Selected.transform;
            _rectTransform.position = Camera.main.WorldToScreenPoint(Planet.Selected.transform.position);
        }
        if (Planet.Selected == null)
        {
            _rectTransform.anchoredPosition = new Vector3(1920,0);
            return;
        }
        if (_targetMode)
        {
            if (Planet.Selected != _target)
            {
                _target = Planet.Selected.transform;
                _rectTransform.position = Camera.main.WorldToScreenPoint(Planet.Selected.transform.position);
            }
            _rectTransform.position = Vector3.Lerp(_rectTransform.position, Camera.main.WorldToScreenPoint(_target.position), Time.deltaTime * 5);
        }
        else
        {
            _rectTransform.anchoredPosition = Vector3.Lerp(_rectTransform.anchoredPosition, new Vector3(1920-_rectTransform.sizeDelta.x, 0), Time.deltaTime * 5);
        }
    }
}
