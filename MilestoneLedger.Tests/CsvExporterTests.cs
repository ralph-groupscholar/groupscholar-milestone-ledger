using MilestoneLedger.Core.Models;
using MilestoneLedger.Core.Services;

namespace MilestoneLedger.Tests;

public sealed class CsvExporterTests
{
    [Fact]
    public void Export_HandlesCommasAndQuotes()
    {
        var entries = new List<MilestoneEntry>
        {
            new(1, "Ava Mitchell", "Spring 2025", "active", "Awarded", new DateOnly(2026, 2, 8), "Needs follow-up, \"urgent\".", true, DateTimeOffset.UtcNow)
        };

        var exporter = new CsvExporter();
        var output = exporter.Export(entries);

        Assert.Contains("\"Needs follow-up, \"\"urgent\"\".\"", output);
        Assert.Contains("Ava Mitchell", output);
    }
}
