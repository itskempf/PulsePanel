# PulsePanel – .NET 9 WPF

PulsePanel is a .NET 9 WPF desktop application for managing game server instances. It supports provisioning, start/stop lifecycle, health monitoring, scheduling, and streaming server console output to the UI.

## Solution layout
- PulsePanel: WPF UI
- PulsePanel.Core: Core domain models and services

## Requirements
- .NET 9 SDK
- Windows 10/11
- Visual Studio 2022 17.12+ (recommended)

## Build
- Open `PulsePanel.sln` in Visual Studio and Build, or:
  - dotnet restore
  - dotnet build

## Run
- Set `PulsePanel` as the startup project and run from Visual Studio, or:
  - dotnet run --project ./PulsePanel/PulsePanel.csproj

## Features
- Manage multiple ServerInstance entries
- Start/Stop via IServerProcessService
- Health telemetry (CPU/RAM, ProcessId) via HealthMonitoringService
- Scheduling via SchedulerService
- Provisioning via ProvisioningService and BlueprintService
- Live console output routed to the selected instance in ServersViewModel
- Settings page for paths (Server Install, Backups), update check placeholder, and app version display

## Paths and data
- Blueprints: `PulsePanel/Data/Blueprints` (copied to output)
- Default install root: `C:\PulsePanel\Servers\<guid>`
- Logs: `Data/provenance.log`

## Settings
Open the Settings page from the app sidebar to configure:
- Server install path
- Backups path
- Check for updates (placeholder)

## Contributing
- Create a feature branch from `main`
- Commit with conventional messages
- Open a PR to `main`

## License
Add a LICENSE file if publishing publicly.
