
using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using PulsePanel.Core.Services;
using PulsePanel.ViewModels;

namespace PulsePanel
{
    public partial class App : Application
    {
        public static ServiceProvider Services { get; private set; } = null!;
        private IHealthMonitoringService? _health;
        private ISchedulerService? _scheduler;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var sc = new ServiceCollection();

            // Core services
            sc.AddSingleton<IProvenanceLogger, ProvenanceLogger>();
            sc.AddSingleton<IPnccLChecker, PnccLChecker>();
            sc.AddSingleton<IServerInstanceRepository, ServerInstanceRepository>();
            sc.AddSingleton<IServerService, ServerService>();
            sc.AddSingleton<IBlueprintService, BlueprintService>();
            sc.AddSingleton<IProvisioningService, ProvisioningService>();
            sc.AddSingleton<IServerProcessService, ServerProcessService>();
            sc.AddSingleton<IBackupService, BackupService>();
            sc.AddSingleton<IHealthMonitoringService, HealthMonitoringService>();
            sc.AddSingleton<ISchedulerService, SchedulerService>();

            // ViewModels
            sc.AddTransient<ServersViewModel>();
            sc.AddTransient<MarketplaceViewModel>();
            sc.AddTransient<CreateServerDialogViewModel>();
            sc.AddTransient<SchedulerViewModel>();
            sc.AddTransient<BackupsViewModel>();

            Services = sc.BuildServiceProvider();

            _health = Services.GetRequiredService<IHealthMonitoringService>();
            _health.Start();
            _scheduler = Services.GetRequiredService<ISchedulerService>();
            _scheduler.Start();

            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _health?.Dispose();
            _scheduler?.Dispose();
            base.OnExit(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "PulsePanel", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
        private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            // log if needed
        }
    }
}
