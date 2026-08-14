namespace SleepMoodJournal.Models;

/// <summary>
/// Un único registro por día. La fecha actúa como clave natural:
/// solo puede existir un DailyEntry por Date (ver AppDbContext).
/// </summary>
public class DailyEntry
{
    public int Id { get; set; }

    /// <summary>Fecha del registro (sin componente de hora).</summary>
    public DateOnly Date { get; set; }

    /// <summary>Horas de sueño, con decimales (ej: 7.5).</summary>
    public double SleepHours { get; set; }

    /// <summary>Calidad del sueño, escala 1 (mala) a 5 (excelente).</summary>
    public int SleepQuality { get; set; }

    /// <summary>Estado de ánimo, escala 1 (muy mal) a 5 (muy bien).</summary>
    public int Mood { get; set; }

    /// <summary>Notas libres opcionales (contexto: estrés, ejercicio, cafeína, etc).</summary>
    public string? Notes { get; set; }
}
