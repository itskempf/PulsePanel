using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public interface IProvisioningService {
        // Simple overload
        Task<ServerInstance> ProvisionServerAsync(string instanceName, string gameName);
        // Compatibility overload used by callers that pass a Blueprint and variables
        Task<ServerInstance> ProvisionServerAsync(Blueprint blueprint, string instanceName, IDictionary<string,string> variables, IProgress<string>? progress = null, CancellationToken ct = default);
    }
}
