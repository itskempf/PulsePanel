using System.IO;

namespace PulsePanel
{
    public static class ServerScanner
    {
        private static readonly Dictionary<string, (string AppId, string GameName)> KnownExecutables = new()
        {
            // Source Engine Games
            { "srcds.exe", ("232250", "Source Dedicated Server") },
            { "cs2.exe", ("730", "Counter-Strike 2") },
            { "hlds.exe", ("90", "Half-Life Dedicated Server") },
            { "tf2_server.exe", ("232250", "Team Fortress 2") },
            { "gmod_server.exe", ("4020", "Garry's Mod") },
            { "left4dead2.exe", ("222860", "Left 4 Dead 2") },
            
            // Military/Tactical Games
            { "arma3server_x64.exe", ("233780", "Arma 3") },
            { "arma3server.exe", ("233780", "Arma 3") },
            { "SquadGameServer.exe", ("403240", "Squad") },
            { "PostScriptumServer.exe", ("736220", "Post Scriptum") },
            { "HellLetLooseServer.exe", ("686810", "Hell Let Loose") },
            { "InsurgencyServer.exe", ("581330", "Insurgency: Sandstorm") },
            
            // Survival/Crafting Games
            { "RustDedicated.exe", ("258550", "Rust") },
            { "7DaysToDieServer.exe", ("294420", "7 Days to Die") },
            { "TheForestDedicatedServer.exe", ("556450", "The Forest") },
            { "valheim_server.exe", ("896660", "Valheim") },
            { "ProjectZomboidServer.exe", ("380870", "Project Zomboid") },
            { "ConanSandboxServer.exe", ("443030", "Conan Exiles") },
            { "ArkAscendedServer.exe", ("2430930", "ARK: Survival Ascended") },
            { "ShooterGameServer.exe", ("376030", "ARK: Survival Evolved") },
            
            // Racing Games
            { "BeamMP-Server.exe", ("284160", "BeamNG.drive") },
            { "assettocorsa_server.exe", ("244210", "Assetto Corsa") },
            { "rFactor2_Dedicated.exe", ("365960", "rFactor 2") },
            
            // Space/Sci-Fi Games
            { "SpaceEngineersServer.exe", ("298740", "Space Engineers") },
            { "AstronauteerDedicatedServer.exe", ("728470", "Astroneer") },
            { "SatisfactoryServer.exe", ("526870", "Satisfactory") },
            
            // Minecraft-like Games
            { "VintageStoryServer.exe", ("559080", "Vintage Story") },
            { "EcoServer.exe", ("382310", "Eco") },
            
            // Other Popular Games
            { "PalServer.exe", ("2394010", "Palworld") },
            { "EnshroudedServer.exe", ("1203620", "Enshrouded") },
            { "V_RisingServer.exe", ("1604030", "V Rising") },
            { "DayZServer_x64.exe", ("223350", "DayZ") },
            { "UnturnedServer.exe", ("304930", "Unturned") },
            { "FoundryDedicatedServer.exe", ("983870", "Foundry") },
            { "FarmingSimulator22Server.exe", ("1248130", "Farming Simulator 22") }
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
                // Source Engine
                "730" => "-dedicated +map de_dust2 +maxplayers 10",
                "4020" => "-console -game garrysmod +map gm_flatgrass +maxplayers 16",
                "232250" => "-console -game tf +map cp_dustbowl +maxplayers 24",
                "222860" => "-console -game left4dead2 +map c1m1_hotel +maxplayers 8",
                
                // Military/Tactical
                "233780" => "-port=2302 -config=server.cfg -world=VR -profiles=C:\\Arma3\\profiles",
                "403240" => "Port=7787 QueryPort=27165",
                "686810" => "-log -USEALLAVAILABLECORES",
                "581330" => "-Port=27102 -QueryPort=27131",
                
                // Survival/Crafting
                "258550" => "-batchmode -nographics +server.port 28015 +server.maxplayers 100",
                "294420" => "-configfile=serverconfig.xml -logfile logs/output_log.txt",
                "556450" => "-batchmode -nographics -dedicated",
                "896660" => "-nographics -batchmode -name \"My Valheim Server\" -port 2456 -world \"Dedicated\" -password \"secret\"",
                "380870" => "-cachedir=C:\\PZServer -adminusername admin -adminpassword changeme",
                "443030" => "ConanSandbox?listen?MaxPlayers=40?PVP=True",
                "376030" => "TheIsland?listen?SessionName=MyARKServer?MaxPlayers=70",
                "2430930" => "TheIsland_WP?listen?SessionName=MyASAServer?MaxPlayers=70",
                
                // Space/Sci-Fi
                "298740" => "-noconsole -path C:\\SpaceEngineersServer",
                "526870" => "-log -unattended",
                
                // Other Games
                "2394010" => "-useperfthreads -NoAsyncLoadingThread -UseMultithreadForDS",
                "1604030" => "-persistentDataPath C:\\VRisingServer\\save-data -saveName MyVRisingServer",
                "223350" => "-config=serverDZ.cfg -port=2302 -profiles=C:\\DayZServer\\profiles",
                "304930" => "-nographics -batchmode +secureserver/MyServer",
                
                _ => "-console"
            };
        }
    }
}