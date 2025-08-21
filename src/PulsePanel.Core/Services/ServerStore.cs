using System.Text.Json;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public class ServerStore {
    private readonly string _filePath;
    private readonly object _lock = new();

    public ServerStore() {
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
}

