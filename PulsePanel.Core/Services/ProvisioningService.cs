namespace PulsePanel.Core.Services;

using System;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

public class ProvisioningService : IProvisioningService
{
    public async Task<ServerInstance> ProvisionServerAsync(string instanceName, string gameName)
    {
        // Simulate a successful installation
        await Task.Delay(1000);

        return new ServerInstance
        {
            Id = Guid.NewGuid(),
            InstanceName = instanceName,
            GameName = gameName,
            Status = ServerStatus.Stopped,
            HealthScore = 100,
            InstallPath = "C\\PulsePanel\\Servers\\" + Guid.NewGuid().ToString()
        };
    }
}