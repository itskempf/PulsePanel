using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public interface IBlueprintService
{
    /// <summary>
    /// Scans the blueprints directory and returns all valid blueprints.
    /// </summary>
    Task<IEnumerable<Blueprint>> GetAllBlueprintsAsync();
}