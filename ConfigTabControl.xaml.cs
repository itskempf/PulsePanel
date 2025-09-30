using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PulsePanel
{
    public partial class ConfigTabControl : UserControl
    {
        private GameServer? _server;
        private string? _currentConfigPath;
        private bool _hasChanges;
        private ComboBox _configFileCombo = null!;
        private TextBox _configEditor = null!;
        private Button _saveBtn = null!;
        private Button _refreshBtn = null!;
        private Button _templateBtn = null!;
        private Button _backupBtn = null!;

        public event Action<string>? OutputReceived;

        public ConfigTabControl()
        {
            InitializeComponent();
            CreateUI();
        }

        private void CreateUI()
        {
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Top panel with file selection and buttons
            var topPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
            
            topPanel.Children.Add(new TextBlock { Text = "Config File:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) });
            
            _configFileCombo = new ComboBox { Width = 150, Margin = new Thickness(0, 0, 10, 0) };
            _configFileCombo.SelectionChanged += ConfigFile_SelectionChanged;
            topPanel.Children.Add(_configFileCombo);
            
            _refreshBtn = new Button { Content = "Refresh", Width = 60, Margin = new Thickness(0, 0, 5, 0) };
            _refreshBtn.Click += Refresh_Click;
            topPanel.Children.Add(_refreshBtn);
            
            _templateBtn = new Button { Content = "Template", Width = 70, Margin = new Thickness(0, 0, 5, 0) };
            _templateBtn.Click += Template_Click;
            topPanel.Children.Add(_templateBtn);
            
            _backupBtn = new Button { Content = "Backup", Width = 60, Margin = new Thickness(0, 0, 5, 0) };
            _backupBtn.Click += Backup_Click;
            topPanel.Children.Add(_backupBtn);
            
            _saveBtn = new Button { Content = "Save", Width = 60, IsEnabled = false };
            _saveBtn.Click += Save_Click;
            _saveBtn.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
            _saveBtn.Foreground = Brushes.White;
            topPanel.Children.Add(_saveBtn);
            
            Grid.SetRow(topPanel, 0);
            mainGrid.Children.Add(topPanel);

            // Config editor
            _configEditor = new TextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Foreground = Brushes.White,
                Margin = new Thickness(5)
            };
            _configEditor.TextChanged += ConfigEditor_TextChanged;
            Grid.SetRow(_configEditor, 1);
            mainGrid.Children.Add(_configEditor);

            // Status bar
            var statusBar = new TextBlock
            {
                Text = "Select a server and config file to edit",
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Padding = new Thickness(5),
                Margin = new Thickness(5, 0, 5, 5)
            };
            Grid.SetRow(statusBar, 2);
            mainGrid.Children.Add(statusBar);

            Content = mainGrid;
        }

        public void UpdateServer(GameServer? server)
        {
            _server = server;
            RefreshConfigFiles();
        }

        private void RefreshConfigFiles()
        {
            _configFileCombo.Items.Clear();
            _configEditor.Clear();
            _currentConfigPath = null;
            _hasChanges = false;
            _saveBtn.IsEnabled = false;
            
            if (_server == null)
            {
                OutputReceived?.Invoke("No server selected");
                return;
            }

            if (!Directory.Exists(_server.InstallPath))
            {
                OutputReceived?.Invoke($"Server directory not found: {_server.InstallPath}");
                return;
            }

            // Get template files for this game
            var templateFiles = ConfigTemplates.GetConfigFiles(_server.GameName);
            
            // Add existing files
            foreach (var file in templateFiles)
            {
                var fullPath = Path.Combine(_server.InstallPath, file);
                _configFileCombo.Items.Add(file);
                
                if (File.Exists(fullPath))
                {
                    OutputReceived?.Invoke($"Found config file: {file}");
                }
            }

            // Add any other common config files found
            var commonFiles = new[] { "server.cfg", "config.cfg", "autoexec.cfg", "settings.ini", "server.properties" };
            foreach (var file in commonFiles)
            {
                if (!_configFileCombo.Items.Contains(file))
                {
                    var fullPath = Path.Combine(_server.InstallPath, file);
                    if (File.Exists(fullPath))
                    {
                        _configFileCombo.Items.Add(file);
                        OutputReceived?.Invoke($"Found additional config file: {file}");
                    }
                }
            }

            if (_configFileCombo.Items.Count > 0)
            {
                _configFileCombo.SelectedIndex = 0;
            }
            else
            {
                OutputReceived?.Invoke("No config files found");
            }
        }

        private void ConfigFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_configFileCombo.SelectedItem is string fileName && _server != null)
            {
                _currentConfigPath = Path.Combine(_server.InstallPath, fileName);
                LoadConfigFile();
            }
        }

        private void LoadConfigFile()
        {
            if (_currentConfigPath == null)
                return;

            try
            {
                if (File.Exists(_currentConfigPath))
                {
                    var content = File.ReadAllText(_currentConfigPath);
                    _configEditor.Text = content;
                    OutputReceived?.Invoke($"Loaded config file: {Path.GetFileName(_currentConfigPath)}");
                }
                else
                {
                    // File doesn't exist, show template without creating file
                    var fileName = Path.GetFileName(_currentConfigPath);
                    var template = ConfigTemplates.GetTemplate(_server!.GameName, fileName);
                    _configEditor.Text = template;
                    OutputReceived?.Invoke($"Config file not found, showing template: {fileName}");
                }
                
                _hasChanges = false;
                _saveBtn.IsEnabled = false;
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Error loading config: {ex.Message}");
                _configEditor.Text = "";
            }
        }

        private void ConfigEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            _hasChanges = true;
            _saveBtn.IsEnabled = true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_currentConfigPath == null || _server == null)
                return;

            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(_currentConfigPath)!);
                
                File.WriteAllText(_currentConfigPath, _configEditor.Text);
                _hasChanges = false;
                _saveBtn.IsEnabled = false;
                
                var fileName = Path.GetFileName(_currentConfigPath);
                OutputReceived?.Invoke($"Saved config file: {fileName}");
                new ToastNotification("Config Saved", $"{fileName} saved successfully");
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Error saving config: {ex.Message}");
                new ToastNotification("Save Failed", ex.Message, true);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (_hasChanges)
            {
                var result = MessageBox.Show("You have unsaved changes. Refresh anyway?", "Unsaved Changes", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                    return;
            }
            
            RefreshConfigFiles();
        }

        private void Template_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null || _configFileCombo.SelectedItem is not string fileName)
                return;

            if (_hasChanges)
            {
                var result = MessageBox.Show("This will overwrite your current changes. Continue?", "Apply Template", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                    return;
            }

            var template = ConfigTemplates.GetTemplate(_server.GameName, fileName);
            _configEditor.Text = template;
            _hasChanges = true;
            _saveBtn.IsEnabled = true;
            
            OutputReceived?.Invoke($"Applied {_server.GameName} template for {fileName}");
        }

        private void Backup_Click(object sender, RoutedEventArgs e)
        {
            if (_currentConfigPath == null || !File.Exists(_currentConfigPath))
            {
                OutputReceived?.Invoke("No config file to backup");
                return;
            }

            try
            {
                var backupPath = _currentConfigPath + $".backup.{DateTime.Now:yyyyMMdd_HHmmss}";
                File.Copy(_currentConfigPath, backupPath);
                
                var fileName = Path.GetFileName(_currentConfigPath);
                OutputReceived?.Invoke($"Created backup: {Path.GetFileName(backupPath)}");
                new ToastNotification("Backup Created", $"Backup of {fileName} created successfully");
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Error creating backup: {ex.Message}");
                new ToastNotification("Backup Failed", ex.Message, true);
            }
        }
    }
}