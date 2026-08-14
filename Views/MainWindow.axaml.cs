using Avalonia.Controls;

namespace SleepMoodJournal.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            // Log and rethrow to ensure Program captures it
            try
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SleepMoodJournal");
                Directory.CreateDirectory(folder);
                File.AppendAllText(Path.Combine(folder, "mainwindow-init.log"), ex.ToString());
            }
            catch { }
            throw;
        }
    }
}
