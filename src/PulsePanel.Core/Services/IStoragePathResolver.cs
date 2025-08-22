namespace PulsePanel.Core.Services;

public interface IStoragePathResolver
{
    string GetServerInstancePath(string serverId);
}