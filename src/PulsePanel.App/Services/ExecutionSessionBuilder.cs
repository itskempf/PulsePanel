using System;
using System.Collections.Generic;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class ExecutionSessionBuilder
    {
        private Guid _id = Guid.NewGuid();
        private DateTime _startedAt = DateTime.UtcNow;
        private string _blueprintName = "";
        private string _blueprintVersion = "";
        private string _triggeredBy = Environment.UserName;
        private string _outcome = "Running";
        private readonly List<LogEntry> _entries = new();
        private ExecutionActionType _action;
        private bool _dryRun;
        private string? _targetNodeId;
        private DateTime? _endedAt;
        private string? _error;

        public ExecutionSessionBuilder WithBlueprint(Blueprint bp)
        {
            _blueprintName = bp.Name;
            _blueprintVersion = bp.Version;
            return this;
        }

        public ExecutionSessionBuilder WithAction(ExecutionActionType action) { _action = action; return this; }
        public ExecutionSessionBuilder WithDryRun(bool dry) { _dryRun = dry; return this; }
        public ExecutionSessionBuilder WithTargetNode(string? nodeId) { _targetNodeId = nodeId; return this; }

        public ExecutionSession Build() => new(
            _id, _startedAt, _blueprintName, _blueprintVersion, _triggeredBy, _outcome,
            _entries, _action, _dryRun, _targetNodeId, _endedAt, _error);

        public void MarkCompleted(bool success, bool cancelled = false, string? error = null)
        {
            _outcome = cancelled ? "Cancelled" : success ? "Success" : "Failed";
            _endedAt = DateTime.UtcNow;
            _error = error;
        }
    }
}