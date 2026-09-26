using UnityEngine;

/// <summary>
/// Session-wide mode flags shared by the menu, the Game scene and the React bridge.
///
/// The lose screen and the tutorial used to live in their own Unity scenes; both now run
/// inside the Game scene, so which mode the Game scene is in has to be carried separately
/// from the active scene name.
/// </summary>
public static class GameSession
{
    /// <summary>True while the Game scene is running the scripted tutorial.</summary>
    public static bool Tutorial { get; private set; }

    /// <summary>True while the run is over and the lose screen is shown (gameplay is frozen).</summary>
    public static bool GameOver { get; private set; }

    /// <summary>Starts a normal run in the Game scene.</summary>
    public static void StartRun()
    {
        Tutorial = false;
        GameOver = false;
        Prepare();
    }

    /// <summary>Starts the tutorial, which also runs inside the Game scene.</summary>
    public static void StartTutorial()
    {
        Tutorial = true;
        GameOver = false;
        Prepare();
    }

    /// <summary>Marks the run as lost. The React HUD swaps to the lose screen from this flag.</summary>
    public static void EndRun()
    {
        GameOver = true;
    }

    /// <summary>Drops back to a neutral session (e.g. the main menu) without touching the score.</summary>
    public static void Clear()
    {
        Tutorial = false;
        GameOver = false;
    }

    // A run can be restarted or started from inside the Game scene, so the scene itself is
    // not unloaded/reloaded in a way that would reset time or the pause state. Do it here.
    private static void Prepare()
    {
        Pausing.Clear();
        if (Scoring.Instance != null)
            Scoring.Instance.ResetRun();
    }
}
