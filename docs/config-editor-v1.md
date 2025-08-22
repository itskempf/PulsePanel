## Config Editor v1.0 (Backend)

Scope: Syntax-aware editing (JSON/YAML/INI/XML/CFG), PNCCL overlay, staged apply, blueprint diff, profiles, dependency hints, lazy load + async validation.

### Interfaces and Models
- IConfigEditorService: load, validate, stage, commit, rollback, diff-to-blueprint/profile.
- IConfigValidationProvider: ValidateAsync(content, fileType) -> PnccLValidationResult.
- IConfigProfileStore: CRUD for profiles per config file.
- IConfigDiffService: GenerateDiffAsync(original, updated) -> ConfigDiff.
- IComplianceOverlayMapper: Map(PnccLValidationResult) -> IEnumerable<ComplianceOverlayItem>.
- Models: ConfigProfile, ConfigDiff, ComplianceOverlayItem, CommitResult, PnccLValidationResult, ValidationFinding, ValidationSeverity.

### Class/Method Plan
- ConfigEditorService
  - LoadConfigAsync(filePath) -> string
  - ValidateConfigAsync(filePath, content) -> PnccLValidationResult
  - StageChangesAsync(filePath, content) -> write to <file>.staged
  - CommitChangesAsync(filePath, message)
    - PNCCL preflight via IConfigValidationProvider
    - Block on errors or offline validator
    - On success: atomically replace, write Provenance (action=ConfigCommitted, ruleset hash)
  - RollbackChangesAsync(filePath) -> delete staged file
  - DiffWithBlueprintAsync(filePath) -> ConfigDiff (placeholder until blueprint defaults wired)
  - DiffWithProfileAsync(filePath, profileName) -> ConfigDiff

- DefaultComplianceOverlayMapper
  - Maps findings to overlay items (line/column unknown without parser context; set 0 and include ruleId, severity, message)

### Data Flow
1) User edits content -> StageChangesAsync
2) User requests commit -> CommitChangesAsync
   - Validate (PNCCL)
   - If offline or errors: block; return failure; log provenance (ConfigCommitBlocked)
   - Else: apply; log provenance (ConfigCommitted)

ProvenanceLogger records: actor(system), ts, entity=config file, action, entity hash (sha256 of content), PNCCL ruleset hash, outcome.

### PNCCL Overlay Mapping Examples
- Error: ruleId=PNCCL-CFG-001, message="Unknown key", severity=Error -> overlay item with Severity=Error.
- Warning: ruleId=PNCCL-CFG-002, message="Deprecated setting", severity=Warning.
- Info: ruleId=PNCCL-CFG-003, message="Default value used", severity=Info.

### Acceptance Test Matrix
- Success: validator returns no Error findings -> commit succeeds, provenance action=ConfigCommitted.
- Warning: validator returns Warning only -> commit succeeds; provenance includes findings.
- Error: validator returns at least one Error -> commit blocked, provenance action=ConfigCommitBlocked.
- Offline: validator throws -> commit blocked with status validator_offline.

Notes
- Dependency hints from blueprint metadata will be added in a follow-up when blueprint defaults are wired to diff service.
