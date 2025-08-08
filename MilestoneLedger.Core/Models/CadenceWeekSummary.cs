namespace MilestoneLedger.Core.Models;

public sealed record CadenceWeekSummary(DateOnly WeekStart, int TotalMilestones, int RiskFlags);
