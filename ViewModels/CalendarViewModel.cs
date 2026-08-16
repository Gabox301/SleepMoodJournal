using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using SleepMoodJournal.Data;
using SleepMoodJournal.Models;
using SleepMoodJournal.Services;

namespace SleepMoodJournal.ViewModels;

public partial class CalendarViewModel : ViewModelBase
{

    [ObservableProperty]
    private int _year;

    [ObservableProperty]
    private int _month;

    public IReadOnlyList<CalendarDay> Days { get; private set; } = Array.Empty<CalendarDay>();

    public event EventHandler<DateOnly>? DateSelected;

    public CalendarViewModel()
    {
        var today = AppTime.Today;
        _year = today.Year;
        _month = today.Month;
        Reload();
    }

    public void Reload()
    {
        var firstOfMonth = new DateOnly(Year, Month, 1);
        var daysInMonth = DateTime.DaysInMonth(Year, Month);

        using var db = new AppDbContext();
        var since = firstOfMonth;
        var until = new DateOnly(Year, Month, daysInMonth);

        var entries = db.Entries
            .Where(e => e.Date >= since && e.Date <= until)
            .Select(e => new { e.Date, e.SleepHours, e.Mood })
            .ToDictionary(e => e.Date);

        var list = new List<CalendarDay>(daysInMonth);
        for (int d = 1; d <= daysInMonth; d++)
        {
            var date = new DateOnly(Year, Month, d);
            if (entries.TryGetValue(date, out var e))
                list.Add(new CalendarDay(date, true, e.SleepHours, e.Mood));
            else
                list.Add(new CalendarDay(date, false));
        }

        Days = list;
        OnPropertyChanged(nameof(Days));
        OnPropertyChanged(nameof(MonthDisplay));
    }

    public string MonthDisplay => new DateTime(Year, Month, 1).ToString("MMMM yyyy");

    [RelayCommand]
    private void PrevMonth()
    {
        var dt = new DateTime(Year, Month, 1).AddMonths(-1);
        Year = dt.Year;
        Month = dt.Month;
        Reload();
    }

    [RelayCommand]
    private void NextMonth()
    {
        var dt = new DateTime(Year, Month, 1).AddMonths(1);
        Year = dt.Year;
        Month = dt.Month;
        Reload();
    }

    [RelayCommand]
    public void SelectDate(DateOnly date)
    {
        DateSelected?.Invoke(this, date);
    }
}
