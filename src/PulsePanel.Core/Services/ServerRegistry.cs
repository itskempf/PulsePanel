using System.Text.Json;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public class ServerRegistry {
    private readonly string _path;
    private readonly Lazy<List<GameDefinition>> _games;

    public ServerRegistry() {
        _path = Path.Combine(AppContext.BaseDirectory, "server_registry.json");
        _games = new Lazy<List<GameDefinition>>(() => {
            var json = File.ReadAllText(_path);
            using var doc = JsonDocument.Parse(json);
            var gamesEl = doc.RootElement.GetProperty("games");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<GameDefinition>>(gamesEl.GetRawText(), opts) ?? new();
        });
    }

    public IReadOnlyList<GameDefinition> Games => _games.Value;

    public GameDefinition? Get(string id) => Games.FirstOrDefault(g => g.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}

