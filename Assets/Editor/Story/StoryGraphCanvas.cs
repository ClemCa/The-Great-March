using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Pannable, zoomable graph surface. Nodes are little name+type tiles, links are drawn lines with
/// a "+" hotspot at the midpoint that inserts a node between the two ends.
/// </summary>
public class StoryGraphCanvas : VisualElement
{
    private const float Origin = 20000f;
    private const float MinZoom = 0.2f;
    private const float MaxZoom = 2.5f;

    public StoryGraph Graph { get; private set; }
    public event Action<StoryNode> NodeSelected;
    public event Action<StoryGraphCanvas> GraphChanged;

    private readonly VisualElement _content;
    private readonly VisualElement _edgeLayer;
    private readonly Dictionary<string, StoryNodeView> _nodeViews = new Dictionary<string, StoryNodeView>();
    private readonly List<(StoryEdge edge, Button button)> _insertButtons = new List<(StoryEdge, Button)>();
    private readonly HashSet<string> _pinned = new HashSet<string>();

    private Vector2 _pan = new Vector2(0f, 0f);
    private float _zoom = 1f;
    private StoryNode _selected;

    private bool _panning;
    private int _panPointer = -1;
    private Vector3 _panStart;
    private Vector2 _panOrigin;

    public float Zoom { get { return _zoom; } }
    public StoryNode SelectedNode { get { return _selected; } }

    public StoryGraphCanvas()
    {
        style.flexGrow = 1;
        style.overflow = Overflow.Hidden;
        style.backgroundColor = new Color(0.12f, 0.12f, 0.13f);
        generateVisualContent += DrawBackground;

        _content = new VisualElement { name = "story-content", pickingMode = PickingMode.Ignore };
        _content.style.position = Position.Absolute;
        _content.style.left = 0;
        _content.style.top = 0;
        _content.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(0f, 0f));
        Add(_content);

        _edgeLayer = new VisualElement { name = "story-edges", pickingMode = PickingMode.Ignore };
        _edgeLayer.style.position = Position.Absolute;
        _edgeLayer.style.left = -Origin;
        _edgeLayer.style.top = -Origin;
        _edgeLayer.style.width = Origin * 2f;
        _edgeLayer.style.height = Origin * 2f;
        _edgeLayer.generateVisualContent += DrawEdges;

