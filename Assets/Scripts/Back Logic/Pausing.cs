using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Pausing : MonoBehaviour
{
    private static Pausing _instance;

    private static bool _paused = false;
    private static bool _blocked = false;
    private static float _timeTarget = 1;

    public static bool Paused { get => _paused; }

    public static void Block()
    {
        _blocked = true;
    }

    public static void Unblock()
    {
        _blocked = false;
    }

    public static void Pause()
    {
        _paused = true;
        _timeTarget = 0.01f;

        _instance.GetComponent<AudioSource>().Play();
        _instance.GetComponent<AudioSource>().time = 0.12f;
        _instance.GetComponent<AudioSource>().volume = 0.5f;
        AudioListener.volume = 0.25f;
    }

    public static void InstantPause()
    {
        _paused = true;
        Time.timeScale = 0;
        _timeTarget = 0;
        _instance.GetComponent<AudioSource>().Play();
        _instance.GetComponent<AudioSource>().time = 0.12f;
        _instance.GetComponent<AudioSource>().volume = 0.5f;
        AudioListener.volume = 0.25f;
    }

    public static void Unpause()
    {
        _paused = false;

        _timeTarget = 1f;

        _instance.GetComponent<AudioSource>().Play();
        _instance.GetComponent<AudioSource>().volume = 0.5f * 0.25f;
        _instance.GetComponent<AudioSource>().time = 0.12f;
        AudioListener.volume = 1f;
    }

    public static void FlipPause()
    {
        if (_paused) Unpause();
        else Pause();
    }

    void Awake()
    {
        _instance = this;
    }

    private float _currentTransparency = 0;
    private bool _step;

    // Update is called once per frame
    void Update()
    {
        var dir = (_timeTarget - Time.timeScale).Sign();
        Time.timeScale = (Time.timeScale + dir * Time.unscaledDeltaTime).Clamp(Time.timeScale.Min(_timeTarget), Time.timeScale.Max(_timeTarget));

        Debug.Log(_paused);
        if (Input.GetKeyDown(KeyCode.Escape) && !_blocked)
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

    void Start()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene current)
    {
        Time.timeScale = 1;
    }

}
