using System;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class ProvenanceLogService : IProvenanceLogService
    {
        public event EventHandler<LogEntry>? LogEntryAdded;
        public void Write(LogEntry entry) => LogEntryAdded?.Invoke(this, entry);
    }
}