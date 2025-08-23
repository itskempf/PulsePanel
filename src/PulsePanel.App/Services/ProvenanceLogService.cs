using System;
using PulsePanel.App.Models; // Added

namespace PulsePanel.App.Services
{
    public interface IProvenanceLogService
    {
        event EventHandler<LogEntry> LogEntryAdded; // Changed to LogEntry
        void Write(LogEntry entry); // Changed to LogEntry
    }

    public class ProvenanceLogService : IProvenanceLogService
    {
        public event EventHandler<LogEntry>? LogEntryAdded; // Changed to LogEntry

        public void Write(LogEntry entry) // Changed to LogEntry
        {
            // Existing file logging here (if any, not shown in prompt)
            LogEntryAdded?.Invoke(this, entry);
        }
    }
}