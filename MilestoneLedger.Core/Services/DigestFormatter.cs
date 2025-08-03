using MilestoneLedger.Core.Models;
using System.Text;

namespace MilestoneLedger.Core.Services;

public sealed class DigestFormatter
{
    public string Format(DigestSummary summary)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Milestone Digest ({summary.StartDate:yyyy-MM-dd} to {summary.EndDate:yyyy-MM-dd})");
        builder.AppendLine($"Total milestones: {summary.TotalMilestones}");
        builder.AppendLine($"Risk flags: {summary.RiskFlags}");
        builder.AppendLine();

        AppendSection(builder, "Milestones by Type", summary.MilestonesByType);
        AppendSection(builder, "Milestones by Cohort", summary.MilestonesByCohort);
        AppendSection(builder, "Scholar Status", summary.StatusCounts);

        return builder.ToString().TrimEnd();
    }

    private static void AppendSection(StringBuilder builder, string title, IReadOnlyDictionary<string, int> data)
    {
        builder.AppendLine(title);
        if (data.Count == 0)
        {
            builder.AppendLine("  (no data)");
            builder.AppendLine();
            return;
        }

        foreach (var entry in data)
        {
            builder.AppendLine($"  - {entry.Key}: {entry.Value}");
        }

        builder.AppendLine();
    }
}
