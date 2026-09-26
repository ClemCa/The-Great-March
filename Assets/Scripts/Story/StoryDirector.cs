using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Runs a story graph at runtime. At each branch point the eligible siblings are gathered, one is
/// chosen by weight, and every other sibling is permanently locked. Decisions complete instantly;
/// beats run their content (appearances, Yarn dialogue, triggers) before completing.
/// </summary>
public class StoryDirector : MonoBehaviour
{
    private static StoryDirector _instance;

    [SerializeField] private StoryGraph _mainGraph;
    [SerializeField] private bool _autoStart = false;
    [SerializeField] private DialogueRunner _dialogueRunner;
    [SerializeField] private float _pollInterval = 0.5f;
    [SerializeField] private bool _logActivity = true;

    private readonly HashSet<string> _completed = new HashSet<string>();
    private readonly HashSet<string> _locked = new HashSet<string>();
    private readonly List<string> _pending = new List<string>();
    private readonly System.Random _rng = new System.Random();

    private StoryConditionContext _context;
    private StoryNode _activeBeat;
    private bool _started;
    private float _nextPoll;

    public static StoryDirector Instance { get { return _instance; } }
    public StoryGraph Graph { get { return _mainGraph; } }
    public bool Started { get { return _started; } }
    public bool IsRunningBeat { get { return _activeBeat != null; } }
    public StoryNode ActiveBeat { get { return _activeBeat; } }

    public IReadOnlyCollection<string> Completed { get { return _completed; } }
    public IReadOnlyCollection<string> Locked { get { return _locked; } }
    public IReadOnlyList<string> Pending { get { return _pending; } }

