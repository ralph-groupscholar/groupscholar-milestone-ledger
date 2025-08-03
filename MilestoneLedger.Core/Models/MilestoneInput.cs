namespace MilestoneLedger.Core.Models;

public sealed record MilestoneInput(
    string FullName,
    string? Cohort,
    string? Status,
    string MilestoneType,
    DateOnly MilestoneDate,
    string? Notes,
    bool RiskFlag
);
