
<img width="935" height="227" alt="BCO 45cd2bf0-c155-4721-8423-a638bad6ef49" src="https://github.com/user-attachments/assets/e26c0986-0ec8-4ee8-91ca-28461fc3f44d" />
# PulsePanel

PulsePanel is a server management panel currently transitioning from a web-based tool to a Windows-native application. This project aims to provide robust server management capabilities through a powerful blueprinting system, with a focus on a native desktop experience.

## Key Features

-   **Blueprint System:** Define server configurations using a simple, declarative `meta.yaml` schema.
-   **Configuration Generation:** Generate server files by combining a blueprint with a set of values, using a template engine that supports conditionals.
-   **Validation:** A robust validation pipeline ensures that blueprints are well-formed, secure, and complete.
-   **Provenance Logging:** Every key action (validation, generation) is logged to a JSONL file for audit and traceability.
-   **Extensible:** Designed to be extended with new blueprint types and integrations.

## Project Status

This project is currently undergoing a significant refactoring effort to transition from a web-based application to a standalone Windows-native desktop application (WinUI 3 / WPF). The web UI and API components are being removed, and core logic is being extracted into shared libraries.

**Current State:**
The project is in an active development phase. The build currently has errors as part of this transition.

## License

This project is licensed under the **PulsePanel Non‑Commercial Collaboration License (PNCCL) v1.0**.

You are free to:
- **Use** — run the software and use the assets for any non‑commercial purpose.
- **Share** — copy and redistribute the material in any medium or format.
- **Adapt** — remix, transform, and build upon the material.

Conditions:
- **Attribution** — Credit the original author(s), link to the source repository, and indicate if changes were made.
- **Non‑Commercial** — No commercial use without explicit written permission.
- **ShareAlike** — Distribute derivatives under the same license.
- **Provenance** — Preserve commit history, authorship metadata, and license notices.

This license covers **both source code and non‑code assets** (blueprints, documentation, UI themes, etc.) and incorporates principles from [Creative Commons BY‑NC‑SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) with additional software‑specific terms.

See the full license text in [`/LICENSE`](./LICENSE).

## Running the Desktop App (WinUI 3)

Prerequisites (local Windows dev):

- .NET SDK 9.x (9.0.304 or later)
- Windows 10/11 SDK (10.0.19041+ recommended)
- MSIX Packaging Tools / Windows app packaging components (provide PRI build tasks)

Install the Windows SDK and packaging components via Visual Studio Installer → Modify → Individual components:

- Windows 10 SDK (10.0.19041 or newer) or Windows 11 SDK
- MSIX Packaging Tools
- Optional: C++ (Universal Windows) tools (ensures PRI build tasks availability)

Build and run locally:

```powershell
# From repo root
dotnet build src/PulsePanel.App/PulsePanel.App.csproj -c Debug
```

If you see MSB4062 about `Microsoft.Build.Packaging.Pri.Tasks.dll`, the Windows packaging/PRI tools are missing. Install the prerequisites above and rebuild.

## Solution structure

- src/PulsePanel.App – WinUI 3 desktop shell (MVVM, navigation)
- src/PulsePanel.Core – shared domain/services
- src/PulsePanel.Cli – command-line entry
- src/PulsePanel.Tests – unit tests

## CI

GitHub Actions builds and tests the solution on Windows on each push/PR.
