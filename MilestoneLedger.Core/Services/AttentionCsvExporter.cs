using MilestoneLedger.Core.Models;
using System.Text;

namespace MilestoneLedger.Core.Services;

public sealed class AttentionCsvExporter
{
    public string Export(IEnumerable<AttentionItem> items)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Scholar,Cohort,Status,LastMilestoneDate,DaysSinceMilestone,RiskCount,LatestRiskDate,Reasons");

        foreach (var item in items)
        {
            builder.AppendLine(string.Join(",",
                Escape(item.ScholarName),
                Escape(item.Cohort),
                Escape(item.Status),
                item.LastMilestoneDate?.ToString("yyyy-MM-dd") ?? "",
                item.DaysSinceMilestone?.ToString() ?? "",
                item.RiskCount.ToString(),
                item.LatestRiskDate?.ToString("yyyy-MM-dd") ?? "",
                Escape(item.Reasons)
            ));
        }

        return builder.ToString().TrimEnd();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n');
        if (!needsQuotes)
        {
            return value;
        }

        return '"' + value.Replace("\"", "\"\"") + '"';
    }
}
