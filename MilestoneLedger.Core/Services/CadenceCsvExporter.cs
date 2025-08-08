using MilestoneLedger.Core.Models;
using System.Text;

namespace MilestoneLedger.Core.Services;

public sealed class CadenceCsvExporter
{
    public string Export(CadenceReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("week_start,total_milestones,risk_flags");

        foreach (var week in report.Weeks)
        {
            builder.AppendLine($"{week.WeekStart:yyyy-MM-dd},{week.TotalMilestones},{week.RiskFlags}");
        }

        return builder.ToString().TrimEnd();
    }
}
