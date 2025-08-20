# Blueprint `meta.yaml` Schema

This document specifies the canonical structure for a `meta.yaml` file used in PulsePanel blueprints.

## Top-Level Fields

| Key           | Type                | Required | Description                                                 |
|---------------|---------------------|----------|-------------------------------------------------------------|
| `name`        | `string`            | Yes      | Unique blueprint identifier (lowercase, hyphen-separated).    |
| `version`     | `string`            | Yes      | Semantic version (SemVer) of the blueprint.                 |
| `description` | `string`            | Yes      | A brief description of what the blueprint does.               |
| `author`      | `string`            | Yes      | The name of the person or organization who created it.        |
| `license`     | `string`            | Yes      | A valid SPDX license identifier (e.g., "MIT").                |
| `provenance`  | `object`            | Yes      | An object detailing the origin of the source software.      |
| `files`       | `array` of `object` | No       | A list of files included in the blueprint and their hashes. |
| `requirements`| `object`            | No       | OS and dependency requirements for the blueprint to run.    |
| `tokens`      | `array` of `object` | No       | A list of placeholder tokens used in template files.        |

---

## `provenance` Object

Details the origin of the primary software artifact distributed by the blueprint.

| Key                | Type     | Required | Description                                                     |
|--------------------|----------|----------|-----------------------------------------------------------------|
| `source_url`       | `string` | Yes      | The direct URL from which the software was downloaded.          |
| `source_version`   | `string` | No       | The specific version number of the downloaded software.         |
| `source_hash`      | `string` | Yes      | The SHA256 hash of the downloaded artifact, prefixed `sha256:`. |
| `retrieval_date`   | `string` | Yes      | The RFC3339 timestamp of when the artifact was downloaded.      |
| `retrieval_method` | `enum`   | Yes      | `manual` or `automated`.                                        |

---

## `files` Array Item

Describes a file within the blueprint, typically a template.

| Key    | Type     | Required | Description                                                      |
|--------|----------|----------|------------------------------------------------------------------|
| `path` | `string` | Yes      | Relative path to the file from the blueprint root.               |
| `hash` | `string` | Yes      | The SHA256 hash of the file's content, prefixed `sha256:`.        |

---

## `requirements` Object

Describes the environment needed to run the software.

| Key            | Type                | Required | Description                                          |
|----------------|---------------------|----------|------------------------------------------------------|
| `os`           | `array` of `string` | No       | A list of compatible operating systems (e.g., "windows"). |
| `dependencies` | `array` of `object` | No       | A list of required software dependencies.            |

### `dependencies` Array Item

| Key                    | Type     | Required | Description                                               |
|------------------------|----------|----------|-----------------------------------------------------------|
| `name`                 | `string` | Yes      | The name of the dependency (e.g., "Java").                |
| `version`              | `string` | Yes      | A SemVer version range (e.g., ">=17").                    |
| `install_instructions` | `string` | No       | A URL or text describing how to install the dependency.   |

---

## `tokens` Array Item

Defines a placeholder that can be replaced during configuration generation.

| Key           | Type     | Required | Description                                                        |
|---------------|----------|----------|--------------------------------------------------------------------|
| `key`         | `string` | Yes      | The token identifier, including `{{` and `}}` (e.g., `{{server.port}}`). |
| `description` | `string` | Yes      | A user-friendly description of the token's purpose.                |
| `type`        | `enum`   | Yes      | The data type: `string`, `int`, `bool`, `enum`.                    |
| `required`    | `bool`   | Yes      | Whether a value must be provided for this token.                   |
| `default`     | `any`    | No       | A default value to use if none is provided.                        |
| `enum`        | `array`  | No       | A list of allowed values if `type` is `enum`.                      |

---

## Example

See `blueprints/minecraft-java-paper/meta.yaml` for a complete example.
