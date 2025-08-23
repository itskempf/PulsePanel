using System;
using System.Collections.Generic;
using System.Linq; // Added

namespace PulsePanel.App.Models
{
    public record ExecutionSession(
        Guid Id,
        DateTime StartedAt,
        string BlueprintName,
        string BlueprintVersion,
        string TriggeredBy,
        string Outcome,
        List<LogEntry> Entries
    )
    {
        public TimeSpan Duration => Entries.Any() ? Entries.Last().Timestamp - Entries.First().Timestamp : TimeSpan.Zero;
        public int WarningCount => Entries.Count(e => e.Severity == LogSeverity.Warning);
        public int ErrorCount => Entries.Count(e => e.Severity == LogSeverity.Error);
    }
}