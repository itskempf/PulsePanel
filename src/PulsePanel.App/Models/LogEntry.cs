using System;

namespace PulsePanel.App.Models
{
    public enum LogSeverity { Info, Warning, Error }

    public record LogEntry(DateTime Timestamp, string Message, LogSeverity Severity);
}