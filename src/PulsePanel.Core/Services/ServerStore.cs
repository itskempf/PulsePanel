using System;
using System.Collections.Concurrent;
using PulsePanel.Core.Models;
using PulsePanel.Core.Events;
using System.Text.Json;

namespace PulsePanel.Core.Services
{
    public class ServerStore : IServerStore {
        private readonly ConcurrentDictionary<string, ServerStatus> _statuses = new();
        private readonly IEventBus _eventBus;
        private readonly string _filePath;
        private readonly object _lock = new();

        public ServerStore(IEventBus eventBus) {
            _eventBus = eventBus;
            var dataDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.GetFullPath(Path.Combine(dataDir, "servers.json"));
            if (!File.Exists(_filePath)) File.WriteAllText(_filePath, "[]");
        }

        public List<ServerEntry> Load() {
            lock (_lock) {
                var json = File.ReadAllText(_filePath);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<ServerEntry>>(json, opts) ?? new();
            }
        }

        public void Save(List<ServerEntry> servers) {
            lock (_lock) {
                var opts = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_filePath, JsonSerializer.Serialize(servers, opts));
            }
        }

        public void UpdateStatus(string serverId, ServerStatus status)
        {
            var oldStatus = _statuses.GetOrAdd(serverId, ServerStatus.Stopped);
            _statuses[serverId] = status;

            // If the server crashed (was running and now failed), publish a crash event
            if (oldStatus == ServerStatus.Running && status == ServerStatus.Failed)
            {
                _eventBus.Publish(new ServerCrashedEvent(serverId, "Server status changed to Failed", null));
            }
        }
    }
}

