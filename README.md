# PulsePanel - SteamCMD Game Server Manager

A modern WPF dashboard application for managing game servers using SteamCMD with an intuitive interface and advanced features.

## ✨ Key Features

### Dashboard-Centric Design
- **Always-visible server list** and console output for instant situational awareness
- **Vertical tab navigation** for server management when a server is selected
- **Color-coded console output** (Red for errors, Yellow for warnings, White for standard output)
- **Modern UI** with professional styling and intuitive controls

### Core Server Management
- **One-click Start/Stop/Restart** with instant control
- **Intelligent Update/Validate** using SteamCMD with progress tracking
- **Automatic Server Detection** - scans your system for existing game servers
- **Configurable SteamCMD Path** - no more hardcoded paths
- **Real-time monitoring** with system resource tracking

### Server Management Tabs
1. **📊 Status & Info**: Real-time server status, per-process CPU/RAM monitoring, crash detection, and quick actions
2. **⚙️ Configuration**: In-app config file editor with syntax highlighting and templates
3. **🔧 Mods & Workshop**: Steam Workshop ID manager with automatic mod downloads
4. **⏰ Scheduling**: Automated updates, restarts, and backups with flexible scheduling

### Supported Games
- Counter-Strike 2
- Garry's Mod
- Team Fortress 2
- Left 4 Dead 2
- Half-Life Dedicated Server
- Any Source engine game

## 🚀 Quick Start

### Prerequisites
1. **SteamCMD**: Download from [Valve Developer Community](https://developer.valvesoftware.com/wiki/SteamCMD)
2. **.NET 8.0 Runtime**: Required for the application

### First Time Setup
1. Launch PulsePanel
2. Click **⚙️ Settings** to configure your SteamCMD path
3. Click **Scan Servers** to automatically detect existing installations
4. Or click **Add Server** to manually add a new server

## 📖 Usage Guide

### Adding Servers
- **Manual**: Click "Add Server" → Select game → Configure paths and settings
- **Automatic**: Click "Scan Servers" to detect existing installations

### Managing Servers
1. **Select a server** from the left panel
2. **Use toolbar buttons** for Start/Stop/Restart/Update operations
3. **Navigate tabs** to access different management features:
   - **Status**: Monitor server health and resources
   - **Config**: Edit server configurations
   - **Mods**: Manage Workshop content
   - **Schedule**: Set up automated tasks

### Monitoring
- **Console Output**: Real-time logs with color coding and timestamps
- **Server Status**: Live status updates in the server list
- **System Resources**: CPU, RAM, and disk usage monitoring

## ⚙️ Configuration

### Settings (⚙️ Settings Button)
- **SteamCMD Path**: Location of your steamcmd.exe
- **Default Install Path**: Base directory for new server installations

### Default Paths
- **SteamCMD**: `C:\steamcmd\steamcmd.exe`
- **Server Installs**: `C:\GameServers\[GameName]`
- **Default Port**: 27015 (configurable per server)

## 🔧 Advanced Features

### Integrated Resource Monitoring
- **Per-Server Process Tracking**: Real-time CPU and RAM usage for individual server processes
- **Instant Crash Detection**: Aggressive toast notifications when servers crash unexpectedly
- **Live Resource Graphs**: Visual progress bars showing current system usage

### Configuration File Editor (Killer Feature)
- **In-App Editor**: Load and edit game config files directly with syntax highlighting
- **Config Templates**: Pre-built templates for popular games (CS2, Garry's Mod, etc.)
- **Automatic Backup**: Create timestamped backups before making changes
- **Multi-File Support**: Detect and edit multiple config files per server

### Mod/Workshop Manager
- **Direct Workshop ID Input**: Paste Steam Workshop IDs for automatic downloads
- **Batch Processing**: Download multiple mods simultaneously
- **Mod Directory Sync**: Automatic detection and file browser for mod folders
- **Validation Tools**: Verify mod integrity and compatibility

### Scheduled Tasks
- **Automated Updates**: Schedule nightly or weekly server updates
- **Automated Restarts**: Rolling restarts every 6/12/24 hours to clear memory leaks
- **Automated Backups**: Full server backups with configurable retention policies
- **Flexible Scheduling**: Multiple timing options for all automated tasks

### Simplified Firewall Management
- **One-Click Port Opening**: Automatic Windows Firewall rule creation
- **UDP/TCP Support**: Creates both inbound and outbound rules
- **Rule Management**: Automatic cleanup when servers are removed

### Automatic Server Detection
Scans common directories for existing game server installations:
- `C:\GameServers`
- `C:\SteamCMD`
- Steam installation directories
- Custom paths

### Color-Coded Console
- 🔴 **Red**: Errors, crashes, failures
- 🟡 **Yellow**: Warnings, alerts
- ⚪ **White**: Standard output, information
- 🔘 **Gray**: Timestamps

### Quick Actions (Status Tab)
- **📄 View Logs**: Open server log directory
- **📁 Open Folder**: Open server installation folder
- **🔗 Connect**: Launch Steam and connect to server
- **🛡️ Open Firewall**: Automatically create firewall rules for server port

## 🛠️ Troubleshooting

### Common Issues
1. **SteamCMD not found**: Use Settings to set correct path
2. **Server won't start**: Check executable path and permissions
3. **Update fails**: Verify SteamCMD installation and internet connection

### System Requirements
- Windows 10/11
- .NET 8.0 Runtime
- SteamCMD installed
- Appropriate firewall rules for server ports

## 📝 Notes

- Servers run as separate processes for stability with real-time monitoring
- Automatic process cleanup on application exit
- Instant crash detection with toast notifications
- Settings and schedules are saved between sessions
- Automatic firewall management (requires administrator privileges)
- Config file backups are created automatically
- Workshop mods are downloaded to appropriate game directories
- Always test server configurations before production use