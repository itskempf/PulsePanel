# PulsePanel Concept Map: Re-engineering WindowsGSM Concepts

This document outlines the re-engineered concepts and features from WindowsGSM, adapted for PulsePanel's modular, blueprint-driven, provenance-first, and Windows-native architecture. It also includes PulsePanel-exclusive enhancements.

## Core Directives (Recap)
- **No cloning**: Only extract *ideas*, *workflows*, and *UX patterns* from WindowsGSM.
- **Native-first**: Implement in WinUI 3 or WPF, no webviews or browser dependencies.
- **Provenance-first**: Every blueprint, plugin, and action must log author, license, and hash.
- **Accessibility**: Dyslexia-friendly fonts, high-contrast mode, keyboard navigation.
- **Compliance**: All plugins/blueprints must pass PNCCL checks before activation.
- **Performance**: Lightweight, low-resource footprint, no unnecessary background processes.

## Extracted Feature Concepts from WindowsGSM (Re-engineered for PulsePanel)

### Server Management

| WindowsGSM Concept | PulsePanel Adaptation/Enhancement |
| :----------------- | :-------------------------------- |
| Multi-server dashboard with sortable columns: ID, Mod, Status, Server Name, IP Address, Game Port, Query Port, Map, Player Count. | **Multi-server Dashboard**: A native WinUI 3/WPF dashboard displaying server instances with sortable columns including ID, Game/Mod, Status, Server Name, IP Address, Game Port, Query Port, Map, and Player Count. Data will be sourced from PulsePanel's internal server registry. |
| One-click install for supported games. | **Blueprint-Driven Installer**: Leverages PulsePanel's existing blueprint system for one-click game server installations. The UI will guide users through selecting a blueprint from the **Blueprint Catalog** and configuring initial parameters. |
| Start/Stop/Restart controls with native Windows process management. | **Native Windows Process Management**: Implement robust Start/Stop/Restart controls for server instances, utilizing native Windows APIs for process management (e.g., `System.Diagnostics.Process` in .NET). This ensures direct control and efficient resource handling. |
| Auto restart/update scheduler with per-server rules. | **Integrated Scheduler with Per-Server Rules**: A dedicated UI for configuring automated restart and update schedules for individual server instances. This will integrate with Windows Task Scheduler for reliable execution, allowing for granular control over timing and conditions. |
| Restart crontab (integrated into scheduler UI). | **Crontab-like Scheduling UI**: The scheduler UI will provide a flexible, crontab-like interface for defining complex recurring schedules for server restarts and updates, making it intuitive for users familiar with such patterns. |
| Start on login (via Windows Task Scheduler integration). | **Windows Task Scheduler Integration for Startup**: Option to configure server instances to automatically start when a user logs in, managed via seamless integration with Windows Task Scheduler. |
| CPU priority and affinity controls per server instance. | **Granular CPU Controls**: UI elements to set CPU priority and affinity for each server process, allowing users to optimize resource allocation for their specific server workloads. |

### Backup & Restore

| WindowsGSM Concept | PulsePanel Adaptation/Enhancement |
| :----------------- | :-------------------------------- |
| Snapshot-based backup/restore system. | **Provenance-Linked Snapshot System**: Implement a snapshot-based backup and restore system. Each snapshot will be provenance-linked, including metadata about its creation (author, timestamp) and hash verification to ensure data integrity and immutability. |

### Notifications & Alerts

| WindowsGSM Concept | PulsePanel Adaptation/Enhancement |
| :----------------- | :-------------------------------- |
| Discord/webhook alerts for server status changes, crashes, or updates. (Optional module, user-configurable). | **Configurable Webhook Notifications**: An optional, user-configurable module for sending notifications (e.g., to Discord or other webhooks) on server status changes (start, stop, crash), updates, or other critical events. This will be implemented as a pluggable component. |

### Plugin & Mod Management

| WindowsGSM Concept | PulsePanel Adaptation/Enhancement |
| :----------------- | :-------------------------------- |
| Plugin/mod manager with manifest-based installs. | **Manifest-Based Plugin/Mod Manager**: A robust manager for plugins and mods, supporting manifest-based installations. This will provide a clear interface for browsing, installing, updating, and managing server-specific plugins and mods. |
| Dual-source plugin system (official + community). | **Dual-Source Plugin System**: Support for both official (curated, verified) and community (user-contributed) plugin sources, providing flexibility while maintaining security and quality control. |

## PulsePanel-Exclusive Enhancements

| Enhancement | Description |
| :---------- | :---------- |
| **Blueprint Catalog** | A searchable, filterable catalog of blueprints, featuring trust scoring and provenance badges to help users identify reliable and secure server configurations. |
| **Config Editor** | An integrated configuration editor with syntax highlighting, inline hints, and real-time validation to simplify the process of modifying server configuration files. |
| **DNA Signature** | A system for embedding unique DNA signatures into official PulsePanel assets to prevent unauthorized rebranding or distribution without proper credit. |
| **Audit-Ready Logs** | Comprehensive logging of every action, install, update, and configuration change, including timestamp, author, and cryptographic hash, ensuring full auditability and traceability. |
| **Accessibility Layer** | A dedicated accessibility layer providing dyslexia-friendly fonts, high-contrast mode, and comprehensive keyboard navigation support to ensure usability for all users. |
| **Compliance Gate** | A critical gate that prevents the activation of any non-compliant plugins or blueprints, enforcing adherence to PNCCL (PulsePanel Native Code Compliance Layer) checks. |
