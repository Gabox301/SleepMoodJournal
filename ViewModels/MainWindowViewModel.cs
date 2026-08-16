using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using SleepMoodJournal.Data;

namespace SleepMoodJournal.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    public EntryViewModel EntryViewModel { get; }
    public TrendsViewModel TrendsViewModel { get; }
    public CalendarViewModel CalendarViewModel { get; }

    public MainWindowViewModel()
        : this(static () => new AppDbContext())
    {
    }

    public MainWindowViewModel(Func<AppDbContext> dbFactory)
    {
        EntryViewModel = new EntryViewModel(dbFactory);
        TrendsViewModel = new TrendsViewModel(dbFactory);
        CalendarViewModel = new CalendarViewModel(dbFactory);
        _currentView = EntryViewModel;

        // Cuando se guarda un registro nuevo, refrescamos los gráficos de tendencia.
        EntryViewModel.EntrySaved += (_, _) => TrendsViewModel.Reload();
        // Cuando se selecciona una fecha en el calendario, cargamos esa fecha en EntryViewModel y mostramos la vista de entrada.
        CalendarViewModel.DateSelected += (_, date) =>
        {
            EntryViewModel.Load(date);
            CurrentView = EntryViewModel;
        };
    }

    [RelayCommand]
    private void ShowEntry() => CurrentView = EntryViewModel;

    [RelayCommand]
    private void ShowTrends()
    {
        TrendsViewModel.Reload();
        CurrentView = TrendsViewModel;
    }

    [RelayCommand]
    private void ShowCalendar()
    {
        CalendarViewModel.Reload();
        CurrentView = CalendarViewModel;
    }
}
