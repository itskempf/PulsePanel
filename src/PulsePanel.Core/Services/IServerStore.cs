using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public interface IServerStore
    {
        void UpdateStatus(string serverId, ServerStatus status);
    }
}