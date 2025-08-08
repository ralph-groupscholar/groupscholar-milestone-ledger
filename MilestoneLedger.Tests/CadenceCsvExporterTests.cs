using MilestoneLedger.Core.Models;
using MilestoneLedger.Core.Services;

namespace MilestoneLedger.Tests;

public sealed class CadenceCsvExporterTests
{
    [Fact]
    public void Export_WritesCsvRows()
    {
        var report = new CadenceReport(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 14),
            null,
            null,
            new List<CadenceWeekSummary>
            {
                new(new DateOnly(2025, 12, 29), 3, 1),
                new(new DateOnly(2026, 1, 5), 1, 0)
            },
            4,
            1);

        var exporter = new CadenceCsvExporter();
        var output = exporter.Export(report);

        Assert.Contains("week_start,total_milestones,risk_flags", output);
        Assert.Contains("2025-12-29,3,1", output);
    }
}
