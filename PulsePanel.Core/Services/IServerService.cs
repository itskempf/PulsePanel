using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public interface IServerService
{
    Task<IEnumerable<ServerInstance>> GetAllServersAsync();
    Task AddNewServerAsync(ServerInstance server);
}