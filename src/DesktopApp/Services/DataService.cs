using System.Collections.Generic;
using PulsePanel.Core.Models;
using PulsePanel.Blueprints.Models;

namespace PulsePanel.DesktopApp.Services
{
    public class DataService
    {
        public List<Server> GetMockServers()
        {
            return new List<Server>
            {
                new Server { Name = "My Minecraft Server", Status = "Online", Players = 10 },
                new Server { Name = "My Valheim Server", Status = "Offline", Players = 0 },
                new Server { Name = "My ARK Server", Status = "Online", Players = 25 }
            };
        }

        public List<Blueprint> GetMockBlueprints()
        {
            return new List<Blueprint>
            {
                new Blueprint { Name = "Minecraft Java Paper", Version = "1.0.0", Description = "Installs and configures a PaperMC Minecraft Java server" },
                new Blueprint { Name = "Valheim Dedicated Server", Version = "1.0.0", Description = "Installs and configures a Valheim dedicated server" },
                new Blueprint { Name = "ARK Survival Evolved", Version = "1.0.0", Description = "Installs and configures an ARK Survival Evolved server" }
            };
        }
    }

    public class Server
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Players { get; set; }
    }
}

