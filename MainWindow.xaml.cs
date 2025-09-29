using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PulsePanel
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<GameServer> _servers = new();
        private SteamCmdManager _steamCmdManager;
        private GameServer? _selectedServer;
        private string _currentTab = "Status";
        private readonly StatusTabControl _statusTab = new();
        private readonly ConfigTabControl _configTab = new();
        private readonly ModsTabControl _modsTab = new();
        private readonly ScheduleTabControl _scheduleTab = new();
        private string _steamCmdPath = @"C:\steamcmd\steamcmd.exe";

        public MainWindow()
        {
            InitializeComponent();
            _steamCmdManager = new SteamCmdManager(_steamCmdPath);
            ServerListBox.ItemsSource = _servers;
            _steamCmdManager.OutputReceived += OnOutputReceived;
            
            // Wire up tab events
            _statusTab.OutputReceived += OnOutputReceived;
            _configTab.OutputReceived += OnOutputReceived;
            _modsTab.OutputReceived += OnOutputReceived;
            _scheduleTab.OutputReceived += OnOutputReceived;
            
            // Initialize with Status tab
            TabContent.Content = _statusTab;
            UpdateTabButtons();
        }

        private void OnOutputReceived(string output)
        {
            Dispatcher.Invoke(() =>
            {
                var paragraph = new Paragraph();
                var timestamp = new Run($"[{DateTime.Now:HH:mm:ss}] ") { Foreground = Brushes.Gray };
                paragraph.Inlines.Add(timestamp);
                
                // Color-code output based on content
                var textRun = new Run(output);
                if (output.ToLower().Contains("error") || output.ToLower().Contains("failed") || output.ToLower().Contains("crash"))
                    textRun.Foreground = Brushes.Red;
                else if (output.ToLower().Contains("warning") || output.ToLower().Contains("warn"))
                    textRun.Foreground = Brushes.Yellow;
                else
                    textRun.Foreground = Brushes.White;
                
                paragraph.Inlines.Add(textRun);
                ConsoleOutput.Document.Blocks.Add(paragraph);
                ConsoleScroller.ScrollToEnd();
            });
        }

        private void AddServer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddServerDialog { Owner = this };
            if (dialog.ShowDialog() == true && dialog.Server != null)
            {
                dialog.Server.PropertyChanged += (s, e) => 
                {
                    if (e.PropertyName == nameof(GameServer.Status))
                    {
                        Dispatcher.Invoke(() => 
                        {
                            UpdateCurrentTab();
                            UpdateButtonStates();
                        });
                    }
                };
                _servers.Add(dialog.Server);
                OnOutputReceived($"Added server: {dialog.Server.Name}");
            }
        }

        private void ScanServers_Click(object sender, RoutedEventArgs e)
        {
            OnOutputReceived("Scanning for existing game servers...");
            Task.Run(() =>
            {
                var foundServers = ServerScanner.ScanForServers();
                Dispatcher.Invoke(() =>
                {
                    foreach (var server in foundServers)
                    {
                        if (!_servers.Any(s => s.InstallPath == server.InstallPath))
                        {
                            server.PropertyChanged += (s, e) => 
                            {
                                if (e.PropertyName == nameof(GameServer.Status))
                                {
                                    Dispatcher.Invoke(() => 
                                    {
                                        UpdateCurrentTab();
                                        UpdateButtonStates();
                                    });
                                }
                            };
                            _servers.Add(server);
                        }
                    }
                    OnOutputReceived($"Found {foundServers.Count} servers");
                });
            });
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsDialog 
            { 
                Owner = this,
                SteamCmdPath = _steamCmdPath
            };
            
            if (dialog.ShowDialog() == true)
            {
                _steamCmdPath = dialog.SteamCmdPath;
                _steamCmdManager = new SteamCmdManager(_steamCmdPath);
                _steamCmdManager.OutputReceived += OnOutputReceived;
                OnOutputReceived($"Updated SteamCMD path: {_steamCmdPath}");
            }
        }

        private void ServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedServer = ServerListBox.SelectedItem as GameServer;
            UpdateCurrentTab();
            UpdateButtonStates();
        }

        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tabName)
            {
                _currentTab = tabName;
                UpdateTabButtons();
                UpdateCurrentTab();
            }
        }

        private void UpdateTabButtons()
        {
            foreach (Button btn in TabMenu.Children.OfType<Button>())
            {
                if (btn.Tag?.ToString() == _currentTab)
                {
                    btn.Background = new SolidColorBrush(Color.FromRgb(0, 120, 204));
                    btn.Foreground = Brushes.White;
                }
                else
                {
                    btn.Background = Brushes.Transparent;
                    btn.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
                }
            }
        }

        private void UpdateCurrentTab()
        {
            switch (_currentTab)
            {
                case "Status":
                    _statusTab.UpdateServer(_selectedServer);
                    TabContent.Content = _statusTab;
                    break;
                case "Config":
                    _configTab.UpdateServer(_selectedServer);
                    TabContent.Content = _configTab;
                    break;
                case "Mods":
                    _modsTab.UpdateServer(_selectedServer);
                    TabContent.Content = _modsTab;
                    break;
                case "Schedule":
                    _scheduleTab.UpdateServer(_selectedServer);
                    TabContent.Content = _scheduleTab;
                    break;
            }
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _selectedServer != null;
            bool canStart = hasSelection && _selectedServer!.Status == ServerStatus.Stopped;
            bool canStop = hasSelection && _selectedServer!.Status == ServerStatus.Running;
            bool canUpdate = hasSelection && _selectedServer!.Status == ServerStatus.Stopped;

            StartServerBtn.IsEnabled = canStart;
            StopServerBtn.IsEnabled = canStop;
            RestartServerBtn.IsEnabled = canStop;
            UpdateServerBtn.IsEnabled = canUpdate;
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedServer != null)
            {
                Task.Run(() => 
                {
                    _steamCmdManager.StartServer(_selectedServer);
                    Dispatcher.Invoke(UpdateButtonStates);
                });
            }
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedServer != null)
            {
                _steamCmdManager.StopServer(_selectedServer);
                UpdateButtonStates();
            }
        }

        private void RestartServer_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedServer != null)
            {
                Task.Run(async () =>
                {
                    _steamCmdManager.StopServer(_selectedServer);
                    await Task.Delay(2000);
                    _steamCmdManager.StartServer(_selectedServer);
                    Dispatcher.Invoke(UpdateButtonStates);
                });
            }
        }

        private async void UpdateServer_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedServer != null)
            {
                UpdateServerBtn.IsEnabled = false;
                await _steamCmdManager.InstallOrUpdateServerAsync(_selectedServer);
                UpdateButtonStates();
            }
        }

        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Document.Blocks.Clear();
        }
    }
}