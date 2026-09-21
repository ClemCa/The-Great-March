using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable

[System.Serializable]
public class PlaylistTrack
{
    public string name;
    public float activityLevel;

    public PlaylistTrack(string name, float activityLevel = 0f)
    {
        this.name = name;
        this.activityLevel = activityLevel;
    }
}

[System.Serializable]
public class Playlist
{
    public string name;
    public List<PlaylistTrack> tracks;
    public Playlist(string name, List<PlaylistTrack> tracks)
    {
        this.name = name;
        this.tracks = tracks;
    }
}

public class Sound : MonoBehaviour
{
    private static WaitForSecondsRealtime _waitForSecondsRealtime1 = new WaitForSecondsRealtime(1f);
    [SerializeField] private SerializableDictionary<string, AudioClip> music = new SerializableDictionary<string, AudioClip>();
    [SerializeField] private List<Playlist> playlists = new List<Playlist>();
    [SerializeField] private SerializableDictionary<string, AudioClip> sounds = new SerializableDictionary<string, AudioClip>();
    [SerializeField] private SerializableDictionary<string, SerializableDictionarySubarray<AudioClip>> multiSounds = new SerializableDictionary<string, SerializableDictionarySubarray<AudioClip>>();
    [SerializeField] private int maxSoundSources = 5;
    [SerializeField] private string startingPlaylist = "";
    [SerializeField] private float maxActivitySpread = 1f;
    [SerializeField] private float typicalActivitySpread = 0.5f;
    [SerializeField] private float crossFadeDuration = 0.5f;
    private static float currentActivityLevel = 0f;
    private Dictionary<string, int> lastPlayedIndices = new Dictionary<string, int>();
    private Dictionary<string, string> lastPlayedTracks = new Dictionary<string, string>();
    private Dictionary<string, List<int>> playlistOrders = new Dictionary<string, List<int>>();
    private Dictionary<string, int> playlistPositions = new Dictionary<string, int>();
    private Dictionary<string, float> playlistOrderActivityLevels = new Dictionary<string, float>();
    private AudioSource? musicSource = null;
    private List<AudioSource> soundSources = new List<AudioSource>();
    private static Sound? instance;
    private Coroutine? playlistLoopCoroutine;
    private HashSet<string> lastFrameSounds = new HashSet<string>();
    public static IEnumerable<AudioSource> AllAudioSources
    {
        get
        {
            if (instance == null)
            {
                return Enumerable.Empty<AudioSource>();
            }
            if (instance.musicSource == null)
            {
               return instance.soundSources.Where(static source => source != null);
            }
            return instance.soundSources.Append(instance.musicSource).Where(static source => source != null);
        }
    }
    void Awake()
    {
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    void Start()
    {
        var existingSource = transform.Find("MusicSource");
        if (existingSource != null && existingSource.TryGetComponent<AudioSource>(out var source))
        {
            musicSource = source;
        }
        else
        {
            var newChild = new GameObject("MusicSource");
            newChild.transform.parent = transform;
            musicSource = newChild.AddComponent<AudioSource>();
        }
        musicSource.loop = false;
        var playlist = playlists.Find(p => p.name == startingPlaylist);
        if (playlist == null || playlist.tracks.Count == 0) return;
        StartPlaylist(playlist.name, playlist.tracks, -1, stopCurrentTrack: false);
    }

    void LateUpdate()
    {
        lastFrameSounds.Clear();
    }

    private AudioClip? GetClip(string name)
    {
        if (sounds.ContainsKey(name))
            return sounds[name];
        if (multiSounds.ContainsKey(name))
        {
            var arr = multiSounds[name].array;
            if (arr.Length == 0) return null;
            int index = Random.Range(0, arr.Length);
            return arr[index];
        }
        return null;
    }

    public static void PlaySound(string name)
    {
        PlaySound(name, VolumeLayer.UI);
    }

    public static void PlaySound(string name, VolumeLayer layer)
    {
        if(instance == null) return;
        if(instance.lastFrameSounds.Contains(name))
            return; // prevent same sound playing multiple times in the same frame
        instance.lastFrameSounds.Add(name);
        AudioSource freeSource = instance.GetFreeSoundSource();
        var clip = instance.GetClip(name);
        if (clip == null) return; // Skip playback if no clip exists
        freeSource.clip = clip;
        freeSource.volume = Settings.ComputedVolume(layer);
        freeSource.pitch = Random.Range(0.95f, 1.05f); // slight pitch variation for less repetition
        freeSource.ignoreListenerPause = Settings.BackgroundSound;
        freeSource.tag = layer.ToString();
        freeSource.Play();
    }

    public static void SetActivityLevel(float level)
    {
        currentActivityLevel = level;
    }

    public static void ChangePlaylist(string playlistName, int activityLevel = -1)
    {
        if(instance == null) return;
        if (activityLevel >= 0)
        {
            currentActivityLevel = activityLevel;
        }
        var playlist = instance.playlists.Find(p => p.name == playlistName);
        if (playlist == null || playlist.tracks.Count == 0) return;
        int index = Random.Range(0, playlist.tracks.Count);
        instance.StartPlaylist(playlistName, playlist.tracks, index, stopCurrentTrack: true);
    }

    private void StartPlaylist(string playlistName, IEnumerable<PlaylistTrack> tracks, int startIndex, bool stopCurrentTrack)
    {
        if (playlistLoopCoroutine != null)
        {
            StopCoroutine(playlistLoopCoroutine);
            playlistLoopCoroutine = null;
        }
        if (stopCurrentTrack && musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
        playlistLoopCoroutine = StartCoroutine(PlaylistLoop(playlistName, tracks, startIndex));
    }

    private AudioSource GetFreeSoundSource()
    {
        foreach (var source in soundSources)
        {
            if (!source.isPlaying)
                return source;
        }
        if (soundSources.Count < maxSoundSources)
        {
            var newChild = new GameObject("SoundSource");
            newChild.transform.parent = transform;
            var newSource = newChild.AddComponent<AudioSource>();
            soundSources.Add(newSource);
            return newSource;
        }
        return soundSources[0]; // all busy, return the first one
    }

    private IEnumerator PlaylistLoop(string playlistName, IEnumerable<PlaylistTrack> tracks, int startIndex)
    {
        var trackList = tracks as IList<PlaylistTrack> ?? tracks.ToList();
        if (trackList.Count == 0)
        {
            yield break;
        }
        int preferredIndex = startIndex;
        while (true)
        {
            if (musicSource == null)
            {
                yield return new WaitForSecondsRealtime(1f);
                continue;
            }
            // Check if current track exceeds max activity spread & fade immediately
            if (musicSource.isPlaying && musicSource.clip != null)
            {
                PlaylistTrack? currentTrack = null;
                foreach (var t in trackList)
                {
                    if (music.TryGetValue(t.name, out var c) && c == musicSource.clip)
                    {
                        currentTrack = t;
                        break;
                    }
                }
                if (currentTrack != null)
                {
                    float spread = Mathf.Abs(currentTrack.activityLevel - currentActivityLevel);
                    if (spread > maxActivitySpread)
                    {
                        // can we actually find anything in that activity range?
                        if (!trackList.Any(t => Mathf.Abs(t.activityLevel - currentActivityLevel) <= typicalActivitySpread))
                        {
                            // no, just let it keep playing
                            // the delay is there to prevent immediately checking again next frame, it's not a cheap check
                            yield return _waitForSecondsRealtime1;
                            continue;
                        }
                        yield return FadeOutAndSwitch(playlistName, trackList);
                        preferredIndex = -1;
                        continue;
                    }
                }
            }
            if (!musicSource.isPlaying)
            {
                int currentIndex = GetNextPlaylistIndex(playlistName, trackList, preferredIndex);
                preferredIndex = -1;
                if (!music.TryGetValue(trackList[currentIndex].name, out var clip) || clip == null)
                {
                    yield return new WaitForSecondsRealtime(1f);
                    continue;
                }
                lastPlayedTracks[playlistName] = trackList[currentIndex].name;
                musicSource.clip = clip;
                musicSource.volume = Settings.ComputedVolume(VolumeLayer.Music);
                musicSource.ignoreListenerPause = Settings.BackgroundSound;
                musicSource.tag = VolumeLayer.Music.ToString();
                musicSource.Play();
            }
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private IEnumerator FadeOutAndSwitch(string playlistName, IList<PlaylistTrack> trackList)
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;
        float elapsed = 0f;
        while (elapsed < crossFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (musicSource != null)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / crossFadeDuration);
            }
            yield return null;
        }

        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }

        // Pick a track from the valid pool (within typical activity spread)
        int nextIndex = GetNextPlaylistIndex(playlistName, trackList, -1);
        if (musicSource != null && music.TryGetValue(trackList[nextIndex].name, out var clip) && clip != null)
        {
            lastPlayedTracks[playlistName] = trackList[nextIndex].name;
            musicSource.clip = clip;
            musicSource.volume = Settings.ComputedVolume(VolumeLayer.Music);
            musicSource.ignoreListenerPause = Settings.BackgroundSound;
            musicSource.tag = VolumeLayer.Music.ToString();
            musicSource.Play();
        }
    }

