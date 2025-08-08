using MilestoneLedger.Core.Models;
using MilestoneLedger.Core.Services;

namespace MilestoneLedger.Tests;

public sealed class CadenceFormatterTests
{
    [Fact]
    public void Format_IncludesFiltersAndTable()
    {
        var report = new CadenceReport(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31),
            "Spring 2025",
            "active",
            new List<CadenceWeekSummary>
            {
                new(new DateOnly(2025, 12, 29), 4, 1),
                new(new DateOnly(2026, 1, 5), 2, 0)
            },
            6,
            1);

        var formatter = new CadenceFormatter();
        var output = formatter.Format(report);

        Assert.Contains("Milestone Cadence (2026-01-01 to 2026-01-31)", output);
        Assert.Contains("Filter: cohort=Spring 2025, status=active", output);
        Assert.Contains("2025-12-29 |", output);
    }
}
