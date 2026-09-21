using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Boots the thought system: builds the starter taxonomy, traits and events, then overlays
/// authored content from Assets/Resources/Thoughts. Authored files that fail to load are
/// reported rather than silently ignored.
/// </summary>
public class ThoughtBootstrap : MonoBehaviour
{
    [SerializeField] private bool _loadAuthoredContent = true;

    public ThoughtTaxonomy Taxonomy { get; private set; }
    public TraitLibrary Traits { get; private set; }
    public List<ThoughtCharacter> Characters { get; private set; }

    private static ThoughtBootstrap _instance;
    private bool _initialized = false;

    public static ThoughtBootstrap Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        Initialize();
    }

    public void Initialize()
    {
        if (_initialized)
            return;
        _initialized = true;

        Taxonomy = ThoughtTaxonomy.CreateStarter();
        Traits = TraitLibrary.CreateStarter();
        Characters = new List<ThoughtCharacter>();

        if (_loadAuthoredContent)
            LoadAuthoredContent();
        else
            RegisterCharacters();
    }

    public void LoadAuthoredContent()
    {
        var assets = Resources.LoadAll<TextAsset>("Thoughts");
        for (int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i];
            if (!asset.name.EndsWith(".character"))
                continue;

            CharacterData data;
            try
            {
                data = JsonConvert.DeserializeObject<CharacterData>(asset.text);
            }
            catch (System.Exception exception)
            {
                Debug.LogError("Failed to parse character '" + asset.name + "': " + exception.Message);
                continue;
            }
            if (data == null || string.IsNullOrEmpty(data.Id))
            {
                Debug.LogError("Character file '" + asset.name + "' has no id.");
                continue;
            }

            var character = data.ToCharacter(Taxonomy, Traits);
            Characters.Add(character);

            // Register immediately so events raised after loading can find them.
            ThoughtCharacterRegistry.Register(character);
        }

        var eventsAsset = Resources.Load<TextAsset>("Thoughts/events");
        if (eventsAsset != null)
        {
            var system = WorldEventSystem.Instance;
            if (system == null)
                system = UnityEngine.Object.FindAnyObjectByType<WorldEventSystem>();
            if (system != null)
            {
                system.EnsureInitialized();
                EventDataFile file;
                try
                {
                    file = JsonConvert.DeserializeObject<EventDataFile>(eventsAsset.text);
                }
                catch (System.Exception exception)
                {
                    Debug.LogError("Failed to parse events: " + exception.Message);
                    file = null;
                }
                if (file != null)
                    for (int i = 0; i < file.Events.Count; i++)
                        system.Register(file.Events[i]);
            }
        }
    }

    private void RegisterCharacters()
    {
        for (int i = 0; i < Characters.Count; i++)
            ThoughtCharacterRegistry.Register(Characters[i]);
    }
}
