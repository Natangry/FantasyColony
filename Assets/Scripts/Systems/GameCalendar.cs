using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SeasonDef
{
    public string name = "Season";
    [Min(1)] public int days = 12;
    public Color color = Color.white; // reserved for future UI lighting/themes
}

/// <summary>
/// Lightweight calendar layered on top of GameClock. Supports custom seasons/lengths.
/// Default year = 4 seasons × 12 days = 48 days.
/// </summary>
public class GameCalendar : MonoBehaviour
{
    [Header("Calendar Structure")]
    [SerializeField] private List<SeasonDef> seasons = new List<SeasonDef>
    {
        new SeasonDef { name = "Spring", days = 12, color = new Color(0.6f, 0.9f, 0.6f) },
        new SeasonDef { name = "Summer", days = 12, color = new Color(0.9f, 0.85f, 0.5f) },
        new SeasonDef { name = "Autumn", days = 12, color = new Color(0.95f, 0.7f, 0.4f) },
        new SeasonDef { name = "Winter", days = 12, color = new Color(0.8f, 0.9f, 1f) }
    };

    [Header("State (read-only)")]
    [SerializeField] private int year = 1;       // starts at 1
    [SerializeField] private int dayOfYear = 1;  // 1..DaysPerYear

    public event Action<int> OnYearChanged;
    public event Action<int> OnSeasonChanged; // passes SeasonIndex

    public int Year => year;
    public int DayOfYear => dayOfYear;
    public int SeasonCount => Mathf.Max(1, seasons.Count);
    public int DaysPerYear { get; private set; }

    public int SeasonIndex
    {
        get
        {
            var (idx, _) = ResolveSeasonAndDay(dayOfYear);
            return idx;
        }
    }

    public string CurrentSeasonName => seasons.Count == 0 ? "Season" : seasons[SeasonIndex].name;

    public int DayOfSeason
    {
        get
        {
            var (_, dayInSeason) = ResolveSeasonAndDay(dayOfYear);
            return dayInSeason;
        }
    }

    private GameClock _clock;

    private void Awake()
    {
        RecomputeDaysPerYear();
        dayOfYear = Mathf.Clamp(dayOfYear, 1, DaysPerYear);
    }

    private void OnEnable()
    {
        _clock = GameClockAPI.Find();
        if (_clock != null) _clock.OnDayChanged += HandleDayAdvanced;
    }

    private void OnDisable()
    {
        if (_clock != null) _clock.OnDayChanged -= HandleDayAdvanced;
        _clock = null;
    }

    private void HandleDayAdvanced(int newClockDay)
    {
        AdvanceOneDay();
    }

    public void ResetCalendar(int newYear, int newDayOfYear)
    {
        RecomputeDaysPerYear();
        int prevSeason = SeasonIndex;
        year = Mathf.Max(1, newYear);
        dayOfYear = Mathf.Clamp(newDayOfYear, 1, DaysPerYear);
        int nowSeason = SeasonIndex;
        if (nowSeason != prevSeason) SafeInvokeSeasonChanged(nowSeason);
    }

    public void AdvanceOneDay()
    {
        int prevSeason = SeasonIndex;
        dayOfYear++;
        if (dayOfYear > DaysPerYear)
        {
            dayOfYear = 1;
            year++;
            SafeInvokeYearChanged(year);
        }
        int nowSeason = SeasonIndex;
        if (nowSeason != prevSeason) SafeInvokeSeasonChanged(nowSeason);
    }

    private void RecomputeDaysPerYear()
    {
        if (seasons == null || seasons.Count == 0)
        {
            seasons = new List<SeasonDef> { new SeasonDef { name = "All-Year", days = 48 } };
        }
        int total = 0;
        foreach (var s in seasons) total += Mathf.Max(1, s.days);
        DaysPerYear = Mathf.Max(1, total);
    }

    private (int seasonIndex, int dayInSeason) ResolveSeasonAndDay(int dayOfYear1)
    {
        int d = Mathf.Clamp(dayOfYear1, 1, DaysPerYear);
        int acc = 0;
        for (int i = 0; i < seasons.Count; i++)
        {
            int len = Mathf.Max(1, seasons[i].days);
            if (d <= acc + len)
            {
                int dayInSeason = d - acc; // 1-based
                return (i, dayInSeason);
            }
            acc += len;
        }
        // Fallback
        return (0, d);
    }

    private void SafeInvokeYearChanged(int y)
    {
        try { OnYearChanged?.Invoke(y); } catch { }
    }

    private void SafeInvokeSeasonChanged(int season)
    {
        try { OnSeasonChanged?.Invoke(season); } catch { }
    }
}

public static class GameCalendarAPI
{
    public static GameCalendar Find() => UnityEngine.Object.FindObjectOfType<GameCalendar>();
}

