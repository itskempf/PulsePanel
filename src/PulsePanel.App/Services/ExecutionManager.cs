using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class ExecutionManager : IExecutionManager, IDisposable
    {
        private readonly IBlueprintExecutor _executor;
        private readonly ObservableCollection<ExecutionJob> _jobs = new();
        private readonly ReadOnlyObservableCollection<ExecutionJob> _jobsView;
        private readonly Channel<ExecutionJob> _queue = Channel.CreateUnbounded<ExecutionJob>();
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _ctsMap = new();
        private readonly CancellationTokenSource _shutdown = new();
        private Task[] _workers = Array.Empty<Task>();
        private int _maxParallelism = Environment.ProcessorCount / 2;

        public ExecutionManager(IBlueprintExecutor executor)
        {
            _executor = executor;
            _jobsView = new ReadOnlyObservableCollection<ExecutionJob>(_jobs);
            StartWorkers();
        }

        public ReadOnlyObservableCollection<ExecutionJob> Jobs => _jobsView;

        public int MaxParallelism
        {
            get => _maxParallelism;
            set { _maxParallelism = Math.Max(1, value); RestartWorkers(); }
        }

        public ExecutionJob Enqueue(Blueprint bp, ExecutionActionType action, ExecutionOptions options)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(options.CancellationToken);
            var opts = new ExecutionOptions
            {
                DryRun = options.DryRun,
                ReplaySessionId = options.ReplaySessionId,
                TargetNodeId = options.TargetNodeId,
                CancellationToken = cts.Token
            };

            var job = new ExecutionJob(bp, action, opts);
            _ctsMap[job.Id] = cts;

            App.MainThreadDispatcher(() => _jobs.Add(job));
            _queue.Writer.TryWrite(job);
            return job;
        }

        public Task CancelAsync(Guid jobId)
        {
            if (_ctsMap.TryRemove(jobId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
            return Task.CompletedTask;
        }

        private void StartWorkers()
        {
            _workers = Enumerable.Range(0, _maxParallelism)
                                 .Select(_ => Task.Run(WorkerLoop))
                                 .ToArray();
        }

        private void RestartWorkers()
        {
            _shutdown.Cancel();
            Task.WaitAll(_workers, 2000);
            _shutdown.Dispose();
            // Cannot restart same CTS; create a new manager in a real app or refactor shutdown logic.
            // For brevity, omit dynamic restart in this snippet.
        }

        private async Task WorkerLoop()
        {
            try
            {
                while (await _queue.Reader.WaitToReadAsync(_shutdown.Token))
                {
                    while (_queue.Reader.TryRead(out var job))
                    {
                        job.Status = JobStatus.Running;
                        job.StartedAt = DateTime.UtcNow;
                        try
                        {
                            await _executor.ExecuteAsync(job.Blueprint, job.Action, job.Options);
                            job.Status = JobStatus.Completed;
                        }
                        catch (OperationCanceledException)
                        {
                            job.Status = JobStatus.Cancelled;
                        }
                        catch (Exception ex)
                        {
                            job.Status = JobStatus.Failed;
                            job.Error = ex.Message;
                        }
                        finally
                        {
                            job.EndedAt = DateTime.UtcNow;
                            _ctsMap.TryRemove(job.Id, out var cts);
                            cts?.Dispose();
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        public void Dispose()
        {
            foreach (var kv in _ctsMap) kv.Value.Cancel();
            _shutdown.Cancel();
        }
    }
}