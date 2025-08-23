using System;
using System.Linq;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class BlueprintExecutor : IBlueprintExecutor
    {
        private readonly IActionHandlerFactory _handlerFactory;
        private readonly IProvenanceLogger _log;
        private readonly IExecutionRecorderFactory _recorderFactory;
        private readonly IProvenanceLogService _logBus;

        public BlueprintExecutor(
            IActionHandlerFactory handlerFactory,
            IProvenanceLogger log,
            IExecutionRecorderFactory recorderFactory,
            IProvenanceLogService logBus)
        {
            _handlerFactory = handlerFactory;
            _log = log;
            _recorderFactory = recorderFactory;
            _logBus = logBus;
        }

        public Task ExecuteInstallAsync(Blueprint blueprint, ExecutionOptions? options = null)
            => ExecuteAsync(blueprint, ExecutionActionType.Install, options ?? ExecutionOptions.Default);

        public Task ExecuteUpdateAsync(Blueprint blueprint, ExecutionOptions? options = null)
            => ExecuteAsync(blueprint, ExecutionActionType.Update, options ?? ExecutionOptions.Default);

        public Task ExecuteValidateAsync(Blueprint blueprint, ExecutionOptions? options = null)
            => ExecuteAsync(blueprint, ExecutionActionType.Validate, options ?? ExecutionOptions.Default);

        public async Task ExecuteAsync(Blueprint blueprint, ExecutionActionType action, ExecutionOptions options)
        {
            using var recorder = _recorderFactory.Start(blueprint, action, options);
            void OnLog(object? s, LogEntry e) { if (e.SessionId is null || e.SessionId == recorder.Session.Id) recorder.Append(e); }
            _logBus.LogEntryAdded += OnLog;

            try
            {
                await _log.InfoAsync($"Starting {action} for {blueprint.Name}@{blueprint.Version}", recorder.Session.Id);

                var actions = action switch
                {
                    ExecutionActionType.Install => blueprint.InstallActions,
                    ExecutionActionType.Update => blueprint.UpdateActions,
                    ExecutionActionType.Validate => blueprint.ValidateActions,
                    _ => Enumerable.Empty<BlueprintAction>()
                };

                foreach (var step in actions)
                {
                    options.CancellationToken.ThrowIfCancellationRequested();

                    var handler = _handlerFactory.Create(step.Type);
                    if (options.DryRun)
                    {
                        await _log.InfoAsync($"[DRY-RUN] Would execute: {step.Type} — {step.Description}", recorder.Session.Id);
                        continue;
                    }

                    await _log.InfoAsync($"Executing: {step.Type} — {step.Description}", recorder.Session.Id);
                    await handler.HandleAsync(step, _log, options.CancellationToken, recorder.Session.Id);
                }

                await _log.InfoAsync("Execution complete.", recorder.Session.Id);
                recorder.Complete(success: true);
            }
            catch (OperationCanceledException)
            {
                await _log.WarningAsync("Execution cancelled.", recorder.Session.Id);
                recorder.Complete(success: false, cancelled: true);
            }
            catch (Exception ex)
            {
                await _log.ErrorAsync($"Execution failed: {ex.Message}", recorder.Session.Id);
                recorder.Complete(success: false, error: ex.Message);
            }
            finally
            {
                _logBus.LogEntryAdded -= OnLog;
            }
        }
    }
}