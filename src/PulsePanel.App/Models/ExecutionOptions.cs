using System;
using System.Threading;

namespace PulsePanel.App.Models
{
    public sealed class ExecutionOptions
    {
        public bool DryRun { get; init; }
        public Guid? ReplaySessionId { get; init; }
        public string? TargetNodeId { get; init; }
        public CancellationToken CancellationToken { get; init; }
        public static ExecutionOptions Default => new() { CancellationToken = CancellationToken.None };
    }
}