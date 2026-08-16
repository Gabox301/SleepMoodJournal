using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.EntityFrameworkCore;

using SleepMoodJournal.Data;
using SleepMoodJournal.Models;
using SleepMoodJournal.Services;

namespace SleepMoodJournal.ViewModels;

public partial class EntryViewModel : ViewModelBase
{
    [ObservableProperty]
    private DateTimeOffset _date = DateTimeOffset.Now;

    [ObservableProperty]
    private double _sleepHours = 7.5;

    [ObservableProperty]
    private int _sleepQuality = 3;

    [ObservableProperty]
    private int _mood = 3;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _summaryTitle = "Noche moderada";

    [ObservableProperty]
    private string? _sevenDayText;

    [ObservableProperty]
    private string? _streakText;

    /// <summary>Horas de sueño en formato amigable (ej: "7h 30m").</summary>
    public string SleepHoursDisplay => FormatSleepHours(SleepHours);

    private static string FormatSleepHours(double hours)
    {
        int h = (int)Math.Floor(hours);
        int m = (int)Math.Round((hours - h) * 60);
        if (m == 60) { h++; m = 0; }
        return m == 0 ? $"{h}h" : $"{h}h {m:00}m";
    }

    partial void OnSleepHoursChanged(double value) => OnPropertyChanged(nameof(SleepHoursDisplay));

    /// <summary>Se dispara cuando se guarda un registro, para que otras vistas se actualicen.</summary>
    public event EventHandler? EntrySaved;

    private readonly Func<AppDbContext> _dbFactory;

    public EntryViewModel()
        : this(static () => new AppDbContext())
    {
    }

    public EntryViewModel(Func<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        Load(AppTime.Today);
    }

    /// <summary>
    /// Carga los datos de la fecha indicada (si existe) y actualiza las propiedades.
    /// </summary>
    public void Load(DateOnly date)
    {
        // Ajusta la propiedad Date (DatePicker) al comienzo del día local
        Date = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local));

        using var db = _dbFactory();
        var existing = db.Entries.FirstOrDefault(e => e.Date == date);
        if (existing is null)
        {
            SleepHours = 7.5;
            SleepQuality = 3;
            Mood = 3;
            Notes = null;
            StatusMessage = null;
        }
        else
        {
            SleepHours = AppTime.RoundSleepHours(existing.SleepHours);
            SleepQuality = existing.SleepQuality;
            Mood = existing.Mood;
            Notes = existing.Notes;
            StatusMessage = "Ya registraste ese día — podés editarlo y guardar de nuevo.";
        }

        RefreshSide();
    }

    [RelayCommand]
    private async Task Save()
    {
        var date = DateOnly.FromDateTime(Date.DateTime);

        using var db = _dbFactory();
        var entry = await db.Entries.FirstOrDefaultAsync(e => e.Date == date);

        if (entry is null)
        {
            entry = new DailyEntry { Date = date };
            db.Entries.Add(entry);
        }

        entry.SleepHours = AppTime.RoundSleepHours(SleepHours);
        entry.SleepQuality = SleepQuality;
        entry.Mood = Mood;
        entry.Notes = Notes;

        await db.SaveChangesAsync();

        StatusMessage = $"Guardado ✓ ({date:dd/MM/yyyy})";
        RefreshSide();
        EntrySaved?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Actualiza la tarjeta lateral de resumen (título, promedio 7 días y racha).
    /// </summary>
    private void RefreshSide()
    {
        SummaryTitle = SleepHours switch
        {
            >= 8 => "Gran descanso",
            >= 7 => "Noche moderada",
            >= 5 => "Noche un poco corta",
            _ => "Muy poco sueño",
        };

        using var db = _dbFactory();
        var today = AppTime.Today;

        var recent = db.Entries
            .Where(e => e.Date <= today && e.Date > today.AddDays(-7))
            .ToList();

        SevenDayText = recent.Count > 0
            ? $"Promedio: {AppTime.RoundSleepHours(recent.Average(e => e.SleepHours)):0.0}h de sueño · Ánimo {recent.Average(e => e.Mood):0.0}/5"
            : "Todavía no hay registros en la última semana.";

        // Racha: contar días consecutivos hacia atrás desde hoy.
        var recorded = db.Entries.Select(e => e.Date).ToHashSet();
        int streak = 0;
        var cursor = today;
        while (recorded.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        StreakText = streak > 0
            ? $"{streak} día{(streak == 1 ? "" : "s")} seguidos registrando 🔥"
            : "No hay racha aún — registrá hoy para empezar.";
    }
}
