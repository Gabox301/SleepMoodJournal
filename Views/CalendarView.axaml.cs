using Avalonia.Controls;
using Avalonia.Interactivity;
using SleepMoodJournal.ViewModels;

namespace SleepMoodJournal.Views;

public partial class CalendarView : UserControl
{
    public CalendarView()
    {
        InitializeComponent();
    }

    private void OnDayClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Avalonia.Controls.Button btn && btn.DataContext is CalendarDay day)
        {
            if (this.DataContext is CalendarViewModel vm)
            {
                vm.SelectDate(day.Date);
            }
        }
    }
}
