using System;
using UnityEngine;

/// <summary>
/// Central 24-hour game clock that advances with Time.deltaTime (respects pause & speed).
/// Exposes Day, Hour, Minute, and useful helpers for schedules/lighting.
/// </summary>
public class GameClock : MonoBehaviour
{
    [Header("Tuning")]
    [Tooltip("Real-time seconds for one full in-game day at 1× speed.")]
    [SerializeField] private float secondsPerGameDay = 600f; // 10 real minutes per game day by default

    [Tooltip("Starting in-game time (24h clock).")]
    [Range(0, 23)] [SerializeField] private int startHour = 8;
    [Range(0, 59)] [SerializeField] private int startMinute = 0;

    [Header("State (read-only)")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private float timeOfDaySeconds; // 0..secondsPerGameDay

    public event Action<int> OnDayChanged;

    public int Day => currentDay;
    public float NormalizedDay => secondsPerGameDay <= 0f ? 0f : Mathf.Clamp01(timeOfDaySeconds / secondsPerGameDay);

    public int Hour24
    {
        get
        {
            float hourLen = HourLengthSeconds;
            return Mathf.FloorToInt(timeOfDaySeconds / hourLen) % 24;
        }
    }

    public int Minute
    {
        get
        {
            float minuteLen = MinuteLengthSeconds;
            return Mathf.FloorToInt(timeOfDaySeconds / minuteLen) % 60;
        }
    }

    public int Second
    {
        get
        {
            float secondLen = SecondLengthSeconds;
            return Mathf.FloorToInt(timeOfDaySeconds / secondLen) % 60;
        }
    }

    public string TimeHHMM => $"{Hour24:00}:{Minute:00}";

    private float HourLengthSeconds => secondsPerGameDay / 24f;
    private float MinuteLengthSeconds => secondsPerGameDay / (24f * 60f);
    private float SecondLengthSeconds => secondsPerGameDay / (24f * 60f * 60f);

    private void Awake()
    {
        if (secondsPerGameDay <= 0f) secondsPerGameDay = 600f;
        InitializeToStartTime();
    }

    private void Update()
    {
        // deltaTime respects pause & timeScale; perfect for the clock.
        timeOfDaySeconds += Time.deltaTime;

        if (timeOfDaySeconds >= secondsPerGameDay)
        {
            timeOfDaySeconds -= secondsPerGameDay;
            currentDay = Mathf.Max(1, currentDay + 1);
            try { OnDayChanged?.Invoke(currentDay); } catch { /* ignore listener errors */ }
        }
    }

    /// <summary>Resets the clock to a specific day/hour/minute (seconds = 0).</summary>
    public void ResetClock(int day, int hour, int minute)
    {
        currentDay = Mathf.Max(1, day);
        SetTimeOfDay(hour, minute, 0);
    }

    private void InitializeToStartTime()
    {
        currentDay = Mathf.Max(1, currentDay);
        SetTimeOfDay(startHour, startMinute, 0);
    }

    private void SetTimeOfDay(int hour, int minute, int second)
    {
        hour = Mathf.Clamp(hour, 0, 23);
        minute = Mathf.Clamp(minute, 0, 59);
        second = Mathf.Clamp(second, 0, 59);

        // Map HH:MM:SS to our simulated-day seconds.
        float realSecondsInDay = (hour * 3600f) + (minute * 60f) + second;
        float t = realSecondsInDay / 86400f; // 0..1
        timeOfDaySeconds = Mathf.Repeat(t * secondsPerGameDay, secondsPerGameDay);
    }
}

// Convenience static accessor if desired elsewhere.
public static class GameClockAPI
{
    public static GameClock Find()
    {
#if UNITY_2023_1_OR_NEWER
        // Prefer the modern API to avoid CS0618 warnings.
        return UnityEngine.Object.FindFirstObjectByType<GameClock>();
#else
        // Fallback for older Unity versions.
        return UnityEngine.Object.FindObjectOfType<GameClock>();
#endif
    }
}
