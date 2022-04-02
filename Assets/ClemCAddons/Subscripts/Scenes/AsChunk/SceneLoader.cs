using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class SceneLoader : MonoBehaviour
{
    [Tooltip("Will only be enabled when at least one scene is loaded")]
    [SerializeField] private GameObject[] _loadingPlatforms;
    [SerializeField] private Transform _positionTarget;
    [SerializeField] private Scenes _scenesAreas;
    [SerializeField] private bool _debug;
    [SerializeField, DrawIf("_debug",true,ComparisonType.Equals)] private ELevelType _debugValue;
    private string[] _lockedScenes = new string[] { };
    private static string _currentScene;
    private static ELevelType _currentSceneElevelType;
    private static bool DebugB;
    private static ELevelType DebugV;

    private AsyncOperation _currentLoading;

    public Scenes ScenesAreas { get => _scenesAreas; set => _scenesAreas = value; }
    public static string CurrentScene { get => _currentScene; }
    public static ELevelType CurrentSceneElevelType { get { return DebugB ? DebugV : _currentSceneElevelType; } }

    [Serializable]
    public class Scenes
    {
        public string[] Names;
        public Transform[] Areas;
        public ELevelType[] LevelTypes;
        public Scenes(string[] scenes, Transform[] areas, ELevelType[] levelType)
        {
            Names = scenes;
            Areas = areas;
            LevelTypes = levelType;
        }
    }

    void Start()
    {
        DebugB = _debug;
        DebugV = _debugValue;
    }

    void Update()
    {
        if (ClemCAddons.Utilities.Timer.MinimumDelay(777, 100))
        {
            if(SceneManager.sceneCount > 1)
            {
                if (SceneManager.GetSceneAt(0) == gameObject.scene)
                    _currentScene = SceneManager.GetSceneAt(1).name;
                else
                    _currentScene = SceneManager.GetSceneAt(0).name;
            }
            try
            {
                _currentSceneElevelType = _scenesAreas.LevelTypes[_scenesAreas.Names.FindIndex(_currentScene)];
            }
            catch(Exception ex)
            {
                Debug.LogError("(Scene Loader) Scene not in build settings!!!!\n"+ex);
            }
            for (int i = 0; i < _scenesAreas.Names.Length; i++)
            {
                if (_scenesAreas.Areas[i].GetComponent<Collider>().bounds.Contains(_positionTarget.position))
                {
                    bool valid = true;
                    for (int t = 0; t < SceneManager.sceneCount; t++)
                    {
                        if (SceneManager.GetSceneAt(t).name == _scenesAreas.Names[i])
                            valid = false;
                    }
                    if (valid)
                    {
                        Debug.Log("Loading " + _scenesAreas.Names[i]);
                        SceneManager.LoadSceneAsync(_scenesAreas.Names[i], LoadSceneMode.Additive);
                    }
                }
            }
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i) == gameObject.scene)
                    continue;
                var r = _scenesAreas.Names.Select((t, index) => new { T = t, Index = index }).Where(o => o.T == SceneManager.GetSceneAt(i).name).Select(o => o.Index);
                if (r.Count() > 0)
                {
                    bool v = true;
                    foreach(var t in r)
                    {
                        if (_scenesAreas.Areas[t].GetComponent<Collider>().bounds.Contains(_positionTarget.position))
                        {
                            v = false;
                        }
                    }
                    if (v && _lockedScenes.FindIndex(SceneManager.GetSceneAt(i).name) == -1)
                    {
                        Debug.Log("Unloading " + SceneManager.GetSceneAt(i).name);
                        SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i).name);
                        return;
                    }
                }
                else
                {
                    Debug.LogWarning("There is a loaded scene outside the system");
                }
            }
        }
        if ((SceneManager.sceneCount <= 1).OnceIfTrueGate("essentials".GetHashCode()))
        {
            foreach(GameObject go in _loadingPlatforms)
                go.SetActive(true);
        }
        if((SceneManager.sceneCount > 1).OnceIfTrueGate("essentials too".GetHashCode()))
        {
            foreach (GameObject go in _loadingPlatforms)
                go.SetActive(false);
        }
    }


    public void UberTravelPreLoad(string fromLvl, string toLvl)
    {
        _currentScene = fromLvl;
        _currentSceneElevelType = _scenesAreas.LevelTypes[_scenesAreas.Names.FindIndex(fromLvl)];
        _lockedScenes = _lockedScenes.Add(toLvl).Add(fromLvl);
        bool valid = true;
        for (int t = 0; t < SceneManager.sceneCount; t++)
        {
            if (SceneManager.GetSceneAt(t).name == toLvl)
                valid = false;
        }
        if (valid)
        {
            Debug.Log("Loading " + toLvl);
            _currentLoading = SceneManager.LoadSceneAsync(toLvl, LoadSceneMode.Additive);
        }
    }

    public void ExecuteUberTravel(Transform player, Vector3 targetPosition, string newLevel)
    {
        StartCoroutine(UberLoading(player, targetPosition, newLevel));
    }

    IEnumerator UberLoading(Transform player, Vector3 targetPosition, string newLevel)
    {
        while (_currentLoading != null && !_currentLoading.isDone)
            yield return new WaitForSeconds(0.1f);
        player.position = targetPosition;
        _lockedScenes = new string[] { };
        _currentScene = newLevel;
        _currentSceneElevelType = _scenesAreas.LevelTypes[_scenesAreas.Names.FindIndex(newLevel)];
    }
}
