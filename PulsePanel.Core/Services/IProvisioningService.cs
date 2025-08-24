using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public interface IProvisioningService
{
    // This will eventually take a Blueprint object as a parameter
    Task<ServerInstance> ProvisionServerAsync(string instanceName, string gameName);
}