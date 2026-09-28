using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Stages everything the in-process Local provider needs into the project so builds are self-contained:
///
///   Assets/StreamingAssets/LLamaSharp/models/&lt;gguf&gt;                 the distilled Qwen3 model
///   Assets/StreamingAssets/LLamaSharp/runtimes/&lt;rid&gt;/native/&lt;avx&gt;/  LLamaSharp native backend
///   Assets/Plugins/LLamaSharp/*.dll                                 managed LLamaSharp + dependencies
///
/// LLamaSharp's native files are copied into StreamingAssets on purpose: Unity ships that folder
/// verbatim and never imports its contents, so the raw .dlls are not mistaken for managed plugins.
/// The managed assemblies are imported as Standalone-only plugins (Local is desktop-only).
///
/// The model folder is the pipeline drop point: the dev places the GGUF at
/// Assets/StreamingAssets/LLamaSharp/models/&lt;name&gt; (Tools > NPC Model > Open Model Folder reveals
/// it). No configuration is needed. The GGUF and the native backend are NOT committed (no Git LFS)
/// and are staged/gathered on demand: run Provision once after cloning and before building, and
/// place the model when prompted. The small managed plugins ARE tracked so a fresh clone compiles.
/// </summary>
public static class LocalModelProvisioner
{
    private const string BundleFolder = "Assets/StreamingAssets/LLamaSharp";
    private const string ModelsFolder = BundleFolder + "/models";
    private const string RuntimesFolder = BundleFolder + "/runtimes";
    private const string PluginsFolder = "Assets/Plugins/LLamaSharp";

    private const string BackendPackage = "llamasharp.backend.cpu";
    private const string BackendVersion = "0.27.0";

    private static readonly string[] Rids = { "win-x64", "linux-x64" };
    private static readonly string[] AvxVariants = { "avx", "avx2", "avx512", "noavx" };

    // package id -> assembly file name. Only assemblies Unity does NOT already ship are listed:
    // Unity provides netstandard2.1 shims for Microsoft.Bcl.AsyncInterfaces, System.Text.Json,
    // System.Runtime.CompilerServices.Unsafe, Diagnostics.DiagnosticSource, IO.Pipelines and
    // Text.Encodings.Web, so shipping those again would be a duplicate-assembly error. The ns2.1
    // build of Microsoft.Bcl.Memory is used because it type-forwards Index/Range to the runtime
    // instead of redefining them (shipping the ns2.0 build causes a CS0433 clash with mscorlib).
    private static readonly KeyValuePair<string, string>[] ManagedAssemblies =
    {
        new KeyValuePair<string, string>("llamasharp", "LLamaSharp.dll"),
        new KeyValuePair<string, string>("communitytoolkit.highperformance", "CommunityToolkit.HighPerformance.dll"),
        new KeyValuePair<string, string>("microsoft.bcl.memory", "Microsoft.Bcl.Memory.dll"),
        new KeyValuePair<string, string>("microsoft.bcl.numerics", "Microsoft.Bcl.Numerics.dll"),
        new KeyValuePair<string, string>("microsoft.extensions.ai.abstractions", "Microsoft.Extensions.AI.Abstractions.dll"),
        new KeyValuePair<string, string>("microsoft.extensions.dependencyinjection.abstractions", "Microsoft.Extensions.DependencyInjection.Abstractions.dll"),
        new KeyValuePair<string, string>("microsoft.extensions.logging.abstractions", "Microsoft.Extensions.Logging.Abstractions.dll"),
        new KeyValuePair<string, string>("system.interactive.async", "System.Interactive.Async.dll"),
        new KeyValuePair<string, string>("system.linq.async", "System.Linq.Async.dll"),
        new KeyValuePair<string, string>("system.linq.asyncenumerable", "System.Linq.AsyncEnumerable.dll"),
        new KeyValuePair<string, string>("system.numerics.tensors", "System.Numerics.Tensors.dll"),
    };

