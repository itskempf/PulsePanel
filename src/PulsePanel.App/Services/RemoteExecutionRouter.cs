using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class RemoteExecutionRouter : IRemoteExecutionRouter
    {
        private readonly INodeRegistry _nodes;
        private readonly IAgentClient _agent;
        private readonly IBlueprintExecutor _local;
        private readonly IProvenanceLogger _log;

        public RemoteExecutionRouter(INodeRegistry nodes, IAgentClient agent, IBlueprintExecutor local, IProvenanceLogger log)
        {
            _nodes = nodes; _agent = agent; _local = local; _log = log;
        }

        public async Task RouteAsync(Blueprint bp, ExecutionActionType action, ExecutionOptions options, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(options.TargetNodeId))
            {
                await _local.ExecuteAsync(bp, action, options);
                return;
            }

            var node = _nodes.GetById(options.TargetNodeId!); // Use non-null assertion as we check for null/whitespace above
            if (node is null)
            {
                await _log.LogError("Routing", bp.Id, $"Target node {options.TargetNodeId} not found. Executing locally.");
                await _local.ExecuteAsync(bp, action, options);
                return;
            }

            var status = await _agent.PingAsync(node, ct);
            if (status != NodeStatus.Online)
            {
                await _log.LogError("Routing", bp.Id, $"Target node {node.Name} offline. Aborting remote execution.");
                return;
            }

            await _agent.ExecuteAsync(node, bp, action, options, _log, ct);
        }
    }
}