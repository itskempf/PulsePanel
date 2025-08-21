using PulsePanel.Blueprints;
using PulsePanel.Core.Services;
using System.Collections.Generic;

namespace PulsePanel.Cli
{
    // Minimal local stubs to satisfy references in the CLI during refactor branch
    internal class StorageManager
    {
        private readonly IProvenanceLogger _logger;
        private readonly Dictionary<string, string> _paths = new();
        public StorageManager(IProvenanceLogger logger) => _logger = logger;
        public void SetStoragePath(string key, string path) => _paths[key] = path;
        public string? GetStoragePath(string key) => _paths.TryGetValue(key, out var v) ? v : null;
    }

    internal class SteamCmdManager
    {
        private readonly IProvenanceLogger _logger;
        public SteamCmdManager(IProvenanceLogger logger) => _logger = logger;
        public void SetupSteamCmd(string path) { /* no-op stub */ }
        public bool VerifySteamCmd(string path) => true;
    }

    internal class FirewallManager
    {
        private readonly IProvenanceLogger _logger;
        public FirewallManager(IProvenanceLogger logger) => _logger = logger;
        public void AddFirewallRule(string name, string protocol, string port) { }
        public void RemoveFirewallRule(string name) { }
    }

    internal class WindowsServiceManager
    {
        private readonly IProvenanceLogger _logger;
        public WindowsServiceManager(IProvenanceLogger logger) => _logger = logger;
        public void InstallService() { }
        public void RemoveService() { }
        public void StartService() { }
        public void StopService() { }
    }
}
