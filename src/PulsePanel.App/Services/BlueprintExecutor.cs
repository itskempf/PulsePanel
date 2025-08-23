using System;
using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public class BlueprintExecutor : IBlueprintExecutor
    {
        private readonly ProvenanceLogger _logger;
        private readonly IActionHandlerFactory _actionFactory;

        public BlueprintExecutor(ProvenanceLogger logger, IActionHandlerFactory actionFactory)
        {
            _logger = logger;
            _actionFactory = actionFactory;
        }

        public async Task ExecuteInstallAsync(Blueprint bp, CancellationToken ct = default)
            => await ExecuteStepsAsync(bp, bp.InstallSteps, "Install", ct);

        public async Task ExecuteUpdateAsync(Blueprint bp, CancellationToken ct = default)
            => await ExecuteStepsAsync(bp, bp.UpdateSteps, "Update", ct);

        public async Task ExecuteValidateAsync(Blueprint bp, CancellationToken ct = default)
            => await ExecuteStepsAsync(bp, bp.ValidateSteps, "Validate", ct);

        private async Task ExecuteStepsAsync(Blueprint bp, 
                                             System.Collections.Generic.List<BlueprintStep> steps, 
                                             string phase, 
                                             CancellationToken ct)
        {
            _logger.LogAction($"{phase}.Start", bp.Id, $"{phase} started for {bp.Name}");

            foreach (var step in steps)
            {
                try
                {
                    _logger.LogAction($"{phase}.Step.Start", bp.Id, $"Action: {step.Action}");
                    var handler = _actionFactory.GetHandler(step.Action);
                    await handler.ExecuteAsync(step.Parameters, ct);
                    _logger.LogAction($"{phase}.Step.Complete", bp.Id, $"Action complete: {step.Action}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{phase}.Step.Error", bp.Id, $"Action failed: {step.Action} — {ex.Message}");
                    throw; // stop execution on failure
                }
            }

            _logger.LogAction($"{phase}.Complete", bp.Id, $"{phase} completed for {bp.Name}");
        }
    }
}