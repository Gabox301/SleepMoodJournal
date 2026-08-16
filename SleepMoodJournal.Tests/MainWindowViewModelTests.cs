using SleepMoodJournal.Services;
using SleepMoodJournal.ViewModels;

namespace SleepMoodJournal.Tests;

public sealed class MainWindowViewModelTests : IDisposable
{
    private static readonly DateTime FixedUtc = new(2024, 5, 15, 12, 0, 0, DateTimeKind.Utc);
    private readonly TestDb _db;

    public MainWindowViewModelTests()
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
    public void AlCrearse_MuestraLaVistaDeEntrada()
    {
        var vm = new MainWindowViewModel(_db.Factory);

        Assert.Same(vm.EntryViewModel, vm.CurrentView);
    }

    [Fact]
    public void Navegacion_CambiaLaVistaActual()
    {
        var vm = new MainWindowViewModel(_db.Factory);

        vm.ShowTrendsCommand.Execute(null);
        Assert.Same(vm.TrendsViewModel, vm.CurrentView);

        vm.ShowCalendarCommand.Execute(null);
        Assert.Same(vm.CalendarViewModel, vm.CurrentView);

        vm.ShowEntryCommand.Execute(null);
        Assert.Same(vm.EntryViewModel, vm.CurrentView);
    }

    [Fact]
    public void SeleccionDeFechaEnCalendario_CargaLaFechaEnEntrada()
    {
        var vm = new MainWindowViewModel(_db.Factory);
        var selected = new DateOnly(2024, 5, 7);

        vm.CalendarViewModel.SelectDateCommand.Execute(selected);

        Assert.Same(vm.EntryViewModel, vm.CurrentView);
        Assert.Equal(selected, DateOnly.FromDateTime(vm.EntryViewModel.Date.DateTime));
    }
}
