using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using SleepMoodJournal.Data;
using SleepMoodJournal.ViewModels;
using SleepMoodJournal.Views;

namespace SleepMoodJournal;

public partial class App : Application
{
    public override void Initialize()
    {
        try
        {
            AvaloniaXamlLoader.Load(this);
            Log("App.Initialize: XAML loaded");
        }
        catch (Exception ex)
        {
            Log("App.Initialize: error: " + ex);
            throw;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Log("App.OnFrameworkInitializationCompleted: starting desktop init");

                // Crea la base SQLite local si no existe, sin migraciones (EnsureCreated
                // alcanza para este proyecto; migrar a EF Migrations si el modelo crece).
                try
                {
                    using (var db = new AppDbContext())
                    {
                        db.Database.EnsureCreated();
                    }
                    Log("App: DB EnsureCreated OK");
                }
                catch (Exception ex)
                {
                    Log("App: DB EnsureCreated error: " + ex);
                }

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                Log("App: MainWindow created");
            }

            base.OnFrameworkInitializationCompleted();
            Log("App.OnFrameworkInitializationCompleted: completed");
        }
        catch (Exception ex)
        {
            Log("App.OnFrameworkInitializationCompleted: error: " + ex);
            throw;
        }
    }

    private static void Log(string text)
    {
        try
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SleepMoodJournal");
            Directory.CreateDirectory(folder);
            var file = Path.Combine(folder, "app-init.log");
            File.AppendAllText(file, $"[{DateTime.Now:O}] {text}\n");
        }
        catch { }
    }
}
