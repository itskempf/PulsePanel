using System.Collections.Generic;
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public interface IServerProcessInspector
    {
        Task<IReadOnlyList<ServerProcessInfo>> GetRunningServersAsync();
    }
}
