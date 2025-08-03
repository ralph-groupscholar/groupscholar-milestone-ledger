namespace MilestoneLedger.Core.Models;

public sealed record AttentionItem(
    string ScholarName,
    string? Cohort,
    string? Status,
    DateOnly? LastMilestoneDate,
    int? DaysSinceMilestone,
    int RiskCount,
    DateOnly? LatestRiskDate,
    string Reason
);
