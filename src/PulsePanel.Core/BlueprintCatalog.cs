using PulsePanel.Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Blueprint = PulsePanel.Blueprints.Models.Blueprint;

namespace PulsePanel.Core;

public class BlueprintCatalog
{
    private readonly string _blueprintsRoot;
    private readonly string _cachePath;
    private static readonly object CacheLock = new();

    public BlueprintCatalog(string blueprintsRoot, string cachePath)
    {
        _blueprintsRoot = blueprintsRoot;
        _cachePath = cachePath;
    }

    public List<BlueprintCatalogEntry> GetCatalog()
    {
        // For simplicity in this sprint, we will not implement caching yet.
        // We will scan the directory every time.
        return ScanForBlueprints();
    }

    public Blueprint? GetBlueprint(string name)
    {
        var entry = GetCatalog().FirstOrDefault(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (entry == null)
        {
            return null;
        }

        var metaFile = Path.Combine(entry.Path, "meta.yaml");
        if (!File.Exists(metaFile))
        {
            return null;
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        try
        {
            var content = File.ReadAllText(metaFile);
            return deserializer.Deserialize<Blueprint>(content);
        }
        catch
        {
            return null; // Failed to parse
        }
    }

    private List<BlueprintCatalogEntry> ScanForBlueprints()
    {
        var entries = new List<BlueprintCatalogEntry>();
        if (!Directory.Exists(_blueprintsRoot))
        {
            return entries;
        }

        var metaFiles = Directory.GetFiles(_blueprintsRoot, "meta.yaml", SearchOption.AllDirectories);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties() // Important for partial deserialization
            .Build();

        foreach (var metaFile in metaFiles)
        {
            try
            {
                var content = File.ReadAllText(metaFile);
                var partialBlueprint = deserializer.Deserialize<BlueprintCatalogEntry>(content);

                // We need the path relative to the root.
                partialBlueprint.Path = Path.GetDirectoryName(metaFile)!;

                if (!string.IsNullOrWhiteSpace(partialBlueprint.Name))
                {
                    entries.Add(partialBlueprint);
                }
            }
            catch
            {
                // Ignore invalid meta.yaml files during scan
            }
        }
        return entries;
    }
}
