# PulsePanel

PulsePanel is a deterministic, provenance-first server management panel, starting with game servers. This project provides the core backend API, a command-line interface (CLI), and a minimal web UI for managing server configurations through a powerful blueprinting system.

## Key Features

-   **Blueprint System:** Define server configurations using a simple, declarative `meta.yaml` schema.
-   **Configuration Generation:** Generate server files by combining a blueprint with a set of values, using a template engine that supports conditionals.
-   **Validation:** A robust validation pipeline ensures that blueprints are well-formed, secure, and complete.
-   **Provenance Logging:** Every key action (validation, generation) is logged to a JSONL file for audit and traceability.
-   **Extensible:** Designed to be extended with new blueprint types and integrations.

## Getting Started (Linux Development)

This project is built on .NET 8.

### Running the API

The main backend API can be run from the `src/PulsePanel.Api` directory. Assuming you have the .NET 8 SDK installed:

```bash
# From the repository root
cd src/PulsePanel.Api
dotnet run
```

The API will start, and by default, it will also serve the static Web UI. You can access the UI at `http://localhost:8070` (or the configured port).

### Using the CLI

The CLI provides command-line access to the core blueprint features.

```bash
# From the repository root
# (Build the CLI first if you haven't)
dotnet build src/PulsePanel.Cli

# Example: Validate a blueprint
./src/PulsePanel.Cli/bin/Debug/net8.0/pulsepanel validate-blueprint blueprints/minecraft-java-paper

# Example: List all blueprints
./src/PulsePanel.Cli/bin/Debug/net8.0/pulsepanel list-blueprints
```

## Documentation

-   **[Blueprint Schema](./docs/blueprint-schema.md):** The canonical specification for `meta.yaml` files.
-   **[Provenance Schema](./docs/provenance-schema.md):** The structure of the JSONL audit logs.
-   **[API Documentation](./docs/api.md):** Details on the available REST API endpoints.