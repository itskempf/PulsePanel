using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface IBlueprintExecutor
    {
        Task ExecuteAsync(Blueprint blueprint, ExecutionActionType action, ExecutionOptions options);
        Task ExecuteInstallAsync(Blueprint blueprint, ExecutionOptions? options = null);
        Task ExecuteUpdateAsync(Blueprint blueprint, ExecutionOptions? options = null);
        Task ExecuteValidateAsync(Blueprint blueprint, ExecutionOptions? options = null);
    }
}