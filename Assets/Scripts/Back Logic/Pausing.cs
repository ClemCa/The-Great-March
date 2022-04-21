using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Pausing : MonoBehaviour
{
    private static Pausing _instance;

    private static bool _paused;
    private static bool _blocked;

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
        if (Time.timeScale != 1)
            return;

        _paused = true;
        ClemCAddons.Utilities.Lerper.ConstantLerp(Time.timeScale, 0.01f, 1.5f, (f) => { Time.timeScale = f; });

        _instance.GetComponent<AudioSource>().Play();
        _instance.GetComponent<AudioSource>().time = 0.12f;
        _instance.GetComponent<AudioSource>().volume = 0.5f;
        AudioListener.volume = 0.25f;
    }

    public static void Unpause()
    {
        if (Time.timeScale != 0.01f)
            return;

        _paused = false;

        ClemCAddons.Utilities.Lerper.ConstantLerp(Time.timeScale, 1, 1.5f   , (f) => { Time.timeScale = f; });

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
