using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public class BlueprintLoader
    {
        private readonly string _blueprintDir;

        public BlueprintLoader(string blueprintDir)
        {
            _blueprintDir = blueprintDir;
        }

        public IEnumerable<Blueprint> LoadAll()
        {
            if (!Directory.Exists(_blueprintDir))
                throw new DirectoryNotFoundException($"Blueprint directory not found: {_blueprintDir}");

            var blueprints = new List<Blueprint>();

            foreach (var file in Directory.GetFiles(_blueprintDir, "*.json"))
            {
                var bp = LoadFromFile(file);
                Validate(bp);
                blueprints.Add(bp);
            }

            return blueprints;
        }

        public Blueprint LoadFromFile(string path)
        {
            var json = File.ReadAllText(path);
            var bp = JsonSerializer.Deserialize<Blueprint>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            if (bp == null)
                throw new InvalidDataException($"Failed to parse blueprint: {path}");

            return bp;
        }

        private void Validate(Blueprint bp)
        {
            if (string.IsNullOrWhiteSpace(bp.Id))
                throw new InvalidDataException("Blueprint missing Id");
            if (string.IsNullOrWhiteSpace(bp.Name))
                throw new InvalidDataException("Blueprint missing Name");
            if (string.IsNullOrWhiteSpace(bp.Version))
                throw new InvalidDataException("Blueprint missing Version");
            if (bp.InstallSteps.Count == 0 && bp.UpdateSteps.Count == 0 && bp.ValidateSteps.Count == 0)
                throw new InvalidDataException($"Blueprint '{bp.Name}' has no execution steps");
        }
    }
}