using System;
using System.Collections.Generic;

[Serializable]
public class ThoughtNode
{
    public string Id = "";
    public string Name = "";
    public string ParentId = "";
    public ThoughtNodeKind Kind = ThoughtNodeKind.Leaf;
    public List<string> Children = new List<string>();
}

/// <summary>
/// The variable-depth topic tree that Raw mode lets the player drill through
/// (e.g. Relationships/Family/Mother). Nodes are branch, leaf, or both.
/// </summary>
public class ThoughtTaxonomy
{
    private readonly Dictionary<string, ThoughtNode> _nodes = new Dictionary<string, ThoughtNode>();
    private readonly List<string> _roots = new List<string>();

    public IReadOnlyList<string> Roots { get { return _roots; } }
    public int Count { get { return _nodes.Count; } }

    public void Add(ThoughtNode node)
    {
        if (node == null || string.IsNullOrEmpty(node.Id))
            return;
        _nodes[node.Id] = node;
        if (string.IsNullOrEmpty(node.ParentId))
        {
            if (!_roots.Contains(node.Id))
                _roots.Add(node.Id);
            return;
        }
        ThoughtNode parent;
        if (_nodes.TryGetValue(node.ParentId, out parent) && !parent.Children.Contains(node.Id))
            parent.Children.Add(node.Id);
    }

    public bool Exists(string id)
    {
        return !string.IsNullOrEmpty(id) && _nodes.ContainsKey(id);
    }

    public ThoughtNode Get(string id)
    {
        ThoughtNode node;
        _nodes.TryGetValue(id, out node);
        return node;
    }

    public string NameOf(string id)
    {
        var node = Get(id);
        return node == null ? id : node.Name;
    }

    public List<string> ChildrenOf(string id)
    {
        var node = Get(id);
        return node == null ? new List<string>() : node.Children;
    }

    public bool IsBranch(string id)
    {
        var node = Get(id);
        if (node == null)
            return false;
        return node.Kind == ThoughtNodeKind.Branch || node.Kind == ThoughtNodeKind.BranchAndLeaf;
    }

    public bool IsResolvable(string id)
    {
        var node = Get(id);
        if (node == null)
            return false;
        return node.Kind == ThoughtNodeKind.Leaf || node.Kind == ThoughtNodeKind.BranchAndLeaf;
    }

    public bool HasChildren(string id)
    {
        var node = Get(id);
        return node != null && node.Children.Count > 0;
    }

    public string DisplayPath(string id)
    {
        var parts = new List<string>();
        var current = Get(id);
        int guard = 0;
        while (current != null && guard++ < 64)
        {
            parts.Add(current.Name);
            current = Get(current.ParentId);
        }
        parts.Reverse();
        return string.Join("/", parts);
    }

    public List<string> Descendants(string id, bool includeSelf = true)
    {
        var result = new List<string>();
        Collect(id, result, includeSelf);
        return result;
    }

    private void Collect(string id, List<string> into, bool includeSelf)
    {
        var node = Get(id);
        if (node == null)
            return;
        if (includeSelf)
            into.Add(id);
        for (int i = 0; i < node.Children.Count; i++)
            Collect(node.Children[i], into, true);
    }

    #region Starter taxonomy

