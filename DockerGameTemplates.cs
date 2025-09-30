using System.Collections.Generic;

namespace PulsePanel
{
    public static class DockerGameTemplates
    {
        public static Dictionary<string, DockerGameTemplate> Templates = new Dictionary<string, DockerGameTemplate>
        {
            ["minecraft"] = new DockerGameTemplate
            {
                Name = "Minecraft Server",
                Image = "itzg/minecraft-server:latest",
                DefaultPorts = new Dictionary<string, string> { ["25565"] = "25565" },
                DefaultEnvironment = new Dictionary<string, string>
                {
                    ["EULA"] = "TRUE",
                    ["TYPE"] = "PAPER",
                    ["MEMORY"] = "2G"
                },
                ConfigPath = "/data"
            },
            ["terraria"] = new DockerGameTemplate
            {
                Name = "Terraria Server",
                Image = "ryshe/terraria:latest",
                DefaultPorts = new Dictionary<string, string> { ["7777"] = "7777" },
                DefaultEnvironment = new Dictionary<string, string>
                {
                    ["WORLD"] = "world1",
                    ["MAXPLAYERS"] = "8"
                },
                ConfigPath = "/config"
            },
            ["teamspeak"] = new DockerGameTemplate
            {
                Name = "TeamSpeak 3 Server",
                Image = "teamspeak:latest",
                DefaultPorts = new Dictionary<string, string> 
                { 
                    ["9987"] = "9987/udp",
                    ["10011"] = "10011",
                    ["30033"] = "30033"
                },
                DefaultEnvironment = new Dictionary<string, string>
                {
                    ["TS3SERVER_LICENSE"] = "accept"
                },
                ConfigPath = "/var/ts3server"
            },
            ["mumble"] = new DockerGameTemplate
            {
                Name = "Mumble Server",
                Image = "phlak/mumble:latest",
                DefaultPorts = new Dictionary<string, string> { ["64738"] = "64738" },
                DefaultEnvironment = new Dictionary<string, string>
                {
                    ["MUMBLE_SERVER_PASSWORD"] = ""
                },
                ConfigPath = "/etc/mumble"
            },
            ["rust-docker"] = new DockerGameTemplate
            {
                Name = "Rust Server (Docker)",
                Image = "didstopia/rust-server:latest",
                DefaultPorts = new Dictionary<string, string> 
                { 
                    ["28015"] = "28015",
                    ["28016"] = "28016"
                },
                DefaultEnvironment = new Dictionary<string, string>
                {
                    ["RUST_SERVER_NAME"] = "My Rust Server",
                    ["RUST_SERVER_SEED"] = "12345",
                    ["RUST_SERVER_WORLDSIZE"] = "3000",
                    ["RUST_SERVER_MAXPLAYERS"] = "50"
                },
                ConfigPath = "/steamcmd/rust"
            },
            ["csgo-docker"] = new DockerGameTemplate
            {
                Name = "CS:GO Server (Docker)",
                Image = "cm2network/csgo:latest",
                DefaultPorts = new Dictionary<string, string> { ["27015"] = "27015" },
                DefaultEnvironment = new Dictionary<string, string>
                {
                    ["SRCDS_TOKEN"] = "",
                    ["SRCDS_HOSTNAME"] = "My CS:GO Server"
                },
                ConfigPath = "/home/steam/csgo-dedicated"
            },
            ["gmod-docker"] = new DockerGameTemplate
            {
                Name = "Garry's Mod Server (Docker)",
                Image = "cm2network/gmod:latest",
                DefaultPorts = new Dictionary<string, string> { ["27015"] = "27015" },
                DefaultEnvironment = new Dictionary<string, string>
                {
                    ["SRCDS_TOKEN"] = "",
                    ["SRCDS_HOSTNAME"] = "My GMod Server",
                    ["SRCDS_GAMEMODE"] = "sandbox"
                },
                ConfigPath = "/home/steam/gmod-dedicated"
            }
        };
    }

    public class DockerGameTemplate
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public Dictionary<string, string> DefaultPorts { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> DefaultEnvironment { get; set; } = new Dictionary<string, string>();
        public string ConfigPath { get; set; }
    }
}