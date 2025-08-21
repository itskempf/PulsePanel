namespace PulsePanel.Core.Services
{
    public interface IStoragePathResolver
    {
        string? GetStoragePath(string key);
    }
}
