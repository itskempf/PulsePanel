using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class ProvenanceLogger : IProvenanceLogger
    {
        private readonly IProvenanceLogService _bus;
        private readonly string _logDir;

        public ProvenanceLogger(IProvenanceLogService bus)
        {
            _bus = bus;
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _logDir = Path.Combine(appData, "PulsePanel", "Logs");
            Directory.CreateDirectory(_logDir);
        }

        public Task InfoAsync(string message, Guid? sessionId = null) => WriteAsync(message, LogSeverity.Info, sessionId);
        public Task WarningAsync(string message, Guid? sessionId = null) => WriteAsync(message, LogSeverity.Warning, sessionId);
        public Task ErrorAsync(string message, Guid? sessionId = null) => WriteAsync(message, LogSeverity.Error, sessionId);

        private Task WriteAsync(string message, LogSeverity sev, Guid? sessionId)
        {
            var entry = new LogEntry { Timestamp = DateTime.UtcNow, Message = message, Severity = sev, SessionId = sessionId };
            // File append
            var line = $"{entry.Timestamp:o} [{sev}] {message}{(sessionId is null ? "" : $" (session:{sessionId:N})")}";
            var path = Path.Combine(_logDir, $"{DateTime.UtcNow:yyyyMMdd}.log");
            File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
            // UI stream
            _bus.Write(entry);
            return Task.CompletedTask;
        }
    }
}