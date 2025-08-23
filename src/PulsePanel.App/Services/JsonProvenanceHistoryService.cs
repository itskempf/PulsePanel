using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class JsonProvenanceHistoryService : IProvenanceHistoryService
    {
        private readonly string _root;
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };
        private readonly string _mutexName = "Global\PulsePanel_History_Lock";

        public JsonProvenanceHistoryService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _root = Path.Combine(appData, "PulsePanel", "History");
            Directory.CreateDirectory(_root);
        }

        public IEnumerable<ExecutionSession> GetAllSessions()
        {
            foreach (var file in Directory.EnumerateFiles(_root, "*.session.json").OrderByDescending(f => f))
            {
                ExecutionSession? s = Read(file);
                if (s != null) yield return s;
            }
        }

        public ExecutionSession? GetSession(Guid id)
        {
            var path = Path.Combine(_root, $"{id:N}.session.json");
            return File.Exists(path) ? Read(path) : null;
        }

        public void SaveSession(ExecutionSession session)
        {
            using var mtx = new Mutex(false, _mutexName);
            try
            {
                mtx.WaitOne();
                var path = Path.Combine(_root, $"{session.Id:N}.session.json");
                var tmp = path + ".tmp";
                using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(fs, session, _json);
                }
                if (File.Exists(path)) File.Replace(tmp, path, null);
                else File.Move(tmp, path);
            }
            finally
            {
                try { mtx.ReleaseMutex(); } catch { /* ignored */ }
            }
        }

        private ExecutionSession? Read(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return JsonSerializer.Deserialize<ExecutionSession>(fs, _json);
            }
            catch { return null; }
        }
    }
}