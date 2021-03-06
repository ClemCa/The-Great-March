using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueMenu : MonoBehaviour
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
        if (_targetMode && (Planet.Selected != null).OnceIfTrueGate("QueueMenu".GetHashCode()))
        {
            _target = Planet.Selected.transform;
            _rectTransform.position = Camera.main.WorldToScreenPoint(Planet.Selected.transform.position);
        }
        if (Planet.Selected == null)
        {
            _rectTransform.anchoredPosition = new Vector3(0, 0);
            return;
        }
        if (_targetMode)
        {
            if (Planet.Selected != _target)
            {
                _target = Planet.Selected.transform;
                _rectTransform.position = Camera.main.WorldToScreenPoint(Planet.Selected.transform.position);
            }
            _rectTransform.position = Vector3.Lerp(_rectTransform.position, Camera.main.WorldToScreenPoint(_target.position), Time.unscaledDeltaTime * 5);
        }
        else
        {
            _rectTransform.anchoredPosition = Vector3.Lerp(_rectTransform.anchoredPosition, new Vector3(_rectTransform.sizeDelta.x, 0), Time.unscaledDeltaTime * 5);
        }
    }
}
