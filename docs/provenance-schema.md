# Provenance Logging Schema

PulsePanel logs key events to a JSONL file for audit and debugging purposes. Each line in the log file is a single JSON object representing one event.

**Default Log Path:** `./data/provenance/log.jsonl`
**Configuration Key:** `PulsePanel:ProvenanceLogPath`

---

## Log Entry Structure

Each log entry contains the following top-level fields:

| Key         | Type     | Description                                                     |
|-------------|----------|-----------------------------------------------------------------|
| `ts`        | `string` | RFC3339 timestamp of the event.                                 |
| `actor`     | `string` | Who or what initiated the event (e.g., "api", "cli", "system"). |
| `action`    | `string` | The type of event that occurred (`validate` or `generate`).     |
| `blueprint` | `object` | Information about the blueprint involved.                       |
| `inputs`    | `object` | Details about the inputs provided for the action.               |
| `results`   | `object` | The outcome of the action.                                      |
| `artifacts` | `object` | Hashes of files involved in the action.                         |
| `license`   | `object` | The result of the license check.                                |
| `env`       | `object` | Information about the application environment.                  |

---

## Example `validate` Event

```json
{
  "ts": "2025-08-20T20:30:00Z",
  "actor": "api",
  "action": "validate",
  "blueprint": {
    "name": "minecraft-java-paper",
    "version": "1.0.0"
  },
  "inputs": {
    "metaPath": "blueprints/minecraft-java-paper/meta.yaml"
  },
  "results": {
    "status": "pass",
    "warnings": [
      {
        "code": "LICENSE_NOTICE",
        "message": "License 'SomeLicense' is not a common SPDX identifier. Please verify it is correct."
      }
    ]
  },
  "license": {
    "id": "MIT",
    "compatible": true
  },
  "env": {
    "platform": "Linux",
    "appVersion": "0.1.0"
  }
}
```

---

## Example `generate` Event

```json
{
  "ts": "2025-08-20T20:35:00Z",
  "actor": "cli",
  "action": "generate",
  "blueprint": {
    "name": "minecraft-java-paper",
    "version": "1.0.0"
  },
  "inputs": {
    "metaPath": "blueprints/minecraft-java-paper/meta.yaml",
    "valuesHash": "sha256:a1b2c3d4..."
  },
  "results": {
    "status": "pass"
  },
  "artifacts": {
    "outputs": [
      {
        "path": "templates/server.properties",
        "hash": "sha256:d7a8fbb3..."
      }
    ]
  },
  "license": {
    "id": "MIT",
    "compatible": true
  },
  "env": {
    "platform": "Linux",
    "appVersion": "0.1.0"
  }
}
```
