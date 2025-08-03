namespace MilestoneLedger.Core.Models;

public sealed record DigestSummary(
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalMilestones,
    int RiskFlags,
    IReadOnlyDictionary<string, int> MilestonesByType,
    IReadOnlyDictionary<string, int> MilestonesByCohort,
    IReadOnlyDictionary<string, int> StatusCounts
);
