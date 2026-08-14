namespace SleepMoodJournal.Services;

/// <summary>
/// Fuente única de "hoy" y normalización de horas de sueño.
/// La fecha se calcula sobre el huso horario local (para esta app, GMT-3),
/// evitando confusiones con UTC u otras conversiones.
/// </summary>
public static class AppTime
{
    /// <summary>Fecha actual en el huso horario local (GMT-3).</summary>
    public static DateOnly Today =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local));

    /// <summary>
    /// Redondea horas a pasos de 30 minutos (0,5h).
    /// Ej: 5,9 → 6,0 · 7,1 → 7,0 · 6,3 → 6,5. Evita decimales sin sentido.
    /// </summary>
    public static double RoundSleepHours(double hours) => Math.Round(hours * 2.0) / 2.0;
}