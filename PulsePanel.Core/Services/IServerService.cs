using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public interface IServerService
    {
        Task<IEnumerable<ServerInstance>> GetAllServersAsync();
        Task<ServerInstance?> FindAsync(Guid id);
        Task AddAsync(ServerInstance server);
        Task AddNewServerAsync(ServerInstance server); // keep existing to avoid breaking other callers
        Task UpdateAsync(ServerInstance server);
    }
}
