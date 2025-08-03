namespace MilestoneLedger.Core.Models;

public sealed record MilestoneEntry(
    int Id,
    string ScholarName,
    string? Cohort,
    string? Status,
    string MilestoneType,
    DateOnly MilestoneDate,
    string? Notes,
    bool RiskFlag,
    DateTimeOffset CreatedAt
);
