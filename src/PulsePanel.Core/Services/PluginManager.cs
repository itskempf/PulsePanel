using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using PulsePanel.Core.Services;
using PulsePanel.Core.Models;
using PulsePanel.Blueprints.Provenance;

namespace PulsePanel.Core.Services
{
    public class PluginManager
    {
        private readonly IProvenanceLogger _logger;
        private readonly string _pluginsRootPath;

        public PluginManager(IProvenanceLogger logger, string pluginsRootPath)
        {
            _logger = logger;
            _pluginsRootPath = pluginsRootPath;
        }

        public List<Plugin> LoadPlugins()
        {
            var plugins = new List<Plugin>();
            if (!Directory.Exists(_pluginsRootPath))
            {
                _logger.Log(new LogEntry { Action = "plugin-load", Results = new ResultsInfo { Status = "fail", Errors = { new ResultDetail { Message = $"Plugins root directory not found: {_pluginsRootPath}" } } } });
                return plugins;
            }

            foreach (var pluginDir in Directory.GetDirectories(_pluginsRootPath))
            {
                var pluginName = new DirectoryInfo(pluginDir).Name;
                var manifestPath = Path.Combine(pluginDir, "plugin.json");
                var pluginDllPath = Path.Combine(pluginDir, $"{pluginName}.dll");

                if (!File.Exists(manifestPath))
                {
                    _logger.Log(new LogEntry { Action = "plugin-load", Results = new ResultsInfo { Status = "fail", Errors = { new ResultDetail { Message = $"Plugin '{pluginName}': manifest.json not found." } } } });
                    continue;
                }

                try
                {
                    var manifestContent = File.ReadAllText(manifestPath);
                    var plugin = JsonSerializer.Deserialize<Plugin>(manifestContent);

                    if (plugin == null || string.IsNullOrWhiteSpace(plugin.Name) || string.IsNullOrWhiteSpace(plugin.Version) || string.IsNullOrWhiteSpace(plugin.License) || string.IsNullOrWhiteSpace(plugin.Sha256Hash))
                    {
                        _logger.Log(new LogEntry { Action = "plugin-load", Results = new ResultsInfo { Status = "fail", Errors = { new ResultDetail { Message = $"Plugin '{pluginName}': Invalid or incomplete manifest.json." } } } });
                        continue;
                    }

                    // Validate DLL hash
                    if (!File.Exists(pluginDllPath))
                    {
                        _logger.Log(new LogEntry { Action = "plugin-load", Results = new ResultsInfo { Status = "fail", Errors = { new ResultDetail { Message = $"Plugin '{pluginName}': Plugin DLL not found." } } } });
                        continue;
                    }

                    var actualHash = ComputeSha256(pluginDllPath);
                    if (!actualHash.Equals(plugin.Sha256Hash, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Log(new LogEntry { Action = "plugin-load", Results = new ResultsInfo { Status = "fail", Errors = { new ResultDetail { Message = $"Plugin '{pluginName}': DLL hash mismatch. Expected '{plugin.Sha256Hash}', got '{actualHash}'." } } } });
                        continue;
                    }

                    // Check license compatibility (simplified for now)
                    if (plugin.License.Contains("GPL", StringComparison.OrdinalIgnoreCase) || plugin.License.Contains("Commercial", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Log(new LogEntry { Action = "plugin-load", Results = new ResultsInfo { Status = "fail", Errors = { new ResultDetail { Message = $"Plugin '{pluginName}': Incompatible license '{plugin.License}'." } } } });
                        continue;
                    }

                    plugins.Add(plugin);
                    _logger.Log(new LogEntry { Action = "plugin-load", Results = new ResultsInfo { Status = "pass" }, Blueprint = new BlueprintInfo { Name = plugin.Name, Version = plugin.Version } });
                }
                catch (Exception ex)
                {
                    _logger.Log(new LogEntry { Action = "plugin-load", Results = new ResultsInfo { Status = "fail", Errors = { new ResultDetail { Message = $"Plugin '{pluginName}': Error loading: {ex.Message}" } } } });
                }
            }
            return plugins;
        }

        private string ComputeSha256(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public class Plugin
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public string Sha256Hash { get; set; } = string.Empty;
    }
}
