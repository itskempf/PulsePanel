namespace PulsePanel.Core.Services
{
    public interface IWindowsServiceManager
    {
        void InstallService();
        void RemoveService();
        void StartService();
        void StopService();
    }
}