    [MenuItem("Tools/NPC Model/Provision Local Bundle", priority = 200)]
    public static void Provision()
    {
        try
        {
            string nugetRoot = ResolveNuGetRoot();
            if (string.IsNullOrEmpty(nugetRoot) || !Directory.Exists(nugetRoot))
            {
                EditorUtility.DisplayDialog(
                    "Provision Local Bundle",
                    "NuGet cache not found. Restore the tooling once (dotnet restore Tools/ThoughtsSmokeTest) " +
                    "so LLamaSharp 0.27.0 is in " + nugetRoot + ".",
                    "OK");
                return;
            }

            int copied = 0;
            copied += CopyNatives(nugetRoot);
            copied += CopyManaged(nugetRoot);
            CleanupStaleManaged();

            AssetDatabase.Refresh();
            ConfigureManagedImporter();

            bool hasModel = ModelPresent();
            Debug.Log("[LocalModelProvisioner] Staged " + copied + " file(s); model present: " + hasModel);

            if (!hasModel)
            {
                EnsureFolder(ModelsFolder);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog(
                    "Provision Local Bundle",
                    "The backend is ready. The GGUF itself is not committed (no Git LFS), so place it here:\n\n" +
                    ModelPath + "\n\nSee Tools > NPC Model > Open Model Folder.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Provision Local Bundle",
                    "Local provider bundle is ready.\n\nBackend files staged: " + copied + "\n\nOpen the game in Play mode to verify.", "OK");
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("[LocalModelProvisioner] " + exception);
            EditorUtility.DisplayDialog("Provision Local Bundle", "Failed:\n" + exception.Message, "OK");
        }
    }

    [MenuItem("Tools/NPC Model/Open Model Folder", priority = 203)]
    public static void OpenModelFolder()
    {
        EnsureFolder(ModelsFolder);
        AssetDatabase.Refresh();
        EditorUtility.RevealInFinder(Path.GetFullPath(ModelsFolder));
        Debug.Log("[LocalModelProvisioner] Place " + LLMSettings.DefaultLocalModelFile + " in " + ModelsFolder);
    }

    [MenuItem("Tools/NPC Model/Verify Local Bundle", priority = 201)]
    public static void Verify()
    {
        bool modelOk = ModelPresent();
        bool win = File.Exists(Path.Combine(RuntimesFolder, "win-x64/native/avx2/llama.dll"));
        bool linux = File.Exists(Path.Combine(RuntimesFolder, "linux-x64/native/avx2/libllama.so"));
        bool managed = File.Exists(Path.Combine(PluginsFolder, "LLamaSharp.dll"));

        string report =
            "model:   " + (modelOk ? "OK" : "MISSING") + "\n" +
            "windows: " + (win ? "OK" : "MISSING") + "\n" +
            "linux:   " + (linux ? "OK" : "MISSING") + "\n" +
            "managed: " + (managed ? "OK" : "MISSING");

        Debug.Log("[LocalModelProvisioner] Verify\n" + report);
        EditorUtility.DisplayDialog("Verify Local Bundle", report, "OK");
    }

    /// <summary>The in-Assets drop folder + filename the model is expected at (part of the pipeline).</summary>
    private static string ModelPath
    {
        get { return Path.Combine(ModelsFolder, LLMSettings.DefaultLocalModelFile); }
    }

    private static bool ModelPresent()
    {
        return File.Exists(ModelPath);
    }

    private static int CopyNatives(string nugetRoot)
    {
        string sourceRoot = Path.Combine(nugetRoot, BackendPackage, BackendVersion, "LLamaSharpRuntimes");
        if (!Directory.Exists(sourceRoot))
            throw new DirectoryNotFoundException("LLamaSharpRuntimes not found under " + sourceRoot);

        int copied = 0;
        foreach (string rid in Rids)
        {
            foreach (string variant in AvxVariants)
            {
                string source = Path.Combine(sourceRoot, rid, "native", variant);
                if (!Directory.Exists(source))
                    continue;
                string target = Path.Combine(RuntimesFolder, rid, "native", variant);
                EnsureFolder(target);
                foreach (string file in Directory.GetFiles(source))
                {
                    string destination = Path.Combine(target, Path.GetFileName(file));
                    if (File.Exists(destination) && new FileInfo(destination).Length == new FileInfo(file).Length)
                        continue;
                    File.Copy(file, destination, true);
                    copied++;
                }
            }
        }
        return copied;
    }

    private static int CopyManaged(string nugetRoot)
    {
        EnsureFolder(PluginsFolder);
        int copied = 0;
        foreach (var assembly in ManagedAssemblies)
        {
            string packageDir = Path.Combine(nugetRoot, assembly.Key);
            string versionDir = HighestVersionDir(packageDir);
            if (versionDir == null)
                throw new DirectoryNotFoundException("Package not restored: " + assembly.Key);

            string source = FindLibrary(versionDir, assembly.Value);
            if (source == null)
                throw new FileNotFoundException("No compatible build of " + assembly.Value + " under " + versionDir);

            string target = Path.Combine(PluginsFolder, assembly.Value);
            if (File.Exists(target) && new FileInfo(target).Length == new FileInfo(source).Length)
                continue;
            File.Copy(source, target, true);
            copied++;
        }
        return copied;
    }

    private static void ConfigureManagedImporter()
    {
        foreach (var assembly in ManagedAssemblies)
        {
            string assetPath = PluginsFolder + "/" + assembly.Value;
            var importer = AssetImporter.GetAtPath(assetPath) as PluginImporter;
            if (importer == null)
            {
                // A stub meta (guid only, e.g. from an interrupted import) leaves the assembly
                // unrecognised as a plugin. Drop the meta and let Unity reimport it properly.
                string meta = assetPath + ".meta";
                if (File.Exists(meta))
                    File.Delete(meta);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                importer = AssetImporter.GetAtPath(assetPath) as PluginImporter;
            }
            if (importer == null)
                continue;

            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(true);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, true);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, true);
            importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);

