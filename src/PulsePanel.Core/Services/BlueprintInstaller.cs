using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PulsePanel.Core.Models;
using PulsePanel.Core.Events;

namespace PulsePanel.Core.Services
{
    public class BlueprintInstaller
    {
        private readonly IProvenanceLogger _provenanceLogger;
        private readonly PnccLChecker _pnccLChecker;
        private readonly ServerProcessService _serverProcessService;
        private readonly IEventBus _eventBus;

        public BlueprintInstaller(
            IProvenanceLogger provenanceLogger,
            PnccLChecker pnccLChecker,
            ServerProcessService serverProcessService,
            IEventBus eventBus)
        {
            _provenanceLogger = provenanceLogger;
            _pnccLChecker = pnccLChecker;
            _serverProcessService = serverProcessService;
            _eventBus = eventBus;
        }

        public async Task<bool> InstallBlueprint(Blueprint blueprint, ServerEntry serverEntry, Dictionary<string, string> userInputs)
        {
            var correlationId = Guid.NewGuid().ToString("n");
            
            // Publish install started event
            var startedEvent = new BlueprintInstallStartedEvent(
                blueprint.Id,
                blueprint.Name,
                serverEntry.Id,
                serverEntry.Name,
                correlationId
            );
            _eventBus.Publish(startedEvent);

            var blueprintCompliance = await _pnccLChecker.CheckBlueprint(blueprint);
            if (!blueprintCompliance.IsValid)
            {
                var failedEvent = new BlueprintInstallFailedEvent(
                    blueprint.Id,
                    blueprint.Name,
                    serverEntry.Id,
                    serverEntry.Name,
                    "Blueprint failed PNCCL compliance check",
                    blueprintCompliance.Findings,
                    correlationId,
                    startedEvent.EventId
                );
                _eventBus.Publish(failedEvent);
                return false;
            }

            try
            {
                _serverProcessService.EnsureLayout(serverEntry.Id);
                var configDir = _serverProcessService.ConfigDir(serverEntry.Id);

                foreach (var template in blueprint.Templates)
                {
                    var processedContent = template.Content;
                    foreach (var input in userInputs)
                    {
                        processedContent = processedContent.Replace($"{{{{{input.Key}}}}}", input.Value);
                    }

                    var targetPath = Path.Combine(configDir, template.Target);
                    await File.WriteAllTextAsync(targetPath, processedContent);

                    var configFileCompliance = await _pnccLChecker.CheckConfigFile(targetPath, Path.GetExtension(targetPath));
                    if (!configFileCompliance.IsValid)
                    {
                        var configFailedEvent = new BlueprintInstallFailedEvent(
                            blueprint.Id,
                            blueprint.Name,
                            serverEntry.Id,
                            serverEntry.Name,
                            "Generated config file failed PNCCL compliance check",
                            new { FileName = template.Target, Findings = configFileCompliance.Findings },
                            correlationId,
                            startedEvent.EventId
                        );
                        _eventBus.Publish(configFailedEvent);
                        return false;
                    }
                }
                
                var succeededEvent = new BlueprintInstallSucceededEvent(
                    blueprint.Id,
                    blueprint.Name,
                    serverEntry.Id,
                    serverEntry.Name,
                    correlationId,
                    startedEvent.EventId
                );
                _eventBus.Publish(succeededEvent);
                return true;
            }
            catch (Exception ex)
            {
                var errorEvent = new BlueprintInstallFailedEvent(
                    blueprint.Id,
                    blueprint.Name,
                    serverEntry.Id,
                    serverEntry.Name,
                    "Unexpected error during installation",
                    ex.ToString(),
                    correlationId,
                    startedEvent.EventId
                );
                _eventBus.Publish(errorEvent);
                return false;
            }
        }
    }
}
