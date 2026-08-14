using Avalonia;
using System;
using System.IO;

namespace SleepMoodJournal;

internal static class Program
{
    // El punto de entrada NO puede usar features de C# que dependan de AppBuilder
    // configurado por el diseñador de Avalonia (evitamos problemas con el previewer).
    [STAThread]
    public static int Main(string[] args)
    {
        // Registrar manejadores globales para capturar excepciones no observadas
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SleepMoodJournal");
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "startup-unhandled.log"), ex?.ToString() ?? "Unknown unhandled exception");
            }
            catch { }
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            try
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SleepMoodJournal");
                Directory.CreateDirectory(folder);
                File.AppendAllText(Path.Combine(folder, "startup-unhandled.log"), e.Exception?.ToString() ?? "Unknown task exception");
            }
            catch { }
        };

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            return 0;
        }
        catch (Exception ex)
        {
            try
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SleepMoodJournal");
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "startup-error.log"), ex.ToString());
            }
            catch { }

            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
