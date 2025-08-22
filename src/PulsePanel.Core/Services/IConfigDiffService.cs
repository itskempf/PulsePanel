using PulsePanel.Core.Models;
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public interface IConfigDiffService
    {
        Task<ConfigDiff> GenerateDiffAsync(string originalContent, string newContent);
    }
}
