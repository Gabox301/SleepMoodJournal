using Microsoft.EntityFrameworkCore;
using SleepMoodJournal.Models;

namespace SleepMoodJournal.Data;

public class AppDbContext : DbContext
{
    // Guarda la base en la carpeta de datos de la app del usuario, no en el
    // directorio de instalación (para que sobreviva actualizaciones).
    private static readonly string DbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SleepMoodJournal",
        "journal.db");

    public DbSet<DailyEntry> Entries => Set<DailyEntry>();

    public AppDbContext()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyEntry>()
            .HasIndex(e => e.Date)
            .IsUnique(); // un solo registro por día
    }
}
