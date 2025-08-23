using System;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface IExecutionRecorder : IDisposable
    {
        ExecutionSession Session { get; }
        void Append(LogEntry entry);
        void Complete(bool success, bool cancelled = false, string? error = null);
    }

    public interface IExecutionRecorderFactory
    {
        IExecutionRecorder Start(Blueprint blueprint, ExecutionActionType action, ExecutionOptions options);
    }
}