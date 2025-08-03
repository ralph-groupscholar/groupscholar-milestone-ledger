namespace MilestoneLedger.Core.Models;

public sealed record Milestone(
    int Id,
    int ScholarId,
    string MilestoneType,
    DateOnly MilestoneDate,
    string? Notes,
    bool RiskFlag,
    DateTimeOffset CreatedAt
);
