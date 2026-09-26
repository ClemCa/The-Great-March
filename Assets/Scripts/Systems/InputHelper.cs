using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Thin convenience wrapper around the new Input System for the few
/// mouse/keyboard queries the project used to make through the legacy
/// <see cref="Input"/> class.
/// </summary>
public static class InputHelper
{
    public static Vector2 MousePosition
    {
        get { return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero; }
    }

    public static Vector2 MouseDelta
    {
        get { return Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero; }
    }

    /// <summary>
    /// Approximates the legacy "Mouse X"/"Mouse Y" axes (already sensitivity scaled).
    /// </summary>
    public static Vector2 MouseAxis
    {
        get { return MouseDelta * 0.1f; }
    }

    /// <summary>
    /// Approximates the legacy "Mouse ScrollWheel" axis (roughly 0.1 per notch).
    /// </summary>
    public static float ScrollAxis
    {
        get { return Mouse.current != null ? Mouse.current.scroll.ReadValue().y / 1200f : 0f; }
    }

    public static bool MouseLeftDown
    {
        get { return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame; }
    }

    public static bool KeyDown(Key key)
    {
        return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
    }

    public static float Horizontal
    {
        get
        {
            if (Keyboard.current == null)
                return 0f;
            float value = 0f;
            if (Keyboard.current.aKey.isPressed) value -= 1f;
            if (Keyboard.current.dKey.isPressed) value += 1f;
            return value;
        }
    }

    public static float Vertical
    {
        get
        {
            if (Keyboard.current == null)
                return 0f;
            float value = 0f;
            if (Keyboard.current.sKey.isPressed) value -= 1f;
            if (Keyboard.current.wKey.isPressed) value += 1f;
            return value;
        }
    }
}