    private void RefillPlaylistOrder(string playlistName, List<int> order, IList<PlaylistTrack> trackList, int preferredIndex)
    {
        order.Clear();
        // Build valid pool: tracks within typical activity spread of the current target level
        for (int i = 0; i < trackList.Count; i++)
        {
            float spread = Mathf.Abs(trackList[i].activityLevel - currentActivityLevel);
            if (spread <= typicalActivitySpread)
            {
                order.Add(i);
            }
        }
        // Fallback: if no tracks in range, use all tracks
        if (order.Count == 0)
        {
            for (int i = 0; i < trackList.Count; i++)
            {
                order.Add(i);
            }
        }
        // Shuffle
        for (int i = order.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (order[i], order[swapIndex]) = (order[swapIndex], order[i]);
        }
        // If preferredIndex is valid and in the pool, move it to front
        if (preferredIndex >= 0 && preferredIndex < trackList.Count && order.Contains(preferredIndex))
        {
            int preferredOrderIndex = order.IndexOf(preferredIndex);
            if (preferredOrderIndex > 0)
            {
                (order[0], order[preferredOrderIndex]) = (order[preferredOrderIndex], order[0]);
            }
        }
        // Avoid repeating the last played track
        int poolCount = order.Count;
        if (poolCount > 1 && lastPlayedIndices.TryGetValue(playlistName, out int lastPlayed) && order[0] == lastPlayed)
        {
            int swapIndex = order.FindIndex(index => index != lastPlayed);
            if (swapIndex > 0)
            {
                (order[0], order[swapIndex]) = (order[swapIndex], order[0]);
            }
        }
        playlistPositions[playlistName] = 0;
        playlistOrderActivityLevels[playlistName] = currentActivityLevel;
    }

