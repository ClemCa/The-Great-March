using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class PlanetMenu : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Transform _target;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if((Planet.Selected != null).OnceIfTrueGate("PlanetMenu".GetHashCode()))
        {
            _target = Planet.Selected.transform;
            _rectTransform.position = Camera.main.WorldToScreenPoint(Planet.Selected.transform.position);
        }
        if (Planet.Selected == null)
        {
            _rectTransform.anchoredPosition = new Vector3(1920,0);
            return;
        }
        if(Planet.Selected != _target)
        {
            _target = Planet.Selected.transform;
            _rectTransform.position = Camera.main.WorldToScreenPoint(Planet.Selected.transform.position);
        }
        _rectTransform.position = Vector3.Lerp(_rectTransform.position, Camera.main.WorldToScreenPoint(_target.position), Time.deltaTime * 5);
    }
}
