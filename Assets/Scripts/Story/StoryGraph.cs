using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Authored, evolutive story tree. Nodes are Decision (gates that open a sub-branch) or Beat
/// (content), edges are parent/child. One root starts the campaign; sibling nodes are mutually
/// exclusive once one of them completes.
/// </summary>
[CreateAssetMenu(fileName = "StoryGraph", menuName = "Story/Story Graph")]
public class StoryGraph : ScriptableObject
{
    public string Title = "Main Story";
    [TextArea] public string Description = "";
    public string RootId = "";
    public List<StoryNode> Nodes = new List<StoryNode>();
    public List<StoryEdge> Edges = new List<StoryEdge>();

    public StoryNode GetNode(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;
        for (int i = 0; i < Nodes.Count; i++)
            if (Nodes[i] != null && Nodes[i].Id == id)
                return Nodes[i];
        return null;
    }

    public StoryNode Root()
    {
        return GetNode(RootId);
    }

    public List<StoryEdge> EdgesFrom(string parentId)
    {
        var result = new List<StoryEdge>();
        for (int i = 0; i < Edges.Count; i++)
            if (Edges[i] != null && Edges[i].ParentId == parentId)
                result.Add(Edges[i]);
        return result;
    }

    public List<StoryNode> ChildrenOf(string parentId)
    {
        var result = new List<StoryNode>();
        var edges = EdgesFrom(parentId);
        for (int i = 0; i < edges.Count; i++)
        {
            var child = GetNode(edges[i].ChildId);
            if (child != null)
                result.Add(child);
        }
        return result;
    }

    public StoryNode ParentOf(string childId)
    {
        for (int i = 0; i < Edges.Count; i++)
            if (Edges[i] != null && Edges[i].ChildId == childId)
                return GetNode(Edges[i].ParentId);
        return null;
    }

    public bool IsRoot(StoryNode node)
    {
        return node != null && node.Id == RootId;
    }

    public void Connect(string parentId, string childId)
    {
        if (string.IsNullOrEmpty(parentId) || string.IsNullOrEmpty(childId) || parentId == childId)
            return;
        for (int i = 0; i < Edges.Count; i++)
            if (Edges[i].ParentId == parentId && Edges[i].ChildId == childId)
                return;
        Edges.Add(new StoryEdge(parentId, childId));
    }

    public void Disconnect(string parentId, string childId)
    {
        for (int i = Edges.Count - 1; i >= 0; i--)
            if (Edges[i].ParentId == parentId && Edges[i].ChildId == childId)
                Edges.RemoveAt(i);
    }

    public StoryNode AddNode(StoryNodeType type, string parentId, Vector2 position, string name = null)
    {
        var node = new StoryNode
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name ?? (type == StoryNodeType.Decision ? "Decision" : "Beat"),
            Type = type,
            EditorPosition = position
        };
        Nodes.Add(node);
        if (string.IsNullOrEmpty(RootId))
            RootId = node.Id;
        else if (!string.IsNullOrEmpty(parentId))
            Connect(parentId, node.Id);
        return node;
    }

    /// <summary>
    /// Removes a node, re-parenting its children onto the removed node's parent so the tree
    /// stays connected. The root cannot be removed this way if it has children.
    /// </summary>
    public void RemoveNode(string id, bool reparentChildren = true)
    {
        var node = GetNode(id);
        if (node == null)
            return;

        var parent = ParentOf(id);
        if (reparentChildren && parent != null)
        {
            var children = ChildrenOf(id);
            for (int i = 0; i < children.Count; i++)
                Connect(parent.Id, children[i].Id);
        }

        for (int i = Edges.Count - 1; i >= 0; i--)
            if (Edges[i].ParentId == id || Edges[i].ChildId == id)
                Edges.RemoveAt(i);

        Nodes.Remove(node);

        if (RootId == id)
            RootId = Nodes.Count > 0 ? Nodes[0].Id : "";
    }

    public void CollectDescendants(string id, List<StoryNode> into)
    {
        var children = ChildrenOf(id);
        for (int i = 0; i < children.Count; i++)
        {
            into.Add(children[i]);
            CollectDescendants(children[i].Id, into);
        }
    }

    public bool IsDescendant(string ancestorId, string candidateId)
    {
        if (string.IsNullOrEmpty(ancestorId) || string.IsNullOrEmpty(candidateId))
            return false;
        if (ancestorId == candidateId)
            return true;
        var children = ChildrenOf(ancestorId);
        for (int i = 0; i < children.Count; i++)
            if (IsDescendant(children[i].Id, candidateId))
                return true;
        return false;
    }

    /// <summary>
    /// Returns authoring problems (missing root, cycles, orphaned nodes, unreachable nodes).
    /// Empty when the graph is well-formed.
    /// </summary>
    public List<string> Validate()
    {
        var problems = new List<string>();
        if (Nodes.Count == 0)
            return problems;

        if (string.IsNullOrEmpty(RootId) || GetNode(RootId) == null)
            problems.Add("No valid root node is set.");

        for (int i = 0; i < Nodes.Count; i++)
        {
            var node = Nodes[i];
            if (node == null || string.IsNullOrEmpty(node.Id))
                continue;
            if (node.Id != RootId && ParentOf(node.Id) == null)
                problems.Add("Node '" + node.Name + "' has no parent and is not the root.");
        }

        if (!string.IsNullOrEmpty(RootId) && GetNode(RootId) != null)
        {
            var reachable = new HashSet<string>();
            var stack = new Stack<string>();
            stack.Push(RootId);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!reachable.Add(current))
                {
                    problems.Add("Cycle detected at node '" + (GetNode(current)?.Name ?? current) + "'.");
                    continue;
                }
                var children = ChildrenOf(current);
                for (int i = 0; i < children.Count; i++)
                    stack.Push(children[i].Id);
            }
            for (int i = 0; i < Nodes.Count; i++)
                if (Nodes[i] != null && !reachable.Contains(Nodes[i].Id))
                    problems.Add("Node '" + Nodes[i].Name + "' is unreachable from the root.");
        }

        return problems;
    }
}
