using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace PulsePanel.App.ViewModels
{
    public partial class ServerHealthViewModel : ObservableObject
    {
        [ObservableProperty] private string name;
        [ObservableProperty] private string game;
        [ObservableProperty] private string statusText;
        [ObservableProperty] private double cpuPercent;
        [ObservableProperty] private double ramPercent;
        [ObservableProperty] private double diskPercent;
        [ObservableProperty] private double netMbps;
        [ObservableProperty] private ulong pid;
        [ObservableProperty] private uint port;
        [ObservableProperty] private double heartbeatAge;
        [ObservableProperty] private string serviceStatus;
        [ObservableProperty] private TimeSpan uptime;

        public IRelayCommand StartCommand { get; }
        public IRelayCommand StopCommand { get; }
        public IRelayCommand RestartCommand { get; }

        public ServerHealthViewModel()
        {
            StartCommand = new RelayCommand(() => StatusText = "Starting");
            StopCommand = new RelayCommand(() => StatusText = "Stopping");
            RestartCommand = new RelayCommand(() => StatusText = "Restarting");
        }
    }
}
