using System;

namespace PulsePanel.App.Services
{
    public interface IProvenanceLogService
    {
        event EventHandler<string> LogEntryAdded;
        void Write(string message);
    }

    public class ProvenanceLogService : IProvenanceLogService
    {
        public event EventHandler<string>? LogEntryAdded;

        public void Write(string message)
        {
            // Existing file logging here
            LogEntryAdded?.Invoke(this, message);
        }
    }
}