        RegisterCallback<WheelEvent>(OnWheel);
        RegisterCallback<PointerDownEvent>(OnPointerDown);
        RegisterCallback<PointerMoveEvent>(OnPointerMove);
        RegisterCallback<PointerUpEvent>(OnPointerUp);
        RegisterCallback<GeometryChangedEvent>(_ => UpdateEdgeButtons());
    }

    public void SetGraph(StoryGraph graph)
    {
        Graph = graph;
        _selected = null;
        _pinned.Clear();
        Rebuild();
        if (Graph != null && IsDegenerate(Graph))
            RunLayout(radial: true);
        UpdateTransform();
        Fit();
    }

    private static bool IsDegenerate(StoryGraph graph)
    {
        for (int i = 0; i < graph.Nodes.Count; i++)
            if (graph.Nodes[i].EditorPosition.sqrMagnitude > 1f)
                return false;
        return graph.Nodes.Count > 0;
    }

    #region Building

    public void Rebuild()
    {
        _content.Clear();
        _nodeViews.Clear();
        _insertButtons.Clear();

        if (Graph == null)
            return;

        _content.Add(_edgeLayer);

        for (int i = 0; i < Graph.Nodes.Count; i++)
        {
            var node = Graph.Nodes[i];
            if (node == null)
                continue;
            var view = new StoryNodeView(node) { Scale = _zoom };
            view.Selected += v => Select(v.Node);
            view.Moved += _ => { UpdateEdgeButtons(); _edgeLayer.MarkDirtyRepaint(); };
            view.Pinned += v => { _pinned.Add(v.Node.Id); GraphChanged?.Invoke(this); };
            view.ContextRequested += v => ShowNodeMenu(v.Node);
            _content.Add(view);
            _nodeViews[node.Id] = view;
        }

        for (int i = 0; i < Graph.Edges.Count; i++)
            CreateInsertButton(Graph.Edges[i]);

        if (_selected != null && _nodeViews.TryGetValue(_selected.Id, out var selectedView))
            selectedView.SetSelected(true);

        UpdateEdgeButtons();
        _edgeLayer.MarkDirtyRepaint();
    }

    private void CreateInsertButton(StoryEdge edge)
    {
        var button = new Button(() => ShowInsertMenu(edge));
        button.text = "+";
        button.style.position = Position.Absolute;
        button.style.width = 18;
        button.style.height = 18;
        button.style.fontSize = 12;
        button.style.unityFontStyleAndWeight = FontStyle.Bold;
        button.style.color = Color.white;
        button.style.backgroundColor = new Color(0.35f, 0.35f, 0.4f);
        button.style.borderTopLeftRadius = 9;
        button.style.borderTopRightRadius = 9;
        button.style.borderBottomLeftRadius = 9;
        button.style.borderBottomRightRadius = 9;
        button.style.borderLeftWidth = 0;
        button.style.borderRightWidth = 0;
        button.style.borderTopWidth = 0;
        button.style.borderBottomWidth = 0;
        button.tooltip = "Insert a node on this link";
        _content.Add(button);
        _insertButtons.Add((edge, button));
    }

    private void UpdateEdgeButtons()
    {
        for (int i = 0; i < _insertButtons.Count; i++)
        {
            var (edge, button) = _insertButtons[i];
            Vector2 midpoint = (GetCenter(edge.ParentId) + GetCenter(edge.ChildId)) * 0.5f;
            button.style.left = midpoint.x - 9f;
            button.style.top = midpoint.y - 9f;
        }
        _edgeLayer.MarkDirtyRepaint();
    }

    private Vector2 GetCenter(string nodeId)
    {
        var node = Graph != null ? Graph.GetNode(nodeId) : null;
        if (node == null)
            return Vector2.zero;
        Vector2 size = new Vector2(130f, 46f);
        if (_nodeViews.TryGetValue(nodeId, out var view) && view.layout.width > 1f)
            size = new Vector2(view.layout.width, view.layout.height);
        return node.EditorPosition + size * 0.5f;
    }

    #endregion

    #region Selection & mutation

    public void Select(StoryNode node)
    {
        if (_selected != null && _nodeViews.TryGetValue(_selected.Id, out var previous))
            previous.SetSelected(false);
        _selected = node;
        if (node != null && _nodeViews.TryGetValue(node.Id, out var current))
            current.SetSelected(true);
        NodeSelected?.Invoke(node);
    }

    public StoryNode AddChild(StoryNode parent, StoryNodeType type)
    {
        if (Graph == null || parent == null)
            return null;
        Vector2 position = parent.EditorPosition + new Vector2(200f, UnityEngine.Random.Range(-40f, 40f));
        var node = Graph.AddNode(type, parent.Id, position);
        Rebuild();
        Select(node);
        GraphChanged?.Invoke(this);
        return node;
    }

    public StoryNode InsertOnEdge(StoryEdge edge, StoryNodeType type)
    {
        if (Graph == null || edge == null)
            return null;
        var parent = Graph.GetNode(edge.ParentId);
        var child = Graph.GetNode(edge.ChildId);
        Vector2 position = parent != null && child != null
            ? (parent.EditorPosition + child.EditorPosition) * 0.5f
            : (parent != null ? parent.EditorPosition + new Vector2(160f, 0f) : Vector2.zero);

        var node = Graph.AddNode(type, parent != null ? parent.Id : null, position);
        if (parent != null && child != null)
        {
            Graph.Disconnect(parent.Id, child.Id);
            Graph.Connect(node.Id, child.Id);
        }
        Rebuild();
        Select(node);
        GraphChanged?.Invoke(this);
        return node;
    }

    public void DeleteSelected()
    {
        if (_selected != null)
            DeleteNode(_selected);
    }

    public void DeleteNode(StoryNode node)
    {
        if (Graph == null || node == null)
            return;
        Graph.RemoveNode(node.Id);
        if (_selected == node)
            _selected = null;
        Rebuild();
        Select(null);
        GraphChanged?.Invoke(this);
    }

    public void SetAsRoot(StoryNode node)
    {
        if (Graph == null || node == null)
            return;
        Graph.RootId = node.Id;
        GraphChanged?.Invoke(this);
    }

    #endregion

    #region Menus

    private void ShowNodeMenu(StoryNode node)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add Decision Child"), false, () => AddChild(node, StoryNodeType.Decision));
        menu.AddItem(new GUIContent("Add Beat Child"), false, () => AddChild(node, StoryNodeType.Beat));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Set as Root"), false, () => SetAsRoot(node));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete"), false, () => DeleteNode(node));
        menu.ShowAsContext();
    }

    private void ShowInsertMenu(StoryEdge edge)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Insert Decision"), false, () => InsertOnEdge(edge, StoryNodeType.Decision));
        menu.AddItem(new GUIContent("Insert Beat"), false, () => InsertOnEdge(edge, StoryNodeType.Beat));
        menu.ShowAsContext();
    }

    #endregion

    #region Layout

    public void RunLayout(bool radial)
    {
        if (Graph == null || Graph.Nodes.Count == 0)
            return;
        var options = new StoryForceLayout.Options { RadialSeed = radial };
        var targets = StoryForceLayout.Compute(Graph, _pinned, options);
        for (int i = 0; i < Graph.Nodes.Count; i++)
        {
            var node = Graph.Nodes[i];
            if (node == null || !targets.TryGetValue(node.Id, out var target))
                continue;
            node.EditorPosition = target;
        }
        foreach (var view in _nodeViews.Values)
            view.RefreshPosition();
        UpdateEdgeButtons();
        GraphChanged?.Invoke(this);
    }

    public void ClearPinsAndLayout()
    {
        _pinned.Clear();
        RunLayout(radial: true);
    }

    public void Fit()
    {
        if (Graph == null || Graph.Nodes.Count == 0 || layout.width < 1f || layout.height < 1f)
            return;

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        for (int i = 0; i < Graph.Nodes.Count; i++)
        {
            var p = Graph.Nodes[i].EditorPosition;
            minX = Mathf.Min(minX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxX = Mathf.Max(maxX, p.x + 140f);
            maxY = Mathf.Max(maxY, p.y + 50f);
        }
        float width = Mathf.Max(1f, maxX - minX);
        float height = Mathf.Max(1f, maxY - minY);
        _zoom = Mathf.Clamp(Mathf.Min(layout.width / (width + 120f), layout.height / (height + 120f)), MinZoom, 1.2f);

        Vector2 graphCenter = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        Vector2 canvasCenter = new Vector2(layout.width * 0.5f, layout.height * 0.5f);
        _pan = canvasCenter - graphCenter * _zoom;
        UpdateTransform();
    }

    public void CenterOnRoot()
    {
        if (Graph == null || Graph.Root() == null || layout.width < 1f)
            return;
        Vector2 canvasCenter = new Vector2(layout.width * 0.5f, layout.height * 0.5f);
        _pan = canvasCenter - (Graph.Root().EditorPosition + new Vector2(65f, 23f)) * _zoom;
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        _content.style.translate = new Translate(_pan.x, _pan.y);
        _content.style.scale = new Scale(new Vector3(_zoom, _zoom, 1f));
        foreach (var view in _nodeViews.Values)
            view.Scale = _zoom;
        _edgeLayer.MarkDirtyRepaint();
    }

    #endregion

    #region Input

    private void OnWheel(WheelEvent evt)
    {
        Vector2 local = this.WorldToLocal(evt.mousePosition);
        Vector2 graphBefore = (local - _pan) / _zoom;
        float factor = Mathf.Exp(evt.delta.y * 0.05f);
        _zoom = Mathf.Clamp(_zoom * factor, MinZoom, MaxZoom);
        _pan = local - graphBefore * _zoom;
        UpdateTransform();
        evt.StopPropagation();
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button == 0 || evt.button == 1 || evt.button == 2)
        {
            _panning = true;
            _panPointer = evt.pointerId;
            _panStart = evt.position;
            _panOrigin = _pan;
            this.CapturePointer(evt.pointerId);
            if (evt.button == 0)
                Select(null);
            evt.StopPropagation();
        }
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_panning || evt.pointerId != _panPointer)
            return;
        Vector2 delta = (Vector2)(evt.position - _panStart);
        _pan = _panOrigin + delta;
        UpdateTransform();
        evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!_panning || evt.pointerId != _panPointer)
            return;
        _panning = false;
        _panPointer = -1;
        if (this.HasPointerCapture(evt.pointerId))
            this.ReleasePointer(evt.pointerId);
        evt.StopPropagation();
    }

    #endregion

    #region Painting

    private void DrawEdges(MeshGenerationContext mgc)
    {
        if (Graph == null)
            return;
        var painter = mgc.painter2D;
        painter.lineWidth = 2f;
        painter.strokeColor = new Color(0.55f, 0.57f, 0.62f, 0.85f);
        for (int i = 0; i < Graph.Edges.Count; i++)
        {
            var edge = Graph.Edges[i];
            Vector2 a = GetCenter(edge.ParentId) + new Vector2(Origin, Origin);
            Vector2 b = GetCenter(edge.ChildId) + new Vector2(Origin, Origin);
            painter.BeginPath();
            painter.MoveTo(a);
            painter.LineTo(b);
            painter.Stroke();
        }
    }

    private void DrawBackground(MeshGenerationContext mgc)
    {
        var painter = mgc.painter2D;
        painter.lineWidth = 1f;
        painter.strokeColor = new Color(1f, 1f, 1f, 0.035f);
        float spacing = 40f * _zoom;
        if (spacing < 8f)
            return;
        float offsetX = _pan.x % spacing;
        float offsetY = _pan.y % spacing;
        for (float x = offsetX; x < layout.width; x += spacing)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(x, 0f));
            painter.LineTo(new Vector2(x, layout.height));
            painter.Stroke();
        }
        for (float y = offsetY; y < layout.height; y += spacing)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(0f, y));
            painter.LineTo(new Vector2(layout.width, y));
            painter.Stroke();
        }
    }

    #endregion
}
