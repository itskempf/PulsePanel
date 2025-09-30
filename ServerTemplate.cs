using System.IO;
using System.Text.Json;

namespace PulsePanel
{
    public class ServerTemplate
    {
        public string Name { get; set; } = "";
        public string GameName { get; set; } = "";
        public string AppId { get; set; } = "";
        public string StartupArgs { get; set; } = "";
        public int Port { get; set; } = 27015;
        public Dictionary<string, string> ConfigFiles { get; set; } = new();
        public DateTime Created { get; set; } = DateTime.Now;
    }

    public static class ServerTemplateManager
    {
        private static readonly string TemplatesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PulsePanel", "templates");

        static ServerTemplateManager()
        {
            Directory.CreateDirectory(TemplatesPath);
        }

        public static void SaveTemplate(GameServer server, string templateName)
        {
            var template = new ServerTemplate
            {
                Name = templateName,
                GameName = server.GameName,
                AppId = server.AppId,
                StartupArgs = server.StartupArgs,
                Port = server.Port
            };

            // Save config files
            var configFiles = ConfigTemplates.GetConfigFiles(server.GameName);
            foreach (var configFile in configFiles)
            {
                var configPath = Path.Combine(server.InstallPath, configFile);
                if (File.Exists(configPath))
                {
                    template.ConfigFiles[configFile] = File.ReadAllText(configPath);
                }
            }

            var templatePath = Path.Combine(TemplatesPath, $"{templateName}.json");
            var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(templatePath, json);
        }

        public static List<ServerTemplate> GetTemplates()
        {
            var templates = new List<ServerTemplate>();
            
            foreach (var file in Directory.GetFiles(TemplatesPath, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var template = JsonSerializer.Deserialize<ServerTemplate>(json);
                    if (template != null)
                        templates.Add(template);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Failed to load template {file}: {ex.Message}");
                }
            }

            return templates.OrderBy(t => t.Name).ToList();
        }

        public static GameServer CreateFromTemplate(ServerTemplate template, string serverName, string installPath)
        {
            var server = new GameServer
            {
                Name = serverName,
                GameName = template.GameName,
                AppId = template.AppId,
                InstallPath = installPath,
                StartupArgs = template.StartupArgs,
                Port = template.Port,
                ExecutablePath = Path.Combine(installPath, "server.exe") // Will be updated when server is scanned
            };

            // Create config files from template
            Directory.CreateDirectory(installPath);
            foreach (var configFile in template.ConfigFiles)
            {
                var configPath = Path.Combine(installPath, configFile.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
                File.WriteAllText(configPath, configFile.Value);
            }

            return server;
        }

        public static void DeleteTemplate(string templateName)
        {
            var templatePath = Path.Combine(TemplatesPath, $"{templateName}.json");
            if (File.Exists(templatePath))
                File.Delete(templatePath);
        }
    }
}