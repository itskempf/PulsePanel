using PulsePanel.Core.Models;
using System.Linq;

namespace PulsePanel.Core.Services;

public class ServerService : IServerService
{
    private readonly IServerInstanceRepository _repository;

    public ServerService(IServerInstanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ServerInstance>> GetAllServersAsync()
    {
        return await _repository.LoadInstancesAsync();
    }

    public async Task AddNewServerAsync(ServerInstance server)
    {
        var currentInstances = (await _repository.LoadInstancesAsync()).ToList();
        currentInstances.Add(server);
        await _repository.SaveInstancesAsync(currentInstances);
    }
}