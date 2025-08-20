# PulsePanel API Documentation

This document describes the REST API for PulsePanel.

## Standard Response Envelope

All API responses follow a standard envelope for consistency.

**Success Response (`200 OK`)**
```json
{
  "status": "ok",
  "data": { ... }
}
```

**Error Response (`4xx` or `5xx`)**
```json
{
  "status": "error",
  "error": {
    "code": "ERROR_CODE",
    "message": "A human-readable error message."
  }
}
```

---

## Endpoints

### Blueprint Endpoints

#### `POST /api/blueprints/validate`

Validates a blueprint directory.

*   **Request Body:**
    ```json
    {
      "path": "blueprints/minecraft-java-paper"
    }
    ```
*   **Success Response (`200 OK`):**
    *   Returns the full validation result object.
*   **Error Response (`400 Bad Request`):**
    *   Returns an error if the blueprint is invalid or the path is incorrect.

---

#### `POST /api/blueprints/generate`

Generates configuration from a blueprint and a set of values.

*   **Request Body:**
    ```json
    {
      "path": "blueprints/minecraft-java-paper",
      "values": {
        "server": {
          "name": "My Awesome Server"
        }
      }
    }
    ```
*   **Success Response (`200 OK`):**
    *   Returns the path to the generated output directory.
    ```json
    {
      "status": "ok",
      "data": {
        "outputPath": "output/minecraft-java-paper/1.0.0/a1b2c3d4..."
      }
    }
    ```

---

#### `GET /api/blueprints`

Lists all available blueprints found in the `blueprints` directory.

*   **Success Response (`200 OK`):**
    *   Returns an array of blueprint catalog entries.

---

#### `GET /api/blueprints/{name}`

Gets the full `meta.yaml` details for a single blueprint by name.

*   **Success Response (`200 OK`):**
    *   Returns the full blueprint object.
*   **Error Response (`404 Not Found`):**
    *   Returns an error if no blueprint with that name exists.

---

### Settings Endpoints

#### `GET /api/settings`

Retrieves the current application settings.

*   **Success Response (`200 OK`):**
    *   Returns the `PulsePanelSettings` object.

---

#### `PUT /api/settings/storage`

Updates application settings. **Note: This endpoint is not fully implemented in the current version.**

*   **Request Body:** A `PulsePanelSettings` object.
*   **Success Response (`501 Not Implemented`):**
    *   Indicates that the feature is not available yet.
