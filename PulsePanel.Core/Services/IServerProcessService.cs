
using System;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public class ServerOutputEventArgs : EventArgs
    {
        public Guid ServerId { get; }
        public string Line { get; }
        public bool IsError { get; }
        public ServerOutputEventArgs(Guid serverId, string line, bool isError) { ServerId = serverId; Line = line; IsError = isError; }
    }

    public interface IServerProcessService
    {
        event EventHandler<ServerOutputEventArgs>? OutputReceived;
        Task StartServerAsync(ServerInstance server);
        Task StopServerAsync(ServerInstance server);
    }
}