    public static ThoughtTaxonomy CreateStarter()
    {
        var t = new ThoughtTaxonomy();

        AddBranch(t, "relationships", "Relationships");
        AddBranchAndLeaf(t, "relationships/family", "Family", "relationships");
        AddLeaf(t, "relationships/family/mother", "Mother", "relationships/family");
        AddLeaf(t, "relationships/family/father", "Father", "relationships/family");
        AddLeaf(t, "relationships/family/siblings", "Siblings", "relationships/family");
        AddLeaf(t, "relationships/family/children", "Children", "relationships/family");
        AddLeaf(t, "relationships/family/extended", "Extended Family", "relationships/family");
        AddBranchAndLeaf(t, "relationships/friends", "Friends", "relationships");
        AddLeaf(t, "relationships/friends/close", "Close Friends", "relationships/friends");
        AddLeaf(t, "relationships/friends/acquaintances", "Acquaintances", "relationships/friends");
        AddLeaf(t, "relationships/friends/lost", "Lost Friends", "relationships/friends");
        AddBranchAndLeaf(t, "relationships/rivals", "Rivals", "relationships");
        AddLeaf(t, "relationships/rivals/enemies", "Enemies", "relationships/rivals");
        AddLeaf(t, "relationships/rivals/competitors", "Competitors", "relationships/rivals");
        AddBranchAndLeaf(t, "relationships/partners", "Partners", "relationships");
        AddLeaf(t, "relationships/partners/current", "Current Partner", "relationships/partners");
        AddLeaf(t, "relationships/partners/past", "Past Partners", "relationships/partners");
        AddBranchAndLeaf(t, "relationships/self", "Self", "relationships");
        AddLeaf(t, "relationships/self/selfworth", "Self Worth", "relationships/self");
        AddLeaf(t, "relationships/self/body", "Body", "relationships/self");
        AddLeaf(t, "relationships/self/past", "My Past", "relationships/self");

        AddBranch(t, "knowledge", "Knowledge");
        AddBranchAndLeaf(t, "knowledge/job", "Job", "knowledge");
        AddLeaf(t, "knowledge/job/role", "Role", "knowledge/job");
        AddLeaf(t, "knowledge/job/colleagues", "Colleagues", "knowledge/job");
        AddLeaf(t, "knowledge/job/skills", "Skills", "knowledge/job");
        AddLeaf(t, "knowledge/job/workplace", "Workplace", "knowledge/job");
        AddLeaf(t, "knowledge/education", "Education", "knowledge");
        AddLeaf(t, "knowledge/background", "Background", "knowledge");

        AddBranch(t, "feelings", "Feelings");
        AddLeaf(t, "feelings/current", "Current Feeling", "feelings");
        AddLeaf(t, "feelings/mood", "Mood", "feelings");
        AddLeaf(t, "feelings/wants", "Wants", "feelings");
        AddLeaf(t, "feelings/fears", "Fears", "feelings");
        AddLeaf(t, "feelings/hopes", "Hopes", "feelings");

        AddBranch(t, "history", "History");
        AddLeaf(t, "history/recent", "Recent", "history");
        AddLeaf(t, "history/personal", "Personal History", "history");
        AddLeaf(t, "history/world", "World History", "history");
        AddLeaf(t, "history/childhood", "Childhood", "history");
        AddLeaf(t, "history/turningpoints", "Turning Points", "history");

        AddBranch(t, "opinions", "Opinions");
        AddLeaf(t, "opinions/people", "People", "opinions");
        AddLeaf(t, "opinions/topics", "Topics", "opinions");
        AddLeaf(t, "opinions/politics", "Politics", "opinions");
        AddLeaf(t, "opinions/values", "Values", "opinions");
        AddLeaf(t, "opinions/culture", "Culture", "opinions");

        AddBranch(t, "present", "Present");
        AddLeaf(t, "present/doing", "Doing", "present");
        AddLeaf(t, "present/plans", "Plans", "present");
        AddLeaf(t, "present/troubles", "Troubles", "present");
        AddLeaf(t, "present/news", "News", "present");

        AddBranch(t, "world", "World");
        AddLeaf(t, "world/events", "Events", "world");
        AddLeaf(t, "world/places", "Places", "world");
        AddLeaf(t, "world/people", "People", "world");
        AddLeaf(t, "world/rumors", "Rumors", "world");
        AddLeaf(t, "world/factions", "Factions", "world");

        return t;
    }

    private static void AddBranch(ThoughtTaxonomy t, string id, string name, string parent = "")
    {
        t.Add(new ThoughtNode { Id = id, Name = name, ParentId = parent, Kind = ThoughtNodeKind.Branch });
    }

    private static void AddBranchAndLeaf(ThoughtTaxonomy t, string id, string name, string parent = "")
    {
        t.Add(new ThoughtNode { Id = id, Name = name, ParentId = parent, Kind = ThoughtNodeKind.BranchAndLeaf });
    }

    private static void AddLeaf(ThoughtTaxonomy t, string id, string name, string parent = "")
    {
        t.Add(new ThoughtNode { Id = id, Name = name, ParentId = parent, Kind = ThoughtNodeKind.Leaf });
    }

    #endregion
}
