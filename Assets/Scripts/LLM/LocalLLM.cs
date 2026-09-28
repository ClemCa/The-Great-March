using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Sampling;
using UnityEngine;

/// <summary>
/// Owns the single in-process LLamaSharp model used by the Local provider. Loading is lazy and
/// serialised, and generation is serialised too because an executor/context is not thread-safe.
/// Generation runs on a worker thread (see <see cref="LocalLlamaProvider"/>), so CPU-bound decoding
/// never stalls the Unity main thread.
/// </summary>
public static class LocalLLM
{
    private const int ContextSize = 2048;
    private const int DefaultMaxTokens = 400;

    private static readonly object LoadGate = new object();
    private static readonly SemaphoreSlim GenerateGate = new SemaphoreSlim(1, 1);

    private static LLamaWeights _weights;
    private static StatelessExecutor _executor;

    public static string ModelDirectory
    {
        get { return Path.Combine(Application.streamingAssetsPath, "LLamaSharp", "models"); }
    }

    public static string ModelPath
    {
        get
        {
            string file = LLMSettings.LocalModelFile;
            if (string.IsNullOrEmpty(file))
                file = LLMSettings.DefaultLocalModelFile;
            // Path.Combine returns the second argument unchanged when it is an absolute path,
            // which lets tooling point at a model outside the bundled folder.
            return Path.Combine(ModelDirectory, file);
        }
    }

    public static bool ModelAvailable()
    {
        return File.Exists(ModelPath);
    }

    public static bool IsLoaded
    {
        get { lock (LoadGate) return _executor != null; }
    }

    /// <summary>
    /// Loads the bundled model. Main-thread only: resolving the path reads settings (PlayerPrefs).
    /// Worker callers must pass an already-resolved path to <see cref="EnsureLoaded(string)"/>.
    /// </summary>
    public static void EnsureLoaded()
    {
        EnsureLoaded(ModelPath);
    }

    /// <summary>
    /// Loads the bundled model from an already-resolved absolute path. Safe on a worker thread: it
    /// touches no Unity API, so the settings/StreamingAssets reads must have happened on the main
    /// thread (see <see cref="LocalLlamaProvider"/>).
    /// </summary>
    private static void EnsureLoaded(string modelPath)
    {
        lock (LoadGate)
        {
            if (_executor != null)
                return;

            LocalLlamaNative.Configure();

            if (!File.Exists(modelPath))
                throw new FileNotFoundException(
                    "Bundled NPC model missing at " + modelPath +
                    ". Run 'Tools > NPC Model > Provision Local Bundle' before building.");

            var parameters = new ModelParams(modelPath)
            {
                ContextSize = ContextSize,
                GpuLayerCount = 0,
                Threads = Math.Max(1, Environment.ProcessorCount / 2)
            };

            _weights = LLamaWeights.LoadFromFile(parameters);
            _executor = new StatelessExecutor(_weights, parameters);
        }
    }

    /// <summary>
    /// Renders the request with the shipped Qwen3 format and streams decoded tokens to
    /// <paramref name="onToken"/>. <paramref name="modelPath"/> must be resolved on the main thread
    /// (it is the absolute path to the GGUF). Blocks its calling thread; run it off the main thread.
    /// </summary>
    public static async Task GenerateAsync(LLMRequest request, string modelPath, Action<string> onToken)
    {
        await GenerateGate.WaitAsync().ConfigureAwait(false);
        try
        {
            EnsureLoaded(modelPath);

            string prompt = Qwen3Prompt.Build(request, false);
            var inference = new InferenceParams
            {
                MaxTokens = request.MaxTokens > 0 ? request.MaxTokens : DefaultMaxTokens,
                AntiPrompts = new List<string> { Qwen3Prompt.ImEnd },
                SamplingPipeline = new DefaultSamplingPipeline { Temperature = request.Temperature }
            };

            await foreach (string token in _executor.InferAsync(prompt, inference))
            {
                if (!string.IsNullOrEmpty(token))
                    onToken(token);
            }
        }
        finally
        {
            GenerateGate.Release();
        }
    }

    public static void Dispose()
    {
        lock (LoadGate)
        {
            _executor = null;
            if (_weights != null)
            {
                _weights.Dispose();
                _weights = null;
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterShutdown()
    {
        Application.quitting += Dispose;
    }
}
