using SleepMoodJournal.Models;
using SleepMoodJournal.Services;
using SleepMoodJournal.ViewModels;

namespace SleepMoodJournal.Tests;

public sealed class EntryViewModelTests : IDisposable
{
    private static readonly DateTime FixedUtc = new(2024, 5, 15, 12, 0, 0, DateTimeKind.Utc);
    private readonly TestDb _db;
    private readonly DateOnly _today;

    public EntryViewModelTests()
    {
        _db = new TestDb();
        AppTime.UtcNowProvider = () => FixedUtc;
        _today = AppTime.Today;
    }

    public void Dispose()
    {
        _db.Dispose();
        AppTime.UtcNowProvider = static () => DateTime.UtcNow;
    }

    private void Seed(DateOnly date, double sleepHours, int quality, int mood, string? notes = null)
    {
        using var ctx = _db.Factory();
        ctx.Entries.Add(new DailyEntry
        {
            Date = date,
            SleepHours = sleepHours,
            SleepQuality = quality,
            Mood = mood,
            Notes = notes,
        });
        ctx.SaveChanges();
    }

    [Fact]
    public void Load_ConRegistroExistente_CompletaLasPropiedades()
    {
        Seed(_today, 7.5, 4, 5, "dormí bien");

        var vm = new EntryViewModel(_db.Factory);

        Assert.Equal(_today, DateOnly.FromDateTime(vm.Date.DateTime));
        Assert.Equal(7.5, vm.SleepHours);
        Assert.Equal(4, vm.SleepQuality);
        Assert.Equal(5, vm.Mood);
        Assert.Equal("dormí bien", vm.Notes);
        Assert.Contains("editarlo", vm.StatusMessage);
    }

    [Fact]
    public void Load_SinRegistro_ReseteaAValoresPorDefecto()
    {
        var vm = new EntryViewModel(_db.Factory);

        Assert.Equal(7.5, vm.SleepHours);
        Assert.Equal(3, vm.SleepQuality);
        Assert.Equal(3, vm.Mood);
        Assert.Null(vm.Notes);
        Assert.Null(vm.StatusMessage);
    }

    [Fact]
    public void SleepHoursDisplay_FormateaEnHorasYMinutos()
    {
        var vm = new EntryViewModel(_db.Factory);

        vm.SleepHours = 7.5;
        Assert.Equal("7h 30m", vm.SleepHoursDisplay);

        vm.SleepHours = 8.0;
        Assert.Equal("8h", vm.SleepHoursDisplay);

        vm.SleepHours = 6.25;
        Assert.Equal("6h 15m", vm.SleepHoursDisplay);
    }

    [Fact]
    public async Task Save_CreaElRegistroDelDia()
    {
        var vm = new EntryViewModel(_db.Factory)
        {
            SleepHours = 7.5,
            SleepQuality = 4,
            Mood = 4,
            Notes = "mejor noche",
        };

        await vm.SaveCommand.ExecuteAsync(null);

        using var ctx = _db.Factory();
        var entry = ctx.Entries.Single();
        Assert.Equal(_today, entry.Date);
        Assert.Equal(7.5, entry.SleepHours);
        Assert.Equal(4, entry.Mood);
        Assert.Equal("mejor noche", entry.Notes);
        Assert.Contains("Guardado", vm.StatusMessage);
    }

    [Fact]
    public async Task Save_ActualizaElRegistroExistente()
    {
        Seed(_today, 6.0, 2, 2);

        var vm = new EntryViewModel(_db.Factory)
        {
            SleepHours = 8.5,
            Mood = 5,
        };
        await vm.SaveCommand.ExecuteAsync(null);

        using var ctx = _db.Factory();
        Assert.Equal(1, ctx.Entries.Count());
        var entry = ctx.Entries.Single();
        Assert.Equal(8.5, entry.SleepHours);
        Assert.Equal(5, entry.Mood);
    }

    [Fact]
    public void SummaryTitle_ClasificaLaCantidadDeSueño()
    {
        Seed(_today, 8.5, 5, 5);
        var vm = new EntryViewModel(_db.Factory);
        Assert.Equal("Gran descanso", vm.SummaryTitle);

        Seed(_today.AddDays(1), 4.0, 1, 1);
        vm.Load(_today.AddDays(1));
        Assert.Equal("Muy poco sueño", vm.SummaryTitle);
    }

    [Fact]
    public void SevenDayText_ConRegistros_MuestraElPromedio()
    {
        Seed(_today, 8.0, 4, 5);

        var vm = new EntryViewModel(_db.Factory);

        Assert.StartsWith("Promedio:", vm.SevenDayText);
        Assert.Contains("de sueño", vm.SevenDayText);
        Assert.Contains("Ánimo", vm.SevenDayText);
    }

    [Fact]
    public void SevenDayText_SinRegistros_Avisa()
    {
        var vm = new EntryViewModel(_db.Factory);

        Assert.Equal("Todavía no hay registros en la última semana.", vm.SevenDayText);
    }

    [Fact]
    public void StreakText_SinRacha_AvisaAEmpezarHoy()
    {
        Seed(_today.AddDays(-3), 7.0, 3, 3);

        var vm = new EntryViewModel(_db.Factory);

        Assert.StartsWith("No hay racha aún", vm.StreakText);
    }

    [Fact]
    public void StreakText_DosDiasSeguidos_SumaLaRacha()
    {
        Seed(_today.AddDays(-1), 7.5, 3, 3);
        Seed(_today, 7.5, 4, 4);

        var vm = new EntryViewModel(_db.Factory);

        Assert.StartsWith("2 día", vm.StreakText);
    }
}
