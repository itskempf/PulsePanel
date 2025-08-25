using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public class ProvisioningService : IProvisioningService {

        public async Task<ServerInstance> ProvisionServerAsync(string instanceName, string gameName) {
            await Task.Delay(1000); // Simulate work
            return new ServerInstance {
                Id = Guid.NewGuid(),
                InstanceName = instanceName,
                GameName = gameName,
                Status = ServerStatus.Stopped,
                HealthScore = 100,
                InstallPath = Path.Combine("C:", "PulsePanel", "Servers", Guid.NewGuid().ToString())
            };
        }

        // Compatibility overload for callers expecting blueprint-based provisioning.
        public async Task<ServerInstance> ProvisionServerAsync(Blueprint blueprint, string instanceName, IDictionary<string,string> variables, IProgress<string>? progress = null, CancellationToken ct = default)
        {
            // Minimal implementation: create instance folder and return instance.
            string serversRoot = Path.Combine("C:\\", "PulsePanel", "Servers");
            Directory.CreateDirectory(serversRoot);
            var serverId = Guid.NewGuid();
            string installPath = Path.Combine(serversRoot, serverId.ToString());
            Directory.CreateDirectory(installPath);
            await Task.Delay(100, ct);
            return new ServerInstance
            {
                Id = serverId,
                InstanceName = instanceName,
                GameName = blueprint.Name,
                Status = ServerStatus.Stopped,
                HealthScore = 100,
                InstallPath = installPath
            };
        }
    }
}
