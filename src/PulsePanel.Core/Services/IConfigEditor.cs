
using PulsePanel.Blueprints;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public interface IConfigEditor
{
    Task<string> ReadConfigFileAsync(string serverId, string filePath);
    Task<ValidationResult> WriteConfigFileAsync(string serverId, string filePath, string content);
}
