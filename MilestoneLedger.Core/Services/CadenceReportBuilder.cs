using MilestoneLedger.Core.Models;

namespace MilestoneLedger.Core.Services;

public sealed class CadenceReportBuilder
{
    public CadenceReport Build(
        DateOnly startDate,
        DateOnly endDate,
        string? cohort,
        string? status,
        IReadOnlyList<CadenceWeekSummary> weeks)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be on or after start date.");
        }

        var normalizedStart = StartOfWeek(startDate);
        var normalizedEnd = StartOfWeek(endDate);

        var lookup = weeks.ToDictionary(entry => entry.WeekStart, entry => entry);
        var filledWeeks = new List<CadenceWeekSummary>();
        var totalMilestones = 0;
        var riskFlags = 0;

        for (var weekStart = normalizedStart; weekStart <= normalizedEnd; weekStart = weekStart.AddDays(7))
        {
            if (lookup.TryGetValue(weekStart, out var summary))
            {
                filledWeeks.Add(summary);
                totalMilestones += summary.TotalMilestones;
                riskFlags += summary.RiskFlags;
                continue;
            }

            filledWeeks.Add(new CadenceWeekSummary(weekStart, 0, 0));
        }

        return new CadenceReport(startDate, endDate, cohort, status, filledWeeks, totalMilestones, riskFlags);
    }

    public static DateOnly StartOfWeek(DateOnly date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var monday = (int)DayOfWeek.Monday;
        var diff = (7 + dayOfWeek - monday) % 7;
        return date.AddDays(-diff);
    }
}
