using MilestoneLedger.Core.Models;
using System.Text;

namespace MilestoneLedger.Core.Services;

public sealed class CadenceFormatter
{
    public string Format(CadenceReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Milestone Cadence ({report.StartDate:yyyy-MM-dd} to {report.EndDate:yyyy-MM-dd})");
        builder.AppendLine($"Total milestones: {report.TotalMilestones}");
        builder.AppendLine($"Risk flags: {report.RiskFlags}");

        if (!string.IsNullOrWhiteSpace(report.Cohort) || !string.IsNullOrWhiteSpace(report.Status))
        {
            builder.AppendLine($"Filter: {FormatFilter(report.Cohort, report.Status)}");
        }

        builder.AppendLine();
        builder.AppendLine("Week Start | Milestones | Risk Flags");
        builder.AppendLine("---------- | ---------- | ---------");

        if (report.Weeks.Count == 0)
        {
            builder.AppendLine("(no data)");
            return builder.ToString().TrimEnd();
        }

        foreach (var week in report.Weeks)
        {
            builder.AppendLine($"{week.WeekStart:yyyy-MM-dd} | {week.TotalMilestones,10} | {week.RiskFlags,9}");
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatFilter(string? cohort, string? status)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(cohort))
        {
            parts.Add($"cohort={cohort}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            parts.Add($"status={status}");
        }

        return string.Join(", ", parts);
    }
}
