using System;
using System.Collections.Generic;

namespace PulsePanel.Core.Instances
{
    // PulsePanel.Core/Instances/InstanceManager.cs
    // Purpose: Manage lifecycle of game server instances (create, start, stop, delete)
    // Provenance: Inspired by public concepts from Pterodactyl (node/allocation) and AMP (modular instances)
    // Original Implementation: Windows-native, blueprint-driven, provenance-logged

    public class InstanceManager
    {
        private readonly IBlueprintLoader _blueprintLoader;
        private readonly IProvenanceLogger _logger;
        private readonly Dictionary<Guid, GameInstance> _instances = new();

        public InstanceManager(IBlueprintLoader blueprintLoader, IProvenanceLogger logger)
        {
            _blueprintLoader = blueprintLoader;
            _logger = logger;
        }

        public GameInstance CreateInstance(string blueprintId, InstanceConfig config)
        {
            var blueprint = _blueprintLoader.Load(blueprintId);
            var instance = new GameInstance(config, blueprint);
            _instances[instance.Id] = instance;

            _logger.Log("instance.create", new { instance.Id, blueprintId, config });
            return instance;
        }

        public void StartInstance(Guid id)
        {
            if (_instances.TryGetValue(id, out var instance))
            {
                instance.Start();
                _logger.Log("instance.start", new { id });
            }
        }

        public void StopInstance(Guid id)
        {
            if (_instances.TryGetValue(id, out var instance))
            {
                instance.Stop();
                _logger.Log("instance.stop", new { id });
            }
        }

        public void DeleteInstance(Guid id)
        {
            if (_instances.Remove(id))
            {
                _logger.Log("instance.delete", new { id });
            }
        }
    }
}
