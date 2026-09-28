using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;

/// <summary>
/// Runs the bundled model fully in-process (no server, no network). LLamaSharp decoding is CPU-bound
/// and synchronous, so generation happens on a worker thread; tokens are queued and delivered from
/// the Unity main thread as the coroutine drains the queue each frame.
/// </summary>
public class LocalLlamaProvider : ILLMProvider
{
    private sealed class WorkerState
    {
        public volatile bool Done;
    }

    public IEnumerator Stream(LLMRequest request, Action<string> onToken, Action onComplete, Action<string> onError)
    {
        // Unity APIs are main-thread only (settings read PlayerPrefs; StreamingAssets gives the
        // model path). This iterator's first MoveNext runs on the main thread, so resolve the path
        // and preload the native backend here so the worker below touches only LLamaSharp + files.
        LocalLlamaNative.Configure();
        string modelPath = LocalLLM.ModelPath;

        var queue = new ConcurrentQueue<Action>();
        var state = new WorkerState();

        Task.Run(async () =>
        {
            try
            {
                await LocalLLM.GenerateAsync(request, modelPath, token => queue.Enqueue(() => onToken(token)));
                queue.Enqueue(onComplete);
            }
            catch (Exception exception)
            {
                string message = exception.Message;
                queue.Enqueue(() => onError(message));
            }
            finally
            {
                state.Done = true;
            }
        });

        while (!state.Done || !queue.IsEmpty)
        {
            Action action;
            while (queue.TryDequeue(out action))
                action();
            yield return null;
        }
    }
}
