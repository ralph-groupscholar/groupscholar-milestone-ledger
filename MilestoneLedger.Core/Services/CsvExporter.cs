using MilestoneLedger.Core.Models;
using System.Text;

namespace MilestoneLedger.Core.Services;

public sealed class CsvExporter
{
    public string Export(IEnumerable<MilestoneEntry> entries)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Scholar,Cohort,Status,MilestoneType,MilestoneDate,RiskFlag,Notes");

        foreach (var entry in entries)
        {
            builder.AppendLine(string.Join(",",
                Escape(entry.ScholarName),
                Escape(entry.Cohort),
                Escape(entry.Status),
                Escape(entry.MilestoneType),
                entry.MilestoneDate.ToString("yyyy-MM-dd"),
                entry.RiskFlag ? "true" : "false",
                Escape(entry.Notes)
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
