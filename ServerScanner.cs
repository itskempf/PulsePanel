using System.IO;

namespace PulsePanel
{
    public static class ServerScanner
    {
        private static readonly Dictionary<string, (string AppId, string GameName)> KnownExecutables = new()
        {
            { "srcds.exe", ("232250", "Source Dedicated Server") },
            { "cs2.exe", ("730", "Counter-Strike 2") },
            { "hlds.exe", ("90", "Half-Life Dedicated Server") },
            { "tf2_server.exe", ("232250", "Team Fortress 2") },
            { "gmod_server.exe", ("4020", "Garry's Mod") }
        };

        public static List<GameServer> ScanForServers(string basePath = @"C:\")
        {
            var foundServers = new List<GameServer>();
            var searchPaths = new[]
            {
                @"C:\GameServers",
                @"C:\SteamCMD",
                @"C:\Steam",
                @"C:\Program Files (x86)\Steam\steamapps\common",
                @"C:\Program Files\Steam\steamapps\common"
            };

            foreach (var searchPath in searchPaths)
            {
                if (Directory.Exists(searchPath))
                {
                    ScanDirectory(searchPath, foundServers);
                }
            }

            return foundServers;
        }

        private static void ScanDirectory(string path, List<GameServer> foundServers)
        {
            try
            {
                foreach (var directory in Directory.GetDirectories(path))
                {
                    foreach (var executable in KnownExecutables.Keys)
                    {
                        var exePath = Path.Combine(directory, executable);
                        if (File.Exists(exePath))
                        {
                            var (appId, gameName) = KnownExecutables[executable];
                            var serverName = Path.GetFileName(directory);
                            
                            var server = new GameServer
                            {
                                Name = serverName,
                                GameName = gameName,
                                AppId = appId,
                                InstallPath = directory,
                                ExecutablePath = exePath,
                                StartupArgs = GetDefaultArgs(appId),
                                Port = 27015
                            };

                            foundServers.Add(server);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }
        }

        private static string GetDefaultArgs(string appId)
        {
            return appId switch
            {
                "730" => "-dedicated +map de_dust2",
                "4020" => "-console -game garrysmod +map gm_flatgrass +maxplayers 16",
                "232250" => "-console -game tf +map cp_dustbowl +maxplayers 24",
                "222860" => "-console -game left4dead2 +map c1m1_hotel",
                _ => "-console"
            };
        }
    }
}