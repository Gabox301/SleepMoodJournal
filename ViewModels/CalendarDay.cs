namespace SleepMoodJournal.ViewModels;

public record CalendarDay(DateOnly Date, bool HasEntry, double SleepHours = 0, int Mood = 0);
