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

                                                            services.AddSingleton<BlueprintLoader>();

                    services.AddSingleton<IActionHandlerFactory, ActionHandlerFactory>();
                    services.AddSingleton<IBlueprintExecutor, BlueprintExecutor>();

                    // Execution Management
                    services.AddSingleton<IExecutionManager, ExecutionManager>();

                    // Node Management and Remote Execution
                    services.AddSingleton<INodeRegistry, InMemoryNodeRegistry>();
                    services.AddHttpClient<IAgentClient, HttpAgentClient>();
                    services.AddSingleton<IRemoteExecutionRouter, RemoteExecutionRouter>();

                    // Windows integrations
                    services.AddSingleton<IWindowsServiceManager, WindowsServiceManager>();
                    services.AddSingleton<ISteamCmdManager, SteamCmdManager>();
                    services.AddSingleton<IFirewallManager, FirewallManager>();

                    // Provenance
                    // Provenance
                    // Provenance
                    // Provenance
                    services.AddSingleton<IProvenanceLogger, ProvenanceLogger>();
                    services.AddSingleton<IProvenanceLogService, ProvenanceLogService>();
                    services.AddSingleton<IProvenanceHistoryService, JsonProvenanceHistoryService>();
                    services.AddSingleton<IProvenanceInsightsService, ProvenanceInsightsService>();
                    services.AddSingleton<IExecutionRecorderFactory, ExecutionRecorderFactory>();

                    // Compliance and Self-Healing
                    services.AddSingleton<IComplianceService, ComplianceService>();
                    services.AddSingleton<ISelfHealingOrchestrator, SelfHealingOrchestrator>();

                    // ViewModels
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<ServersViewModel>();
                                        services.AddTransient(sp => new BlueprintCatalogViewModel(
                        sp.GetRequiredService<BlueprintLoader>(),
                        sp.GetRequiredService<IBlueprintExecutor>(),
                        sp.GetRequiredService<IProvenanceHistoryService>(),
                        sp.GetRequiredService<IRemoteExecutionRouter>(),
                        sp.GetRequiredService<INodeRegistry>(),
                        sp.GetRequiredService<NodesViewModel>()
                    ));
                    services.AddTransient<BlueprintExecutionLogViewModel>();
                    services.AddTransient<BlueprintControlCenterPage>();
                    services.AddTransient<BlueprintHistoryViewModel>();
                    services.AddTransient<JobQueueViewModel>();
                    services.AddTransient<NodesViewModel>();
                    services.AddTransient<NodesAdminViewModel>();
                    services.AddTransient<ComplianceDashboardViewModel>();

                    // Pages
                    services.AddTransient<Views.JobQueuePage>();
                    services.AddTransient<Views.NodesPage>();
                                        services.AddTransient<Views.ComplianceDashboardPage>();
                    services.AddTransient<Views.ShellPage>();
                });
    }
}


