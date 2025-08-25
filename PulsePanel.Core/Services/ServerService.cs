using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public class ServerService : IServerService
    {
        private readonly IServerInstanceRepository _repository;
        public ServerService(IServerInstanceRepository repository) { _repository = repository; }
        public async Task<IEnumerable<ServerInstance>> GetAllServersAsync() => await _repository.LoadInstancesAsync();
        public async Task AddNewServerAsync(ServerInstance server)
        {
            var instances = (await _repository.LoadInstancesAsync()).ToList();
            instances.Add(server);
            await _repository.SaveInstancesAsync(instances);
        }
        public async Task AddAsync(ServerInstance server) => await AddNewServerAsync(server);
        public async Task<ServerInstance?> FindAsync(Guid id)
        {
            var instances = await _repository.LoadInstancesAsync();
            return instances.FirstOrDefault(s => s.Id == id);
        }
        public async Task UpdateAsync(ServerInstance server)
        {
            var instances = (await _repository.LoadInstancesAsync()).ToList();
            var idx = instances.FindIndex(s => s.Id == server.Id);
            if (idx >= 0) instances[idx] = server;
            else instances.Add(server);
            await _repository.SaveInstancesAsync(instances);
        }
    }
}
