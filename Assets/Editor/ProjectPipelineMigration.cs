using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// One-click helper for moving the project from the built-in render pipeline
/// to the Universal Render Pipeline (URP).
/// </summary>
public static class ProjectPipelineMigration
{
    private const string SettingsFolder = "Assets/Settings";
    private const string RendererPath = SettingsFolder + "/UniversalRP-Renderer.asset";
    private const string AssetPath = SettingsFolder + "/UniversalRP.asset";

    [MenuItem("Tools/Migration/Convert Project to URP", priority = 100)]
    public static void ConvertProjectToURP()
    {
        if (!EditorUtility.DisplayDialog(
                "Convert to URP",
                "This will:\n" +
                "  - create a URP asset + renderer under Assets/Settings\n" +
                "  - assign it in Graphics and Quality settings\n" +
                "  - convert built-in materials to URP shaders\n\n" +
                "Make sure your work is saved first. Continue?",
                "Convert", "Cancel"))
        {
            return;
        }

        var pipeline = CreateOrLoadPipelineAsset();
        AssignPipeline(pipeline);
        int converted = ConvertMaterials();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "URP Migration",
            $"URP pipeline assigned.\nMaterials converted: {converted}\n\n" +
            "If a few materials still look wrong, run\nWindow > Rendering > Render Pipeline Converter\nfor a full conversion pass.",
            "OK");
    }

    [MenuItem("Tools/Migration/Convert Materials to URP Only", priority = 101)]
    public static void ConvertMaterialsOnly()
    {
        int converted = ConvertMaterials();
        AssetDatabase.SaveAssets();
        Debug.Log($"[URP Migration] Converted {converted} material(s) to URP shaders.");
    }

    private static UniversalRenderPipelineAsset CreateOrLoadPipelineAsset()
    {
        if (!AssetDatabase.IsValidFolder(SettingsFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }

        var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        if (rendererData == null)
        {
            rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            ResourceReloader.ReloadAllNullIn(rendererData, UniversalRenderPipelineAsset.packagePath);
            AssetDatabase.CreateAsset(rendererData, RendererPath);
        }
        else
        {
            ResourceReloader.ReloadAllNullIn(rendererData, UniversalRenderPipelineAsset.packagePath);
            EditorUtility.SetDirty(rendererData);
        }

        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetPath);
        if (pipeline == null)
        {
            pipeline = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(pipeline, AssetPath);
        }
        else
        {
            EditorUtility.SetDirty(pipeline);
        }

        return pipeline;
    }

    private static void AssignPipeline(UniversalRenderPipelineAsset pipeline)
    {
        GraphicsSettings.defaultRenderPipeline = pipeline;

        int originalQuality = QualitySettings.GetQualityLevel();
        for (int i = 0; i < QualitySettings.count; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipeline;
        }
        QualitySettings.SetQualityLevel(originalQuality, false);
    }

    private static List<MaterialUpgrader> BuildUpgraders()
    {
        return new List<MaterialUpgrader>
        {
            new StandardUpgrader("Standard"),
            new StandardUpgrader("Standard (Specular setup)"),
            new TerrainUpgrader("Nature/Terrain/Standard"),
            new ParticleUpgrader("Particles/Standard Surface"),
            new ParticleUpgrader("Particles/Standard Unlit"),
            new AutodeskInteractiveUpgrader("Autodesk Interactive"),
        };
    }

    private static int ConvertMaterials()
    {
        var upgraders = BuildUpgraders();
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int converted = 0;

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (EditorUtility.DisplayCancelableProgressBar(
                        "URP Migration", path, (float)i / Mathf.Max(1, guids.Length)))
                {
                    break;
                }

                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null || material.shader == null)
                {
                    continue;
                }

                string shaderName = material.shader.name;
                if (shaderName.StartsWith("Universal Render Pipeline/"))
                {
                    continue;
                }

                var upgrader = upgraders.Find(u => u.OldShaderPath == shaderName);
                if (upgrader != null)
                {
                    upgrader.Upgrade(material, MaterialUpgrader.UpgradeFlags.None);
                    EditorUtility.SetDirty(material);
                    converted++;
                }
                else if (IsBuiltIn3DShader(shaderName))
                {
                    ConvertFallback(material);
                    EditorUtility.SetDirty(material);
                    converted++;
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        return converted;
    }

    private static bool IsBuiltIn3DShader(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return name.StartsWith("Legacy Shaders/")
            || name.StartsWith("Mobile/")
            || name.StartsWith("Nature/")
            || name.StartsWith("Particles/")
            || name.StartsWith("Unlit/")
            || name.StartsWith("Transparent/")
            || name.StartsWith("Bumped")
            || name == "Diffuse"
            || name == "Specular"
            || name == "VertexLit";
    }

    private static void ConvertFallback(Material material)
    {
        bool unlit = material.shader.name.IndexOf("Unlit", StringComparison.OrdinalIgnoreCase) >= 0
                  || material.shader.name.IndexOf("Flare", StringComparison.OrdinalIgnoreCase) >= 0;

        var target = Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit");
        if (target == null)
        {
            return;
        }

        Texture mainTexture = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
        Color color = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;

        material.shader = target;

        if (mainTexture != null && material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", mainTexture);
        }
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
    }
}
