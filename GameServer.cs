using System.ComponentModel;
using System.Diagnostics;

namespace PulsePanel
{
    public enum ServerStatus
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Updating,
        Crashed
    }

    public class GameServer : INotifyPropertyChanged
    {
        private ServerStatus _status = ServerStatus.Stopped;
        private Process? _serverProcess;

        public string Name { get; set; } = "";
        public string GameName { get; set; } = "";
        public string AppId { get; set; } = "";
        public string InstallPath { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
        public string StartupArgs { get; set; } = "";
        public int Port { get; set; } = 27015;

        public ServerStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public Process? ServerProcess
        {
            get => _serverProcess;
            set => _serverProcess = value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{Name} ({GameName}) - {Status}";
        }
    }
}