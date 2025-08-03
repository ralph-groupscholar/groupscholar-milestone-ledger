namespace MilestoneLedger.Core.Services;

public static class DateRange
{
    public static (DateOnly Start, DateOnly End) ForLastWeeks(int weeks)
    {
        if (weeks < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(weeks), "Weeks must be at least 1.");
        }

        var end = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var start = end.AddDays(-(weeks * 7) + 1);
        return (start, end);
    }
}
