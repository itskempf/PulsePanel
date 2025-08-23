using System;
using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class SelfHealingOrchestrator : ISelfHealingOrchestrator, IDisposable
    {
        private readonly IComplianceService _compliance;
        private readonly IRemoteExecutionRouter _router;
        private readonly IProvenanceLogger _log;
        private CancellationTokenSource? _cts;
        public bool Enabled { get; set; }

        public SelfHealingOrchestrator(IComplianceService compliance, IRemoteExecutionRouter router, IProvenanceLogger log)
        {
            _compliance = compliance; _router = router; _log = log;
        }

        public async Task ScanAndHealAsync(Blueprint bp, string? nodeId, CancellationToken ct)
        {
            var report = await _compliance.ScanAsync(bp, nodeId, ct);
            if (report.Overall == ComplianceStatus.Pass)
            {
                _log.LogAction("Compliance", bp.Id, $"Compliance OK: {bp.Name} {(nodeId ?? "local")}");
                return;
            }

            _log.LogAction("Compliance", bp.Id, $"Compliance drift detected for {bp.Name} ({report.Overall}).");
            if (!Enabled)
            {
                _log.LogAction("Compliance", bp.Id, "Self‑Healing disabled. No corrective action taken.");
                return;
            }

            _log.LogAction("Compliance", bp.Id, "Triggering corrective Install.");
            var opts = new ExecutionOptions { DryRun = false, TargetNodeId = nodeId };
            await _router.RouteAsync(bp, ExecutionActionType.Install, opts, ct);
        }

        public void StartScheduler(TimeSpan interval)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                var ct = _cts.Token;
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        // In a real app, iterate all blueprints and nodes.
                        await Task.Delay(interval, ct);
                    }
                    catch (OperationCanceledException) { }
                }
            }, _cts.Token);
        }

        public void StopScheduler() => _cts?.Cancel();
        public void Dispose() => _cts?.Cancel();
    }
}