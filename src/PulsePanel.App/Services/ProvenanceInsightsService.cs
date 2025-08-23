using System;
using System.Collections.Generic;
using System.Linq;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public class ProvenanceInsightsService : IProvenanceInsightsService
    {
        private readonly IProvenanceHistoryService _historyService;

        public ProvenanceInsightsService(IProvenanceHistoryService historyService)
        {
            _historyService = historyService;
        }

        public ExecutionSummary GetSummary(ExecutionSession session)
        {
            var infoCount = session.Entries.Count(e => e.Severity == LogSeverity.Info);
            var warningCount = session.WarningCount;
            var errorCount = session.ErrorCount;
            var isSuccess = session.Outcome == "Success";

            var mostCommonWarning = session.Entries
                .Where(e => e.Severity == LogSeverity.Warning)
                .GroupBy(e => e.Message)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            var mostCommonError = session.Entries
                .Where(e => e.Severity == LogSeverity.Error)
                .GroupBy(e => e.Message)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            return new ExecutionSummary(
                isSuccess,
                session.Duration,
                infoCount,
                warningCount,
                errorCount,
                mostCommonWarning,
                mostCommonError
            );
        }

        public IEnumerable<BlueprintStats> GetBlueprintPerformanceStats()
        {
            var allSessions = _historyService.GetAllSessions();

            return allSessions.GroupBy(s => s.BlueprintName)
                .Select(g =>
                {
                    var totalRuns = g.Count();
                    var totalDurationSeconds = g.Sum(s => s.Duration.TotalSeconds);
                    var failedRuns = g.Count(s => s.Outcome == "Failed");

                    return new BlueprintStats(
                        g.Key,
                        totalRuns > 0 ? totalDurationSeconds / totalRuns : 0,
                        totalRuns > 0 ? (double)failedRuns / totalRuns : 0,
                        totalRuns
                    );
                })
                .ToList();
        }
    }
}