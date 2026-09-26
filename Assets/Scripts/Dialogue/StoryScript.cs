using UnityEngine;

/// <summary>
/// Boots the story. The old linear StoryStage counter is kept only so pre-graph saves still load;
/// progression now lives in the StoryDirector's StoryGraph.
/// </summary>
public class StoryScript : MonoBehaviour
{
    public static int SlotLoader = -1;

    [Tooltip("Legacy linear stage from pre-graph saves. No longer written by new saves.")]
    public static int StoryStage = 0;

    void Start()
    {
        if (SlotLoader != -1)
        {
            Saver.Instance.Load(SlotLoader);
            return;
        }
        CheckStage();
    }

    public static void CheckStage()
    {
        if (StoryStage == 0 && !Application.isEditor)
            StoryDirector.Instance?.EnsureStarted(null);
    }
}
