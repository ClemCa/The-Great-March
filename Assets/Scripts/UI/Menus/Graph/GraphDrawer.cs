using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEngine.UI;
using System;
using System.Linq;

public class GraphDrawer : MonoBehaviour
{
    private Texture2D texture;
    private RawImage target;
    private Line[] lines = new Line[] { };
    private static GraphDrawer instance;

    public static GraphDrawer Instance { get => instance; }

    public class Line
    {
        public int[] Array;
        public Color Color;
        public string Legend;
        public Line(int[] array, Color color, string legend)
        {
            Array = array;
            Color = color;
            Legend = legend;
        }
    }

    void Awake()
    {
        target = GetComponent<RawImage>();
        texture = target.texture as Texture2D;
        instance = this;
    }

    void Update()
    {
        if (!Planet.Selected)
        {
            Clear();
            return;
        }
        if(ClemCAddons.Utilities.Timer.MinimumDelay("GraphDrawer".GetHashCode(), 1000))
        {
            Clear();
            if (Planet.Selected)
            {
                UpdatePoints(Planet.Selected.PeopleOverTime.ToArray(), Color.white, "People");
                UpdatePoints(Planet.Selected.ResourcesOverTime.ToArray(), Color.green, "Resources");
                UpdatePoints(Planet.Selected.FacilitiesOverTime.ToArray(), Color.blue, "Facilities");
            }
            if (lines.Length == 0)
                return;
            float max = lines.Max(t => t.Array.Max()).Max(1);
            lines = lines.OrderByDescending(t => t.Array.Last()).ToArray();
            if (transform.childCount != lines.Length)
            {
                while (transform.childCount > lines.Length)
                    Destroy(transform.GetChild(0));
                while (transform.childCount < lines.Length)
                {
                    var obj = Instantiate(new GameObject(), transform);
                    obj.AddComponent<TMPro.TextMeshProUGUI>().enableAutoSizing = true;
                    obj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 10);
                    obj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    obj.GetComponent<RectTransform>().anchorMin = new Vector2(0,1);
                    obj.GetComponent<RectTransform>().anchorMax = new Vector2(0,1);
                }
            }
            for(int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                DrawLegend(i, line.Color, line.Legend);
                DrawGraph(line, max);
            }
        }
    }

    public void UpdatePoints(int[] points, Color color, string legend)
    {
        var line = Array.FindIndex(lines, t => t.Color == color);
        if(line == -1)
        {
            lines = lines.Add(new Line(points, color, legend));
            line = lines.Length - 1;
        }
        else
        {
            while(points.Length < 0)
            {
                points = points.Add(0, 0);
            }
            while(points.Length > 10)
            {
                points = points.RemoveAt(0);
            }
            lines[line].Array = points;
        }
    }
    
    public void AddPoint(int point, Color color, string legend)
    {
        var line = Array.FindIndex(lines, t => t.Color == color);
        if(line == -1)
        {
            lines = lines.Add(new Line(new int[10], color, legend));
            line = lines.Length - 1;
        }
        lines[line].Array = lines[line].Array.RemoveAt(0).Add(point);
    }

    private void DrawLegend(int id, Color color, string legend)
    {
        var pixels = new Color[20 * 10];
        Array.Fill(pixels, color);
        texture.SetPixels(20, texture.height - 20 - 30 * id, 20, 10, pixels);
        transform.GetChild(id).GetComponent<TMPro.TMP_Text>().text = legend;
        transform.GetChild(id).GetComponent<RectTransform>().anchoredPosition = new Vector2(40, -2 -30 * id);
    }

    private void Clear()
    {
        var pixels = new Color[texture.width * texture.height];
        texture.SetPixels(pixels);
        texture.Apply();
    }

    private void DrawGraph(Line line, float max)
    {
        var points = line.Array;
        var color = line.Color;
        var p = new Vector2Int[points.Length];
        for(int i = 0; i < points.Length; i++)
        {
            p[i] = new Vector2Int(10 + i * (texture.width - 20) / points.Length, (int)(points[i] / max * texture.height));
        }
        DrawGraph(p, 3, color);
    }

    private void DrawGraph(Vector2Int[] points, int thickness, Color color)
    {
        int halfThickness = Mathf.CeilToInt(thickness / 2f);
        var half = thickness % 2 != 0;
        var size = new Vector2(texture.width, texture.height);
        int length = points.Length;
        for(int i = 0; i < length - 1; i++)
        {
            var point = points[i];
            var nextPoint = points[i + 1];
            for (int t = -halfThickness; t < halfThickness; t++)
            {
                var addedThickness = new Vector2(0, t);
                if (half && t.Abs() == halfThickness)
                    DrawLine(ref texture, point + addedThickness, nextPoint + addedThickness, color.SetA(0.5f));
                else
                    DrawLine(ref texture, point + addedThickness, nextPoint + addedThickness, color);
            }
        }
        texture.Apply();
    }

    public void DrawLine(ref Texture2D tex, Vector2 p1, Vector2 p2, Color col)
    {
        var max = new Vector2(texture.width - 1, texture.height - 1);
        Vector2 t = p1;
        Vector2 tb;
        float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
        float ctr = 0;

        while ((int)t.x != (int)p2.x || (int)t.y != (int)p2.y)
        {
            t = Vector2.Lerp(p1, p2, ctr);
            tb = t.Clamp(Vector2.zero, max);
            ctr += frac;
            tex.SetPixel((int)tb.x, (int)tb.y, col);
        }
    }
}
