using System;
using System.Text.Json;
using System.IO;

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
            _logService.Write($"ACTION: {action} | BP: {blueprintId} | Details: {details}"); // Write to UI log
        }

        public void LogError(string action, string blueprintId, string details)
        {
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
            _logService.Write($"ERROR: {action} | BP: {blueprintId} | Details: {details}"); // Write to UI log
        }
    }
}