using System.Globalization;

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using SleepMoodJournal.Models;
using SleepMoodJournal.Services;
using SleepMoodJournal.ViewModels;

namespace SleepMoodJournal.Tests;

public sealed class TrendsViewModelTests : IDisposable
{
    private static readonly DateTime FixedUtc = new(2024, 5, 15, 12, 0, 0, DateTimeKind.Utc);
    private readonly TestDb _db;
    private readonly DateOnly _today;

    public TrendsViewModelTests()
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

    private void Seed(DateOnly date, double sleepHours, int mood)
    {
        using var ctx = _db.Factory();
        ctx.Entries.Add(new DailyEntry
        {
            Date = date,
            SleepHours = sleepHours,
            SleepQuality = 3,
            Mood = mood,
        });
        ctx.SaveChanges();
    }

    [Fact]
    public void SinRegistros_EscenarioVacio()
    {
        var vm = new TrendsViewModel(_db.Factory);

        Assert.Equal("Todavía no hay registros en este período.", vm.SummaryText);
        Assert.Equal("--", vm.StatSleep);
        Assert.Equal("--", vm.StatMood);
        Assert.Equal("0 días", vm.StatDays);
    }

    [Fact]
    public void ConRegistros_CalculaPromediosDelPeriodo()
    {
        Seed(_today.AddDays(-2), 6.0, 3);
        Seed(_today.AddDays(-1), 8.0, 5);

        var vm = new TrendsViewModel(_db.Factory);

        Assert.Equal("2 días", vm.StatDays);
        Assert.Equal(7.0, double.Parse(vm.StatSleep.TrimEnd('h'), CultureInfo.CurrentCulture), 3);
        Assert.Equal(4.0, double.Parse(vm.StatMood.Replace("/5", ""), CultureInfo.CurrentCulture), 3);
        Assert.StartsWith("promedio 7", vm.SleepAvgText);
        Assert.StartsWith("promedio 4", vm.MoodAvgText);
    }

    [Fact]
    public void CorrelacionSueñoAnimo_CuandoHayDosGrupos()
    {
        Seed(_today.AddDays(-3), 8.0, 5);   // buen sueño
        Seed(_today.AddDays(-2), 8.0, 4);   // buen sueño
        Seed(_today.AddDays(-1), 4.0, 2);   // poco sueño

        var vm = new TrendsViewModel(_db.Factory);

        Assert.Contains("Con ≥7h de sueño", vm.SummaryText);
        Assert.Contains("vs", vm.SummaryText);
    }

    [Fact]
    public void Correlacion_soloUnGrupo_PideMasRegistros()
    {
        Seed(_today.AddDays(-1), 6.0, 2);

        var vm = new TrendsViewModel(_db.Factory);

        Assert.Contains("Registrá más días", vm.SummaryText);
    }

    [Fact]
    public void DaysToShow_ReduceLaVentana()
    {
        Seed(_today.AddDays(-10), 7.0, 4);
        Seed(_today.AddDays(-40), 7.0, 4);

        var vm = new TrendsViewModel(_db.Factory);
        Assert.Equal("1 día", vm.StatDays);

        vm.DaysToShow = 7;
        Assert.Equal("0 días", vm.StatDays);
    }

    [Fact]
    public void Series_SePueblanConUnPuntoPorRegistro()
    {
        Seed(_today.AddDays(-1), 7.5, 4);
        Seed(_today, 8.0, 5);

        var vm = new TrendsViewModel(_db.Factory);

        Assert.Single(vm.XAxes);
        var sleepLine = Assert.IsType<LineSeries<double>>(vm.SleepSeries[0]);
        var moodLine = Assert.IsType<LineSeries<double>>(vm.MoodSeries[0]);
        Assert.NotNull(sleepLine.Values);
        Assert.Equal(2, sleepLine.Values!.ToArray().Length);

        Assert.NotNull(moodLine.Values);
        Assert.Equal(2, moodLine.Values!.ToArray().Length);
    }
}
