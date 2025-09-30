# PulsePanel - Professional Game Server Manager

A modern WPF application for managing game servers using SteamCMD with an intuitive interface and advanced features. Supports 40+ popular games with comprehensive configuration management.

![PulsePanel](https://img.shields.io/badge/Version-1.0-blue) ![.NET](https://img.shields.io/badge/.NET-8.0-purple) ![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey)

## ✨ Key Features

### 🎮 Extensive Game Support (40+ Games)
- **Source Engine**: Counter-Strike 2, Garry's Mod, Team Fortress 2, Left 4 Dead 2, Half-Life
- **Military/Tactical**: Arma 3, Squad, Hell Let Loose, Insurgency: Sandstorm, Post Scriptum
- **Survival/Crafting**: Rust, 7 Days to Die, Valheim, ARK (both versions), Project Zomboid, Conan Exiles, The Forest
- **Space/Sci-Fi**: Space Engineers, Satisfactory, Astroneer
- **Racing**: BeamNG.drive, Assetto Corsa, rFactor 2
- **Popular**: Palworld, V Rising, DayZ, Unturned, Enshrouded, Foundry, Farming Simulator 22

### 📝 Professional Configuration Editor
- **Syntax-highlighted editor** with dark theme and Consolas font
- **Game-specific templates** for instant configuration
- **Backup system** with timestamp-based versioning
- **Real-time change tracking** and validation
- **Multi-file support** for complex game configurations

### 🖥️ Dashboard-Centric Design
- **Always-visible server list** with real-time status updates
- **Color-coded console output** (Red for errors, Yellow for warnings, White for info)
- **Vertical tab navigation** for server management
- **Modern UI** with professional styling and intuitive controls

### ⚙️ Core Server Management
- **One-click Start/Stop/Restart** with instant control
- **Intelligent Update/Validate** using SteamCMD with progress tracking
- **Automatic Server Detection** - scans your system for existing game servers
- **Configurable SteamCMD Path** - no more hardcoded paths
- **Real-time monitoring** with process tracking and crash detection

### 🔧 Advanced Features
- **Toast Notifications** for all server events and crashes
- **Settings Persistence** - all configurations saved automatically
- **Process Monitoring** with CPU and RAM tracking
- **Firewall Management** for automatic port configuration
- **Template System** with pre-built configs for all supported games

## 🚀 Quick Start

### Prerequisites
1. **SteamCMD**: Download from [Valve Developer Community](https://developer.valvesoftware.com/wiki/SteamCMD)
2. **.NET 8.0 Runtime**: Required for the application

### Installation
1. Download the latest release from [Releases](https://github.com/itskempf/PulsePanel/releases)
2. Extract to your desired location
3. Run `PulsePanel.exe`

### First Time Setup
1. Launch PulsePanel
2. Click **⚙️ Settings** to configure your SteamCMD path
3. Click **Scan Servers** to automatically detect existing installations
4. Or click **Add Server** to manually add a new server from 40+ supported games

## 📖 Usage Guide

### Adding Servers
- **Manual**: Click "Add Server" → Select from 40+ games → Configure paths and settings
- **Automatic**: Click "Scan Servers" to detect existing installations

### Managing Servers
1. **Select a server** from the left panel
2. **Use toolbar buttons** for Start/Stop/Restart/Update operations
3. **Navigate tabs** to access different management features:
   - **Status**: Monitor server health, resources, and quick actions
   - **Config**: Edit server configurations with templates and syntax highlighting
   - **Mods**: Manage Workshop content and mod installations
   - **Schedule**: Set up automated tasks and backups

### Configuration Management
- **File Editor**: Professional text editor with syntax highlighting
- **Templates**: Pre-built configurations for all supported games
- **Backups**: Automatic timestamped backups before changes
- **Validation**: Real-time change tracking and error detection

## 🎯 Supported Games & Templates

### Source Engine Games
- Counter-Strike 2 (`server.cfg`)
- Garry's Mod (`server.cfg`)
- Team Fortress 2 (`server.cfg`)
- Left 4 Dead 2 (`server.cfg`)
- Half-Life Dedicated Server (`server.cfg`)

### Military/Tactical Games
- Arma 3 (`server.cfg`)
- Squad (`Server.cfg`)
- Hell Let Loose (startup parameters)
- Insurgency: Sandstorm (startup parameters)
- Post Scriptum (startup parameters)

### Survival/Crafting Games
- Rust (`server.cfg`)
- 7 Days to Die (`serverconfig.xml`)
- Valheim (`start_server.bat`)
- ARK: Survival Evolved (`GameUserSettings.ini`)
- ARK: Survival Ascended (`GameUserSettings.ini`)
- Project Zomboid (startup parameters)
- Conan Exiles (startup parameters)
- The Forest (startup parameters)

### Space/Sci-Fi Games
- Space Engineers (startup parameters)
- Satisfactory (startup parameters)
- Astroneer (startup parameters)

### Other Popular Games
- Palworld (startup parameters)
- V Rising (startup parameters)
- DayZ (`serverDZ.cfg`)
- Unturned (startup parameters)
- Enshrouded (startup parameters)
- Foundry (startup parameters)
- Farming Simulator 22 (startup parameters)

## ⚙️ Configuration Templates

Each supported game includes professionally crafted configuration templates:

- **Counter-Strike 2**: Complete server.cfg with competitive settings
- **Garry's Mod**: Sandbox configuration with prop limits and permissions
- **Arma 3**: Full server.cfg with mission rotation and BattlEye settings
- **Rust**: Comprehensive server configuration with PvP/PvE options
- **7 Days to Die**: Complete XML configuration with all game settings
- **Valheim**: Batch file template with server parameters
- **ARK**: GameUserSettings.ini with multipliers and server options
- **DayZ**: Complete serverDZ.cfg with all gameplay settings

## 🔧 Advanced Features

### Real-time Process Monitoring
- **CPU and RAM tracking** for individual server processes
- **Crash detection** with instant toast notifications
- **Resource usage alerts** and performance monitoring

### Professional Configuration Editor
- **Syntax highlighting** with dark theme
- **Change tracking** with unsaved changes warnings
- **Template system** with game-specific configurations
- **Backup creation** with timestamp-based versioning
- **Multi-file support** for complex game setups

### Automated Management
- **Server scanning** for automatic detection
- **Settings persistence** across application restarts
- **Toast notifications** for all events and errors
- **Firewall management** for automatic port configuration

## 🛠️ System Requirements

- **OS**: Windows 10/11
- **Runtime**: .NET 8.0
- **Dependencies**: SteamCMD installed
- **Permissions**: Administrator privileges recommended for firewall management

## 📝 Development

### Building from Source
```bash
git clone https://github.com/itskempf/PulsePanel.git
cd PulsePanel
dotnet restore
dotnet build
```

### Project Structure
- **Core Classes**: GameServer, SteamCmdManager, ProcessMonitor
- **UI Components**: MainWindow, StatusTabControl, ConfigTabControl, ModsTabControl, ScheduleTabControl
- **Configuration**: ConfigTemplates, SettingsManager
- **Utilities**: ServerScanner, FirewallManager, ToastNotification

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Valve Corporation** for SteamCMD
- **Game Developers** for creating amazing server software
- **Community** for feedback and feature requests

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/itskempf/PulsePanel/issues)
- **Discussions**: [GitHub Discussions](https://github.com/itskempf/PulsePanel/discussions)
- **Wiki**: [Project Wiki](https://github.com/itskempf/PulsePanel/wiki)

---

**PulsePanel** - Professional Game Server Management Made Simple 🎮