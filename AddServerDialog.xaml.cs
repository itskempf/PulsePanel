using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PulsePanel
{
    public partial class AddServerDialog : Window
    {
        public GameServer? Server { get; private set; }

        public AddServerDialog()
        {
            InitializeComponent();
        }

        private void GameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameComboBox.SelectedItem is ComboBoxItem item)
            {
                AppIdBox.Text = item.Tag?.ToString() ?? "";
                
                var gameName = item.Content?.ToString() ?? "";
                if (!string.IsNullOrEmpty(gameName))
                {
                    var defaultPath = Path.Combine(@"C:\GameServers", gameName.Replace(":", "").Replace(" ", ""));
                    InstallPathBox.Text = defaultPath;
                    
                    // Set default executable and args based on game
                    var appId = item.Tag?.ToString();
                    switch (appId)
                    {
                        // Source Engine Games
                        case "730": // CS2
                            ExecutableBox.Text = Path.Combine(defaultPath, "game", "bin", "win64", "cs2.exe");
                            break;
                        case "4020": // Garry's Mod
                        case "232250": // TF2
                        case "222860": // L4D2
                        case "90": // Half-Life
                            ExecutableBox.Text = Path.Combine(defaultPath, "srcds.exe");
                            break;
                            
                        // Military/Tactical Games
                        case "233780": // Arma 3
                            ExecutableBox.Text = Path.Combine(defaultPath, "arma3server_x64.exe");
                            break;
                        case "403240": // Squad
                            ExecutableBox.Text = Path.Combine(defaultPath, "SquadGameServer.exe");
                            break;
                        case "686810": // Hell Let Loose
                            ExecutableBox.Text = Path.Combine(defaultPath, "HellLetLooseServer.exe");
                            break;
                        case "581330": // Insurgency
                            ExecutableBox.Text = Path.Combine(defaultPath, "InsurgencyServer.exe");
                            break;
                            
                        // Survival/Crafting Games
                        case "258550": // Rust
                            ExecutableBox.Text = Path.Combine(defaultPath, "RustDedicated.exe");
                            break;
                        case "294420": // 7 Days to Die
                            ExecutableBox.Text = Path.Combine(defaultPath, "7DaysToDieServer.exe");
                            break;
                        case "556450": // The Forest
                            ExecutableBox.Text = Path.Combine(defaultPath, "TheForestDedicatedServer.exe");
                            break;
                        case "896660": // Valheim
                            ExecutableBox.Text = Path.Combine(defaultPath, "valheim_server.exe");
                            break;
                        case "380870": // Project Zomboid
                            ExecutableBox.Text = Path.Combine(defaultPath, "ProjectZomboidServer.exe");
                            break;
                        case "443030": // Conan Exiles
                            ExecutableBox.Text = Path.Combine(defaultPath, "ConanSandboxServer.exe");
                            break;
                        case "376030": // ARK: Survival Evolved
                            ExecutableBox.Text = Path.Combine(defaultPath, "ShooterGameServer.exe");
                            break;
                        case "2430930": // ARK: Survival Ascended
                            ExecutableBox.Text = Path.Combine(defaultPath, "ArkAscendedServer.exe");
                            break;
                            
                        // Space/Sci-Fi Games
                        case "298740": // Space Engineers
                            ExecutableBox.Text = Path.Combine(defaultPath, "SpaceEngineersServer.exe");
                            break;
                        case "526870": // Satisfactory
                            ExecutableBox.Text = Path.Combine(defaultPath, "SatisfactoryServer.exe");
                            break;
                            
                        // Other Popular Games
                        case "2394010": // Palworld
                            ExecutableBox.Text = Path.Combine(defaultPath, "PalServer.exe");
                            break;
                        case "1604030": // V Rising
                            ExecutableBox.Text = Path.Combine(defaultPath, "V_RisingServer.exe");
                            break;
                        case "223350": // DayZ
                            ExecutableBox.Text = Path.Combine(defaultPath, "DayZServer_x64.exe");
                            break;
                        case "304930": // Unturned
                            ExecutableBox.Text = Path.Combine(defaultPath, "UnturnedServer.exe");
                            break;
                            
                        default:
                            ExecutableBox.Text = Path.Combine(defaultPath, "server.exe");
                            break;
                    }
                    
                    // Set startup arguments from ServerScanner
                    var args = GetDefaultArgsForAppId(appId ?? "");
                    StartupArgsBox.Text = args;
                }
            }
        }

        private void BrowseInstallPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Select Installation Directory",
                FileName = "Select Folder",
                DefaultExt = "folder",
                Filter = "Folder|*.folder"
            };

            if (dialog.ShowDialog() == true)
            {
                InstallPathBox.Text = Path.GetDirectoryName(dialog.FileName) ?? "";
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServerNameBox.Text))
            {
                MessageBox.Show("Please enter a server name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (GameComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a game.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(PortBox.Text, out int port) || port <= 0 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port number (1-65535).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedGame = (ComboBoxItem)GameComboBox.SelectedItem;
            
            Server = new GameServer
            {
                Name = ServerNameBox.Text.Trim(),
                GameName = selectedGame.Content?.ToString() ?? "",
                AppId = AppIdBox.Text.Trim(),
                InstallPath = InstallPathBox.Text.Trim(),
                ExecutablePath = ExecutableBox.Text.Trim(),
                StartupArgs = StartupArgsBox.Text.Trim(),
                Port = port
            };

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        
        private static string GetDefaultArgsForAppId(string appId)
        {
            return appId switch
            {
                // Source Engine
                "730" => "-dedicated +map de_dust2 +maxplayers 10",
                "4020" => "-console -game garrysmod +map gm_flatgrass +maxplayers 16",
                "232250" => "-console -game tf +map cp_dustbowl +maxplayers 24",
                "222860" => "-console -game left4dead2 +map c1m1_hotel +maxplayers 8",
                "90" => "-console +map crossfire +maxplayers 16",
                
                // Military/Tactical
                "233780" => "-port=2302 -config=server.cfg -world=VR",
                "403240" => "Port=7787 QueryPort=27165",
                "686810" => "-log -USEALLAVAILABLECORES",
                "581330" => "-Port=27102 -QueryPort=27131",
                
                // Survival/Crafting
                "258550" => "-batchmode -nographics +server.port 28015 +server.maxplayers 100",
                "294420" => "-configfile=serverconfig.xml",
                "556450" => "-batchmode -nographics -dedicated",
                "896660" => "-nographics -batchmode -name \"My Server\" -port 2456 -world \"Dedicated\" -password \"secret\"",
                "380870" => "-adminusername admin -adminpassword changeme",
                "443030" => "ConanSandbox?listen?MaxPlayers=40",
                "376030" => "TheIsland?listen?SessionName=MyServer?MaxPlayers=70",
                "2430930" => "TheIsland_WP?listen?SessionName=MyServer?MaxPlayers=70",
                
                // Space/Sci-Fi
                "298740" => "-noconsole",
                "526870" => "-log -unattended",
                
                // Other Games
                "2394010" => "-useperfthreads -NoAsyncLoadingThread",
                "1604030" => "-persistentDataPath ./save-data -saveName MyServer",
                "223350" => "-config=serverDZ.cfg -port=2302",
                "304930" => "-nographics -batchmode",
                
                _ => "-console"
            };
        }
    }
}