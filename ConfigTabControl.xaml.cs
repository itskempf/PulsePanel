using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PulsePanel
{
    public partial class ConfigTabControl : UserControl
    {
        private GameServer? _server;
        private string? _currentConfigPath;
        private bool _hasChanges;

        public event Action<string>? OutputReceived;

        public ConfigTabControl()
        {
            InitializeComponent();
        }

        public void UpdateServer(GameServer? server)
        {
            _server = server;
            RefreshConfigFiles();
        }

        private void RefreshConfigFiles()
        {
            ConfigFileCombo.Items.Clear();
            ConfigEditor.Clear();
            
            if (_server == null || !Directory.Exists(_server.InstallPath))
                return;

            var configFiles = new[]
            {
                "server.cfg", "GameUserSettings.ini", "Game.ini", 
                "ServerSettings.ini", "config.cfg", "autoexec.cfg"
            };

            foreach (var file in configFiles)
            {
                var fullPath = Path.Combine(_server.InstallPath, file);
                if (File.Exists(fullPath))
                {
                    ConfigFileCombo.Items.Add(file);
                }
            }

            if (ConfigFileCombo.Items.Count > 0)
                ConfigFileCombo.SelectedIndex = 0;
        }

        private void ConfigFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConfigFileCombo.SelectedItem is string fileName && _server != null)
            {
                _currentConfigPath = Path.Combine(_server.InstallPath, fileName);
                LoadConfigFile();
            }
        }

        private void LoadConfigFile()
        {
            if (_currentConfigPath == null || !File.Exists(_currentConfigPath))
                return;

            try
            {
                var content = File.ReadAllText(_currentConfigPath);
                ConfigEditor.Text = content;
                _hasChanges = false;
                SaveBtn.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            _hasChanges = true;
            SaveBtn.IsEnabled = true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_currentConfigPath == null) return;

            try
            {
                File.WriteAllText(_currentConfigPath, ConfigEditor.Text);
                _hasChanges = false;
                SaveBtn.IsEnabled = false;
                OutputReceived?.Invoke($"Configuration saved: {Path.GetFileName(_currentConfigPath)}");
                MessageBox.Show("Configuration saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshConfigFiles();
        }

        private void Template_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null) return;

            var template = GetConfigTemplate(_server.GameName);
            if (!string.IsNullOrEmpty(template))
            {
                ConfigEditor.Text = template;
                _hasChanges = true;
                SaveBtn.IsEnabled = true;
            }
        }

        private void Backup_Click(object sender, RoutedEventArgs e)
        {
            if (_currentConfigPath == null || !File.Exists(_currentConfigPath))
                return;

            try
            {
                var backupPath = _currentConfigPath + $".backup.{DateTime.Now:yyyyMMdd_HHmmss}";
                File.Copy(_currentConfigPath, backupPath);
                OutputReceived?.Invoke($"Config backup created: {Path.GetFileName(backupPath)}");
                MessageBox.Show($"Backup created: {Path.GetFileName(backupPath)}", "Backup Created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GetConfigTemplate(string gameName)
        {
            return gameName switch
            {
                "Counter-Strike 2" => @"// Counter-Strike 2 Server Configuration
hostname ""My CS2 Server""
sv_password """"
rcon_password ""changeme""
sv_cheats 0
mp_autoteambalance 1
mp_limitteams 2
mp_roundtime 2
mp_freezetime 15
mp_maxrounds 30",

                "Garry's Mod" => @"// Garry's Mod Server Configuration
hostname ""My GMod Server""
sv_password """"
rcon_password ""changeme""
sbox_maxprops 200
sbox_maxragdolls 10
sbox_maxvehicles 4
sbox_godmode 0
sbox_noclip 0",

                _ => @"// Server Configuration Template
// Edit these settings according to your game's requirements
hostname ""My Game Server""
sv_password """"
rcon_password ""changeme"""
            };
        }
    }
}