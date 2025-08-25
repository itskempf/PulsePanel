using System.Collections.Generic;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public interface IServerInstanceRepository
    {
        Task<IEnumerable<ServerInstance>> LoadInstancesAsync();
        Task SaveInstancesAsync(IEnumerable<ServerInstance> instances);
    }
}