            // Some references (System.Memory, Unsafe, ...) are satisfied by Unity's own netstandard
            // shims rather than by files we ship, which trips reference validation. Turn it off.
            var serialized = new SerializedObject(importer);
            var validate = serialized.FindProperty("validateReferences");
            if (validate != null)
            {
                validate.boolValue = false;
                serialized.ApplyModifiedProperties();
            }
            importer.SaveAndReimport();
        }
    }

    /// <summary>Removes managed plugins staged by an earlier run that are no longer part of the set.</summary>
    private static int CleanupStaleManaged()
    {
        var wanted = new HashSet<string>();
        foreach (var assembly in ManagedAssemblies)
            wanted.Add(assembly.Value);

        int removed = 0;
        if (!Directory.Exists(PluginsFolder))
            return 0;
        foreach (string file in Directory.GetFiles(PluginsFolder, "*.dll"))
        {
            string name = Path.GetFileName(file);
            if (wanted.Contains(name))
                continue;
            File.Delete(file);
            string meta = file + ".meta";
            if (File.Exists(meta))
                File.Delete(meta);
            Debug.Log("[LocalModelProvisioner] removed stale managed plugin " + name);
            removed++;
        }
        return removed;
    }

    private static string FindLibrary(string versionDir, string fileName)
    {
        string[] folders =
        {
            "lib/netstandard2.1",
            "lib/netstandard2.0",
            "lib/net6.0",
            "lib/net8.0",
            "lib/net9.0",
        };
        foreach (string folder in folders)
        {
            string candidate = Path.Combine(versionDir, folder.Replace('/', Path.DirectorySeparatorChar), fileName);
            if (File.Exists(candidate))
                return candidate;
        }
        return null;
    }

    private static string HighestVersionDir(string packageDir)
    {
        if (!Directory.Exists(packageDir))
            return null;
        string best = null;
        Version bestVersion = null;
        foreach (string dir in Directory.GetDirectories(packageDir))
        {
            string name = Path.GetFileName(dir);
            Version version;
            if (!Version.TryParse(name, out version))
                continue;
            if (bestVersion == null || version > bestVersion)
            {
                bestVersion = version;
                best = dir;
            }
        }
        return best;
    }

    internal static bool IsStandaloneDesktop(BuildTarget target)
    {
        return target == BuildTarget.StandaloneWindows64
            || target == BuildTarget.StandaloneLinux64;
    }

    private static string ResolveNuGetRoot()
    {
        string configured = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        if (!string.IsNullOrEmpty(configured))
            return configured;
        string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(profile, ".nuget", "packages");
    }

    private static void EnsureFolder(string assetFolder)
    {
        string[] parts = assetFolder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}

/// <summary>
/// Fails a desktop build early (with a clear instruction) when the Local bundle was never staged,
/// instead of shipping a game whose bundled model is missing at runtime.
/// </summary>
public class LocalModelBuildCheck : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (!LocalModelProvisioner.IsStandaloneDesktop(report.summary.platform))
            return;

        string model = "Assets/StreamingAssets/LLamaSharp/models/" + LLMSettings.DefaultLocalModelFile;
        if (File.Exists(model))
            return;

        throw new BuildFailedException(
            "The bundled Local model is missing (" + model + "). " +
            "Run 'Tools > NPC Model > Provision Local Bundle' before building.");
    }
}
