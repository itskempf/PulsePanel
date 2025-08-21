using System.Collections.Generic;
using System.Linq;
using PulsePanel.Core.Models;
using PulsePanel.Core;
using PulsePanel.Core.Services;

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

        public List<PulsePanel.Core.Models.BlueprintCatalogEntry> GetBlueprints()
        {
            var blueprintsRoot = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "blueprints");
            var cachePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data", "cache");
            var catalog = new BlueprintCatalog(blueprintsRoot, cachePath);
            return catalog.GetCatalog().ToList();
        }
    }

    public class Server
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Players { get; set; }
    }
}

