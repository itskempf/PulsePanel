using System;
using System.Text.Json;
using System.IO;
using PulsePanel.App.Models; // Added

namespace PulsePanel.App.Services
{
    public interface IProvenanceLogger
    {
        void LogAction(string action, string blueprintId, string details);
        void LogError(string action, string blueprintId, string details);
    }

    public class ProvenanceLogger : IProvenanceLogger
    {
        private readonly IProvenanceLogService _logService;

        public ProvenanceLogger(IProvenanceLogService logService)
        {
            _logService = logService;
        }

        public void LogAction(string action, string blueprintId, string details)
        {
            var logMessage = $"ACTION: {action} | BP: {blueprintId} | Details: {details}";
            var entry = new
            {
                Timestamp = DateTime.UtcNow,
                Action = action,
                BlueprintId = blueprintId,
                Details = details,
                Actor = Environment.UserName
            };

            var json = JsonSerializer.Serialize(entry);
            File.AppendAllText("provenance.log", json + Environment.NewLine);
            _logService.Write(new LogEntry(DateTime.UtcNow, logMessage, LogSeverity.Info)); // Write to UI log with LogEntry
        }

        public void LogError(string action, string blueprintId, string details)
        {
            var logMessage = $"ERROR: {action} | BP: {blueprintId} | Details: {details}";
            var entry = new
            {
                Timestamp = DateTime.UtcNow,
                Action = action,
                BlueprintId = blueprintId,
                Details = details,
                Actor = Environment.UserName
            };

            var json = JsonSerializer.Serialize(entry);
            File.AppendAllText("provenance.log", json + Environment.NewLine);
            _logService.Write(new LogEntry(DateTime.UtcNow, logMessage, LogSeverity.Error)); // Write to UI log with LogEntry
        }
    }
}