namespace MilestoneLedger.Core.Models;

public sealed record AttentionRecord(
    string ScholarName,
    string? Cohort,
    string? Status,
    DateOnly? LastMilestoneDate,
    int RiskCount,
    DateOnly? LatestRiskDate
);
