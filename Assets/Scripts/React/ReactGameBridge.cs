using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;
using ReactUnity;

namespace TheGreatMarch.React
{
    /// <summary>
    /// Shared surface between the Unity game and the React (ReactUnity) UI.
    ///
    /// Data flows Unity -> React through a serialized JSON snapshot written to the
    /// ReactUnity `Globals` record under the "state" key.
    /// Actions flow React -> Unity through delegates stored on `Globals`, which the
    /// ReactUnity JS engines expose as callable functions.
    ///
    /// The bridge auto-installs itself on play, so no scene wiring is required. If a
    /// bridge already exists in the scene it is respected instead.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class ReactGameBridge : MonoBehaviour
    {
        private const int SaveSlotCount = 5;

        public static ReactGameBridge Instance { get; private set; }

        [Tooltip("How often the game state snapshot is pushed to React, in seconds.")]
        public float pushInterval = 0.1f;

        private ReactRendererBase renderer;
        private ReactUnity.Helpers.GlobalRecord registeredGlobals;
        private GameObject persistentCanvas;
        private string lastStateJson;
        private float nextPushTime;
        private float nextFaceColorFixTime;

        // Hover tooltip target, set by React as the pointer enters/leaves a HUD element.
        private string hoverKind;
        private string hoverId;

        // Shipping submenu flow state (React replaces the legacy `ShippingSubMenu` prefab UI).
        private string shippingMode = "ships";
        private int shippingShip = -1;
        private string shippingResourceId = string.Empty;
        private bool shippingResourceAdvanced;
        private int shippingResourceAmount;
        private int shippingPeopleAmount;

        // Strong references so the delegates cannot be garbage collected while
        // they are only referenced from the (native) Globals record.
        private readonly List<Delegate> actionRoots = new List<Delegate>();

        // Hand-authored menu scenes keep their own React screen. Everything else is treated as
        // the in-game HUD, regardless of what the scene happens to be called.
        private static readonly Dictionary<string, string[]> LegacyMenuRoots = new Dictionary<string, string[]>
        {
            { "MainMenu", new[] { "Menu", "Credits", "Developpers" } },
            { "LoseMenu", new[] { "Menu", "Result" } },
        };

        // uGUI canvases that React now renders instead. Their Canvas component is disabled rather
        // than the GameObject, so the gameplay scripts on them (pause hotkeys, dialogue streaming,
        // queue updates) keep running while the visuals are handed to React. Looked up by name in
        // every non-menu scene, so an unknown scene (e.g. StoryTest) still hands its dialogs over.
        private static readonly string[] ReplacedHudRoots = { "Menu", "Queue", "PauseCanvas", "Dialogs", "Prompt", "Quest" };

        private static bool IsMenuScene(string sceneName) => LegacyMenuRoots.ContainsKey(sceneName);


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null || FindObjectOfType<ReactGameBridge>() != null) return;

