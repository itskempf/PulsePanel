
namespace PulsePanel.Core.Services;

public class StoragePathResolver : IStoragePathResolver
{
    public string GetServerInstancePath(string serverId)
    {
        // TODO: Implement actual path resolution based on serverId
        return Path.Combine(Path.GetTempPath(), "PulsePanelServers", serverId);
    }
}
