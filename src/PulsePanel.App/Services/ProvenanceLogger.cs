using System;
using System.Text.Json;
using System.IO;

namespace PulsePanel.Services
{
    public static class ProvenanceLogger
    {
        public static void LogAction(string action, string blueprintId, string details)
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
        }
    }
}