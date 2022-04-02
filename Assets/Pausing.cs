using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEngine.UI;

public class Pausing : MonoBehaviour
{
    private static bool _paused;

    public static bool Paused { get => _paused; }

    public static void Pause()
    {
        _paused = true;
        Time.timeScale = 0;
    }

    public static void Unpause()
    {
        _paused = false;
        Time.timeScale = 1;
    }

    public static void FlipPause()
    {
        _paused = !_paused;
        Time.timeScale = (!_paused).ToInt();
    }

    private float _currentTransparency = 0;
    private bool _step;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FlipPause();
        }
        if ((!_paused).OnceIfTrueGate("Pause".GetHashCode()))
        {
            transform.GetChild(0).GetComponent<Image>().color = transform.GetChild(0).GetComponent<Image>().color.SetA(0);
            transform.GetChild(1).GetComponent<Image>().color = transform.GetChild(1).GetComponent<Image>().color.SetA(0);
            _currentTransparency = 0;
            _step = false;
        }
        if(_paused)
        {
            if (_step)
                _currentTransparency += Time.unscaledDeltaTime;
            else
                _currentTransparency -= Time.unscaledDeltaTime;
            if (!_currentTransparency.IsBetween(0, 1))
            {
                _currentTransparency = _currentTransparency.Clamp01();
                _step = !_step;
            }
            transform.GetChild(0).GetComponent<Image>().color = transform.GetChild(0).GetComponent<Image>().color.SetA(_currentTransparency);
            transform.GetChild(1).GetComponent<Image>().color = transform.GetChild(1).GetComponent<Image>().color.SetA(_currentTransparency);
        }
    }

}
