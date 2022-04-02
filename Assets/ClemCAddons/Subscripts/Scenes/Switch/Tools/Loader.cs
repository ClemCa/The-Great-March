using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClemCAddons
{
    namespace MultiScene
    {
        using static MultiSceneTypes;
        public static class Loader
        {
            private static readonly List<SceneOperation> _loadedScenes = new List<SceneOperation>();

            public static Scene CurrentScene { get => SceneManager.GetActiveScene(); }

            public static void LoadScene(string sceneName)
            {
                AsyncOperation r = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                r.allowSceneActivation = false;
                _loadedScenes.Add(new SceneOperation(sceneName, r));
                Debug.Log("Loading scene " + sceneName);
            }

            public static void UnloadScene(string sceneName, string customMessage = "", bool remove = true)
            {
                if (customMessage == "")
                {
                    customMessage = "Unloading scene " + sceneName;
                }
                if (remove)
                {
                    _loadedScenes.RemoveAll(t => t.Name == sceneName);
                }
                SceneManager.UnloadSceneAsync(sceneName);
                Debug.Log(customMessage);
            }

            public static void Switch(string sceneName)
            {
                Debug.Log("Getting ready to switch active scene");
                if (_loadedScenes.Exists(t => t.Name == sceneName))
                {
                    Debug.Log("Valid scene loading operation found");
                    SceneOperation sceneToLoad = _loadedScenes.Find(t => t.Name == sceneName);
                    _ = WaitThenLoad(sceneToLoad);
                }
                else
                {
                    Debug.Log("No valid scene loading operation found, loading synchronously");
                    SceneManager.LoadScene(sceneName);
                }
                _loadedScenes.RemoveAll(t => t.Name == sceneName);
            }

            private async static Task WaitThenLoad(SceneOperation operation)
            {
                while (operation.Operation.progress < 0.9f)
                {
                    await Task.Delay(25);
                }
                if (operation.Operation.progress == 1)
                {
                    Debug.Log("Scene switcher was too close from spawn to let the scene spawn correctly, falling back to synchronous loading...");
                    SceneManager.LoadScene(operation.Name);
                }
                else
                {
                    string sceneName = SceneManager.GetActiveScene().name;
                    operation.Operation.allowSceneActivation = true;
                    Debug.Log("Setting scene " + operation.Name + " active");
                    while (operation.Operation.progress < 1)
                    {
                        await Task.Delay(1);
                    }
                    operation.Operation.completed += delegate
                    {
                        UnloadScene(sceneName, "Clearing old scene (" + sceneName + ")", false);
                    };
                }
            }
        }
    }
}