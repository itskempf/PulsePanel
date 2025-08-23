using System.Collections.Generic;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface IProvenanceInsightsService
    {
        ExecutionSummary GetSummary(ExecutionSession session);
        IEnumerable<BlueprintStats> GetBlueprintPerformanceStats();
    }

    public record ExecutionSummary(
        bool IsSuccess,
        TimeSpan Duration,
        int InfoCount,
        int WarningCount,
        int ErrorCount,
        string MostCommonWarning,
        string MostCommonError
    );

    public record BlueprintStats(
        string BlueprintName,
        double AverageDurationSeconds,
        double FailureRate,
        int TotalRuns
    );
}