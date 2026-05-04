# VeloFile System Architecture

## Lifecycle Metadata

- Status: approved
- Scope: canonical baseline architecture for VeloFile V1
- Related proposal: [V1 Product Direction](../../proposals/2026-05-04-v1-product-direction.md)
- Related spec: [V1 Product Scope](../../../specs/v1-product-scope.md)
- Context diagram: [diagrams/context.mmd](diagrams/context.mmd)
- Container diagram: [diagrams/container.mmd](diagrams/container.mmd)
- Component diagram: [diagrams/desktop-app-components.mmd](diagrams/desktop-app-components.mmd)
- Last updated: 2026-05-04

## 1. Introduction and Goals

VeloFile V1 is a Windows 10/11 desktop file explorer focused on daily local file-management workflows. The architecture must satisfy the approved V1 scope without expanding into Explorer replacement behavior, OS shell menu integration, plugin marketplaces, cross-platform runtime support, or global indexing.

Primary goals:

- Keep browsing, filtering, tab switching, built-in context menus, and first viewport rendering responsive.
- Keep file operations safe by default, especially delete behavior.
- Use Windows-native integration for file associations, Recycle Bin, drag/drop, thumbnails/icons, long paths, DPI, terminal launch, and MSIX packaging.
- Keep open-source contribution boundaries visible through explicit services, providers, adapters, diagnostics, and ADRs.
- Preserve post-V1 extension points without pre-building out-of-scope features.

## 2. Architecture Constraints

- Runtime target is Windows 10 and Windows 11 only.
- V1 uses WinUI 3 via the Windows App SDK with C# as the primary app language.
- V1 ships side by side with Windows File Explorer and does not register as a global Explorer replacement.
- V1 release builds ship as signed MSIX packages through a documented stable update channel.
- V1 does not expose OS Shell extension context menu entries or host third-party Shell menu handlers.
- Shell-owned behavior is preferred for Windows-correct copy, move, delete, drag/drop, file association, thumbnail/icon, long-path, and Recycle Bin behavior.
- Paths, file names, terminal targets, persisted state, and diagnostic inputs are untrusted.
- Diagnostics are local-only by default. No diagnostics, telemetry, crash reports, paths, filenames, or preview-derived content are uploaded without a separate approved proposal and explicit user opt-in.

## 3. Context and Scope

VeloFile sits between users and the Windows platform. It is responsible for the application shell, user workflows, persistence, diagnostics, benchmarks, and packaging. Windows remains responsible for file-system semantics, user file associations, Recycle Bin behavior, drag/drop interoperability, system icons/thumbnails, process launch, and MSIX installation.

External systems:

- Windows file system and storage devices
- Windows Shell APIs and COM surfaces
- Windows file association and Open With registry/app model
- Explorer, browsers, IDEs, Office, and other drag/drop peers
- Terminal targets: Windows Terminal, PowerShell 7, Windows PowerShell, Command Prompt, Git Bash, WSL distributions
- MSIX installer/update channel
- Local AppData or MSIX-equivalent app data storage

Out of scope for V1:

- OS shell menu integration
- third-party Shell extension hosting
- global file indexing
- remote telemetry upload
- durable resumable file operations
- Explorer replacement mode

## 4. Solution Strategy

The architecture uses a layered Windows desktop app:

- **UI shell** owns WinUI windows, tabs, sidebar, breadcrumb/path bar, file list, preview/details pane, built-in context menu, dialogs, and keyboard flow.
- **Application services** own navigation, tabs, search, filtering, commands, session restore, settings, diagnostics, preview orchestration, and benchmark hooks.
- **Windows integration adapters** wrap Shell COM, Win32, WinRT, drag/drop, terminal launch, file associations, thumbnails/icons, and MSIX/platform behaviors.
- **Persistence providers** own versioned session state, settings, crash markers, last-action markers, diagnostic logs, benchmark reports, favorites, and recent locations.

Hot-path work is isolated from slow Windows integration work. Navigation renders the first viewport before thumbnails and nonessential metadata complete. Slow or failing tabs are scoped so unaffected tabs remain usable.

## 5. Building Block View

### Diagram Levels

- **Container view**: deployable, execution, storage, release, and external platform boundaries for V1.
- **Component view**: in-process service/provider/adapter boundaries inside the VeloFile desktop app.

### Top-Level Containers

