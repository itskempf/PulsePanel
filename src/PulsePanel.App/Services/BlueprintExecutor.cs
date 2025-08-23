using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public class BlueprintExecutor : IBlueprintExecutor
    {
        private readonly IProvenanceLogger _logger; // Changed to interface
        private readonly IActionHandlerFactory _actionFactory;
        private readonly IProvenanceHistoryService _historyService; // Added
        private readonly IProvenanceLogService _logService; // Added to subscribe to events

        public BlueprintExecutor(IProvenanceLogger logger, IActionHandlerFactory actionFactory,
                                 IProvenanceHistoryService historyService, IProvenanceLogService logService) // Modified constructor
        {
            _logger = logger;
            _actionFactory = actionFactory;
            _historyService = historyService;
            _logService = logService;
        }

        public async Task ExecuteInstallAsync(Blueprint bp, CancellationToken ct = default)
            => await ExecuteStepsAsync(bp, bp.InstallSteps, "Install", ct);

        public async Task ExecuteUpdateAsync(Blueprint bp, CancellationToken ct = default)
            => await ExecuteStepsAsync(bp, bp.UpdateSteps, "Update", ct);

        public async Task ExecuteValidateAsync(Blueprint bp, CancellationToken ct = default)
            => await ExecuteStepsAsync(bp, bp.ValidateSteps, "Validate", ct);

        private async Task ExecuteStepsAsync(Blueprint bp,
                                             List<BlueprintStep> steps,
                                             string phase,
                                             CancellationToken ct)
        {
            var sessionId = Guid.NewGuid();
            var startedAt = DateTime.UtcNow;
            var outcome = "Success";
            var sessionLogs = new List<LogEntry>();

            EventHandler<LogEntry> logEntryHandler = (sender, entry) =>
            {
                sessionLogs.Add(entry);
            };

            _logService.LogEntryAdded += logEntryHandler; // Subscribe to log events

            try
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
                        outcome = "Failed";
                        throw; // stop execution on failure
                    }
                }

                _logger.LogAction($"{phase}.Complete", bp.Id, $"{phase} completed for {bp.Name}");
            }
            catch (Exception)
            {
                outcome = "Failed";
                throw;
            }
            finally
            {
                _logService.LogEntryAdded -= logEntryHandler; // Unsubscribe
                var session = new ExecutionSession(sessionId, startedAt, bp.Name, bp.Version,
                                                   Environment.UserName, outcome, sessionLogs);
                _historyService.SaveSession(session);
            }
        }
    }
}