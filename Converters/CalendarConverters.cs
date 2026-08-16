using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SleepMoodJournal.Converters;

/// <summary>
/// Convierte horas de sueño (0-12) en un brush de intensidad "moon"
/// para el heatmap de la vista de calendario. 0 (sin registro) => casi transparente.
/// </summary>
public class SleepHeatBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double hours = value is double d ? d : 0;
        if (hours <= 0)
            return new SolidColorBrush(Color.FromArgb(12, 0xFF, 0xFF, 0xFF));

        double intensity = Math.Clamp(hours / 10.0, 0.15, 1.0);
        byte alpha = (byte)(110 + (int)((1.0 - intensity) * 0 + intensity * 130));
        return new SolidColorBrush(Color.FromArgb(alpha, 0xF2, 0xC8, 0x79));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Convierte el estado de ánimo (1-5) en un color para el punto del calendario.
/// 0 (sin registro) => transparente.
/// </summary>
public class MoodDotBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int mood = value is int m ? m : 0;
        var color = mood switch
        {
            1 => new Color(0xFF, 0xF0, 0x78, 0x8A), // rose
            2 => new Color(0xFF, 0xE0, 0x97, 0x5F),
            3 => new Color(0xFF, 0xC9, 0xA0, 0x5A),
            4 => new Color(0xFF, 0x7F, 0xD9, 0xA8), // ok
            5 => new Color(0xFF, 0x8B, 0x7C, 0xF6), // dusk
            _ => new Color(0x00, 0x00, 0x00, 0x00),
        };
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
