using UnityEngine;
using ClemCAddons.MultiScene;
using UnityEngine.SceneManagement;
using ClemCAddons.Player;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private string _scene;
    [SerializeField] private float _preloadDistance = 10f;
    private Transform _player;
    private bool _loaded = false;
    private bool _lock = false;
    void Start()
    {
        _player = FindObjectOfType<CharacterMovement>().transform;
        if(SceneManager.GetActiveScene().name == _scene)
        {
            Debug.LogError("Scene Switcher: The current value is equal to the current scene");
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        Loader.Switch(_scene);
        _lock = true;
    }

    void Update()
    {
        if (_lock) return;
        if(Vector3.Distance(transform.position,_player.position) <= _preloadDistance)
        {
            if (!_loaded)
            {
                _loaded = true;
                Loader.LoadScene(_scene);
            }
        } else
        {
            if (_loaded)
            {
                _loaded = false;
                Loader.UnloadScene(_scene);
            }
        }
    }
}
