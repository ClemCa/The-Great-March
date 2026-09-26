using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Runs the scripted tutorial inside the Game scene instead of a dedicated Tutorial scene.
///
/// The Game scene already ships everything the tutorial needs, so this bootstrap only strips
/// the systems the tutorial deliberately leaves out (lose cloud, thoughts, intro story), wakes
/// the dialogue/pause UI, and spawns the quest runner the tutorial drives. It acts only while
/// <see cref="GameSession.Tutorial"/> is set, which is cleared as soon as a normal run starts.
/// </summary>
public class TutorialBootstrap : MonoBehaviour
{
    private static TutorialBootstrap _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null) return;

        var host = new GameObject("[TutorialBootstrap]");
        DontDestroyOnLoad(host);
        host.AddComponent<TutorialBootstrap>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Apply(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply(scene);
    }

    private static void Apply(Scene scene)
    {
        if (!scene.IsValid() || scene.name != "Game" || !GameSession.Tutorial)
            return;

        DisableRoot(scene, "Cloud");
        DisableRoot(scene, "ThoughtExplorer");
        DisableRoot(scene, "IntroDialog");

        EnableRoot(scene, "Dialogs");
        EnableRoot(scene, "PauseCanvas");

        SpawnQuestRunner();
    }

    private static void SpawnQuestRunner()
    {
        if (Object.FindAnyObjectByType<Questing>() != null)
            return;

        var runner = new GameObject("[TutorialQuests]");
        runner.AddComponent<Questing>();
        runner.AddComponent<TutorialQuests>();
    }

    private static GameObject FindRoot(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name)
                return root;
        }
        return null;
    }

    private static void DisableRoot(Scene scene, string name)
    {
        var root = FindRoot(scene, name);
        if (root != null && root.activeSelf)
            root.SetActive(false);
    }

    private static void EnableRoot(Scene scene, string name)
    {
        var root = FindRoot(scene, name);
        if (root == null || root.activeSelf)
            return;

        root.SetActive(true);

        // React renders these now; keep the authored canvas from drawing over it.
        var canvas = root.GetComponent<Canvas>();
        if (canvas != null)
            canvas.enabled = false;
    }
}
