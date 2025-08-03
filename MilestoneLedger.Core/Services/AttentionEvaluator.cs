using MilestoneLedger.Core.Models;

namespace MilestoneLedger.Core.Services;

public sealed class AttentionEvaluator
{
    public IReadOnlyList<AttentionItem> BuildQueue(
        IReadOnlyList<AttentionRecord> records,
        DateOnly asOf,
        int inactiveDays)
    {
        var items = new List<AttentionItem>();

        foreach (var record in records)
        {
            var reasons = new List<string>();
            int? daysSince = null;

            if (record.LastMilestoneDate is null)
            {
                reasons.Add("No milestones recorded");
            }
            else
            {
                var delta = asOf.DayNumber - record.LastMilestoneDate.Value.DayNumber;
                daysSince = Math.Max(0, delta);
                if (daysSince > inactiveDays)
                {
                    reasons.Add($"No milestones in {daysSince} days");
                }
            }

            if (record.RiskCount > 0)
            {
                if (record.LatestRiskDate is not null)
                {
                    reasons.Add($"Risk flagged on {record.LatestRiskDate:yyyy-MM-dd}");
                }
                else
                {
                    reasons.Add("Risk flagged milestone");
                }
            }

            if (reasons.Count == 0)
            {
                continue;
            }

            items.Add(new AttentionItem(
                record.ScholarName,
                record.Cohort,
                record.Status,
                record.LastMilestoneDate,
                daysSince,
                record.RiskCount,
                record.LatestRiskDate,
                string.Join("; ", reasons)));
        }

        return items
            .OrderByDescending(item => item.RiskCount > 0)
            .ThenByDescending(item => item.DaysSinceMilestone ?? int.MaxValue)
            .ThenBy(item => item.ScholarName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
