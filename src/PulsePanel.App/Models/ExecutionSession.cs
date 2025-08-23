using System;
using System.Collections.Generic;

namespace PulsePanel.App.Models
{
    public enum ExecutionActionType { Install, Update, Validate }

    public sealed class ExecutionSession
    {
        public Guid Id { get; init; }
        public DateTime StartedAt { get; init; }
        public DateTime? EndedAt { get; set; }
        public string BlueprintName { get; init; } = "";
        public string BlueprintVersion { get; init; } = "";
        public string TriggeredBy { get; init; } = "";
        public string Outcome { get; set; } = "Running";
        public ExecutionActionType Action { get; init; }
        public bool DryRun { get; init; }
        public string? TargetNodeId { get; init; }
        public string? Error { get; set; }
        public List<LogEntry> Entries { get; } = new();

        public TimeSpan? Duration => EndedAt is null ? null : EndedAt - StartedAt;
        public int WarningCount => Entries.FindAll(e => e.Severity == LogSeverity.Warning).Count;
        public int ErrorCount => Entries.FindAll(e => e.Severity == LogSeverity.Error).Count;
    }
}