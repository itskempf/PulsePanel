using PulsePanel.Core.Models;
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public interface IConfigEditorService
    {
        Task<string> LoadConfigAsync(string filePath);
        Task<PnccLValidationResult> ValidateConfigAsync(string filePath, string content);
        Task StageChangesAsync(string filePath, string content);
        Task<CommitResult> CommitChangesAsync(string filePath, string commitMessage);
        Task RollbackChangesAsync(string filePath);
        Task<ConfigDiff> DiffWithBlueprintAsync(string filePath);
        Task<ConfigDiff> DiffWithProfileAsync(string filePath, string profileName);
    }
}
