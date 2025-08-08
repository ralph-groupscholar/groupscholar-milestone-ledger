using MilestoneLedger.Core.Models;
using MilestoneLedger.Core.Services;

namespace MilestoneLedger.Tests;

public sealed class CadenceReportBuilderTests
{
    [Fact]
    public void Build_FillsMissingWeeksAndTotals()
    {
        var weeks = new List<CadenceWeekSummary>
        {
            new(new DateOnly(2026, 1, 26), 2, 1),
            new(new DateOnly(2026, 2, 9), 3, 0)
        };

        var builder = new CadenceReportBuilder();
        var report = builder.Build(
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 15),
            "Spring 2025",
            "active",
            weeks);

        Assert.Equal(3, report.Weeks.Count);
        Assert.Equal(5, report.TotalMilestones);
        Assert.Equal(1, report.RiskFlags);
        Assert.Equal(new DateOnly(2026, 2, 2), report.Weeks[1].WeekStart);
        Assert.Equal(0, report.Weeks[1].TotalMilestones);
    }

    [Fact]
    public void StartOfWeek_ReturnsMonday()
    {
        var date = new DateOnly(2026, 2, 5);
        var weekStart = CadenceReportBuilder.StartOfWeek(date);

        Assert.Equal(new DateOnly(2026, 2, 2), weekStart);
    }
}
