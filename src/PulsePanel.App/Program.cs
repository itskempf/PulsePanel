using PulsePanel.App.Services;
using System.IO;
using System;

namespace PulsePanel.App
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            Application.Start(p =>
            {
                var app = new App
                {
                    Services = host.Services
                };
            });
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Core services
                    services.AddSingleton<IConfigValidationProvider, ConfigValidationProvider>();
                    services.AddSingleton<IConfigDiffService, ConfigDiffService>();
                    services.AddSingleton<IServerProcessService, ServerProcessService>();
                    services.AddSingleton<IStorageManager, StorageManager>();

                    services.AddSingleton(sp =>
                        new BlueprintLoader(Path.Combine(AppContext.BaseDirectory, "Assets", "Blueprints")));

                    // Windows integrations
                    services.AddSingleton<IWindowsServiceManager, WindowsServiceManager>();
                    services.AddSingleton<ISteamCmdManager, SteamCmdManager>();
                    services.AddSingleton<IFirewallManager, FirewallManager>();

                    // Provenance
                    services.AddSingleton<IProvenanceLogger, ProvenanceLogger>();

                    // ViewModels
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<ServersViewModel>();
                });
    }
}