    public event Action<StoryNode> NodeCompleted;
    public event Action<StoryNode> BeatStarted;
    public event Action<StoryNode> BeatFinished;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (_autoStart && _mainGraph != null)
            Begin(_mainGraph);
    }

    private void Update()
    {
        if (!_started || _activeBeat != null || _mainGraph == null)
            return;
        if (Time.unscaledTime < _nextPoll)
            return;
        _nextPoll = Time.unscaledTime + _pollInterval;
        Evaluate();
    }

    public void EnsureStarted(StoryGraph graph)
    {
        if (!_started)
            Begin(graph);
    }

    public void Begin(StoryGraph graph)
    {
        if (graph != null)
            _mainGraph = graph;

        _completed.Clear();
        _locked.Clear();
        _pending.Clear();
        _activeBeat = null;
        _started = true;

        if (_mainGraph == null)
        {
            Debug.LogWarning("[Story] Begin called without a graph.");
            return;
        }

        var root = _mainGraph.Root();
        if (root == null)
        {
            Debug.LogWarning("[Story] Graph '" + _mainGraph.name + "' has no valid root.");
            return;
        }

        _pending.Add(root.Id);
        Evaluate();
    }

    public void Restart()
    {
        Begin(_mainGraph);
    }

    public void EvaluateNow()
    {
        Evaluate();
    }

    private void Evaluate()
    {
        if (_graphInvalid())
            return;

        _context = new StoryConditionContext
        {
            ResourceReader = StoryResourceReader.Read,
            YarnReader = ReadYarnVariable,
            Now = GameClock.Now,
            NowDay = GameClock.Day
        };

        while (_activeBeat == null)
        {
            PrunePending();
            if (_pending.Count == 0)
                return;

            var candidates = new List<StoryNode>();
            for (int i = 0; i < _pending.Count; i++)
            {
                var node = _mainGraph.GetNode(_pending[i]);
                if (node == null)
                    continue;
                if (StoryConditionEvaluator.NodeGatePassed(node, _context))
                    candidates.Add(node);
            }

            if (candidates.Count == 0)
                return;

            var chosen = StoryConditionEvaluator.PickWeighted(candidates, _rng);
            if (chosen == null)
                return;

            // Mutually exclusive siblings: choosing one locks the rest for the whole campaign.
            for (int i = 0; i < _pending.Count; i++)
                if (_pending[i] != chosen.Id)
                    _locked.Add(_pending[i]);
            _pending.Clear();

            if (chosen.Type == StoryNodeType.Decision)
            {
                CompleteNode(chosen);
                continue;
            }

            _activeBeat = chosen;
            StartCoroutine(RunBeatCoroutine(chosen));
            return;
        }
    }

    private bool _graphInvalid()
    {
        if (_mainGraph == null)
            return true;
        return false;
    }

    private void PrunePending()
    {
        for (int i = _pending.Count - 1; i >= 0; i--)
            if (_completed.Contains(_pending[i]) || _locked.Contains(_pending[i]))
                _pending.RemoveAt(i);
    }

    private void CompleteNode(StoryNode node)
    {
        if (!_completed.Add(node.Id))
            return;

        NodeCompleted?.Invoke(node);
        if (_logActivity)
            Debug.Log("[Story] Completed " + (node.Type == StoryNodeType.Decision ? "decision" : "beat") + " '" + node.Name + "'.");

        var children = _mainGraph.ChildrenOf(node.Id);
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if (_completed.Contains(child.Id) || _locked.Contains(child.Id))
                continue;
            if (!_pending.Contains(child.Id))
                _pending.Add(child.Id);
        }
    }

    private IEnumerator RunBeatCoroutine(StoryNode node)
    {
        BeatStarted?.Invoke(node);
        if (_logActivity)
            Debug.Log("[Story] Beat '" + node.Name + "' started.");

        if (node.Appearances != null)
            for (int i = 0; i < node.Appearances.Count; i++)
                StoryAppearanceService.Apply(node.Appearances[i]);

        if (node.YarnNodes != null)
        {
            var runner = ResolveRunner();
            for (int i = 0; i < node.YarnNodes.Count; i++)
            {
                string title = node.YarnNodes[i];
                if (string.IsNullOrEmpty(title))
                    continue;
                if (runner == null)
                {
                    Debug.LogWarning("[Story] Beat '" + node.Name + "' wants Yarn node '" + title + "' but no DialogueRunner is available.");
                    yield return null;
                    continue;
                }
                runner.StartDialogue(title);
                while (runner.IsDialogueRunning)
                    yield return null;
            }
        }

        if (node.Triggers != null)
            for (int i = 0; i < node.Triggers.Count; i++)
                StoryTriggerRegistry.Invoke(node.Triggers[i].Key, node.Triggers[i].Payload);

        BeatFinished?.Invoke(node);
        _activeBeat = null;
        CompleteNode(node);
        Evaluate();
    }

    private DialogueRunner ResolveRunner()
    {
        if (_dialogueRunner != null)
            return _dialogueRunner;
        if (DialogDisplayer.Instance != null)
            return DialogDisplayer.Instance.Runner;
        return null;
    }

    public DialogueRunner GetRunner()
    {
        return ResolveRunner();
    }

    private object ReadYarnVariable(string variable)
    {
        if (string.IsNullOrEmpty(variable))
            return null;
        var runner = ResolveRunner();
        var storage = runner != null ? runner.VariableStorage : null;
        if (storage == null)
            return null;
        if (storage.TryGetValue<float>(variable, out float number))
            return number;
        if (storage.TryGetValue<bool>(variable, out bool boolean))
            return boolean;
        if (storage.TryGetValue<string>(variable, out string text))
            return text;
        return null;
    }

    #region Persistence

    public StorySaveState CaptureState()
    {
        var state = new StorySaveState
        {
            Completed = new List<string>(_completed),
            Locked = new List<string>(_locked),
            Pending = new List<string>(_pending),
            ActiveBeat = _activeBeat != null ? _activeBeat.Id : ""
        };
        return state;
    }

    public void RestoreState(StorySaveState state)
    {
        _completed.Clear();
        _locked.Clear();
        _pending.Clear();
        _activeBeat = null;
        _started = true;

        if (state == null || _mainGraph == null)
            return;

        if (state.Completed != null)
            foreach (var id in state.Completed)
                if (!string.IsNullOrEmpty(id))
                    _completed.Add(id);
        if (state.Locked != null)
            foreach (var id in state.Locked)
                if (!string.IsNullOrEmpty(id))
                    _locked.Add(id);
        if (state.Pending != null)
            foreach (var id in state.Pending)
                if (!string.IsNullOrEmpty(id) && !_completed.Contains(id) && !_locked.Contains(id))
                    _pending.Add(id);
        if (!string.IsNullOrEmpty(state.ActiveBeat))
            _pending.Add(state.ActiveBeat);

        Evaluate();
    }

    #endregion
}
