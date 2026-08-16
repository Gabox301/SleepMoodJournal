using SleepMoodJournal.Models;
using SleepMoodJournal.Services;
using SleepMoodJournal.ViewModels;

namespace SleepMoodJournal.Tests;

public sealed class CalendarViewModelTests : IDisposable
{
    private static readonly DateTime FixedUtc = new(2024, 5, 15, 12, 0, 0, DateTimeKind.Utc);
    private readonly TestDb _db;

    public CalendarViewModelTests()
    {
        _db = new TestDb();
        AppTime.UtcNowProvider = () => FixedUtc;
    }

    public void Dispose()
    {
        _db.Dispose();
        AppTime.UtcNowProvider = static () => DateTime.UtcNow;
    }

    [Fact]
    public void Ctor_MuestraElMesActualCompleto()
    {
        var vm = new CalendarViewModel(_db.Factory);

        Assert.Equal(2024, vm.Year);
        Assert.Equal(5, vm.Month);
        Assert.Equal(31, vm.Days.Count);
        Assert.Contains("2024", vm.MonthDisplay);
    }

    [Fact]
    public void DiasConRegistro_TienenDatos()
    {
        using (var ctx = _db.Factory())
        {
            ctx.Entries.Add(new DailyEntry
            {
                Date = new DateOnly(2024, 5, 3),
                SleepHours = 7.5,
                SleepQuality = 4,
                Mood = 4,
            });
            ctx.SaveChanges();
        }

        var vm = new CalendarViewModel(_db.Factory);

        Assert.Equal(new CalendarDay(new DateOnly(2024, 5, 3), true, 7.5, 4), vm.Days[2]);
        Assert.False(vm.Days[0].HasEntry);
    }

    [Fact]
    public void PrevMonth_RetrocedeAlMesAnterior()
    {
        var vm = new CalendarViewModel(_db.Factory);

        vm.PrevMonthCommand.Execute(null);

        Assert.Equal(4, vm.Month);
        Assert.Equal(2024, vm.Year);
        Assert.Equal(30, vm.Days.Count);
    }

    [Fact]
    public void FebreroBisiesto_Tiene29Dias()
    {
        var vm = new CalendarViewModel(_db.Factory);
        vm.PrevMonthCommand.Execute(null); // abril
        vm.PrevMonthCommand.Execute(null); // marzo
        vm.PrevMonthCommand.Execute(null); // febrero 2024 (bisiesto)

        Assert.Equal(2, vm.Month);
        Assert.Equal(2024, vm.Year);
        Assert.Equal(29, vm.Days.Count);
    }

    [Fact]
    public void SelectDate_LevantaElEvento()
    {
        DateOnly? selected = null;
        var vm = new CalendarViewModel(_db.Factory);
        vm.DateSelected += (_, date) => selected = date;

        vm.SelectDateCommand.Execute(new DateOnly(2024, 5, 20));

        Assert.Equal(new DateOnly(2024, 5, 20), selected);
    }
}
