using PulsePanel.Core.Models;
using System.Text.Json;

namespace PulsePanel.Core.Services;

public class ServerInstanceRepository : IServerInstanceRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public ServerInstanceRepository()
    {
        var basePath = AppContext.BaseDirectory;
        var dataPath = Path.Combine(basePath, "Data");
        _filePath = Path.Combine(dataPath, "servers.json");
        
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    public async Task<IEnumerable<ServerInstance>> LoadInstancesAsync()
    {
        if (!File.Exists(_filePath))
        {
            return Enumerable.Empty<ServerInstance>();
        }

        try
        {
            var jsonContent = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return Enumerable.Empty<ServerInstance>();
            }
            
            var instances = JsonSerializer.Deserialize<List<ServerInstance>>(jsonContent);
            return instances ?? Enumerable.Empty<ServerInstance>();
        }
        catch (Exception ex)
        {
            // Proper logging would go here
            Console.WriteLine($"Error loading server instances: {ex.Message}");
            return Enumerable.Empty<ServerInstance>();
        }
    }

    public async Task SaveInstancesAsync(IEnumerable<ServerInstance> instances)
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var jsonContent = JsonSerializer.Serialize(instances, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, jsonContent);
        }
        catch (Exception ex)
        {
            // Proper logging would go here
            Console.WriteLine($"Error saving server instances: {ex.Message}");
        }
    }
}
