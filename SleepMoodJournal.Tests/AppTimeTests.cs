using SleepMoodJournal.Services;

namespace SleepMoodJournal.Tests;

public sealed class AppTimeTests : IDisposable
{
    private static readonly DateTime FixedUtc = new(2024, 5, 15, 12, 0, 0, DateTimeKind.Utc);

    public AppTimeTests() => AppTime.UtcNowProvider = () => FixedUtc;

    public void Dispose() => AppTime.UtcNowProvider = static () => DateTime.UtcNow;

    [Fact]
    public void RoundSleepHours_RedondeaEnPasosDeMediaHora()
    {
        Assert.Equal(6.0, AppTime.RoundSleepHours(5.9));
        Assert.Equal(7.0, AppTime.RoundSleepHours(7.1));
        Assert.Equal(6.5, AppTime.RoundSleepHours(6.3));
    }

    [Fact]
    public void RoundSleepHours_NoAlteraHorasExactas()
    {
        Assert.Equal(8.0, AppTime.RoundSleepHours(8.0));
        Assert.Equal(7.5, AppTime.RoundSleepHours(7.5));
    }

    [Fact]
    public void Today_RespetaElRelojConfigurado()
    {
        var expected = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(FixedUtc, TimeZoneInfo.Local));

        Assert.Equal(expected, AppTime.Today);
    }
}