    private int GetNextPlaylistIndex(string playlistName, IList<PlaylistTrack> trackList, int preferredIndex)
    {
        int count = trackList.Count;
        bool needsRebuild = !playlistOrders.TryGetValue(playlistName, out var order)
            || !playlistOrderActivityLevels.TryGetValue(playlistName, out float builtAtLevel)
            || !Mathf.Approximately(builtAtLevel, currentActivityLevel);
        if (needsRebuild)
        {
            order = new List<int>(count);
            playlistOrders[playlistName] = order;
            playlistPositions.Remove(playlistName);
        }
        if (!playlistPositions.TryGetValue(playlistName, out int position) || position >= order.Count)
        {
            RefillPlaylistOrder(playlistName, order, trackList, preferredIndex);
            position = 0;
        }

        int nextIndex = order[position];
        playlistPositions[playlistName] = position + 1;
        lastPlayedIndices[playlistName] = nextIndex;

        // Avoid immediate repeat when the chosen track name matches the last played track name
        if (trackList.Count > 1 && lastPlayedTracks.TryGetValue(playlistName, out var lastTrack) && trackList[nextIndex].name == lastTrack)
        {
            int currentOrderPosition = playlistPositions[playlistName] - 1;
            int swapPosition = order.FindIndex(currentOrderPosition + 1, index => trackList[index].name != lastTrack);
            if (swapPosition >= 0)
            {
                (order[currentOrderPosition], order[swapPosition]) = (order[swapPosition], order[currentOrderPosition]);
                nextIndex = order[currentOrderPosition];
                lastPlayedIndices[playlistName] = nextIndex;
            }
        }
        return nextIndex;
    }
}