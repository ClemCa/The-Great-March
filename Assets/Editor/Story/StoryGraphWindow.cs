using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Authoring window for evolutive story graphs. Open with Window/Story Graph. Shows the tree as a
/// pannable 360-degree force layout; click a node to edit it on the right, use a link's "+" to
/// insert a node between two beats.
/// </summary>
public class StoryGraphWindow : EditorWindow
{
    private StoryGraph _graph;
    private StoryGraphCanvas _canvas;
    private StoryGraphInspector _inspector;
    private ObjectField _graphField;
    private Label _status;

    [MenuItem("Window/Story Graph")]
    public static void Open()
    {
        var window = GetWindow<StoryGraphWindow>();
        window.titleContent = new GUIContent("Story Graph");
        window.minSize = new Vector2(820f, 520f);
    }

    private void OnEnable()
    {
        BuildUI();
        Selection.selectionChanged += OnProjectSelection;
        if (_graph == null)
            _graph = Selection.activeObject as StoryGraph;
        SetGraph(_graph);
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnProjectSelection;
        Save();
    }

    private void BuildUI()
    {
        rootVisualElement.style.flexDirection = FlexDirection.Column;

        var toolbar = new VisualElement();
        toolbar.style.flexDirection = FlexDirection.Row;
        toolbar.style.alignItems = Align.Center;
        toolbar.style.paddingLeft = 6;
        toolbar.style.paddingRight = 6;
        toolbar.style.paddingTop = 4;
        toolbar.style.paddingBottom = 4;

        _graphField = new ObjectField("Graph") { objectType = typeof(StoryGraph) };
        _graphField.style.width = 260;
        _graphField.RegisterValueChangedCallback(evt => SetGraph(evt.newValue as StoryGraph));
        toolbar.Add(_graphField);

        toolbar.Add(MakeButton("New", CreateGraph));
        toolbar.Add(MakeButton("Layout", () => _canvas.ClearPinsAndLayout()));
        toolbar.Add(MakeButton("Relayout", () => _canvas.RunLayout(radial: true)));
        toolbar.Add(MakeButton("Fit", () => _canvas.Fit()));
        toolbar.Add(MakeButton("Center Root", () => _canvas.CenterOnRoot()));
        toolbar.Add(MakeButton("Validate", ValidateGraph));
        toolbar.Add(MakeButton("Save", Save));

        rootVisualElement.Add(toolbar);

        var main = new VisualElement();
        main.style.flexDirection = FlexDirection.Row;
        main.style.flexGrow = 1;

        _canvas = new StoryGraphCanvas();
        _canvas.style.flexGrow = 1;
        _canvas.GraphChanged += OnGraphChanged;
        _canvas.NodeSelected += OnNodeSelected;

        _inspector = new StoryGraphInspector();
        _inspector.Changed += OnInspectorChanged;
        _inspector.RequestDelete += node => _canvas.DeleteNode(node);

        main.Add(_canvas);
        main.Add(_inspector);
        rootVisualElement.Add(main);

        var statusBar = new VisualElement();
        statusBar.style.flexDirection = FlexDirection.Row;
        statusBar.style.paddingLeft = 6;
        statusBar.style.paddingTop = 2;
        statusBar.style.paddingBottom = 2;
        _status = new Label("Select or create a graph.");
        statusBar.Add(_status);
        rootVisualElement.Add(statusBar);

        rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);
        rootVisualElement.focusable = true;
    }

    private Button MakeButton(string text, System.Action action)
    {
        var button = new Button(action) { text = text };
        button.style.marginLeft = 4;
        return button;
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Delete && _canvas.SelectedNode != null)
        {
            _canvas.DeleteNode(_canvas.SelectedNode);
            evt.StopPropagation();
        }
    }

    private void OnProjectSelection()
    {
        if (Selection.activeObject is StoryGraph graph && graph != _graph)
            SetGraph(graph);
    }

    private void SetGraph(StoryGraph graph)
    {
        _graph = graph;
        if (_graphField != null)
            _graphField.SetValueWithoutNotify(graph);
        _inspector.SetGraph(graph);
        _canvas.SetGraph(graph);
        _canvas.schedule.Execute(() => _canvas.Fit()).ExecuteLater(80);
        SetStatus(graph == null ? "No graph selected." : graph.Title + " — " + graph.Nodes.Count + " nodes, " + graph.Edges.Count + " links.");
    }

    private void OnNodeSelected(StoryNode node)
    {
        _inspector.SetNode(node);
        if (node != null)
            SetStatus((node.Type == StoryNodeType.Decision ? "Decision: " : "Beat: ") + node.Name);
    }

    private void OnInspectorChanged()
    {
        if (_graph == null)
            return;
        EditorUtility.SetDirty(_graph);
        _canvas.Rebuild();
        _inspector.SetNode(_canvas.SelectedNode);
    }

    private void OnGraphChanged(StoryGraphCanvas canvas)
    {
        if (_graph == null)
            return;
        EditorUtility.SetDirty(_graph);
        SetStatus(_graph.Title + " — " + _graph.Nodes.Count + " nodes, " + _graph.Edges.Count + " links.");
    }

    private void CreateGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Story Graph", "StoryGraph", "asset", "Choose where to save the new story graph.");
        if (string.IsNullOrEmpty(path))
            return;

        var graph = CreateInstance<StoryGraph>();
        graph.Title = System.IO.Path.GetFileNameWithoutExtension(path);
        var root = graph.AddNode(StoryNodeType.Decision, null, new Vector2(0f, 0f), "Start");
        graph.RootId = root.Id;
        AssetDatabase.CreateAsset(graph, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = graph;
        SetGraph(graph);
    }

    private void ValidateGraph()
    {
        if (_graph == null)
            return;
        var problems = _graph.Validate();
        if (problems.Count == 0)
        {
            SetStatus("Graph is valid.");
            Debug.Log("[StoryGraph] '" + _graph.name + "' is valid.", _graph);
            return;
        }
        for (int i = 0; i < problems.Count; i++)
            Debug.LogWarning("[StoryGraph] " + problems[i], _graph);
        SetStatus(problems.Count + " problem(s) found — see Console.");
    }

    private void Save()
    {
        if (_graph == null)
            return;
        EditorUtility.SetDirty(_graph);
        AssetDatabase.SaveAssets();
    }

    private void SetStatus(string text)
    {
        if (_status != null)
            _status.text = text;
    }
}
