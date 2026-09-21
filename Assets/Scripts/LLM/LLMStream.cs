using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Receives raw response bytes and splits them into complete lines as they stream in.
/// Splitting on the newline byte is UTF-8 safe because 0x0A never appears inside a
/// multi-byte sequence.
/// </summary>
public class LLMStreamHandler : DownloadHandlerScript
{
    private readonly List<byte> _pending = new List<byte>();
    private readonly Queue<string> _lines = new Queue<string>();

    public LLMStreamHandler() : base(new byte[4096])
    {
    }

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (data == null || dataLength == 0)
            return false;

        for (int i = 0; i < dataLength; i++)
            _pending.Add(data[i]);

        int start = 0;
        for (int i = 0; i < _pending.Count; i++)
        {
            if (_pending[i] != (byte)'\n')
                continue;
            int length = i - start;
            if (length > 0)
            {
                var line = Encoding.UTF8.GetString(_pending.ToArray(), start, length).TrimEnd('\r');
                if (line.Length > 0)
                    _lines.Enqueue(line);
            }
            start = i + 1;
        }
        if (start > 0)
            _pending.RemoveRange(0, start);
        return true;
    }

    public bool TryDequeueLine(out string line)
    {
        if (_lines.Count > 0)
        {
            line = _lines.Dequeue();
            return true;
        }
        line = null;
        return false;
    }
}

public static class LLMStreamer
{
    /// <summary>
    /// Drives a UnityWebRequest while draining parsed tokens. parseLine returns the token
    /// contained in a line, or null for keep-alives / terminators.
    /// </summary>
    public static IEnumerator Stream(UnityWebRequest request, Func<string, string> parseLine, Action<string> onToken, Action onComplete, Action<string> onError)
    {
        var handler = request.downloadHandler as LLMStreamHandler;
        var operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            Drain(handler, parseLine, onToken);
            yield return null;
        }
        Drain(handler, parseLine, onToken);

        bool success = request.result == UnityWebRequest.Result.Success;
        if (!success)
        {
            string message = request.error;
            if (string.IsNullOrEmpty(message))
                message = "Request failed (" + request.responseCode + ")";
            if (onError != null)
                onError(message);
        }
        else if (onComplete != null)
        {
            onComplete();
        }

        request.Dispose();
    }

    private static void Drain(LLMStreamHandler handler, Func<string, string> parseLine, Action<string> onToken)
    {
        if (handler == null)
            return;
        string line;
        while (handler.TryDequeueLine(out line))
        {
            string token = null;
            try
            {
                token = parseLine(line);
            }
            catch
            {
                token = null;
            }
            if (!string.IsNullOrEmpty(token) && onToken != null)
                onToken(token);
        }
    }
}
