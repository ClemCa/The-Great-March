using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The compact graph tile: just the node's name plus a type badge, as requested. Dragging moves
/// it and pins it; the canvas owns the pan/zoom and reacts to the events raised here.
/// </summary>
public class StoryNodeView : VisualElement
{
    public StoryNode Node { get; private set; }

    /// <summary>Set by the canvas so drag deltas can be converted from screen to graph space.</summary>
    public float Scale = 1f;

    public event Action<StoryNodeView> Selected;
    public event Action<StoryNodeView> Moved;
    public event Action<StoryNodeView> Pinned;
    public event Action<StoryNodeView> ContextRequested;

    private static readonly Color DecisionColor = new Color(0.23f, 0.43f, 0.65f);
    private static readonly Color BeatColor = new Color(0.27f, 0.46f, 0.29f);
    private static readonly Color DecisionAccent = new Color(0.45f, 0.67f, 0.92f);
    private static readonly Color BeatAccent = new Color(0.52f, 0.78f, 0.52f);

    private readonly VisualElement _badge;
    private readonly Label _title;
    private bool _dragging;
    private Vector2 _dragStartPanel;
    private Vector2 _dragStartPosition;

    public StoryNodeView(StoryNode node)
    {
        Node = node;
        name = "story-node";
        AddToClassList("story-node");

        style.position = Position.Absolute;
        style.minWidth = 110;
        style.maxWidth = 200;
        style.minHeight = 46;
        style.flexDirection = FlexDirection.Column;
        style.paddingLeft = 8;
        style.paddingRight = 8;
        style.paddingTop = 5;
        style.paddingBottom = 5;
        style.marginLeft = 0;
        style.marginRight = 0;
        style.borderTopLeftRadius = 5;
        style.borderTopRightRadius = 5;
        style.borderBottomLeftRadius = 5;
        style.borderBottomRightRadius = 5;
        style.borderLeftWidth = 2;
        style.borderRightWidth = 2;
        style.borderTopWidth = 2;
        style.borderBottomWidth = 2;
        style.backgroundColor = new Color(0.16f, 0.16f, 0.18f);

        _title = new Label(node.Name);
        _title.style.unityFontStyleAndWeight = FontStyle.Bold;
        _title.style.fontSize = 12;
        _title.style.color = new Color(0.92f, 0.92f, 0.92f);
        _title.style.overflow = Overflow.Hidden;
        _title.style.textOverflow = TextOverflow.Ellipsis;
        _title.style.whiteSpace = WhiteSpace.NoWrap;
        Add(_title);

        _badge = new VisualElement();
        _badge.style.marginTop = 3;
        _badge.style.paddingLeft = 5;
        _badge.style.paddingRight = 5;
        _badge.style.paddingTop = 1;
        _badge.style.paddingBottom = 1;
        _badge.style.alignSelf = Align.FlexStart;
        _badge.style.borderTopLeftRadius = 3;
        _badge.style.borderTopRightRadius = 3;
        _badge.style.borderBottomLeftRadius = 3;
        _badge.style.borderBottomRightRadius = 3;
        var badgeLabel = new Label();
        badgeLabel.name = "badge-label";
        badgeLabel.style.fontSize = 9;
        badgeLabel.style.color = Color.white;
        _badge.Add(badgeLabel);
        Add(_badge);

        ApplyType();
        RefreshPosition();
        SetSelected(false);

        RegisterCallback<PointerDownEvent>(OnPointerDown);
        RegisterCallback<PointerMoveEvent>(OnPointerMove);
        RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private void ApplyType()
    {
        bool decision = Node.Type == StoryNodeType.Decision;
        _badge.style.backgroundColor = decision ? DecisionColor : BeatColor;
        _badge.Q<Label>("badge-label").text = decision ? "DECISION" : "BEAT";
        borderColor = decision ? DecisionAccent : BeatAccent;
        style.borderLeftColor = borderColor;
        style.borderRightColor = borderColor;
        style.borderTopColor = borderColor;
        style.borderBottomColor = borderColor;
    }

    private Color borderColor;

    public void Refresh()
    {
        _title.text = Node.Name;
        ApplyType();
        RefreshPosition();
    }

    public void RefreshPosition()
    {
        style.left = Node.EditorPosition.x;
        style.top = Node.EditorPosition.y;
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            style.borderLeftWidth = 3;
            style.borderRightWidth = 3;
            style.borderTopWidth = 3;
            style.borderBottomWidth = 3;
            style.backgroundColor = new Color(0.22f, 0.24f, 0.29f);
        }
        else
        {
            style.borderLeftWidth = 2;
            style.borderRightWidth = 2;
            style.borderTopWidth = 2;
            style.borderBottomWidth = 2;
            style.backgroundColor = new Color(0.16f, 0.16f, 0.18f);
        }
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button == 1)
        {
            ContextRequested?.Invoke(this);
            evt.StopPropagation();
            return;
        }
        if (evt.button != 0)
            return;

        Selected?.Invoke(this);
        _dragging = true;
        _dragStartPanel = (Vector2)evt.position;
        _dragStartPosition = Node.EditorPosition;
        this.CapturePointer(evt.pointerId);
        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_dragging)
            return;
        Vector2 delta = (Vector2)evt.position - _dragStartPanel;
        Node.EditorPosition = _dragStartPosition + delta / Mathf.Max(0.01f, Scale);
        RefreshPosition();
        Moved?.Invoke(this);
        evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!_dragging)
            return;
        _dragging = false;
        if (this.HasPointerCapture(evt.pointerId))
            this.ReleasePointer(evt.pointerId);
        Pinned?.Invoke(this);
        evt.StopPropagation();
    }
}
