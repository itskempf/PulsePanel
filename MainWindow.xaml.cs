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
            KeyboardShortcuts.RegisterShortcuts(this, this);
            LoadSettings();
            _steamCmdManager = new SteamCmdManager(_steamCmdPath);
            ServerListBox.ItemsSource = _servers;
            _steamCmdManager.OutputReceived += OnOutputReceived;
            
            _statusTab.OutputReceived += OnOutputReceived;
            _configTab.OutputReceived += OnOutputReceived;
            _modsTab.OutputReceived += OnOutputReceived;
            _scheduleTab.OutputReceived += OnOutputReceived;
            
            TabContent.Content = _statusTab;
            UpdateTabButtons();
            SetupServerListContextMenu();
        }

        private async void LoadSettings()
        {
            try
            {
                var settings = await Task.Run(() => SettingsManager.LoadSettings());
                _steamCmdPath = settings.SteamCmdPath;
                
                foreach (var server in settings.Servers)
                {
                    server.PropertyChanged += (s, e) => 
                    {
                        if (e.PropertyName == nameof(GameServer.Status))
                        {
                            Dispatcher.BeginInvoke(() => 
                            {
                                UpdateCurrentTab();
                                UpdateButtonStates();
                            });
                        }
                    };
                    _servers.Add(server);
                }
                
                OnOutputReceived($"Loaded {settings.Servers.Count} servers from settings");
                Logger.LogInfo($"Application started with {settings.Servers.Count} servers");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to load settings", ex);
                OnOutputReceived("Error loading settings - using defaults");
            }
        }

        private async void SaveSettings()
        {
            try
            {
                var settings = new AppSettings
                {
                    SteamCmdPath = _steamCmdPath,
                    Servers = _servers.ToList()
                };
                await Task.Run(() => SettingsManager.SaveSettings(settings));
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to save settings", ex);
                OnOutputReceived("Warning: Settings could not be saved");
            }
        }

        private void OnOutputReceived(string output)
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (ConsoleOutput.Document.Blocks.Count > 1000)
                    {
                        var blocksToRemove = ConsoleOutput.Document.Blocks.Take(500).ToList();
                        foreach (var block in blocksToRemove)
                            ConsoleOutput.Document.Blocks.Remove(block);
                    }
                    
                    var paragraph = new Paragraph();
                    var timestamp = new Run($"[{DateTime.Now:HH:mm:ss}] ") { Foreground = Brushes.Gray };
                    paragraph.Inlines.Add(timestamp);
                    
                    var textRun = new Run(output);
                    var lowerOutput = output.ToLower();
                    if (lowerOutput.Contains("error") || lowerOutput.Contains("failed") || lowerOutput.Contains("crash"))
                        textRun.Foreground = Brushes.Red;
                    else if (lowerOutput.Contains("warning") || lowerOutput.Contains("warn"))
                        textRun.Foreground = Brushes.Yellow;
                    else
                        textRun.Foreground = Brushes.White;
                    
                    paragraph.Inlines.Add(textRun);
                    ConsoleOutput.Document.Blocks.Add(paragraph);
                    ConsoleScroller.ScrollToEnd();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Console output error", ex);
                }
            });
        }

        private void SetupServerListContextMenu()
        {
            var contextMenu = new ContextMenu();
            
            var startItem = new MenuItem { Header = "Start Server" };
            startItem.Click += (s, e) => StartServer_Click(s, null);
            contextMenu.Items.Add(startItem);
            
            var stopItem = new MenuItem { Header = "Stop Server" };
            stopItem.Click += (s, e) => StopServer_Click(s, null);
            contextMenu.Items.Add(stopItem);
            
            var restartItem = new MenuItem { Header = "Restart Server" };
            restartItem.Click += (s, e) => RestartServer_Click(s, null);
            contextMenu.Items.Add(restartItem);
            
            contextMenu.Items.Add(new Separator());
            
            var updateItem = new MenuItem { Header = "Update Server" };
            updateItem.Click += (s, e) => UpdateServer_Click(s, null);
            contextMenu.Items.Add(updateItem);
            
            var templateItem = new MenuItem { Header = "Save as Template" };
            templateItem.Click += SaveAsTemplate_Click;
            contextMenu.Items.Add(templateItem);
            
            ServerListBox.ContextMenu = contextMenu;
        }

        private void SaveAsTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedServer == null) return;
            
            var dialog = new InputDialog("Enter template name:", "Save Server Template", _selectedServer.Name + " Template")
            {
                Owner = this
            };
                
            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.InputText))
            {
                try
                {
                    ServerTemplateManager.SaveTemplate(_selectedServer, dialog.InputText);
                    OnOutputReceived($"Server template '{dialog.InputText}' saved successfully");
                    new ToastNotification("Template Saved", $"Template '{dialog.InputText}' created");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to save server template", ex);
                    OnOutputReceived($"Failed to save template: {ex.Message}");
                }
            }
        }

        public void SwitchTab(string tabName)
        {
            _currentTab = tabName;
            UpdateTabButtons();
            UpdateCurrentTab();
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
                    btn.Background = new SolidColorBrush(Color.FromRgb(74, 144, 226));
                    btn.Foreground = Brushes.White;
                }
                else
                {
                    btn.Background = Brushes.Transparent;
                    btn.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
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

        public void StartServer_Click(object sender, RoutedEventArgs? e)
        {
            if (_selectedServer != null)
            {
                Task.Run(() => 
                {
                    var success = _steamCmdManager.StartServer(_selectedServer);
                    if (success) MetricsCollector.RegisterServer(_selectedServer);
                    Dispatcher.Invoke(UpdateButtonStates);
                });
            }
        }

        public void StopServer_Click(object sender, RoutedEventArgs? e)
        {
            if (_selectedServer != null)
            {
                _steamCmdManager.StopServer(_selectedServer);
                MetricsCollector.UnregisterServer(_selectedServer);
                UpdateButtonStates();
            }
        }

        public void RestartServer_Click(object sender, RoutedEventArgs? e)
        {
            if (_selectedServer != null)
            {
                Task.Run(async () =>
                {
                    _steamCmdManager.StopServer(_selectedServer);
                    MetricsCollector.UnregisterServer(_selectedServer);
                    await Task.Delay(2000);
                    var success = _steamCmdManager.StartServer(_selectedServer);
                    if (success) MetricsCollector.RegisterServer(_selectedServer);
                    Dispatcher.Invoke(UpdateButtonStates);
                });
            }
        }

        public async void UpdateServer_Click(object sender, RoutedEventArgs? e)
        {
            if (_selectedServer != null)
            {
                UpdateServerBtn.IsEnabled = false;
                await _steamCmdManager.InstallOrUpdateServerAsync(_selectedServer);
                UpdateButtonStates();
            }
        }

        public void AddServer_Click(object sender, RoutedEventArgs? e)
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
                SaveSettings();
                OnOutputReceived($"Added server: {dialog.Server.Name}");
            }
        }

        public async void ScanServers_Click(object sender, RoutedEventArgs? e)
        {
            ScanServersBtn.IsEnabled = false;
            OnOutputReceived("Scanning for existing game servers...");
            
            try
            {
                var foundServers = await RetryHelper.RetryAsync(() => 
                    Task.Run(() => ServerScanner.ScanForServers()), 2);
                    
                foreach (var server in foundServers)
                {
                    if (!_servers.Any(s => s.InstallPath == server.InstallPath))
                    {
                        server.PropertyChanged += (s, e) => 
                        {
                            if (e.PropertyName == nameof(GameServer.Status))
                            {
                                Dispatcher.BeginInvoke(() => 
                                {
                                    UpdateCurrentTab();
                                    UpdateButtonStates();
                                });
                            }
                        };
                        _servers.Add(server);
                    }
                }
                SaveSettings();
                OnOutputReceived($"Found {foundServers.Count} servers");
            }
            catch (Exception ex)
            {
                Logger.LogError("Server scan failed", ex);
                OnOutputReceived($"Server scan failed: {ex.Message}");
            }
            finally
            {
                ScanServersBtn.IsEnabled = true;
            }
        }

        public void Settings_Click(object sender, RoutedEventArgs? e)
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
                SaveSettings();
                OnOutputReceived($"Updated SteamCMD path: {_steamCmdPath}");
            }
        }

        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Document.Blocks.Clear();
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                SaveSettings();
                _statusTab?.Dispose();
                PerformanceCache.Clear();
                MetricsCollector.Dispose();
                Logger.LogInfo("Application closed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during application shutdown", ex);
            }
            finally
            {
                base.OnClosed(e);
            }
        }
    }
}