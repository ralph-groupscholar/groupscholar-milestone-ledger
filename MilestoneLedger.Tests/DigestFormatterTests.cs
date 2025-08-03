using MilestoneLedger.Core.Models;
using MilestoneLedger.Core.Services;

namespace MilestoneLedger.Tests;

public sealed class DigestFormatterTests
{
    [Fact]
    public void Format_ReturnsReadableSummary()
    {
        var summary = new DigestSummary(
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 7),
            5,
            1,
            new Dictionary<string, int> { { "Awarded", 2 }, { "Mentor Match", 3 } },
            new Dictionary<string, int> { { "Spring 2025", 4 }, { "Fall 2024", 1 } },
            new Dictionary<string, int> { { "active", 4 }, { "paused", 1 } }
        );

        var formatter = new DigestFormatter();
        var output = formatter.Format(summary);

        Assert.Contains("Milestone Digest (2026-02-01 to 2026-02-07)", output);
        Assert.Contains("Total milestones: 5", output);
        Assert.Contains("Risk flags: 1", output);
        Assert.Contains("Milestones by Type", output);
        Assert.Contains("- Awarded: 2", output);
    }
}
