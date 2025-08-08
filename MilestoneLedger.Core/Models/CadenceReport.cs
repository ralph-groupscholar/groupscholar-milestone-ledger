namespace MilestoneLedger.Core.Models;

public sealed record CadenceReport(
    DateOnly StartDate,
    DateOnly EndDate,
    string? Cohort,
    string? Status,
    IReadOnlyList<CadenceWeekSummary> Weeks,
    int TotalMilestones,
    int RiskFlags);
