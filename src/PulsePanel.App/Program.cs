using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using PulsePanel.Core.Services;
using PulsePanel.Windows;

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
