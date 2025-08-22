using PulsePanel.Core;
using PulsePanel.Windows;
using PulsePanel.Cli;
using PulsePanel.Core.Services;
using System;
using System.IO;
using System.Text.Json;

if (args.Length < 1)
{
    PrintUsage();
    return 1;
}

// stubs moved to CliStubs.cs

var command = args[0];
var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
};

var logger = new ProvenanceLogger("d:\\PulsePanel\\data\\provenance\\log.jsonl");

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
        return await ValidateBlueprint(args[1], jsonOptions, logger);

    case "generate-config":
        if (args.Length < 3)
        {
            Console.WriteLine("Error: Missing required arguments <blueprintPath> and/or <valuesFile>");
            PrintUsage();
            return 1;
        }
        return await GenerateConfig(args[1], args[2], logger);

    case "install-service":
        return InstallService(logger);

    case "remove-service":
        return RemoveService(logger);

    case "start-service":
        return StartService(logger);

    case "stop-service":
        return StopService(logger);

    case "firewall-add":
        return AddFirewallRule(logger);

    case "firewall-remove":
        return RemoveFirewallRule(logger);

    case "steamcmd-setup":
        return SetupSteamCmd(logger);

    case "steamcmd-verify":
        return VerifySteamCmd(logger);

    case "set-storage-path":
        if (args.Length < 3)
        {
            Console.WriteLine("Error: Missing required arguments <key> and <path>");
            PrintUsage();
            return 1;
        }
        return SetStoragePath(args[1], args[2], logger);

    case "get-storage-path":
        if (args.Length < 2)
        {
            Console.WriteLine("Error: Missing required argument <key>");
            PrintUsage();
            return 1;
        }
        return GetStoragePath(args[1], logger);

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
    Console.WriteLine("  install-service                         Installs the PulsePanel API as a Windows service.");
    Console.WriteLine("  remove-service                          Removes the PulsePanel API Windows service.");
    Console.WriteLine("  start-service                           Starts the PulsePanel API Windows service.");
    Console.WriteLine("  stop-service                            Stops the PulsePanel API Windows service.");
    Console.WriteLine("  firewall-add                            Adds a firewall rule for PulsePanel.");
    Console.WriteLine("  firewall-remove                         Removes the firewall rule for PulsePanel.");
    Console.WriteLine("  steamcmd-setup                          Downloads and installs SteamCMD.");
    Console.WriteLine("  steamcmd-verify                         Verifies the SteamCMD installation.");
    Console.WriteLine("  set-storage-path <key> <path>           Sets a storage path.");
    Console.WriteLine("  get-storage-path <key>                  Gets a storage path.");
}

static int SetStoragePath(string key, string path, IProvenanceLogger logger)
{
    var storageManager = new StorageManager(logger);
    storageManager.SetStoragePath(key, path);
    Console.WriteLine($"Storage path '{key}' set to '{path}'.");
    return 0;
}

static int GetStoragePath(string key, IProvenanceLogger logger)
{
    var storageManager = new StorageManager(logger);
    var path = storageManager.GetStoragePath(key);
    if (path != null)
    {
        Console.WriteLine($"Storage path '{key}': {path}");
        return 0;
    }
    else
    {
        Console.WriteLine($"Storage path '{key}' not found.");
        return 1;
    }
}

static int SetupSteamCmd(IProvenanceLogger logger)
{
    var steamCmdManager = new SteamCmdManager(logger);
    // In a real implementation, we would allow the user to specify the installation path.
    steamCmdManager.SetupSteamCmd("d:\\PulsePanel\\steamcmd");
    return 0;
}

static int VerifySteamCmd(IProvenanceLogger logger)
{
    var steamCmdManager = new SteamCmdManager(logger);
    // In a real implementation, we would allow the user to specify the installation path.
    if (steamCmdManager.VerifySteamCmd("d:\\PulsePanel\\steamcmd"))
    {
        Console.WriteLine("SteamCMD is installed and verified.");
        return 0;
    }
    else
    {
        Console.WriteLine("SteamCMD is not installed or could not be verified.");
        return 1;
    }
}

static int AddFirewallRule(IProvenanceLogger logger)
{
    var firewallManager = new FirewallManager(logger);
    // In a real implementation, we would get the rule name, protocol, and port from the blueprint.
    firewallManager.AddFirewallRule("PulsePanel-Minecraft", "TCP", "25565");
    Console.WriteLine("Firewall rule added successfully.");
    return 0;
}

static int RemoveFirewallRule(IProvenanceLogger logger)
{
    var firewallManager = new FirewallManager(logger);
    // In a real implementation, we would get the rule name from the blueprint.
    firewallManager.RemoveFirewallRule("PulsePanel-Minecraft");
    Console.WriteLine("Firewall rule removed successfully.");
    return 0;
}

static int InstallService(IProvenanceLogger logger)
{
    var serviceManager = new WindowsServiceManager(logger);
    serviceManager.InstallService();
    Console.WriteLine("PulsePanel service installed successfully.");
    return 0;
}

static int RemoveService(IProvenanceLogger logger)
{
    var serviceManager = new WindowsServiceManager(logger);
    serviceManager.RemoveService();
    Console.WriteLine("PulsePanel service removed successfully.");
    return 0;
}

static int StartService(IProvenanceLogger logger)
{
    var serviceManager = new WindowsServiceManager(logger);
    serviceManager.StartService();
    Console.WriteLine("PulsePanel service started successfully.");
    return 0;
}

static int StopService(IProvenanceLogger logger)
{
    var serviceManager = new WindowsServiceManager(logger);
    serviceManager.StopService();
    Console.WriteLine("PulsePanel service stopped successfully.");
    return 0;
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
        Console.WriteLine("{0,-30} {1,-15} {2}", "NAME", "VERSION", "DESCRIPTION");
        Console.WriteLine(new string('-', 70));
        foreach (var entry in entries)
        {
            var description = entry.Description.Length > 50 ? entry.Description.Substring(0, 47) + "..." : entry.Description;
            Console.WriteLine($"{entry.Name,-30} {entry.Version,-15} {description}");
        }
    }
    return 0;
}

static async Task<int> ValidateBlueprint(string blueprintPath, JsonSerializerOptions options, IProvenanceLogger logger)
{
    if (!Directory.Exists(blueprintPath))
    {
        Console.WriteLine($"Error: Directory not found at '{blueprintPath}'");
        return 1;
    }

    var validator = new BlueprintValidator(logger);
    var result = validator.Validate(blueprintPath);

    var output = new
    {
        Status = result.Status,
        Errors = result.Errors,
        Warnings = result.Warnings,
        Blueprint = result.Blueprint != null ? new { Name = result.Blueprint.Name, Version = result.Blueprint.Version } : null
    };

    Console.WriteLine(JsonSerializer.Serialize(output, options));
    return result.IsValid ? 0 : 1;
}

static async Task<int> GenerateConfig(string blueprintPath, string valuesFile, IProvenanceLogger logger)
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
