namespace PulsePanel.Core.Services
{
    public interface IServerProcessService
    {
        void StartServer(string id, string exePath, string args);
        void StopServer(string id);
        bool IsServerRunning(string id);
    }
}