using System;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface IProvenanceLogService
    {
        event EventHandler<LogEntry> LogEntryAdded;
        void Write(LogEntry entry);
    }
}