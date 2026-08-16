using Microsoft.EntityFrameworkCore;

using SleepMoodJournal.Models;

namespace SleepMoodJournal.Tests;

public sealed class AppDbContextTests
{
    [Fact]
    public void Save_And_Read_HaceRoundTripCompleto()
    {
        using var db = new TestDb();
        var date = new DateOnly(2024, 5, 10);

        using (var ctx = db.Factory())
        {
            ctx.Entries.Add(new DailyEntry
            {
                Date = date,
                SleepHours = 7.5,
                SleepQuality = 4,
                Mood = 5,
                Notes = "dormí bien",
            });
            ctx.SaveChanges();
        }

        using (var ctx = db.Factory())
        {
            var entry = ctx.Entries.Single();
            Assert.Equal(date, entry.Date);
            Assert.Equal(7.5, entry.SleepHours);
            Assert.Equal(4, entry.SleepQuality);
            Assert.Equal(5, entry.Mood);
            Assert.Equal("dormí bien", entry.Notes);
        }
    }

    [Fact]
    public void IndiceUnico_ImpideDosRegistrosElMismoDia()
    {
        using var db = new TestDb();
        var date = new DateOnly(2024, 5, 10);

        using (var ctx = db.Factory())
        {
            ctx.Entries.Add(new DailyEntry
            {
                Date = date,
                SleepHours = 7.0,
                SleepQuality = 3,
                Mood = 3,
            });
            ctx.SaveChanges();

            ctx.Entries.Add(new DailyEntry
            {
                Date = date,
                SleepHours = 5.0,
                SleepQuality = 2,
                Mood = 2,
            });

            Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
        }
    }
}
