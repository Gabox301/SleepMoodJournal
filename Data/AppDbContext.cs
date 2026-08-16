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

    public AppDbContext() { }

    /// <summary>
    /// Ctor pensado para tests/DI: permite apuntar a… otra base (ej. SQLite en
    /// memoria) en lugar de la ruta fija de la app.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (options.IsConfigured)
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
        options.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyEntry>()
            .HasIndex(e => e.Date)
            .IsUnique(); // un solo registro por día
    }
}
