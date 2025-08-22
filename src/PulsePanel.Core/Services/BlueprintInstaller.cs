using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PulsePanel.Core.Models;
using PulsePanel.Blueprints.Provenance;

namespace PulsePanel.Core.Services;

public class BlueprintInstaller
{
    private readonly ProvenanceLogger _provenanceLogger;
    private readonly PnccLChecker _pnccLChecker;
    private readonly ServerProcessService _serverProcessService; // To get server directories

    public BlueprintInstaller(ProvenanceLogger provenanceLogger, PnccLChecker pnccLChecker, ServerProcessService serverProcessService)
    {
        _provenanceLogger = provenanceLogger;
        _pnccLChecker = pnccLChecker;
        _serverProcessService = serverProcessService;
    }

    public async Task<bool> InstallBlueprint(Blueprint blueprint, ServerEntry serverEntry, Dictionary<string, string> userInputs)
    {
        _provenanceLogger.Log(new LogEntry
        {
            Action = "Blueprint_Install_Attempt",
            EntityType = "Blueprint",
            EntityIdentifier = blueprint.Id,
            Metadata = new Dictionary<string, object>
            {
                { "blueprintName", blueprint.Name },
                { "blueprintVersion", blueprint.Version },
                { "serverName", serverEntry.Name },
                { "userInputs", userInputs }
            }
        });

        // 1. Perform PNCCL check on the blueprint itself
        var blueprintCompliance = await _pnccLChecker.CheckBlueprint(blueprint);
        if (!blueprintCompliance.IsValid)
        {
            _provenanceLogger.Log(new LogEntry
            {
                Action = "Blueprint_Install_Failed",
                EntityType = "Blueprint",
                EntityIdentifier = blueprint.Id,
                Metadata = new Dictionary<string, object>
                {
                    { "blueprintName", blueprint.Name },
                    { "serverName", serverEntry.Name },
                    { "reason", "Blueprint failed PNCCL compliance check" },
                    { "complianceFindings", blueprintCompliance.Findings }
                }
            });
            return false;
        }

        try
        {
            _serverProcessService.EnsureLayout(serverEntry.Id); // Ensure server directories exist
            var configDir = _serverProcessService.ConfigDir(serverEntry.Id);

            // 2. Simulate template processing and file generation
            foreach (var template in blueprint.Templates)
            {
                var processedContent = template.Content; // Dummy: no actual token replacement yet
                foreach (var input in userInputs)
                {
                    processedContent = processedContent.Replace($"{{{{{input.Key}}}}}", input.Value); // Simple token replacement
                }

                var targetPath = Path.Combine(configDir, template.Target);
                await File.WriteAllTextAsync(targetPath, processedContent);

                // 3. Perform PNCCL check on generated config file
                var configFileCompliance = await _pnccLChecker.CheckConfigFile(targetPath, Path.GetExtension(targetPath));
                if (!configFileCompliance.IsValid)
                {
                    _provenanceLogger.Log(new LogEntry
                    {
                        Action = "Blueprint_Install_Failed",
                        EntityType = "ConfigFile",
                        EntityIdentifier = targetPath,
                        Metadata = new Dictionary<string, object>
                        {
                            { "blueprintName", blueprint.Name },
                            { "serverName", serverEntry.Name },
                            { "reason", "Generated config file failed PNCCL compliance check" },
                            { "complianceFindings", configFileCompliance.Findings }
                        }
                    });
                    // Decide whether to halt installation or just log a warning
                    // For now, we'll halt if any generated config is invalid.
                    return false;
                }
            }

            // 4. Simulate game installation (delegated)
            // In a real scenario, this would call a SteamCmdManager or similar service
            await Task.Delay(500); // Simulate game installation time
            Console.WriteLine($"Simulating game installation for {blueprint.GameDefinition.Label}");

            _provenanceLogger.Log(new LogEntry
            {
                Action = "Blueprint_Installed",
                EntityType = "Blueprint",
                EntityIdentifier = blueprint.Id,
                Metadata = new Dictionary<string, object>
                {
                    { "blueprintName", blueprint.Name },
                    { "blueprintVersion", blueprint.Version },
                    { "serverName", serverEntry.Name },
                    { "userInputs", userInputs }
                }
            });

            return true;
        }
        catch (Exception ex)
        {
            _provenanceLogger.Log(new LogEntry
            {
                Action = "Blueprint_Install_Exception",
                EntityType = "Blueprint",
                EntityIdentifier = blueprint.Id,
                Metadata = new Dictionary<string, object>
                {
                    { "blueprintName", blueprint.Name },
                    { "serverName", serverEntry.Name },
                    { "exception", ex.Message }
                }
            });
            return false;
        }
    }
}
