using PulsePanel.Blueprints;
using System;
using System.IO;
using System.Text.Json;

if (args.Length < 1)
{
    PrintUsage();
    return 1;
}

var command = args[0];
var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
};

switch (command)
{
    case "list-blueprints":
        var useJson = args.Length > 1 && args[1] == "--json";
        return await ListBlueprints(useJson, jsonOptions);

    case "validate-blueprint":
        if (args.Length < 2)
        {
            Console.WriteLine("Error: Missing required argument <path>");
            PrintUsage();
            return 1;
        }
        return await ValidateBlueprint(args[1], jsonOptions);

    case "generate-config":
        if (args.Length < 3)
        {
            Console.WriteLine("Error: Missing required arguments <blueprintPath> and/or <valuesFile>");
            PrintUsage();
            return 1;
        }
        return await GenerateConfig(args[1], args[2]);

    default:
        Console.WriteLine($"Error: Unknown command '{command}'");
        PrintUsage();
        return 1;
}


static void PrintUsage()
{
    Console.WriteLine("Usage: pulsepanel <command> [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  list-blueprints [--json]                Lists all available blueprints.");
    Console.WriteLine("  validate-blueprint <path>               Validates a blueprint directory.");
    Console.WriteLine("  generate-config <blueprintPath> <valuesFile>  Generates a configuration from a blueprint.");
}

static async Task<int> ListBlueprints(bool useJson, JsonSerializerOptions options)
{
    var blueprintsRoot = Path.Combine(Directory.GetCurrentDirectory(), "blueprints");
    var cachePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "cache");
    var catalog = new BlueprintCatalog(blueprintsRoot, cachePath);
    var entries = catalog.GetCatalog();

    if (useJson)
    {
        Console.WriteLine(JsonSerializer.Serialize(entries, options));
    }
    else
    {
        Console.WriteLine($"{"NAME",-30} {"VERSION",-15} {"DESCRIPTION"}");
        Console.WriteLine(new string('-', 70));
        foreach (var entry in entries)
        {
            var description = entry.Description.Length > 50 ? entry.Description.Substring(0, 47) + "..." : entry.Description;
            Console.WriteLine($"{entry.Name,-30} {entry.Version,-15} {description}");
        }
    }
    return 0;
}

static async Task<int> ValidateBlueprint(string blueprintPath, JsonSerializerOptions options)
{
    if (!Directory.Exists(blueprintPath))
    {
        Console.WriteLine($"Error: Directory not found at '{blueprintPath}'");
        return 1;
    }

    var logger = new ProvenanceLogger("./data/provenance/log.jsonl");
    var validator = new BlueprintValidator(logger);
    var result = validator.Validate(blueprintPath);

    var output = new
    {
        result.Status,
        result.Errors,
        result.Warnings,
        Blueprint = result.Blueprint != null ? new { result.Blueprint.Name, result.Blueprint.Version } : null
    };

    Console.WriteLine(JsonSerializer.Serialize(output, options));
    return result.IsValid ? 0 : 1;
}

static async Task<int> GenerateConfig(string blueprintPath, string valuesFile)
{
    if (!Directory.Exists(blueprintPath))
    {
        Console.WriteLine($"Error: Blueprint directory not found at '{blueprintPath}'");
        return 1;
    }
    if (!File.Exists(valuesFile))
    {
        Console.WriteLine($"Error: Values file not found at '{valuesFile}'");
        return 1;
    }

    var logger = new ProvenanceLogger("./data/provenance/log.jsonl");
    var generator = new ConfigGenerator(logger);
    var outputRoot = Path.Combine(Directory.GetCurrentDirectory(), "output");
    var result = generator.Generate(blueprintPath, valuesFile, outputRoot);

    if (result.Success)
    {
        Console.WriteLine($"Configuration generated successfully at: {result.OutputPath}");
        return 0;
    }
    else
    {
        Console.WriteLine("Error: Configuration generation failed.");
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"- {error}");
        }
        return 1;
    }
}
