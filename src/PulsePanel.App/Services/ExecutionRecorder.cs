using System;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class ExecutionRecorder : IExecutionRecorder
    {
        private readonly IProvenanceHistoryService _history;
        public ExecutionSession Session { get; }

        public ExecutionRecorder(IProvenanceHistoryService history, Blueprint blueprint, ExecutionActionType action, ExecutionOptions options)
        {
            _history = history;
            Session = new ExecutionSession
            {
                Id = Guid.NewGuid(),
                StartedAt = DateTime.UtcNow,
                BlueprintName = blueprint.Name,
                BlueprintVersion = blueprint.Version,
                TriggeredBy = Environment.UserName,
                Action = action,
                DryRun = options.DryRun,
                TargetNodeId = options.TargetNodeId
            };
        }

        public void Append(LogEntry entry)
        {
            Session.Entries.Add(entry with { SessionId = Session.Id });
        }

        public void Complete(bool success, bool cancelled = false, string? error = null)
        {
            Session.Outcome = cancelled ? "Cancelled" : success ? "Success" : "Failed";
            Session.Error = error;
            Session.EndedAt = DateTime.UtcNow;
            _history.SaveSession(Session);
        }

        public void Dispose() => Complete(success: true);
    }

    public sealed class ExecutionRecorderFactory : IExecutionRecorderFactory
    {
        private readonly IProvenanceHistoryService _history;
        public ExecutionRecorderFactory(IProvenanceHistoryService history) => _history = history;
        public IExecutionRecorder Start(Blueprint blueprint, ExecutionActionType action, ExecutionOptions options)
            => new ExecutionRecorder(_history, blueprint, action, options);
    }
}