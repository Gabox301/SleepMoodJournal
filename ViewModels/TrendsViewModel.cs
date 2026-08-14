using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SleepMoodJournal.Data;
using SleepMoodJournal.Services;

namespace SleepMoodJournal.ViewModels;

public partial class TrendsViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _daysToShow = 30;

    [ObservableProperty]
    private string? _summaryText;

    [ObservableProperty]
    private string _statSleep = "--";

    [ObservableProperty]
    private string _statMood = "--";

    [ObservableProperty]
    private string _statDays = "--";

    [ObservableProperty]
    private string _sleepAvgText = "promedio --";

    [ObservableProperty]
    private string _moodAvgText = "promedio --";

    public ISeries[] SleepSeries { get; private set; } = [];
    public ISeries[] MoodSeries { get; private set; } = [];
    public Axis[] XAxes { get; private set; } = [];

    public TrendsViewModel()
    {
        Reload();
    }

    public void Reload()
    {
        using var db = new AppDbContext();
        var since = AppTime.Today.AddDays(-DaysToShow);

        var entries = db.Entries
            .Where(e => e.Date >= since)
            .OrderBy(e => e.Date)
            .ToList();

        var labels = entries.Select(e => e.Date.ToString("dd/MM")).ToArray();

        SleepSeries =
        [
            new LineSeries<double>
            {
                Name = "Horas de sueño",
                Values = entries.Select(e => AppTime.RoundSleepHours(e.SleepHours)).ToArray(),
                Stroke = new SolidColorPaint(SKColor.Parse("#F2C879"), 2.5f),
                Fill = new LinearGradientPaint(
                    new[] { new SKColor(0xF2, 0xC8, 0x79, 0x55), new SKColor(0xF2, 0xC8, 0x79, 0x00) },
                    new SKPoint(0, 0),
                    new SKPoint(0, 1)),
                GeometrySize = 0,
                LineSmoothness = 0.25,
            }
        ];

        MoodSeries =
        [
            new LineSeries<double>
            {
                Name = "Ánimo (1-5)",
                Values = entries.Select(e => (double)e.Mood).ToArray(),
                Stroke = new SolidColorPaint(SKColor.Parse("#8B7CF6"), 2.5f),
                Fill = new LinearGradientPaint(
                    new[] { new SKColor(0x8B, 0x7C, 0xF6, 0x55), new SKColor(0x8B, 0x7C, 0xF6, 0x00) },
                    new SKPoint(0, 0),
                    new SKPoint(0, 1)),
                GeometrySize = 0,
                LineSmoothness = 0.25,
            }
        ];

        XAxes =
        [
            new Axis
            {
                Labels = labels,
                LabelsRotation = 45,
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#7D7D92")),
                TextSize = 11,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#18FFFFFF")),
            }
        ];

        if (entries.Count == 0)
        {
            SummaryText = "Todavía no hay registros en este período.";
            StatSleep = "--";
            StatMood = "--";
            StatDays = "0 días";
            SleepAvgText = "promedio --";
            MoodAvgText = "promedio --";
            return;
        }

        var avgSleep = entries.Average(e => e.SleepHours);
        var avgMood = entries.Average(e => e.Mood);

        StatSleep = $"{avgSleep:0.0}h";
        StatMood = $"{avgMood:0.0}/5";
        StatDays = $"{entries.Count} días";
        SleepAvgText = $"promedio {avgSleep:0.0}h";
        MoodAvgText = $"promedio {avgMood:0.0}";

        // Correlación simple: ánimo promedio en días de "buen sueño" (>= 7h) vs el resto.
        var goodSleepDays = entries.Where(e => e.SleepHours >= 7).ToList();
        var otherDays = entries.Where(e => e.SleepHours < 7).ToList();

        var goodSleepMood = goodSleepDays.Count > 0 ? goodSleepDays.Average(e => e.Mood) : (double?)null;
        var otherMood = otherDays.Count > 0 ? otherDays.Average(e => e.Mood) : (double?)null;

        SummaryText = $"Promedio: {avgSleep:0.0}h de sueño, ánimo {avgMood:0.0}/5. " +
            (goodSleepMood is not null && otherMood is not null
                ? $"Con ≥7h de sueño el ánimo promedio es {goodSleepMood:0.0}, vs {otherMood:0.0} con menos horas."
                : "Registrá más días para ver la correlación sueño/ánimo.");
    }

    partial void OnDaysToShowChanged(int value) => Reload();

    [RelayCommand]
    private void SetPeriod(object? parameter)
    {
        if (parameter is not null && int.TryParse(parameter.ToString(), out int days))
            DaysToShow = days;
    }
}
