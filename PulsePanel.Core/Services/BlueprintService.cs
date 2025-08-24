using PulsePanel.Core.Models;
using System.Text.Json;

namespace PulsePanel.Core.Services;

public class BlueprintService : IBlueprintService
{
    private readonly string _blueprintsPath;

    public BlueprintService()
    {
        // Get the directory where the application is running and combine it with our data folder.
        var basePath = AppContext.BaseDirectory;
        _blueprintsPath = Path.Combine(basePath, "Data", "Blueprints");
    }

    public async Task<IEnumerable<Blueprint>> GetAllBlueprintsAsync()
    {
        if (!Directory.Exists(_blueprintsPath))
        {
            // Or we could create it: Directory.CreateDirectory(_blueprintsPath);
            return Enumerable.Empty<Blueprint>();
        }

        var blueprintFiles = Directory.GetFiles(_blueprintsPath, "*.json");
        var blueprints = new List<Blueprint>();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        foreach (var file in blueprintFiles)
        {
            try
            {
                var jsonContent = await File.ReadAllTextAsync(file);
                var blueprint = JsonSerializer.Deserialize<Blueprint>(jsonContent, options);
                if (blueprint != null)
                {
                    blueprints.Add(blueprint);
                }
            }
            catch (Exception ex)
            {
                // In a real app, we'd log this error properly.
                Console.WriteLine($"Failed to parse blueprint {file}: {ex.Message}");
            }
        }

        return blueprints;
    }
}