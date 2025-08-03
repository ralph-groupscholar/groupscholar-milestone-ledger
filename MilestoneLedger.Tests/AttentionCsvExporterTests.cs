using MilestoneLedger.Core.Models;
using MilestoneLedger.Core.Services;

namespace MilestoneLedger.Tests;

public sealed class AttentionCsvExporterTests
{
    [Fact]
    public void Export_EscapesReasonsWithCommas()
    {
        var items = new List<AttentionItem>
        {
            new(
                "Ava Mitchell",
                "Spring 2025",
                "active",
                new DateOnly(2026, 1, 10),
                29,
                1,
                new DateOnly(2026, 1, 10),
                "Risk flagged on 2026-01-10, needs follow-up")
        };

        var exporter = new AttentionCsvExporter();
        var output = exporter.Export(items);

        Assert.Contains("\"Risk flagged on 2026-01-10, needs follow-up\"", output);
        Assert.Contains("Ava Mitchell", output);
    }
}
