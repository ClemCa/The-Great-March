using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Makes the bundled llama.cpp backend loadable without relying on LLamaSharp's native-config API.
///
/// On the .NET Standard 2.0 build of LLamaSharp that Unity has historically used, the library never
/// loads its own natives; it falls back to the OS loader by bare name (llama, mtmd, ggml,
/// ggml-base) on the first P/Invoke. StreamingAssets is not on the loader's search path, so this
/// preloads every backend library from StreamingAssets by absolute path, dependencies first. The
/// later by-name P/Invokes then bind to the already-loaded modules.
///
/// This intentionally uses only LoadLibrary/dlopen (no System.Runtime.InteropServices.NativeLibrary,
/// no System.Runtime.Intrinsics) so it compiles and runs under both the Mono and CoreCLR runtimes.
/// </summary>
public static class LocalLlamaNative
{
    private const int RtldNow = 0x2;
    private const int RtldGlobal = 0x100;

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibraryW(string fileName);

    [DllImport("libdl.so.2", EntryPoint = "dlopen")]
    private static extern IntPtr dlopen(string fileName, int flags);

    private static bool _configured;

    public static string NativeRoot
    {
        get { return Path.Combine(Application.streamingAssetsPath, "LLamaSharp"); }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Bootstrap()
    {
        Configure();
    }

    public static void Configure()
    {
        if (_configured)
            return;
        _configured = true;

        try
        {
            string directory = NativeDirectory();
            if (directory == null)
            {
                Debug.LogWarning("[LocalLLM] Native backend not found under " + NativeRoot);
                return;
            }
            if (!PreloadAll(directory, IsWindowsTarget()))
                Debug.LogWarning("[LocalLLM] Some native libraries failed to preload from " + directory);
        }
        catch (Exception exception)
        {
            Debug.LogError("[LocalLLM] Failed to load native backend: " + exception.Message);
        }
    }

    private static string NativeDirectory()
    {
        string rid = IsWindowsTarget() ? "win-x64" : "linux-x64";
        string root = Path.Combine(NativeRoot, "runtimes", rid, "native");
        if (!Directory.Exists(root))
            return null;

        // ggml-cpu selects kernels at runtime; prefer the widest bundled build, degrade by presence.
        foreach (string variant in new[] { "avx2", "avx", "noavx" })
        {
            string candidate = Path.Combine(root, variant);
            if (Directory.Exists(candidate))
                return candidate;
        }
        return null;
    }

    private static bool IsWindowsTarget()
    {
        return Application.platform == RuntimePlatform.WindowsPlayer
            || Application.platform == RuntimePlatform.WindowsEditor;
    }

    private static bool PreloadAll(string directory, bool windows)
    {
        // Dependencies first: ggml-base -> ggml-cpu -> ggml -> llama -> mtmd.
        string[] names = { "ggml-base", "ggml-cpu", "ggml", "llama", "mtmd" };
        bool ok = true;
        foreach (string name in names)
        {
            string file = Path.Combine(directory, windows ? name + ".dll" : "lib" + name + ".so");
            if (!File.Exists(file))
                continue;

            IntPtr handle = windows
                ? LoadLibraryW(file)
                : dlopen(file, RtldNow | RtldGlobal);

            if (handle == IntPtr.Zero)
            {
                ok = false;
                string error = windows ? Marshal.GetLastWin32Error().ToString() : "dlopen failed";
                Debug.LogWarning("[LocalLLM] Could not load " + file + " (" + error + ")");
            }
        }
        return ok;
    }
}
