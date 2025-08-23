using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class BlueprintLoader
    {
        private readonly string _root;

        public BlueprintLoader(string? root = null)
        {
            _root = root ?? Path.Combine(AppContext.BaseDirectory, "Assets", "Blueprints");
            Directory.CreateDirectory(_root);
        }

        public IEnumerable<Blueprint> LoadAll()
            => Directory.EnumerateFiles(_root, "*.blueprint.json").Select(Load);

        public Blueprint Load(string path)
        {
            var json = File.ReadAllText(path);
            var bp = JsonSerializer.Deserialize<Blueprint>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new Blueprint();
            var sidecar = Path.ChangeExtension(path, ".compliance.json");
            if (File.Exists(sidecar))
            {
                var rules = JsonSerializer.Deserialize<List<ComplianceRule>>(File.ReadAllText(sidecar), new JsonSerializerOptions(JsonSerializerDefaults.Web));
                bp.ComplianceRules = rules ?? new List<ComplianceRule>();
            }
            return bp;
        }
    }
}