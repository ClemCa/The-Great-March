using UnityEngine;

/// <summary>
/// In-game clock. Thought decay and scheduling run on this clock, never on wall-clock time.
/// "Now" is the total number of in-game minutes elapsed since the start of the campaign.
/// </summary>
public class GameClock : MonoBehaviour
{
    public const int MinutesPerDay = 24 * 60;

    private static GameClock _instance;

    [SerializeField] private double _minutesPerRealSecond = 1.0;
    [SerializeField] private long _startMinuteOfDay = 8 * 60;

    private double _now;

    public static GameClock Instance { get { return _instance; } }
    public static long Now { get { return _instance == null ? 0L : (long)_instance._now; } }

    public static long MinuteOfDay
    {
        get
        {
            long now = Now % MinutesPerDay;
            return now < 0 ? now + MinutesPerDay : now;
        }
    }

    public static float DayFraction { get { return MinuteOfDay / (float)MinutesPerDay; } }
    public static long Day { get { return Now / MinutesPerDay; } }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        _now = _startMinuteOfDay;
    }

    private void Update()
    {
        if (_minutesPerRealSecond > 0.0)
            _now += Time.deltaTime * _minutesPerRealSecond;
    }

    public static void Advance(double minutes)
    {
        if (_instance != null)
            _instance._now += minutes;
    }

    public static void SetNow(long totalMinutes)
    {
        if (_instance != null)
            _instance._now = totalMinutes;
    }

    public static long MinutesBetween(long from, long to)
    {
        return to - from;
    }

    public static float DaysBetween(long from, long to)
    {
        return (to - from) / (float)MinutesPerDay;
    }
}
