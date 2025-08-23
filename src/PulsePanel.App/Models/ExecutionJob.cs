using System;
using System.Threading;

namespace PulsePanel.App.Models
{
    public enum JobStatus { Pending, Running, Completed, Failed, Cancelled }

    public sealed class ExecutionJob
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Blueprint Blueprint { get; } // Assuming Blueprint is already defined
        public ExecutionActionType Action { get; } // Assuming ExecutionActionType is already defined
        public ExecutionOptions Options { get; } // Assuming ExecutionOptions is already defined
        public JobStatus Status { get; internal set; } = JobStatus.Pending;
        public string? Error { get; internal set; }
        public DateTime? StartedAt { get; internal set; }
        public DateTime? EndedAt { get; internal set; }

        public ExecutionJob(Blueprint bp, ExecutionActionType action, ExecutionOptions options)
        {
            Blueprint = bp; Action = action; Options = options;
        }
    }
}