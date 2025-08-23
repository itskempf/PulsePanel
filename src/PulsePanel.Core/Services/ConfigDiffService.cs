using System;
using System.IO;
using System.Linq;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace PulsePanel.Core.Services
{
    public class ConfigDiffService : IConfigDiffService
    {
        private readonly IProvenanceLogger _logger;

        public ConfigDiffService(IProvenanceLogger logger)
        {
            _logger = logger;
        }

        public DiffResult DiffWithBlueprintAsync(string configPath, string blueprintPath)
        {
            var configText = File.ReadAllText(configPath);
            var blueprintText = File.ReadAllText(blueprintPath);

            var diffBuilder = new InlineDiffBuilder(new Differ());
            var diff = diffBuilder.BuildDiffModel(blueprintText, configText);

            var changes = diff.Lines.Count(l => l.Type != ChangeType.Unchanged);

            _logger.Log(new ProvenanceEvent
            {
                Action = "ConfigDiffed",
                Timestamp = DateTime.UtcNow,
                Metadata = new
                {
                    Config = configPath,
                    Blueprint = blueprintPath,
                    Changes = changes
                }
            });

            return new DiffResult
            {
                HasChanges = changes > 0,
                DiffLines = diff.Lines
            };
        }
    }
}
