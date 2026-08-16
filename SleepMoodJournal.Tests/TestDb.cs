using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using SleepMoodJournal.Data;

namespace SleepMoodJournal.Tests;

/// <summary>
/// Base de datos SQLite en memoria (":memory:") aislada por test.
/// <see cref="Factory"/> devuelve contexts sobre la misma conexión abierta,
/// para que los datos sembrados persistan entre operaciones de una prueba.
/// </summary>
internal sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public Func<AppDbContext> Factory { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        Factory = () => new AppDbContext(_options);

        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();
}
