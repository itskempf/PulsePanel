
using PulsePanel.Blueprints;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public class ConfigEditor : IConfigEditor
{
    private readonly IStoragePathResolver _storagePathResolver;

    public ConfigEditor(IStoragePathResolver storagePathResolver)
    {
        _storagePathResolver = storagePathResolver;
    }

    public async Task<string> ReadConfigFileAsync(string serverId, string filePath)
    {
        var serverPath = _storagePathResolver.GetServerInstancePath(serverId);
        var fullPath = Path.Combine(serverPath, filePath);
        return await File.ReadAllTextAsync(fullPath);
    }

    public async Task<ValidationResult> WriteConfigFileAsync(string serverId, string filePath, string content)
    {
        var serverPath = _storagePathResolver.GetServerInstancePath(serverId);
        var fullPath = Path.Combine(serverPath, filePath);
        await File.WriteAllTextAsync(fullPath, content);
        return new ValidationResult { Status = "pass" };
    }
}