- **VeloFile desktop app**: WinUI 3 desktop application that hosts UI, commands, search/filter orchestration, preview orchestration, Shell interop adapters, and file-operation orchestration.
- **Local app data**: versioned JSON and local diagnostic log files for settings, favorites, recent locations, session state, local crash markers, last-action markers, and diagnostics.
- **Benchmark harness**: test executable that creates the deterministic corpus, drives the app process, and emits benchmark reports for release gating.
- **MSIX package and stable release channel**: signed package and release metadata for side-by-side install, stable update channel documentation, versioning policy, update cadence expectation, and rollback/uninstall instructions.

### Desktop App Components

- **UI shell**: visual shell for windows, tabs, file list, sidebar, breadcrumb, preview/details, command surfaces, and accessibility.
- **Command layer**: built-in context menu, keyboard shortcuts, command availability, clipboard commands, Open/Open With, and terminal command routing.
- **Session service**: tab lifecycle, history, active tab, path navigation, invalid/missing path states, crash recovery flow, and restore orchestration.
- **Persistence service**: versioned session state, settings, favorites, recent locations capped at 20, terminal choice, visibility settings, crash markers, last-action markers, and atomic local writes.
- **Diagnostics service**: local crash markers, last-action markers, diagnostic logs, benchmark report metadata, preview-release triage data, diagnostic redaction, retention/rotation, and export conformance.
- **File listing service**: directory enumeration, sorting inputs, view-model projection, virtualization feed, hidden/system/extension visibility.
- **Search and filter service**: current-folder substring filtering and recursive search with 10,000-result cap, cancellation, skipped-location reporting, and result-limit state.
- **Preview providers**: image, text/code, PDF, metadata fallback, timeout budgets, preview terminal states, and thumbnail concurrency.
- **File operation service**: copy, move, rename, Recycle Bin delete, permanent delete confirmation, conflicts, progress, cancellation, and session-scoped undo where supported.
- **Shell interop adapters**: Shell COM, Win32, WinRT, OLE drag/drop, file association, icon/thumbnail, terminal process launch, long-path canonicalization, DPI, and accessibility platform calls.

### Requirement-to-Architecture Mapping

| Spec area | Architecture responsibility |
|---|---|
| R1-R4, R90-R93 | Packaging and Release Assets; WinUI App Shell; Windows Integration Layer |
| R5-R13 | WinUI App Shell; Navigation and Tab Service; File Listing Service |
| R14-R17 | WinUI App Shell; Persistence Service |
| R18-R27 | Navigation and Tab Service; Persistence Service; Diagnostics Service |
| R28-R35 | Search and Filter Service; File Listing Service |
| R36-R46 | File Operation Service; Windows Integration Layer; Diagnostics Service |
| R47-R53 | Command Service; WinUI App Shell; Persistence Service |
| R54-R59 | Command Service; Windows Integration Layer; Persistence Service |
| R60-R72 | Preview and Metadata Service; Windows Integration Layer; Diagnostics Service |
| R73-R83 | WinUI App Shell; File Listing Service; Windows Integration Layer; Persistence Service |
| R84-R89 | Diagnostics Service; Benchmark Harness and Corpus Generator |
| Performance P1-P16 | Benchmark Harness and Corpus Generator; all hot-path services |

## 6. Runtime View

### Launch and Session Restore

1. App starts and initializes the UI shell with theme-correct surfaces.
2. Persistence service reads settings and versioned session state.
3. Invalid fields are ignored or defaulted per field; unknown fields are ignored.
4. Crash markers are read before session restore; repeated-failure state can surface a start-fresh option.
5. Active tab first viewport is restored before non-active tab background work is completed.
6. Missing restored paths stay visible with missing-location state, path display, and close-tab action.

Startup persistence recovery reads the canonical durable document first, then the last-known-good backup, then safe defaults. Malformed required structural fields trigger last-known-good recovery. Malformed optional fields fall back per field and produce a redacted local diagnostic event. A failed session or settings restore must not block app launch.

### Durable Persistence Writes

Session, settings, favorites, and recent locations use the same partial-write-safe protocol:

1. Serialize the complete durable document in memory with a document header.
2. Validate parseability before touching the canonical file.
3. Write a unique temporary file in the same directory as the canonical file.
4. Flush and close the temporary file.
5. Replace the canonical file using Windows-safe same-volume atomic replacement or equivalent rename/swap behavior.
6. Preserve a last-known-good backup.
7. Recover from canonical failure by trying last-known-good, then safe defaults.
8. Ignore unknown fields and fall back per field wherever safe.
9. Log local redacted diagnostics for fallback and migration events.

### Navigation