            var host = new GameObject("[ReactGameBridge]");
            DontDestroyOnLoad(host);
            host.AddComponent<ReactGameBridge>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
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

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Hook();
        }

        private void Start()
        {
            Hook();
        }

        private void Hook()
        {
            var all = FindObjectsOfType<ReactRendererBase>();
            if (all == null || all.Length == 0) return;

            // Prefer the canvas we already adopted (it survives scene loads) so a single
            // React app keeps running for the whole session, then suppress the duplicate
            // canvas that every scene ships with.
            ReactRendererBase chosen = null;
            if (persistentCanvas != null) chosen = persistentCanvas.GetComponent<ReactRendererBase>();
            if (chosen == null) chosen = all[0];

            if (persistentCanvas != chosen.gameObject)
            {
                persistentCanvas = chosen.gameObject;
                DontDestroyOnLoad(persistentCanvas);
                ConfigureCanvas(chosen);
            }

            if (renderer != chosen)
            {
                renderer = chosen;
                lastStateJson = null;
            }

            if (!ReferenceEquals(registeredGlobals, renderer.Globals))
            {
                RegisterActions(renderer.Globals);
                registeredGlobals = renderer.Globals;
            }

            foreach (var candidate in all)
            {
                if (candidate != chosen && candidate.gameObject.activeSelf)
                    candidate.gameObject.SetActive(false);
            }

            PushState(renderer.Globals, true);
            HideLegacyUi();
        }

        /// <summary>
        /// Makes the React canvas author in the same 1920x1080 reference space the original
        /// uGUI menus used, so the two can be compared one-to-one at any resolution.
        /// </summary>
        private static void ConfigureCanvas(ReactRendererBase target)
        {
            var scaler = target.GetComponent<CanvasScaler>();
            if (scaler == null) return;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
        }

        private static void HideLegacyUi()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return;

            var roots = scene.GetRootGameObjects();

            if (LegacyMenuRoots.TryGetValue(scene.name, out var menuRoots))
            {
                // Menu scenes: fully deactivate the authored menu roots React replaces.
                foreach (var root in roots)
                {
                    if (Array.IndexOf(menuRoots, root.name) >= 0 && root.activeSelf)
                        root.SetActive(false);
                }
                DisableLegacySettings();
                return;
            }

            // Every other scene is the HUD: hand the replaced canvases to React while leaving
            // their gameplay scripts (pause hotkeys, dialogue streaming, queue updates) running.
            foreach (var root in roots)
            {
                if (Array.IndexOf(ReplacedHudRoots, root.name) < 0) continue;
                var canvas = root.GetComponent<Canvas>();
                if (canvas != null) canvas.enabled = false;
            }
            DisableLegacySettings();
        }

        // React owns the settings UI now; keep the authored prefab from being shown over it.
        private static void DisableLegacySettings()
        {
            foreach (var settingsMenu in FindObjectsByType<SettingsMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (settingsMenu != null && settingsMenu.gameObject.activeSelf)
                    settingsMenu.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The Chakra Petch font asset ships with a black `_FaceColor` on its shared material;
        /// uGUI text overrides this per instance, but ReactUnity text inherits the black tint,
        /// which turns white text invisible. Force the face color back to neutral white so the
        /// per-character (CSS `color`) tints render correctly.
        /// </summary>
        private void EnsureReactTextFaceColor()
        {
            if (persistentCanvas == null) return;

            foreach (var text in persistentCanvas.GetComponentsInChildren<TMP_Text>(true))
            {
                var material = text.fontMaterial;
                if (material != null && material.HasProperty(ShaderUtilities.ID_FaceColor) &&
                    material.GetColor(ShaderUtilities.ID_FaceColor) != Color.white)
                {
                    material.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
                    text.ForceMeshUpdate();
                }
            }
        }

        private void Update()
        {
            if (renderer == null)
            {
                Hook();
                return;
            }

            if (Time.unscaledTime >= nextFaceColorFixTime)
            {
                nextFaceColorFixTime = Time.unscaledTime + 0.5f;
                EnsureReactTextFaceColor();
            }

            // A live tooltip tracks the cursor and a dialogue typewriter streams text, so both
            // need a per-frame push; everything else is throttled to `pushInterval`.
            var hoverActive = !string.IsNullOrEmpty(hoverKind) && Settings.Loaded && Settings.ShowPrompt;
            var dialogActive = DialogDisplayer.Instance != null && DialogDisplayer.Instance.IsVisible;
            if (!hoverActive && !dialogActive && Time.unscaledTime < nextPushTime) return;
            nextPushTime = Time.unscaledTime + Mathf.Max(0.02f, pushInterval);
            PushState(renderer.Globals, false);
        }

        // ---------------------------------------------------------------------
        // React -> Unity actions
        // ---------------------------------------------------------------------

        private void RegisterActions(ReactUnity.Helpers.GlobalRecord globals)
        {
            Set(globals, "ping", (Func<string, string>)Ping);

            Set(globals, "newGame", (Action)NewGame);
            Set(globals, "loadGame", (Action<int>)LoadGame);
            Set(globals, "tutorial", (Action)Tutorial);
            Set(globals, "exit", (Action)Exit);
            Set(globals, "restart", (Action)Restart);
            Set(globals, "goToMainMenu", (Action)GoToMainMenu);

            Set(globals, "saveGame", (Action<int>)SaveGame);
            Set(globals, "slotUsed", (Func<int, bool>)SlotUsed);

            Set(globals, "setPaused", (Action<bool>)SetPaused);
            Set(globals, "togglePause", (Action)TogglePause);
            Set(globals, "setPrompt", (Action<bool>)SetPrompt);
            Set(globals, "click", (Action)PlayClick);

            Set(globals, "buildFacility", (Action<string>)BuildFacility);
            Set(globals, "movePriority", (Action<int, int, int, bool>)MovePriority);
            Set(globals, "dialogSelect", (Action<int>)DialogSelect);
            Set(globals, "dialogFlip", (Action)DialogFlip);
            Set(globals, "startDialogue", (Action<string>)StartDialogue);

            Set(globals, "promptTarget", (Action<string, string>)SetPromptTarget);
            Set(globals, "clearPrompt", (Action)ClearPromptTarget);

            Set(globals, "setSettingFloat", (Action<string, float>)SetSettingFloat);
            Set(globals, "setSettingBool", (Action<string, bool>)SetSettingBool);
            Set(globals, "setLlmSetting", (Action<string, string>)SetLlmSetting);
            Set(globals, "cycleSetting", (Action<string>)CycleSetting);

            Set(globals, "shippingSetMode", (Action<string>)ShippingSetMode);
            Set(globals, "shippingSelectShip", (Action<int>)ShippingSelectShip);
            Set(globals, "shippingRefuel", (Action)ShippingRefuel);
            Set(globals, "shippingSelectResource", (Action<string, bool>)ShippingSelectResource);
            Set(globals, "shippingSetResourceAmount", (Action<int>)ShippingSetResourceAmount);
            Set(globals, "shippingSetPeopleAmount", (Action<int>)ShippingSetPeopleAmount);
            Set(globals, "shippingLaunch", (Action)ShippingLaunch);
            Set(globals, "shippingPlayerMove", (Action)ShippingPlayerMove);
            Set(globals, "shippingLaunchPeople", (Action)ShippingLaunchPeople);
            Set(globals, "shippingLaunchResource", (Action)ShippingLaunchResource);
            Set(globals, "shippingReturn", (Action)ShippingReturn);

            Set(globals, "openUrl", (Action<string>)OpenUrl);
            Set(globals, "openSettings", (Action)OpenSettings);
        }

        private void Set(ReactUnity.Helpers.GlobalRecord globals, string key, Delegate value)
        {
            actionRoots.Add(value);
            globals[key] = value;
        }

        public string Ping(string message) => "pong:" + message;

        public void NewGame()
        {
            StoryScript.SlotLoader = -1;
            _ = SceneManager.LoadSceneAsync("Game");
            PlayClick();
        }

        public void LoadGame(int slot)
        {
            StoryScript.SlotLoader = slot;
            _ = SceneManager.LoadSceneAsync("Game");
            PlayClick();
        }

        public void Tutorial()
        {
            _ = SceneManager.LoadSceneAsync("Tutorial");
            PlayClick();
        }

        public void Exit()
        {
            PlayClick();
            Application.Quit();
        }

        public void Restart()
        {
            PlayClick();
            _ = SceneManager.LoadSceneAsync("Game");
        }

        public void GoToMainMenu()
        {
            PlayClick();
            _ = SceneManager.LoadSceneAsync("MainMenu");
        }

        public void SaveGame(int slot)
        {
            if (Saver.Instance != null) Saver.Instance.Save(slot);
            PlayClick();
        }

        public bool SlotUsed(int slot)
        {
            return Saver.Instance != null && Saver.Instance.SlotUsed(slot);
        }

        public void SetPaused(bool paused)
        {
            if (paused) Pausing.Pause();
            else Pausing.Unpause();
        }

        public void TogglePause()
        {
            Pausing.FlipPause();
        }

        public void SetPrompt(bool enabled)
        {
            if (Settings.Loaded) Settings.ShowPrompt = enabled;
        }

        // ---------------------------------------------------------------------
        // Tooltip
        // ---------------------------------------------------------------------

        public void SetPromptTarget(string kind, string id)
        {
            hoverKind = kind;
            hoverId = id;
        }

        public void ClearPromptTarget()
        {
            hoverKind = null;
            hoverId = null;
        }

        // ---------------------------------------------------------------------
        // Settings
        // ---------------------------------------------------------------------

        public void SetSettingFloat(string key, float value)
        {
            if (!Settings.Loaded) return;
            switch (key)
            {
                case "master": Settings.MasterVolume = value; break;
                case "ui": Settings.UIVolume = value; break;
                case "sfx": Settings.SFXVolume = value; break;
                case "music": Settings.MusicVolume = value; break;
                case "temperature": LLMSettings.Temperature = value; LLMSettings.Save(); break;
                case "verbatim": LLMSettings.VerbatimHistory = Mathf.RoundToInt(value); LLMSettings.Save(); break;
                case "summarized": LLMSettings.SummarizedHistory = Mathf.RoundToInt(value); LLMSettings.Save(); break;
            }
        }

        public void SetSettingBool(string key, bool value)
        {
            if (!Settings.Loaded) return;
            switch (key)
            {
                case "backgroundSound": Settings.BackgroundSound = value; break;
                case "showPrompt": Settings.ShowPrompt = value; break;
                case "vsync": Settings.VSync = value; break;
                case "fullscreen": Settings.Fullscreen = value; break;
                case "thoughts": LLMSettings.IncludeThoughtsJson = value; LLMSettings.Save(); break;
            }
        }

        public void SetLlmSetting(string key, string value)
        {
            switch (key)
            {
                case "baseUrl": LLMSettings.BaseUrl = value; break;
                case "model": LLMSettings.Model = value; break;
                case "apiKey": LLMSettings.ApiKey = value; break;
            }
            LLMSettings.Save();
        }

        public void CycleSetting(string key)
        {
            if (!Settings.Loaded) return;
            switch (key)
            {
                case "resolution":
                {
                    var resolutions = Screen.resolutions;
                    int count = resolutions != null ? resolutions.Length : 0;
                    int next = Settings.ResolutionIndex + 1;
                    Settings.ResolutionIndex = next >= count ? -1 : next;
                    break;
                }
                case "quality":
                {
                    int count = QualitySettings.names != null ? QualitySettings.names.Length : 0;
                    if (count == 0) break;
                    int next = Settings.QualityLevel + 1;
                    Settings.QualityLevel = next >= count ? 0 : next;
                    break;
                }
                case "antialiasing":
                {
                    var values = (AntialiasingMode[])Enum.GetValues(typeof(AntialiasingMode));
                    int index = Array.IndexOf(values, Settings.AntialiasingMode);
                    Settings.AntialiasingMode = values[(index + 1) % values.Length];
                    break;
                }
                case "antialiasingQuality":
                {
                    var values = (AntialiasingQuality[])Enum.GetValues(typeof(AntialiasingQuality));
                    int index = Array.IndexOf(values, Settings.AntialiasingQuality);
                    Settings.AntialiasingQuality = values[(index + 1) % values.Length];
                    break;
                }
                case "mode":
                    LLMSettings.Mode = LLMSettings.Mode == LLMMode.Raw ? LLMMode.LLM : LLMMode.Raw;
                    LLMSettings.Save();
                    break;
                case "provider":
                    LLMSettings.Provider = LLMSettings.Provider == LLMProviderKind.Ollama ? LLMProviderKind.OpenAICompatible : LLMProviderKind.Ollama;
                    LLMSettings.Save();
                    break;
            }
        }

        // ---------------------------------------------------------------------
        // Shipping
        // ---------------------------------------------------------------------

        private void ResetShippingTransient()
        {
            shippingShip = -1;
            shippingResourceId = string.Empty;
            shippingResourceAdvanced = false;
            shippingResourceAmount = 0;
            shippingPeopleAmount = 0;
        }

        public void ShippingSetMode(string mode)
        {
            shippingMode = string.IsNullOrEmpty(mode) ? "menu" : mode;
            if (shippingMode == "menu" || shippingMode == "ships")
                ResetShippingTransient();
            else if (shippingMode == "people")
                shippingPeopleAmount = 0;
            else if (shippingMode == "resources")
            {
                shippingResourceId = string.Empty;
                shippingResourceAdvanced = false;
                shippingResourceAmount = 0;
            }
            PlayClick();
        }

        public void ShippingSelectShip(int index)
        {
            if (Planet.Selected == null || index < 0 || index >= Planet.Selected.Ships.Count) return;
            shippingShip = index;
            shippingResourceId = string.Empty;
            shippingResourceAdvanced = false;
            shippingResourceAmount = 0;
            shippingPeopleAmount = 0;
            PlayClick();
        }

        public void ShippingRefuel()
        {
            if (Planet.Selected == null || shippingShip < 0 || shippingShip >= Planet.Selected.Ships.Count) return;
            var ship = Planet.Selected.Ships[shippingShip];
            var required = Registry.Instance.GetRequiredFuel(ship.Type);
            if (ship.Fuel >= required)
            {
                shippingMode = ship.Type == Registry.ShipType.Presidential ? "president" : "cargo";
                shippingResourceId = string.Empty;
                shippingResourceAdvanced = false;
                shippingResourceAmount = 0;
                shippingPeopleAmount = 0;
            }
            else
            {
                Planet.Selected.ConsumeFuel(required - ship.Fuel);
                ship.Fuel += required;
            }
            PlayClick();
        }

        public void ShippingSelectResource(string id, bool advanced)
        {
            shippingResourceId = id ?? string.Empty;
            shippingResourceAdvanced = advanced;
            shippingResourceAmount = string.IsNullOrEmpty(shippingResourceId) ? 0 : 1;
            PlayClick();
        }

        public void ShippingSetResourceAmount(int value)
        {
            if (Planet.Selected == null || shippingShip < 0 || shippingShip >= Planet.Selected.Ships.Count) return;
            shippingResourceAmount = Mathf.Clamp(value, 0, ShippingMaxResource());
        }

        public void ShippingSetPeopleAmount(int value)
        {
            if (Planet.Selected == null || shippingShip < 0 || shippingShip >= Planet.Selected.Ships.Count) return;
            shippingPeopleAmount = Mathf.Clamp(value, 0, ShippingMaxPeople());
        }

        public void ShippingLaunch()
        {
            var planet = Planet.Selected;
            if (planet == null || shippingShip < 0 || shippingShip >= planet.Ships.Count) return;
            var id = planet.ShipIDs;
            planet.ShipIDs++;
            planet.ReservedShips.Add(new KeyValuePair<int, Registry.Ship>(id, planet.Ships[shippingShip]));
            planet.Ships.RemoveAt(shippingShip);

            var people = shippingPeopleAmount;
            if (string.IsNullOrEmpty(shippingResourceId))
                planet.EngageMoveSelectionMode(people, id);
            else if (shippingResourceAdvanced && Enum.TryParse(shippingResourceId, out Registry.AdvancedResources adv))
                planet.EngageMoveSelectionMode(adv, shippingResourceAmount, id, people);
            else if (Enum.TryParse(shippingResourceId, out Registry.Resources res))
                planet.EngageMoveSelectionMode(res, shippingResourceAmount, id, people);
            else
                planet.EngageMoveSelectionMode(people, id);

            shippingMode = "ships";
            ResetShippingTransient();
            PlayClick();
        }

        public void ShippingPlayerMove()
        {
            var planet = Planet.Selected;
            if (planet == null || !planet.HasPlayer || Cargo.LeaderInTransit) return;
            planet.EngageMoveSelectionMode(0);
        }

        public void ShippingLaunchPeople()
        {
            var planet = Planet.Selected;
            if (planet == null || shippingPeopleAmount <= 0) return;
            planet.EngageMoveSelectionMode(shippingPeopleAmount, 0);
            shippingMode = "ships";
            ResetShippingTransient();
            PlayClick();
        }

        public void ShippingLaunchResource()
        {
            var planet = Planet.Selected;
            if (planet == null || string.IsNullOrEmpty(shippingResourceId) || shippingResourceAmount <= 0) return;
            if (shippingResourceAdvanced && Enum.TryParse(shippingResourceId, out Registry.AdvancedResources adv))
                planet.EngageMoveSelectionMode(adv, shippingResourceAmount, 0);
            else if (Enum.TryParse(shippingResourceId, out Registry.Resources res))
                planet.EngageMoveSelectionMode(res, shippingResourceAmount, 0);
            shippingMode = "ships";
            ResetShippingTransient();
            PlayClick();
        }

        public void ShippingReturn()
        {
            shippingMode = "ships";
            ResetShippingTransient();
            PlayClick();
        }

        private int SelectedShipResourceCount()
        {
            if (Planet.Selected == null || string.IsNullOrEmpty(shippingResourceId)) return 0;
            return shippingResourceAdvanced
                ? (Enum.TryParse(shippingResourceId, out Registry.AdvancedResources adv) ? SafeResource(Planet.Selected, adv) : 0)
                : (Enum.TryParse(shippingResourceId, out Registry.Resources res) ? SafeResource(Planet.Selected, res) : 0);
        }

        private int ShippingMaxResource()
        {
            var available = SelectedShipResourceCount();
            if (available <= 0 || Planet.Selected == null || shippingShip < 0 || shippingShip >= Planet.Selected.Ships.Count) return 0;
            switch (Planet.Selected.Ships[shippingShip].Type)
            {
                case Registry.ShipType.Cargo: return Mathf.Min(available, Mathf.Max(0, 11 - shippingPeopleAmount));
                case Registry.ShipType.Passenger: return Mathf.Min(available, Mathf.Max(0, 15 - shippingPeopleAmount), 5);
                default: return 0;
            }
        }

        private int ShippingMaxPeople()
        {
            if (Planet.Selected == null || shippingShip < 0 || shippingShip >= Planet.Selected.Ships.Count) return 0;
            var available = Planet.Selected.GetPeople();
            switch (Planet.Selected.Ships[shippingShip].Type)
            {
                case Registry.ShipType.Cargo: return Mathf.Min(available, Mathf.Max(0, 11 - shippingResourceAmount), 5);
                case Registry.ShipType.Passenger: return Mathf.Min(available, Mathf.Max(0, 15 - shippingResourceAmount));
                default: return Mathf.Min(available, 5);
            }
        }

        /// <summary>
        /// Queues a facility build on the selected planet, mirroring the original
        /// `FacilityMenu`/`TransformationFacilityMenu` click handlers.
        /// </summary>
        public void BuildFacility(string facilityId)
        {
            if (Planet.Selected == null || string.IsNullOrEmpty(facilityId)) return;
            if (Registry.Instance == null || OrderHandler.Instance == null) return;

            var info = Registry.Instance.GetFacilityInfo(facilityId);
            var wildcard = info != null && info.Wildcard;

            OrderHandler.Instance.Queue(
                new OrderHandler.Order(
                    OrderHandler.OrderType.Building,
                    wildcard ? 180f : 140f,
                    wildcard ? 0.75f : 1.5f,
                    wildcard ? 10 : 5,
                    new OrderHandler.OrderExec(Planet.Selected, facilityId)),
                Planet.Selected);
            PlayClick();
        }

        /// <summary>
        /// Swaps a resource with its neighbour inside the food/fuel consumption priority
        /// list, mirroring `ReorderButton`.
        /// </summary>
        public void MovePriority(int display, int index, int delta, bool global)
        {
            List<int> list;
            if (global)
                list = Planet.GetGlobalPriorities(display);
            else if (Planet.Selected != null)
                list = Planet.Selected.GetLocalPriorities(display);
            else
                return;

            var target = index + delta;
            if (index < 0 || index >= list.Count || target < 0 || target >= list.Count) return;

            var swap = list[index];
            list[index] = list[target];
            list[target] = swap;

            if (global) Planet.SetGlobalPriorities(display, list);
            else Planet.Selected.SetLocalPriorities(display, list);
        }

        public void DialogSelect(int index)
        {
            if (DialogDisplayer.Instance != null) DialogDisplayer.Instance.Select(index);
        }

        public void DialogFlip()
        {
            if (DialogDisplayer.Instance != null) DialogDisplayer.Instance.Flip();
        }

        public void StartDialogue(string dialogue)
        {
            if (DialogDisplayer.Instance != null && !string.IsNullOrEmpty(dialogue))
                DialogDisplayer.Instance.StartDialogue(dialogue);
        }

        public void OpenUrl(string url)
        {
            if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
        }

        public void OpenSettings()
        {
            // React owns the settings UI; the authored `SettingsMenu` is kept disabled so a stray
            // legacy caller cannot revive the prefab over the React panel.
            PlayClick();
        }

        private static void PlayClick()
        {
            if (MenuAudioManager.Instance != null) MenuAudioManager.Instance.PlayClick();
        }

        // ---------------------------------------------------------------------
        // Unity -> React state snapshot
        // ---------------------------------------------------------------------

        private void PushState(ReactUnity.Helpers.GlobalRecord globals, bool force)
        {
            var json = JsonUtility.ToJson(BuildSnapshot());
            if (!force && json == lastStateJson) return;
            lastStateJson = json;
            globals["state"] = json;
        }

        private StateSnapshot BuildSnapshot()
        {
            var sceneName = SceneManager.GetActiveScene().name;

            var snapshot = new StateSnapshot
            {
                scene = sceneName,
                isGameScene = !IsMenuScene(sceneName),
                paused = Pausing.Paused,
                prompt = Settings.Loaded && Settings.ShowPrompt,
                survivalTime = Scoring.survivalTime,
                totalTime = Scoring.totalTime,
                systems = Scoring.systems,
                naturalResourcesUnits = Scoring.naturalResourcesUnits,
                advancedResourcesUnits = Scoring.advancedResourcesUnits,
                facilitiesCount = Scoring.facilitiesCount,
                transformativeFacilitiesCount = Scoring.transformativeFacilitiesCount,
                slots = BuildSlots(),
                queue = BuildQueue(),
                hasSelectedPlanet = Planet.Selected != null,
            };

            if (Planet.Selected != null)
            {
                snapshot.selectedPlanet = BuildPlanet(Planet.Selected);
                snapshot.selectedPlanet.shipping = BuildShipping(Planet.Selected);
            }

            var dialog = DialogDisplayer.Instance;
            snapshot.dialog = new DialogSnapshot
            {
                visible = dialog != null && dialog.IsVisible,
                name = dialog != null ? dialog.DisplayName : string.Empty,
                text = dialog != null ? dialog.DisplayText : string.Empty,
                choices = dialog != null ? dialog.ChoiceLabels : new string[0],
            };

            snapshot.quest = new QuestSnapshot
            {
                visible = Questing.Instance != null && Questing.Instance.IsRunningQuestline,
                text = Questing.Instance != null ? Questing.Instance.CurrentObjective : string.Empty,
            };

            snapshot.settings = BuildSettings();

            var tooltip = BuildTooltip();
            var mouse = InputHelper.MousePosition;
            tooltip.x = Screen.width > 0 ? mouse.x / Screen.width : 0f;
            tooltip.y = Screen.height > 0 ? mouse.y / Screen.height : 0f;
            snapshot.tooltip = tooltip;

            return snapshot;
        }

        private SettingsSnapshot BuildSettings()
        {
            var s = new SettingsSnapshot();
            if (!Settings.Loaded) return s;

            s.master = Settings.MasterVolume;
            s.ui = Settings.UIVolume;
            s.sfx = Settings.SFXVolume;
            s.music = Settings.MusicVolume;
            s.backgroundSound = Settings.BackgroundSound;
            s.showPrompt = Settings.ShowPrompt;
            s.vsync = Settings.VSync;
            s.fullscreen = Settings.Fullscreen;
            s.resolution = ResolutionLabel();
            s.quality = QualityLabel();
            s.antialiasing = Settings.AntialiasingMode.ToString();
            s.antialiasingQuality = Settings.AntialiasingQuality.ToString();

            s.mode = LLMSettings.Mode.ToString();
            s.provider = LLMSettings.Provider.ToString();
            s.baseUrl = LLMSettings.BaseUrl;
            s.model = LLMSettings.Model;
            s.apiKey = LLMSettings.ApiKey;
            s.temperature = LLMSettings.Temperature;
            s.verbatim = LLMSettings.VerbatimHistory;
            s.summarized = LLMSettings.SummarizedHistory;
            s.thoughts = LLMSettings.IncludeThoughtsJson;
            return s;
        }

        private static string ResolutionLabel()
        {
            var resolutions = Screen.resolutions;
            if (resolutions == null || resolutions.Length == 0) return "Native";
            int index = Settings.ResolutionIndex;
            if (index < 0 || index >= resolutions.Length)
                return "Native (" + Screen.currentResolution.width + "x" + Screen.currentResolution.height + ")";
            var r = resolutions[index];
            return r.width + "x" + r.height + " @" + Mathf.RoundToInt((float)r.refreshRateRatio.value) + "Hz";
        }

        private static string QualityLabel()
        {
            var names = QualitySettings.names;
            if (names == null || names.Length == 0) return "Default";
            int index = Settings.QualityLevel;
            if (index < 0 || index >= names.Length) return names[names.Length - 1];
            return names[index];
        }

        private ShippingSnapshot BuildShipping(Planet planet)
        {
            var s = new ShippingSnapshot
            {
                mode = shippingMode,
                selectedShip = shippingShip,
                hasPlayer = planet.HasPlayer,
                leaderInTransit = Cargo.LeaderInTransit,
                resourceId = shippingResourceId,
                resourceAdvanced = shippingResourceAdvanced,
                resourceAmount = shippingResourceAmount,
                peopleAmount = shippingPeopleAmount,
                people = planet.GetPeople(),
            };

            var registry = Registry.Instance;
            var list = new List<ShipEntry>();
            if (planet.Ships != null)
            {
                for (var i = 0; i < planet.Ships.Count; i++)
                {
                    var ship = planet.Ships[i];
                    var required = registry != null ? registry.GetRequiredFuel(ship.Type) : 0;
                    var canLaunch = ship.Fuel >= required;
                    var missing = Mathf.Max(0, required - ship.Fuel);
                    list.Add(new ShipEntry
                    {
                        index = i,
                        type = ship.Type.ToString(),
                        icon = SpriteName(registry != null ? registry.GetShipSprite(ship.Type) : null),
                        fuel = ship.Fuel,
                        requiredFuel = required,
                        canLaunch = canLaunch,
                        canRefuel = !canLaunch && missing < planet.GetAvailableFuel(),
                        refuelAmount = missing,
                    });
                }
            }
            s.ships = list.ToArray();

            var shipValid = shippingShip >= 0 && planet.Ships != null && shippingShip < planet.Ships.Count;
            s.shipType = shipValid ? planet.Ships[shippingShip].Type.ToString() : string.Empty;
            s.maxResource = shipValid ? ShippingMaxResource() : 0;
            s.maxPeople = shipValid ? ShippingMaxPeople() : 0;
            s.availableFuel = planet.GetAvailableFuel();

            return s;
        }

        private TooltipSnapshot BuildTooltip()
        {
            var t = new TooltipSnapshot();
            if (!Settings.Loaded || !Settings.ShowPrompt || string.IsNullOrEmpty(hoverKind)) return t;

            var registry = Registry.Instance;
            if (registry == null) return t;

            if (hoverKind == "slot")
            {
                if (Planet.Selected == null || !Enum.TryParse(hoverId, out Registry.Resources slotRes)) return t;
                if (Planet.Selected.HasFacility(slotRes))
                    FillFacility(t, registry.GetFacilityInfo(Planet.Selected.GetFacility(slotRes)), false);
                else
                {
                    t.title = registry.GetResourceName(slotRes);
                    t.description = registry.GetResourceDescription(slotRes);
                }
                return t;
            }

            if (hoverKind == "facility" || hoverKind == "transformation")
            {
                FillFacility(t, registry.GetFacilityInfo(hoverId), hoverKind == "transformation");
                return t;
            }

            if (hoverKind == "wildcard")
            {
                if (string.IsNullOrEmpty(hoverId))
                {
                    t.title = "Plot of land";
                    t.description = "An empty plot of land";
                }
                else
                {
                    FillFacility(t, registry.GetFacilityInfo(hoverId), true);
                }
                return t;
            }

            if (hoverKind == "resource" || hoverKind == "select")
            {
                if (!Enum.TryParse(hoverId, out Registry.Resources res)) return t;
                t.title = registry.GetResourceName(res) + " (" + (Planet.Selected != null ? SafeResource(Planet.Selected, res) : 0) + ")";
                t.description = registry.GetResourceDescription(res);
                return t;
            }

            if (hoverKind == "advancedresource" || hoverKind == "selectadvanced")
            {
                if (!Enum.TryParse(hoverId, out Registry.AdvancedResources adv)) return t;
                t.title = registry.GetResourceName(adv) + " (" + (Planet.Selected != null ? SafeResource(Planet.Selected, adv) : 0) + ")";
                t.description = registry.GetResourceDescription(adv);
                return t;
            }

            if (hoverKind == "ship")
            {
                if (Planet.Selected == null || !int.TryParse(hoverId, out var index)
                    || index < 0 || index >= Planet.Selected.Ships.Count) return t;

                var ship = Planet.Selected.Ships[index];
                var required = registry.GetRequiredFuel(ship.Type);
                if (ship.Fuel >= required) return t;

                t.title = "Refuel";
                if (required - ship.Fuel >= Planet.Selected.GetAvailableFuel())
                {
                    t.description = "Missing fuel";
                    return t;
                }

                var simulated = Planet.Selected.SimulateFuel(required);
                var lines = new List<string>();
                AddFuelLine(lines, simulated[0], registry.GetResourceName(Registry.Resources.Gas));
                AddFuelLine(lines, simulated[1], registry.GetResourceName(Registry.Resources.Oil));
                AddFuelLine(lines, simulated[2], registry.GetResourceName(Registry.Resources.Hydrogen));
                AddFuelLine(lines, simulated[3], registry.GetResourceName(Registry.AdvancedResources.HighEfficiencyFuel));
                AddFuelLine(lines, simulated[4], registry.GetResourceName(Registry.AdvancedResources.HydrogenBattery));
                t.description = string.Join("\n", lines);
                return t;
            }

            return t;
        }

        private static void AddFuelLine(List<string> lines, int amount, string name)
        {
            if (amount > 0) lines.Add(name + ": -" + amount);
        }

        private static void FillFacility(TooltipSnapshot t, Registry.FacilityData info, bool includeConsume)
        {
            if (info == null) return;
            t.title = info.Name;
            t.description = info.Description;
            t.info1 = includeConsume ? "Consumes " + DescribeEffects(info.GetEffects(Registry.FacilityEffectType.Consume)) : string.Empty;
            t.info2 = "Produces " + DescribeEffects(info.GetEffects(Registry.FacilityEffectType.Produce)) + " every " + info.Cooldown + "s";
        }

        private static string DescribeEffects(Registry.FacilityEffect[] effects)
        {
            if (effects == null || effects.Length == 0) return "nothing";
            var parts = new string[effects.Length];
            for (int i = 0; i < effects.Length; i++)
                parts[i] = effects[i].Amount + " " + EffectResourceName(effects[i]);
            if (parts.Length == 1) return parts[0];
            if (parts.Length == 2) return parts[0] + " and " + parts[1];
            return string.Join(", ", parts, 0, parts.Length - 1) + " and " + parts[parts.Length - 1];
        }

        private static string EffectResourceName(Registry.FacilityEffect effect)
        {
            return effect.Advanced
                ? Registry.Instance.GetResourceName(effect.AdvancedResource)
                : Registry.Instance.GetResourceName(effect.Resource);
        }

        private static SlotEntry[] BuildSlots()
        {
            var slots = new SlotEntry[SaveSlotCount];
            for (var i = 0; i < SaveSlotCount; i++)
            {
                slots[i] = new SlotEntry
                {
                    index = i + 1,
                    used = Saver.Instance != null && Saver.Instance.SlotUsed(i + 1),
                };
            }
            return slots;
        }

        private static PlanetSnapshot BuildPlanet(Planet planet)
        {
            var resources = new List<ResourceEntry>();
            var slots = new List<FacilitySlotEntry>();
            var wildcards = new List<WildcardEntry>();
            var options = new List<FacilityOption>();
            var registry = Registry.Instance;

            if (registry != null)
            {
                foreach (Registry.Resources resource in Enum.GetValues(typeof(Registry.Resources)))
                {
                    resources.Add(new ResourceEntry
                    {
                        id = resource.ToString(),
                        name = registry.GetResourceName(resource),
                        amount = SafeResource(planet, resource),
                        advanced = false,
                        icon = SpriteName(registry.GetResourceSprite(resource)),
                        value = registry.GetResourceValue(resource),
                    });

                    if (!planet.GetSlot(resource)) continue;

                    var built = planet.HasFacility(resource);
                    var facilityId = built ? planet.GetFacility(resource) : null;
                    slots.Add(new FacilitySlotEntry
                    {
                        resourceId = resource.ToString(),
                        resourceName = registry.GetResourceName(resource),
                        resourceIcon = SpriteName(registry.GetResourceSprite(resource)),
                        built = built,
                        facilityId = facilityId,
                        facilityName = FacilityName(registry, facilityId),
                        facilityIcon = FacilityIcon(registry, facilityId),
                        progression = planet.GetFactoryProgression(resource),
                    });
                }

                foreach (Registry.AdvancedResources resource in Enum.GetValues(typeof(Registry.AdvancedResources)))
                {
                    resources.Add(new ResourceEntry
                    {
                        id = resource.ToString(),
                        name = registry.GetResourceName(resource),
                        amount = SafeResource(planet, resource),
                        advanced = true,
                        icon = SpriteName(registry.GetAdvancedResourceSprite(resource)),
                        value = registry.GetResourceValue(resource),
                    });
                }

                foreach (var facilityId in planet.TransformationFacilities)
                {
                    wildcards.Add(new WildcardEntry
                    {
                        empty = false,
                        facilityId = facilityId,
                        facilityName = FacilityName(registry, facilityId),
                        facilityIcon = FacilityIcon(registry, facilityId),
                        progression = planet.GetFactoryProgression(facilityId),
                    });
                }

                foreach (var info in registry.GetAllFacilities())
                {
                    options.Add(new FacilityOption
                    {
                        id = info.Id,
                        name = info.Name,
                        description = info.Description,
                        icon = SpriteName(info.Sprite),
                        wildcard = info.Wildcard,
                        slotResource = info.SlotResource.ToString(),
                        canBuild = planet.CanBuildFacility(info.Id),
                        built = planet.IsFacilityBuilt(info.Id),
                    });
                }

                foreach (var info in registry.GetWildcardFacilities())
                {
                    options.Add(new FacilityOption
                    {
                        id = info.Id,
                        name = info.Name,
                        description = info.Description,
                        icon = SpriteName(info.Sprite),
                        wildcard = true,
                        slotResource = info.SlotResource.ToString(),
                        canBuild = planet.CanBuildFacility(info.Id),
                        built = planet.IsFacilityBuilt(info.Id),
                    });
                }
            }

            return new PlanetSnapshot
            {
                name = planet.Name,
                people = planet.GetPeople(),
                hasPlayer = planet.HasPlayer,
                temperate = ((Registry.PlanetType)planet.TemperateType).ToString(),
                fuel = SafeResource(planet, Registry.Resources.Gas)
                    + SafeResource(planet, Registry.Resources.Oil)
                    + SafeResource(planet, Registry.Resources.Hydrogen)
                    + 10 * (SafeResource(planet, Registry.AdvancedResources.HighEfficiencyFuel)
                        + SafeResource(planet, Registry.AdvancedResources.HydrogenBattery)),
                ships = planet.Ships != null ? planet.Ships.Count : 0,
                resources = resources.ToArray(),
                facilities = new List<string>(planet.Facilities).ToArray(),
                graph = new GraphSnapshot
                {
                    people = planet.PeopleOverTime.ToArray(),
                    resources = planet.ResourcesOverTime.ToArray(),
                    facilities = planet.FacilitiesOverTime.ToArray(),
                },
                facilitySlots = slots.ToArray(),
                wildcards = wildcards.ToArray(),
                wildcardSlots = planet.GetWildcardSlots(),
                facilityOptions = options.ToArray(),
                prioritiesFood = SafePriorities(() => planet.GetLocalPriorities(0)),
                prioritiesFuel = SafePriorities(() => planet.GetLocalPriorities(1)),
                globalPrioritiesFood = SafePriorities(() => Planet.GetGlobalPriorities(0)),
                globalPrioritiesFuel = SafePriorities(() => Planet.GetGlobalPriorities(1)),
            };
        }

        // Priorities fall back to `Registry.Instance.DefaultPriorities`, which is only present
        // once the registry has been initialised (i.e. a real New Game session).
        private static int[] SafePriorities(Func<List<int>> getter)
        {
            try
            {
                var list = getter();
                return list != null ? list.ToArray() : new int[0];
            }
            catch
            {
                return new int[0];
            }
        }

        private static string SpriteName(Sprite sprite) => sprite != null ? sprite.name : string.Empty;

        // Planets only carry the resources they actually have; reading a missing key throws.
        private static int SafeResource(Planet planet, Registry.Resources resource)
        {
            return planet.Resources != null && planet.Resources.TryGetValue(resource, out var amount) ? amount : 0;
        }

        private static int SafeResource(Planet planet, Registry.AdvancedResources resource)
        {
            return planet.AdvancedResources != null && planet.AdvancedResources.TryGetValue(resource, out var amount) ? amount : 0;
        }

        private static string FacilityName(Registry registry, string facilityId)
        {
            if (string.IsNullOrEmpty(facilityId)) return string.Empty;
            var info = registry.GetFacilityInfo(facilityId);
            return info != null ? info.Name : facilityId;
        }

        private static string FacilityIcon(Registry registry, string facilityId)
        {
            if (string.IsNullOrEmpty(facilityId)) return string.Empty;
            return SpriteName(registry.GetFacilitySprite(facilityId));
        }

        private static QueueEntry[] BuildQueue()
        {
            var handler = OrderHandler.Instance;
            if (handler == null || handler.QueueData == null) return new QueueEntry[0];

            var result = new List<QueueEntry>();
            foreach (var pair in handler.QueueData)
            {
                if (pair.Value == null) continue;
                foreach (var order in pair.Value)
                {
                    if (order == null) continue;
                    result.Add(new QueueEntry
                    {
                        planet = order.Planet,
                        type = order.Type.ToString(),
                        progress = order.Length > 0f ? 1f - Mathf.Clamp01(order.LengthLeft / order.Length) : 0f,
                        assigned = order.Assigned,
                        maxPeople = order.MaxPeople,
                        length = Mathf.RoundToInt(order.Length),
                        lengthLeft = Mathf.RoundToInt(order.LengthLeft),
                        speed = order.SpeedPerPerson * order.Assigned,
                    });
                }
            }
            return result.ToArray();
        }

        // ---------------------------------------------------------------------
        // Snapshot DTOs (JsonUtility-compatible)
        // ---------------------------------------------------------------------

        [Serializable]
        public class ResourceEntry
        {
            public string id;
            public string name;
            public int amount;
            public bool advanced;
            public string icon;
            public int value;
        }

        [Serializable]
        public class SlotEntry
        {
            public int index;
            public bool used;
        }

        [Serializable]
        public class GraphSnapshot
        {
            public int[] people;
            public int[] resources;
            public int[] facilities;
        }

        [Serializable]
        public class FacilitySlotEntry
        {
            public string resourceId;
            public string resourceName;
            public string resourceIcon;
            public bool built;
            public string facilityId;
            public string facilityName;
            public string facilityIcon;
            public float progression;
        }

        [Serializable]
        public class WildcardEntry
        {
            public bool empty;
            public string facilityId;
            public string facilityName;
            public string facilityIcon;
            public float progression;
        }

        [Serializable]
        public class FacilityOption
        {
            public string id;
            public string name;
            public string description;
            public string icon;
            public bool wildcard;
            public string slotResource;
            public bool canBuild;
            public bool built;
        }

        [Serializable]
        public class DialogSnapshot
        {
            public bool visible;
            public string name;
            public string text;
            public string[] choices;
        }

        [Serializable]
        public class QuestSnapshot
        {
            public bool visible;
            public string text;
        }

        [Serializable]
        public class TooltipSnapshot
        {
            public bool visible;
            public string title;
            public string description;
            public string info1;
            public string info2;
            public float x;
            public float y;
        }

        [Serializable]
        public class SettingsSnapshot
        {
            public float master;
            public float ui;
            public float sfx;
            public float music;
            public bool backgroundSound;
            public bool showPrompt;
            public bool vsync;
            public bool fullscreen;
            public string resolution;
            public string quality;
            public string antialiasing;
            public string antialiasingQuality;
            public string mode;
            public string provider;
            public string baseUrl;
            public string model;
            public string apiKey;
            public float temperature;
            public int verbatim;
            public int summarized;
            public bool thoughts;
        }

        [Serializable]
        public class ShipEntry
        {
            public int index;
            public string type;
            public string icon;
            public int fuel;
            public int requiredFuel;
            public bool canLaunch;
            public bool canRefuel;
            public int refuelAmount;
        }

        [Serializable]
        public class ShippingSnapshot
        {
            public string mode;
            public int selectedShip;
            public bool hasPlayer;
            public bool leaderInTransit;
            public string resourceId;
            public bool resourceAdvanced;
            public int resourceAmount;
            public int peopleAmount;
            public int people;
            public string shipType;
            public int maxResource;
            public int maxPeople;
            public int availableFuel;
            public ShipEntry[] ships;
        }

        [Serializable]
        public class PlanetSnapshot
        {
            public string name;
            public int people;
            public bool hasPlayer;
            public string temperate;
            public int fuel;
            public int ships;
            public ResourceEntry[] resources;
            public string[] facilities;
            public GraphSnapshot graph;
            public FacilitySlotEntry[] facilitySlots;
            public WildcardEntry[] wildcards;
            public int wildcardSlots;
            public FacilityOption[] facilityOptions;
            public int[] prioritiesFood;
            public int[] prioritiesFuel;
            public int[] globalPrioritiesFood;
            public int[] globalPrioritiesFuel;
            public ShippingSnapshot shipping;
        }

        [Serializable]
        public class QueueEntry
        {
            public string planet;
            public string type;
            public float progress;
            public int assigned;
            public int maxPeople;
            public int length;
            public int lengthLeft;
            public float speed;
        }

        [Serializable]
        public class StateSnapshot
        {
            public string scene;
            public bool isGameScene;
            public bool paused;
            public bool prompt;
            public int survivalTime;
            public int totalTime;
            public int systems;
            public long naturalResourcesUnits;
            public long advancedResourcesUnits;
            public long facilitiesCount;
            public long transformativeFacilitiesCount;
            public SlotEntry[] slots;
            public bool hasSelectedPlanet;
            public PlanetSnapshot selectedPlanet;
            public QueueEntry[] queue;
            public DialogSnapshot dialog;
            public QuestSnapshot quest;
            public TooltipSnapshot tooltip;
            public SettingsSnapshot settings;
        }
    }
}
