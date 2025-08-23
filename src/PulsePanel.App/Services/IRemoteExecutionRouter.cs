using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface IRemoteExecutionRouter
    {
        Task RouteAsync(Blueprint bp, ExecutionActionType action, ExecutionOptions options, CancellationToken ct);
    }
}