1. User navigates through launch, path bar, breadcrumb, sidebar, favorite, recent location, drive, or tab history.
2. Navigation service validates and creates/updates the tab location.
3. File listing service streams or batches enough entries for the first viewport.
4. UI renders first viewport.
5. Metadata and thumbnail work starts asynchronously and can be ignored/cancelled if the tab changes.

### Filtering and Recursive Search

Current-folder filtering runs against the active tab's current listing only. Recursive search is a separate explicit workflow that traverses below the current folder, streams results, stops adding after 10,000 results by default, shows result-limit state, reports skipped locations, and stays cancellable.

### File Operations

Commands flow through the command service into the file operation service. Delete defaults to Recycle Bin where supported. Permanent delete requires a distinct gesture and confirmation. Conflicts are surfaced without silently aborting unaffected work. Progress, completion, cancellation, failure, and undo eligibility are user-visible.

### Preview

Selection changes clear the previous preview immediately. If preview generation exceeds 200 ms, the UI shows loading. Provider budgets are enforced: image decode 2 s, text read and encoding detection 1 s, PDF first-page render 3 s, thumbnail generation 500 ms per item with at most 4 concurrent thumbnail operations. Preview ends in loading, success, unsupported, or failed, and never modifies source files.

### Terminal Launch

Terminal discovery is lazy or background and never blocks app launch. Terminal launch is explicit, uses the selected folder as structured process data or working directory, and never builds a shell command by concatenating folder paths.

### Diagnostics and Benchmarks

Last-action markers are written around high-level workflows and retain only the latest marker per marker category needed for crash attribution. Crash markers are local and retain at most the latest 10 markers. Diagnostic logs use the allowed-field and redaction contract in section 8 and never upload by default. Diagnostic retention failure must not block app launch, navigation, preview, search, or file operations. Benchmark runs drive the app process against a generated corpus and produce median, p95, p99, environment, and release-gating output.

## 7. Deployment View

V1 deployment is a signed MSIX package installed side by side with File Explorer. The package uses a documented stable update channel with release source, signing identity, versioning policy, update cadence expectation, and rollback/uninstall instructions.

Runtime storage:

- Session state, settings, favorites, and recent locations: local AppData or MSIX-equivalent local app data, written with same-directory temporary files, Windows-safe atomic replacement or equivalent rename/swap behavior, and last-known-good backups.
- Diagnostics: local app data, retained for at most 30 days or 50 MB total, whichever limit is reached first. Individual log files rotate at or before 5 MB. Crash markers retain at most the latest 10 markers. Last-action markers retain only the latest marker per category needed for crash attribution. Oldest diagnostics are deleted or overwritten first when limits are reached.
- Benchmark corpus: generated deterministically outside the application package in a user-selected or test-controlled workspace.

Rollback is uninstalling the MSIX. VeloFile does not own global Explorer replacement behavior or system file associations, so uninstall does not require system repair.

## 8. Cross-Cutting Concepts

### Responsiveness

The UI thread must not perform slow file enumeration, preview decoding, thumbnail loading, recursive search, file operations, terminal discovery, or benchmark work. Hot-path surfaces render first viewport state first, then enrich.

### Safety

Recycle Bin delete is the default. Permanent delete is a separate command with confirmation. Terminal launch treats paths as data. Preview providers fail closed to metadata fallback.

### Windows Compatibility

Shell/Windows integration adapters centralize Windows behavior so the UI and application services do not duplicate platform semantics. Compatibility corpus tests cover file operations, drag/drop, long paths, junctions, symlinks, reparse points, file associations, and mixed DPI.

### Persistence and Migration

Session, settings, favorites, and recent locations are versioned local documents from V1. Each durable document includes a header with `documentType`, `schemaVersion`, `minimumReaderVersion`, `appVersion`, `writtenAtUtc`, and `payload`.

Unknown fields are ignored. Missing fields fall back to documented defaults. Malformed optional fields fall back per field and log a redacted local diagnostic event. Malformed required structural fields trigger last-known-good recovery. Newer schema versions degrade to known fields where safe. Session state does not persist selection, filter text, recursive search results, clipboard, authentication, or in-flight operations.

### Observability

Local diagnostics include crash markers, last-action markers, diagnostic logs, and benchmark reports. They are local-only by default. No diagnostics, telemetry, crash reports, paths, filenames, or preview-derived content are uploaded without a separate approved proposal and explicit user opt-in.

Diagnostic events may include only these field categories:

- Event id, event type, UTC timestamp, monotonic sequence number, severity, component, operation id, and correlation id.
- App version, build channel, package identity, OS version/build, process architecture, and app uptime bucket.
- Operation kind, result state, reason/error code, duration, timeout budget, retry count, and cancellation flag.
- Bounded counts such as item count, result-count bucket, conflict count, tab count, queue depth, and byte-size bucket.
- Preview provider id, file category, extension class if needed, size bucket, dimension bucket, and timeout/fallback state.
- Persistence document type, schema version, migration result, fallback source, unknown-field count, and corrupt-field count.
- Release triage inputs such as crash-marker presence, last-action marker category, preview failure category, and post-V1 shell-menu marker category.
- Path classification such as local, removable, mapped, network, cloud-placeholder, protected, or unknown.
- Optional non-reversible per-installation path fingerprint when repeated local failure correlation is needed.

Diagnostic events must not include file contents, raw full paths, raw file names, usernames from paths, raw environment variables, search query text, clipboard contents, authentication state, tokens, cookies, credentials, secrets, raw terminal command lines, shell-composed command strings, or text extracted from previews.

The default path rule is: classify, do not record. When repeated local failure correlation is needed, diagnostics may store a non-reversible path fingerprint generated with a per-installation local salt/key. The fingerprint must not allow path reconstruction. The salt/key remains local and rotates when the user clears diagnostics or resets diagnostics privacy. User-initiated diagnostic export defaults to redacted content and should show the export payload or a summary before export; raw paths require a separate explicit per-export choice.

Ownership boundaries:

- Architecture owner: diagnostic schema, redaction rules, storage lifecycle, and new diagnostic field-category review.
- Release owner: numeric preview-release promotion thresholds and promotion/blocking decisions when thresholds are crossed.
- Diagnostics owner: conformance tests for schema, redaction, retention, marker recording, and export behavior.

### Security and Privacy

File paths, file names, settings, session files, and diagnostics are untrusted. Diagnostic data follows the local-only allowed/prohibited field contract above. V1 does not host third-party Shell extension menu handlers.

### Accessibility

Every V1 workflow has a keyboard path. Focus state stays visible. Destructive dialogs state consequences. Loading, unsupported, failed, and empty states are distinct.

## 9. Architecture Decisions

Durable decisions are recorded in ADRs:

