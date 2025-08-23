using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public class ProvenanceHistoryService : IProvenanceHistoryService
    {
        private readonly string _historyFilePath;
        private readonly object _lock = new();

        public ProvenanceHistoryService()
        {
            _historyFilePath = Path.Combine(AppContext.BaseDirectory, "provenance_history.json");
            if (!File.Exists(_historyFilePath))
            {
                File.WriteAllText(_historyFilePath, "[]"); // Initialize with empty array
            }
        }

        public void SaveSession(ExecutionSession session)
        {
            lock (_lock)
            {
                var sessions = GetAllSessions().ToList();
                sessions.Add(session);
                var json = JsonSerializer.Serialize(sessions, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_historyFilePath, json);
            }
        }

        public IEnumerable<ExecutionSession> GetAllSessions()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_historyFilePath);
                return JsonSerializer.Deserialize<List<ExecutionSession>>(json) ?? new List<ExecutionSession>();
            }
        }

        public ExecutionSession? GetSession(Guid id)
        {
            return GetAllSessions().FirstOrDefault(s => s.Id == id);
        }
    }
}