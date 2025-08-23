using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface IAgentClient
    {
        Task ExecuteAsync(NodeInfo node, Blueprint blueprint, ExecutionActionType action, ExecutionOptions options, IProvenanceLogger log, CancellationToken ct);
        Task<NodeStatus> PingAsync(NodeInfo node, CancellationToken ct);
    }
}