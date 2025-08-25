
using System.Collections.Generic;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public interface IBlueprintService
    {
        Task<IReadOnlyList<Blueprint>> GetBlueprintsAsync();
        Task<string[]> GetTemplateVariablesAsync(Blueprint blueprint);
    }
}
