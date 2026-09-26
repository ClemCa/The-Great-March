using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fruchterman-Reingold force-directed layout with an optional radial seed, so a freshly authored
/// tree fans out into a 360-degree spread before the springs settle it. Pinned nodes are held in
/// place and act as anchors.
/// </summary>
public static class StoryForceLayout
{
    public class Options
    {
        public int Iterations = 400;
        public float IdealLength = 170f;
        public float Gravity = 0.012f;
        public bool RadialSeed = true;
        public float RadialSpacing = 190f;
        public float InitialTemperature = 120f;
    }

    public static Dictionary<string, Vector2> Compute(StoryGraph graph, HashSet<string> pinned, Options options = null)
    {
        options = options ?? new Options();
        var result = new Dictionary<string, Vector2>();
        if (graph == null || graph.Nodes.Count == 0)
            return result;

        var nodes = graph.Nodes;
        int n = nodes.Count;

        var positions = new Vector2[n];
        var indexOf = new Dictionary<string, int>();
        for (int i = 0; i < n; i++)
        {
            indexOf[nodes[i].Id] = i;
            positions[i] = nodes[i].EditorPosition;
        }

        if (options.RadialSeed || IsDegenerate(positions))
            SeedRadial(graph, positions, options.RadialSpacing);

        var edges = new List<Vector2Int>();
        for (int i = 0; i < graph.Edges.Count; i++)
        {
            var edge = graph.Edges[i];
            if (edge == null)
                continue;
            if (indexOf.TryGetValue(edge.ParentId, out int a) && indexOf.TryGetValue(edge.ChildId, out int b))
                edges.Add(new Vector2Int(a, b));
        }

        float area = Mathf.Max(1f, Mathf.Sqrt(n) * options.IdealLength * 3f);
        area *= area;
        float k = Mathf.Sqrt(area / Mathf.Max(1, n));
        float temperature = options.InitialTemperature;
        var center = new Vector2(0f, 0f);
        var displacement = new Vector2[n];

        for (int iteration = 0; iteration < options.Iterations; iteration++)
        {
            for (int i = 0; i < n; i++)
                displacement[i] = Vector2.zero;

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    Vector2 delta = positions[i] - positions[j];
                    float distance = Mathf.Max(0.01f, delta.magnitude);
                    Vector2 force = (delta / distance) * (k * k / distance);
                    displacement[i] += force;
                    displacement[j] -= force;
                }
            }

            for (int e = 0; e < edges.Count; e++)
            {
                int a = edges[e].x;
                int b = edges[e].y;
                Vector2 delta = positions[a] - positions[b];
                float distance = Mathf.Max(0.01f, delta.magnitude);
                Vector2 force = (delta / distance) * (distance * distance / k);
                displacement[a] -= force;
                displacement[b] += force;
            }

            for (int i = 0; i < n; i++)
            {
                displacement[i] += (center - positions[i]) * options.Gravity;
                float magnitude = displacement[i].magnitude;
                if (magnitude > 0.0001f)
                    positions[i] += (displacement[i] / magnitude) * Mathf.Min(magnitude, temperature);
            }

            temperature *= 0.96f;
            if (temperature < 0.4f)
                break;
        }

        for (int i = 0; i < n; i++)
        {
            if (pinned != null && pinned.Contains(nodes[i].Id))
                continue;
            result[nodes[i].Id] = positions[i];
        }
        return result;
    }

    private static bool IsDegenerate(Vector2[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
            if (positions[i].sqrMagnitude > 1f)
                return false;
        return true;
    }

    /// <summary>
    /// Places the root at the centre and fans each level outward, distributing siblings evenly
    /// around a full circle so the tree reads as a 360-degree burst.
    /// </summary>
    private static void SeedRadial(StoryGraph graph, Vector2[] positions, float spacing)
    {
        var indexOf = new Dictionary<string, int>();
        for (int i = 0; i < graph.Nodes.Count; i++)
            indexOf[graph.Nodes[i].Id] = i;

        var root = graph.Root();
        if (root == null)
            return;

        var queue = new Queue<string>();
        var depth = new Dictionary<string, int> { [root.Id] = 0 };
        queue.Enqueue(root.Id);
        positions[indexOf[root.Id]] = Vector2.zero;

        var siblingCount = new Dictionary<string, int>();
        while (queue.Count > 0)
        {
            string currentId = queue.Dequeue();
            int currentDepth = depth[currentId];
            var children = graph.ChildrenOf(currentId);
            siblingCount[currentId] = children.Count;
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (depth.ContainsKey(child.Id))
                    continue;
                depth[child.Id] = currentDepth + 1;
                queue.Enqueue(child.Id);
            }
        }

        queue.Enqueue(root.Id);
        while (queue.Count > 0)
        {
            string currentId = queue.Dequeue();
            var children = graph.ChildrenOf(currentId);
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (!depth.TryGetValue(child.Id, out int d))
                    continue;
                int total = Mathf.Max(1, siblingCount[currentId]);
                float angle = (i / (float)total) * Mathf.PI * 2f + d * 0.6f;
                float radius = d * spacing;
                positions[indexOf[child.Id]] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                queue.Enqueue(child.Id);
            }
        }
    }
}
