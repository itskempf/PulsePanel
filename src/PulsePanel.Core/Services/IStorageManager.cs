namespace PulsePanel.Core.Services
{
    public interface IStorageManager
    {
        string GetStoragePath();
        void SetStoragePath(string path);
    }
}
