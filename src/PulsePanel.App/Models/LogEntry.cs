using System;

namespace PulsePanel.App.Models
{
    public enum LogSeverity { Info, Warning, Error }

    public sealed class LogEntry
    {
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string Message { get; init; } = "";
        public LogSeverity Severity { get; init; } = LogSeverity.Info;
        public Guid? SessionId { get; init; }
    }
}