- [ADR 0001: WinUI 3 with C# and Windows App SDK](../../adr/0001-winui3-csharp-windows-app-sdk.md)
- [ADR 0002: Shell API Ownership and No V1 Shell Menu Integration](../../adr/0002-shell-api-ownership-and-no-v1-shell-menu.md)
- [ADR 0003: Benchmark Corpus and Release Gates](../../adr/0003-benchmark-corpus-and-release-gates.md)
- [ADR 0004: Preview Provider Boundaries](../../adr/0004-preview-provider-boundaries.md)
- [ADR 0005: Terminal Launch Safety](../../adr/0005-terminal-launch-safety.md)
- [ADR 0006: Session Restore Persistence](../../adr/0006-session-restore-persistence.md)
- [ADR 0007: Explorer Parity Policy](../../adr/0007-explorer-parity-policy.md)
- [ADR 0008: Diagnostics and Preview Triage](../../adr/0008-diagnostics-and-preview-triage.md)

## 10. Quality Requirements

Quality requirements are expressed as measurable scenarios. Design mechanisms stay in sections 5-8.

| ID | Quality attribute | Stimulus | Environment | Response | Measure |
|---|---|---|---|---|---|
| QS-RESP-01 | Responsiveness | User launches VeloFile and navigates between representative folders. | Baseline machine class, deterministic generated corpus, warm-cache and cold-cache benchmark variants. | App reaches first interactive frame and renders the requested viewport without blocking the UI thread on metadata, thumbnail, or preview work. | p95 cold launch <= 1500 ms, p95 warm launch <= 600 ms, p95 warm medium-folder switch <= 150 ms, p95 warm large-folder first viewport <= 400 ms after the benchmark harness is accepted. |
| QS-SAFE-DELETE-01 | File-operation safety | User invokes the normal delete command on one or more files. | Local writable folder with normal user permissions. | App uses the Recycle Bin path by default, shows visible progress for non-trivial operations, exposes cancellation where Shell operation supports it, and does not permanently delete without the explicit permanent-delete gesture and confirmation flow. | 100% of normal delete test cases route to Recycle Bin behavior; permanent-delete tests require explicit gesture plus confirmation; no ad hoc direct deletion implementation is used for normal delete. |
| QS-SLOW-TAB-01 | Slow-location isolation | A tab is enumerating an unavailable or slow network/removable location while the user switches to or works in another local tab. | One slow or unavailable path plus one local generated-corpus folder. | Slow work remains scoped to the affected tab; local-tab switching, command routing, and cancellation UI continue to respond. | p95 tab switch to rendered local viewport <= 80 ms; no UI input stall over 100 ms attributable to the slow tab in benchmark/manual instrumentation; slow tab exposes progress, retry, or cancellation state. |
| QS-PREVIEW-TIMEOUT-01 | Preview boundedness | User selects a huge, corrupt, unsupported, or slow-to-read preview target. | Image/text/PDF/metadata preview corpus including over-limit and corrupt inputs. | Previous preview is cleared, loading state appears after the configured threshold, preview work times out or falls back, source file is not modified, and selection change cancels stale work. | Loading state appears after 200 ms when work is still pending; image decode timeout <= 2 s; text read/encoding timeout <= 1 s; PDF first-page render timeout <= 3 s; thumbnail timeout <= 500 ms per item; no more than 4 concurrent thumbnail operations; cancellation is visible within 50 ms on selection change. |
| QS-SESSION-RECOVERY-01 | Session crash recovery | App crashes or loses power during a session/settings write. | Canonical file, same-directory temp file, and last-known-good combinations generated by fault-injection tests. | App starts without a crash loop, reads canonical if valid, otherwise recovers last-known-good, otherwise launches safe defaults. Missing restored paths remain visible with recoverable missing-location state. | 100% of fault-injection cases produce old-valid, new-valid, last-known-good, or safe-default state; fallback diagnostic event is logged locally; no missing-path tab is silently skipped. |
| QS-DIAG-PRIV-01 | Diagnostics privacy | Preview, persistence, search, or file-operation failure occurs on a path containing username, sensitive filename, or document title. | Local diagnostics enabled with default privacy settings. | App records only allowed redacted fields, stores logs locally, applies rotation/retention, and does not upload data. | Automated redaction tests find zero raw paths, filenames, usernames, search queries, clipboard contents, or file contents in logs/exports by default; logs retain at most 30 days or 50 MB; no network upload occurs. |
| QS-MSIX-ROLLBACK-01 | Release rollback | User needs to uninstall, roll back, or continue after a bad V1 release. | Signed MSIX side-by-side install on supported Windows version. | VeloFile can be uninstalled or rolled back using documented stable-channel instructions, Explorer remains available, global Explorer replacement and system file association ownership are not used, and older/newer local state is ignored or migrated without blocking launch. | Manual release test verifies install/update/uninstall/rollback instructions; Explorer file management remains available after uninstall; app starts with migrated, ignored, or safe-default local state after version change. |

## 11. Risks and Technical Debt

- WinUI 3 and Shell integration rough edges may require targeted interop. Mitigation: isolate Windows adapters and capture Shell ownership in ADR 0002.
- File operation parity is broad and safety-sensitive. Mitigation: fixed compatibility corpus before release claims.
- Thumbnail and preview providers may block or fail. Mitigation: timeouts, concurrency limits, cancellation/ignore behavior, metadata fallback.
- Slow or unavailable drives can still consume worker resources. Mitigation: per-tab work scoping and cancellation.
- Benchmark numbers are only meaningful after corpus/harness exist. Mitigation: no public performance claims before harness and corpus.
- V1 excludes OS shell menu integration, which may disappoint users needing extension commands. Mitigation: explicit docs and post-V1 ADR/proposal gate.
- Diagnostics may accidentally reveal sensitive path data. Mitigation: allowed/prohibited diagnostic field contract, path classification by default, non-reversible per-installation fingerprints only when needed, local retention limits, and redacted export defaults.

## 12. Glossary

- **Adapter**: boundary that translates VeloFile service calls into Windows platform calls.
- **Built-in context menu**: VeloFile-owned menu with V1 commands only.
- **Compatibility corpus**: generated or fixed test data for Windows behavior parity.
- **Provider**: component that supplies content or data behind a stable application boundary, such as preview or metadata.
- **Shell-owned behavior**: behavior delegated to Windows Shell APIs instead of reimplemented in VeloFile.
- **V1**: first public release scope defined by the approved V1 product scope spec.

## Next Artifacts

- `plan-review` for the V1 product scope execution plan.
- `specs/v1-product-scope.test.md` after plan review.
- Implementation after plan review and test-spec approval.

## Follow-on Artifacts

- [V1 Product Scope Execution Plan](../../plans/2026-05-04-v1-product-scope.md)

## Readiness

Approved by `architecture-review` and ready for `plan-review` plus `test-spec`. No open architecture questions block execution planning.
