using MilestoneLedger.Core.Models;
using MilestoneLedger.Core.Services;

namespace MilestoneLedger.Tests;

public sealed class AttentionEvaluatorTests
{
    [Fact]
    public void BuildQueue_FlagsRiskAndInactivity()
    {
        var records = new List<AttentionRecord>
        {
            new("Ava Mitchell", "Spring 2025", "active", new DateOnly(2026, 1, 1), 0, null),
            new("Darius Reed", "Spring 2025", "active", new DateOnly(2026, 2, 5), 1, new DateOnly(2026, 2, 5)),
            new("Mei Santos", "Fall 2024", "active", null, 0, null)
        };

        var evaluator = new AttentionEvaluator();
        var queue = evaluator.BuildQueue(records, new DateOnly(2026, 2, 8), 30);

        Assert.Equal(2, queue.Count);
        Assert.Contains(queue, item => item.ScholarName == "Darius Reed" && item.RiskCount == 1);
        Assert.Contains(queue, item => item.ScholarName == "Mei Santos" && item.Reason.Contains("No milestones recorded"));
    }

    [Fact]
    public void BuildQueue_SkipsRecentlyActiveWithoutRisk()
    {
        var records = new List<AttentionRecord>
        {
            new("Priya Patel", "Fall 2024", "active", new DateOnly(2026, 2, 1), 0, null)
        };

        var evaluator = new AttentionEvaluator();
        var queue = evaluator.BuildQueue(records, new DateOnly(2026, 2, 8), 30);

        Assert.Empty(queue);
    }
}
