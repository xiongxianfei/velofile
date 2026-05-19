# VeloFile System Architecture

## Lifecycle Metadata

- Status: approved; includes 2026-05-18 PR CI validation tiering amendment approved by architecture-review-r1
- Scope: canonical baseline architecture for VeloFile V1
- Related V1 proposal: [V1 Product Direction](../../proposals/2026-05-04-v1-product-direction.md)
- Related V1 spec: [V1 Product Scope](../../../specs/v1-product-scope.md)
- Related UI proposal: [UI Design System and Shell Redesign](../../proposals/2026-05-11-ui-design-system-shell-redesign.md)
- Related UI spec: [UI Design System and Shell Redesign](../../../specs/ui-design-system-shell-redesign.md)
- Related shell visual-coherence proposal: [Shell Visual Coherence Follow-up](../../proposals/2026-05-11-shell-visual-coherence-follow-up.md)
- Related shell visual-coherence spec: [UI Shell Visual Coherence](../../../specs/ui-shell-visual-coherence.md)
- Related test runtime proposal: [Test Runtime Optimization](../../proposals/2026-05-16-test-runtime-optimization.md)
- Related test runtime spec: [Test Runtime Optimization](../../../specs/test-runtime-optimization.md)
- Related PR CI proposal: [PR CI Validation Tiering](../../proposals/2026-05-18-pr-ci-validation-tiering.md)
- Related PR CI spec: [PR CI Validation Tiering](../../../specs/pr-ci-validation-tiering.md)
- Context diagram: [diagrams/context.mmd](diagrams/context.mmd)
- Container diagram: [diagrams/container.mmd](diagrams/container.mmd)
- Component diagram: [diagrams/desktop-app-components.mmd](diagrams/desktop-app-components.mmd)
- Last updated: 2026-05-18

## 1. Introduction and Goals

VeloFile V1 is a Windows 10/11 desktop file explorer focused on daily local file-management workflows. The architecture must satisfy the approved V1 scope without expanding into Explorer replacement behavior, OS shell menu integration, plugin marketplaces, cross-platform runtime support, or global indexing.

Primary goals:

- Keep browsing, filtering, tab switching, built-in context menus, and first viewport rendering responsive.
- Keep file operations safe by default, especially delete behavior.
- Use Windows-native integration for file associations, Recycle Bin, drag/drop, thumbnails/icons, long paths, DPI, terminal launch, and MSIX packaging.
- Keep open-source contribution boundaries visible through explicit services, providers, adapters, diagnostics, and ADRs.
- Keep visible UI quality governed by repo-owned WinUI design contracts, validation tools, and reviewed evidence rather than external prototype packages.
- Keep whole-shell visual coherence governed by additive token/scope contracts, deterministic vector fixture icons, optional full-shell review artifacts, and behavior-preservation matrices.
- Keep validation feedback loops tiered so contributors and hosted PR workflows can run fast confidence checks without removing release-evidence validation from full closeout paths.
- Preserve post-V1 extension points without pre-building out-of-scope features.

## 2. Architecture Constraints

- Runtime target is Windows 10 and Windows 11 only.
- V1 uses WinUI 3 via the Windows App SDK with C# as the primary app language.
- V1 ships side by side with Windows File Explorer and does not register as a global Explorer replacement.
- V1 release builds ship as signed MSIX packages through a documented stable update channel.
- V1 does not expose OS Shell extension context menu entries or host third-party Shell menu handlers.
- The UI design system is owned by repository specs, token contracts, XAML resources, validation tools, and accepted evidence; external design packages are reference input only.
- The first UI redesign slice uses fixed dark and comfortable defaults and does not persist new theme or density preferences.
- Shell-wide visual-coherence contracts extend `docs/ui/tokens.v1.json` and `docs/ui/ui-contract-scopes.v1.json` additively unless an approved incompatible redesign reset creates a new major contract.
- Deterministic fixture icons use VeloFile-owned XAML vector resources and allowlisted icon kinds; first visual baselines do not depend on real Windows Shell icon extraction.
- Full-shell visual artifacts may use effective-pixel app-window review profiles and remain optional soft-review context until a later accepted decision hardens screenshot comparison.
- Test runtime optimization keeps `scripts/ci.ps1` as the broad closeout command. Hosted CI splitting follows the approved PR CI validation tiering spec: ordinary PR confidence, release evidence, and full closeout are separate named lanes.
- Hosted CI validation introduced or changed by the PR CI tiering work runs on Windows GitHub Actions runners, uses PowerShell 7 (`pwsh`) for PowerShell and repository script steps unless a reviewed exception is recorded, and selects the repository-approved .NET SDK before restore, build, test, UI contract, release-evidence, or closeout commands.
- Prepared corpus tool execution is internal to tests for the first slice, uses test-owned scratch/temp roots, and is guarded by a current-run prepared-tool manifest.
- Public corpus wrapper command-line contracts remain backward compatible in the first test runtime optimization slice.
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
- **UI design resources** own VeloFile token/resource dictionaries, shell surface resources, command/sidebar/status/preview resources, deterministic vector fixture icons, file-list row presentation resources, focus and density constants, and resource consumption rules inside the WinUI app.
- **UI contract tooling** owns static validation from repo-owned UI contracts to WinUI resources, fixture icon resources, screenshot sidecars, and scoped literal checks without launching the app.
- **Corpus validation tooling** owns validation tiers, corpus contract tests, public script smoke tests, hermetic wrapper isolation, test-internal prepared-tool execution, and runtime duration evidence for contributor and closeout validation.
- **Hosted CI validation orchestration** owns GitHub Actions workflow lanes, runner/shell/SDK setup contracts, workflow contract validation, lane-specific command selection, runtime summaries, TRX artifacts, and shadow-run evidence for branch-protection transition.
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
- **UI contract tooling**: solution-included .NET console tooling plus review-gated PowerShell orchestration for validating UI token/scope contracts, checking WinUI resources, and updating reviewed visual baselines.
- **Corpus validation tooling**: solution-included corpus test harnesses, public PowerShell script smoke coverage, hermetic wrapper isolation checks, prepared-tool test execution, and runtime reporting for validation tiers.
- **Hosted CI validation workflows**: GitHub Actions workflow files and shared reporting helper that run `ci-fast-required`, `ci-release-evidence`, and `ci-full-closeout` on Windows runners with `pwsh`, explicit SDK setup, lane-specific commands, TRX output, and job summaries.
- **Visual baseline evidence**: committed PNG screenshots and JSON sidecars under reviewed profiles. Generated current captures and diffs are transient validation output and are not committed.
- **Benchmark harness**: test executable that creates the deterministic corpus, drives the app process, and emits benchmark reports for release gating.
- **MSIX package and stable release channel**: signed package and release metadata for side-by-side install, stable update channel documentation, versioning policy, update cadence expectation, and rollback/uninstall instructions.

### Desktop App Components

- **UI shell**: visual shell for windows, tabs, file list, sidebar, breadcrumb, preview/details, command surfaces, and accessibility.
- **UI design resources**: checked-in WinUI `ResourceDictionary` files for VeloFile-owned colors, brushes, typography, spacing, sizing, radius, density, focus, state, motion, shell surface resources, deterministic fixture vector icons, and file-list component resources.
- **UI test fixture host**: Debug/test-only app launch path that renders hardcoded deterministic visual fixture states and allowlisted fixture icon kinds when the fixture flag, environment guard, and allowlist all pass.
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
| UI redesign R1-R14 | UI Shell; UI Design Resources; File Listing Service |
| UI redesign R15-R47 | UI Contract Tooling; UI Design Resources; WinUI App Shell |
| UI redesign R48-R61 | UI Shell; UI Design Resources; File Listing Service |
| UI redesign R62-R79 | UI Test Fixture Host; UI Contract Tooling; Visual Baseline Evidence |
| UI redesign R80-R84 | Architecture Decisions; UI Design Resources; Visual Baseline Evidence |
| Shell coherence R1-R8 | UI Shell; UI Design Resources; Architecture Decisions |
| Shell coherence R9-R15 | UI Contract Tooling; UI Design Resources; Visual Baseline Evidence |
| Shell coherence R16-R27 | UI Shell; UI Design Resources; Visual Baseline Evidence |
| Shell coherence R28-R43 | UI Shell; UI Design Resources; UI Test Fixture Host; UI Contract Tooling |
| Shell coherence R44-R65 | UI Shell; Command Layer; File Listing Service; Preview Providers; File Operation Service |
| Shell coherence R66-R77 | UI Test Fixture Host; UI Contract Tooling; Visual Baseline Evidence |
| Shell coherence R78-R82 | Test Specs; UI Shell; Application Services; Windows Integration Layer |
| Test runtime R1-R15 | Corpus Validation Tooling; Test Projects; Architecture Decisions |
| Test runtime R16-R21 | Contributor Documentation; CI and Script Orchestration |
| Test runtime R22-R38 | Corpus Validation Tooling; Public Corpus Scripts; Scratch/Temp Workspace |
| Test runtime R39-R43 | Release Evidence; CI and Script Orchestration; Corpus Validation Tooling |
| Test runtime R44-R47 | Test Projects; Corpus Validation Tooling |
| Test runtime R48-R60 | Runtime Reports; Review Evidence; CI and Script Orchestration |
| PR CI R1-R13 | Hosted CI Validation Workflows; Branch-Protection Handoff Evidence; Architecture Decisions |
| PR CI R65-R69 | Hosted CI Execution Environment; Workflow Contract Tests; GitHub Actions Windows Runners |
| PR CI R14-R27 | Hosted CI Validation Workflows; UI Contract Tooling; Product Test Projects; Corpus Validation Tooling |
| PR CI R28-R39 | Release Evidence; Full Closeout; CI and Script Orchestration |
| PR CI R40-R48 | CI Runtime Summary Helper; TRX Artifacts; GitHub Actions Job Summaries |
| PR CI R49-R53 | Shadow-Run Evidence; Branch-Protection Handoff; Rollback Decision Record |
| PR CI R54-R64 | Release Evidence Preservation; Security/Privacy; Workflow Contract Tests |

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

### Test Runtime Validation

Test runtime optimization separates validation work by purpose without removing broad closeout validation:

1. Contributors use documented `Fast` and `Contract` filters for inner-loop validation after the relevant projects are already built.
2. `VeloFile.Corpus.Tests` category inventory enforces the accepted taxonomy and prevents expensive evidence tiers from silently entering the fast default path.
3. Corpus contract tests exercise schemas, report shapes, manifests, redaction, profile decisions, scope classification, and release classification without public PowerShell wrappers when the wrapper itself is not the behavior under test.
4. Public corpus wrappers keep minimal `CorpusScript` + `Smoke` coverage for supported script families.
5. One common hermetic wrapper isolation test keeps scratch copy/publish behavior and no-repo-output behavior covered.
6. Tests that need process execution but not hermetic publish on every assertion may use a prepared corpus tool under a test-owned scratch/temp root.
7. Prepared-tool execution checks a current-run manifest before invoking the tool. A missing root, outside-root path, missing manifest, mismatched setup id, wrong manifest-declared tool metadata, or missing expected artifact fails before tool execution.
8. `ReleaseEvidence`, `Benchmark`, `Visual`, and `ManualEvidence` tiers remain explicit and available for full closeout, release readiness, or future full validation commands.
9. Runtime reports record before/after command durations, top slow tests, and whether full `scripts/ci.ps1` improved, stayed the same, or regressed.

### Hosted CI Validation

Hosted CI validation separates ordinary PR confidence from expensive evidence lanes without changing product runtime behavior:

1. `ci-fast-required` is the ordinary PR confidence lane. It restores, builds, validates production UI contracts, runs Core/App/Windows test projects directly without category filters, runs Corpus `Fast|Contract`, and runs Corpus `CorpusScript&Smoke`.
2. `ci-fast-required` does not call `scripts/ci.ps1` and does not run Corpus `ReleaseEvidence` by default. Its summary labels the lane as fast PR confidence, not release readiness.
3. `ci-release-evidence` is the explicit expensive evidence lane. It is manually runnable, scheduled nightly or daily, and runs for release branches/tags or merge queue when merge queue is used as a release-readiness gate.
4. `ci-release-evidence` runs build-producing restore/build before `--no-build` release-evidence tests and reports whether `ReleaseEvidence`, `Benchmark`, `Visual`, and `ManualEvidence` categories ran, were absent, or were intentionally not selected.
5. `ci-full-closeout` is the manual broad validation lane and invokes `scripts/ci.ps1` unchanged.
6. All hosted lanes introduced or changed by this architecture use Windows runners, `pwsh` for PowerShell/repository script steps unless reviewed exceptions exist, and SDK setup before validation commands.
7. A shared PowerShell reporting helper owns GitHub job-summary rendering and TRX slow-test extraction for hosted lanes. Workflows pass lane name, trigger, selected categories, release-evidence status, full-closeout status, command timing, and test result paths into the helper.
8. Runtime summaries are written even after a validation command fails once the job has started. If TRX output is missing because a build or earlier command failed, the summary reports that limitation rather than fabricating slow-test data.
9. `ci-fast-required` shadow-runs as non-required for at least one PR cycle before maintainers change branch protection. The change record, not the workflow file alone, records shadow-run comparison and external branch-protection handoff evidence.

### UI Contract Validation and Visual Fixtures

First-slice UI validation starts from repo-owned contracts, not from `hifi-design/`. The UI contract tool reads `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, and governed XAML resource dictionaries as static artifacts. It validates required token keys, resource types, directly comparable values, color-to-brush relationships, duplicate governed keys, and first-slice literal rules without launching the app or depending on the WinUI runtime.

The first file-list visual evidence uses a Debug/test-only fixture route:

1. App startup parses `--test-ui-fixture`.
2. Release or production builds reject the flag with a nonzero exit before showing normal or fixture UI.
3. Debug/test builds reject the flag with a nonzero exit unless `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` is present.
4. The fixture registry accepts only hardcoded allowlisted names and never accepts arbitrary fixture data paths in the first slice.
5. Allowed fixtures render deterministic app-shell or view-model states for reviewed screenshots.

Visual baseline updates are explicit maintainer actions. Normal CI may generate current screenshots and diffs, but must not mutate committed baselines. The baseline update script copies reviewed current outputs into `tests/visual/baselines/` only when a review id is supplied and current screenshots exist.

### Shell Visual-Coherence Review Artifacts

Shell-wide visual artifacts may extend the first-slice fixture and baseline flow when maintainers choose to record screenshots or manual visual notes. The app remains the rendering boundary for screenshots, while static validation remains the first line of defense for token, scope, icon, and optional sidecar conformance.

1. Shell-wide token and scope entries are added to `docs/ui/tokens.v1.json` and `docs/ui/ui-contract-scopes.v1.json` as additive V1 contract entries.
2. Governed shell resources are checked statically for tokenized surface, command band, sidebar, status, preview, file-list, and icon rules.
3. Fixture rows expose allowlisted icon kinds. The fixture host maps those kinds to VeloFile-owned vector geometry resources and never accepts arbitrary icon resource keys.
4. Optional full-shell fixture states may be captured for chosen shell states and effective-pixel review profiles when automation supports them.
5. Optional high-DPI artifacts such as `shell-standard-1440x900-200` may be recorded manually when useful, but missing screenshot automation is not a closeout blocker.
6. Screenshot sidecars, when recorded, carry profile, effective window size, scale, theme, density, fixture, evidence kind, dynamic regions, and review id.
7. Full-shell screenshots remain optional soft-review artifacts until a later accepted spec and architecture decision define stable hard-gate comparison.

## 7. Deployment View

V1 deployment is a signed MSIX package installed side by side with File Explorer. The package uses a documented stable update channel with release source, signing identity, versioning policy, update cadence expectation, and rollback/uninstall instructions.

Runtime storage:

- Session state, settings, favorites, and recent locations: local AppData or MSIX-equivalent local app data, written with same-directory temporary files, Windows-safe atomic replacement or equivalent rename/swap behavior, and last-known-good backups.
- Diagnostics: local app data, retained for at most 30 days or 50 MB total, whichever limit is reached first. Individual log files rotate at or before 5 MB. Crash markers retain at most the latest 10 markers. Last-action markers retain only the latest marker per category needed for crash attribution. Oldest diagnostics are deleted or overwritten first when limits are reached.
- Benchmark corpus: generated deterministically outside the application package in a user-selected or test-controlled workspace.

Validation and repository evidence:

- UI token and scope contracts live under `docs/ui/`.
- WinUI token and component resources live in the app resource tree and are merged into the application resource dictionaries.
- UI contract tooling lives under `tools/` and is included in `VeloFile.sln`; it has no dependency on the app runtime for static validation.
- Test runtime category metadata lives in test source and is checked by category inventory validation.
- Prepared corpus tool manifests live only under test-owned scratch/temp roots and identify the current test-harness setup invocation, expected tool kind, configuration, target framework, and entrypoint.
- Test runtime reports are review evidence and must identify command, configuration, filter, date, measured duration, top slow tests, and local environment assumptions.
- Hosted CI workflows live under `.github/workflows/`. The PR CI tiering architecture uses separate workflow files for ordinary PR confidence, release evidence, and full closeout so triggers and branch-protection check names remain stable and reviewable.
- Hosted CI job summaries are GitHub Actions job-summary output generated by a repository PowerShell helper. TRX or equivalent structured test outputs may be uploaded as artifacts when present.
- Workflow contract tests inspect committed GitHub Actions workflow YAML through a structured YAML parser and a test-owned workflow model rather than relying only on ad hoc string checks. They prove lane names, triggers, Windows runner use, `pwsh`, SDK setup ordering, command selection, release-evidence separation, and summary/reporting hooks.
- First-slice committed visual baselines live under `tests/visual/baselines/winui/<profile>/` with JSON sidecars.
- Optional shell-wide full-shell visual artifacts may use reviewed profiles under `tests/visual/baselines/winui/<profile>/` with JSON sidecars. Suggested shell visual-coherence profiles are `shell-min-900x560-100`, `shell-standard-1440x900-100`, and `shell-standard-1440x900-200`; missing profile coverage is not a closeout blocker.
- Generated visual outputs under `tests/visual/current/` and `tests/visual/diffs/` are transient and ignored by Git.

Rollback is uninstalling the MSIX. VeloFile does not own global Explorer replacement behavior or system file associations, so uninstall does not require system repair.

## 8. Cross-Cutting Concepts

### Responsiveness

The UI thread must not perform slow file enumeration, preview decoding, thumbnail loading, recursive search, file operations, terminal discovery, or benchmark work. Hot-path surfaces render first viewport state first, then enrich.

File-list visual resources must preserve virtualization and stable row height. First-slice row presentation may change XAML resources, templates, and styles, but must not add synchronous filesystem, thumbnail, preview, or metadata work to row rendering.

### Safety

Recycle Bin delete is the default. Permanent delete is a separate command with confirmation. Terminal launch treats paths as data. Preview providers fail closed to metadata fallback.

### Windows Compatibility

Shell/Windows integration adapters centralize Windows behavior so the UI and application services do not duplicate platform semantics. Compatibility corpus tests cover file operations, drag/drop, long paths, junctions, symlinks, reparse points, file associations, and mixed DPI.

### Persistence and Migration

Session, settings, favorites, and recent locations are versioned local documents from V1. Each durable document includes a header with `documentType`, `schemaVersion`, `minimumReaderVersion`, `appVersion`, `writtenAtUtc`, and `payload`.

Unknown fields are ignored. Missing fields fall back to documented defaults. Malformed optional fields fall back per field and log a redacted local diagnostic event. Malformed required structural fields trigger last-known-good recovery. Newer schema versions degrade to known fields where safe. Session state does not persist selection, filter text, recursive search results, clipboard, authentication, or in-flight operations.

### Observability

Local diagnostics include crash markers, last-action markers, diagnostic logs, and benchmark reports. They are local-only by default. No diagnostics, telemetry, crash reports, paths, filenames, or preview-derived content are uploaded without a separate approved proposal and explicit user opt-in.

Test runtime observability is separate from product diagnostics. Runtime reports record validation command durations, filters, configuration, date, local environment assumptions, and top slow tests. They must not be used as universal performance guarantees. Category inventory and prepared-tool boundary diagnostics should name the offending test, category, rejected condition, or allowed root without exposing unrelated private local paths.

Hosted CI observability is a separate contributor/release surface. GitHub Actions job summaries record lane name, trigger, selected categories, whether release evidence ran, whether `CorpusScript&Smoke` ran, whether full closeout ran, total/build/test-project durations when available, and slowest tests from TRX when available. Hosted success for `ci-fast-required` must not be described as release readiness.

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

Test runtime artifacts follow the same privacy posture. Prepared-tool manifests must not store raw local usernames, private profile paths, secrets, tokens, credentials, or machine-specific private data. Prepared-tool execution must not mutate user PATH, global .NET configuration, or repository build output outside the assigned scratch/temp root.

Hosted CI artifacts follow the same privacy posture. Runtime summaries, TRX artifacts, cache keys, runner/shell exception evidence, and workflow diagnostics must not include secrets, signing material, release tokens, credentials, raw private profile details, or unrelated machine inventory. Ordinary PR workflows should not require new repository secrets and should grant only the token permissions needed for checkout, setup, validation, summary writing, and artifact upload.

### Accessibility

Every V1 workflow has a keyboard path. Focus state stays visible. Destructive dialogs state consequences. Loading, unsupported, failed, and empty states are distinct.

### UI Design Contracts

`hifi-design/` is reference input only. Production UI authority flows from the accepted spec into `docs/ui/tokens.v1.json`, `docs/ui/ui-contract-scopes.v1.json`, checked-in WinUI resources, validation evidence, and recorded design deviations.

New token and component resource dictionaries are held to strict tokenized-literal rules. Legacy XAML outside the active first-slice scope is not globally cleaned up by this architecture; literal enforcement expands region by region as specs approve more shell surfaces.

Shell visual-coherence work extends the same contract chain additively. `docs/ui/tokens.v1.json` remains the V1 token contract and `docs/ui/ui-contract-scopes.v1.json` remains the V1 governed-scope contract unless a later accepted decision introduces incompatible token or scope semantics. This avoids a parallel shell-token authority while still allowing a future major version for an intentional redesign reset.

Deterministic fixture icons are VeloFile-owned vector resources under `src/VeloFile.App/Resources/Icons/`. The fixture host accepts only allowlisted icon kinds and maps them to named geometry resources. Governed fixture/file-list icon scopes reject `SymbolIcon`, `PathIcon`, private-use glyph fonts, ellipsized extension chips, unapproved icon colors, and unapproved icon sizes. Real Windows Shell icons remain future integration evidence, not first deterministic fixture-baseline input.

Shell-wide visual artifacts may use effective-pixel app-window profiles. `shell-min-900x560-100`, `shell-standard-1440x900-100`, and `shell-standard-1440x900-200` remain useful review profiles, but screenshots/manual visual notes are optional and missing profile coverage does not block region closeout.

Theme and density are not persisted by the first slice. Any future persisted theme or density setting must update the spec, persistence architecture, migration expectations, and tests before implementation.

Visual artifacts are not release proof for filesystem integration by themselves. First-slice deterministic fixtures prove row presentation stability and reviewability; real listing, thumbnail, search, operation, drag/drop, and preview behavior still require existing V1 integration, adapter, corpus, or manual evidence as appropriate.

Shell visual-coherence slices must maintain a behavior-preservation matrix that maps touched shell regions to V1 behavior routes and tests or explicit manual evidence. Screenshots and fixture-only evidence cannot replace behavior-route evidence through the App/Core/Windows boundaries.

### Test Runtime Validation Contracts

Validation tiers are an architecture boundary for contributor workflow, not a way to weaken release proof. `Fast` and `Contract` are inner-loop categories. `CorpusScript`, `ReleaseEvidence`, `Benchmark`, `Visual`, and `ManualEvidence` remain explicit cost-bearing categories and are excluded from the default fast command unless intentionally selected or explicitly justified.

Corpus script smoke and corpus contract tests have different responsibilities. Contract tests should avoid PowerShell and scratch publish overhead when wrapper behavior is not the claim. Script smoke tests prove public entrypoints and representative output. Release-evidence tests own full profile and scope matrices.

Prepared-tool execution is a test harness optimization only. It is not a public script feature in the first slice. The prepared tool root must stay inside the allowed scratch/temp root, carry a current-run manifest, declare expected tool metadata, and contain the expected artifact before invocation. Source-hash and cross-run cache staleness detection are deferred unless a later accepted decision introduces cross-run prepared-tool reuse.

### Hosted CI Validation Contracts

Hosted validation tiers are an architecture boundary for repository workflow, not a replacement for release evidence. Ordinary PRs should receive fast required feedback through `ci-fast-required`, while release readiness remains explicit through `ci-release-evidence`, `ci-full-closeout`, local `scripts/ci.ps1`, or another accepted release gate.

`scripts/ci.ps1` stays broad and local-runnable. It must not be narrowed to fast-only filters as part of PR CI tiering. The full closeout lane invokes it rather than duplicating its closeout role through copied workflow commands.

Workflow contract tests own static protection for the hosted CI architecture. They parse workflow YAML through a structured parser into a test-owned model. They should fail if Core/App/Windows tests are selected through solution-level Corpus category filters, if release evidence is run by default in `ci-fast-required`, if `scripts/ci.ps1` is called from the fast lane, if hosted lanes omit the Windows/pwsh/SDK setup contract, or if runtime summaries cannot report selected evidence tiers.

Dependency caching remains secondary. Cache setup may reduce restore cost, but cache misses must not switch the ordinary PR lane to full release-evidence validation.

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
- [ADR 0009: UI Design Contracts, Static Validation, and Visual Fixtures](../../adr/0009-ui-design-contracts-static-validation-and-visual-fixtures.md)
- [ADR 0010: Shell Visual-Coherence Contracts and Evidence](../../adr/0010-shell-visual-coherence-contracts.md)
- [ADR 0011: Test Runtime Validation Tiers and Corpus Harness Optimization](../../adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md)
- [ADR 0012: Hosted PR CI Validation Tiers](../../adr/0012-hosted-pr-ci-validation-tiers.md) (accepted by architecture-review-r1)

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
| QS-UI-CONTRACT-01 | UI design-system conformance | A contributor changes first-slice tokens, file-list resources, or governed XAML. | Local validation or CI on Windows/.NET. | Static UI contract validation fails fast for missing token keys, wrong comparable values, duplicate governed keys, and forbidden first-slice literals without launching the app. | 100% of governed first-slice token and scope contract violations produce nonzero validation with actionable file/key/scope output. |
| QS-UI-FIXTURE-01 | Fixture safety | A process starts the app with `--test-ui-fixture`. | Release build, Debug without environment guard, Debug with guard, and unknown fixture names. | Only allowed Debug/test launches with `VELOFILE_ENABLE_TEST_UI_FIXTURES=1` and a hardcoded fixture name render fixture UI; all other fixture requests exit nonzero before rendering normal or fixture UI. | Production/Release, missing guard, and unknown fixture cases all fail nonzero; allowed case renders deterministic fixture state. |
| QS-UI-VISUAL-01 | Visual evidence reviewability | A maintainer updates first-slice screenshot baselines. | Reviewed current screenshots and sidecars for the approved profile. | Baseline mutation happens only through the review-gated command with a review id; normal CI never mutates committed baselines. | Missing review id or missing current screenshots exits nonzero; committed baselines have matching JSON sidecars and generated current/diff outputs remain uncommitted. |
| QS-UI-SHELL-01 | Shell visual coherence | A contributor changes governed shell surface, command band, sidebar, status, preview, file-list, or icon resources. | Local validation or CI on Windows/.NET. | Static UI contract validation rejects unapproved literals, raw/default governed visuals, forbidden icon controls, arbitrary fixture icon keys, and invalid optional sidecar metadata when sidecars are present. | 100% of governed shell-scope contract violations produce nonzero validation with actionable file/scope/rule output. |
| QS-UI-SHELL-02 | Optional full-shell visual artifacts | A maintainer chooses to record shell visual-coherence screenshots or manual visual notes. | Chosen shell states and effective-pixel profiles. | Optional full-shell screenshots and sidecars remain supporting review context, generated current/diff outputs remain uncommitted, and behavior preservation is cited separately. | Optional visual artifacts never replace behavior proof; committed/referenced sidecars are privacy-safe and generated current/diff outputs remain uncommitted. |
| QS-TEST-RUNTIME-01 | Validation feedback speed | A contributor runs the documented fast or corpus contract command after a build. | Local Windows/.NET validation with projects already built. | Fast/contract filters exclude script smoke and release-evidence tiers by default, while full closeout validation remains available through `scripts/ci.ps1`. | Corpus category inventory rejects missing/unknown categories and invalid expensive-tier combinations; runtime evidence records baseline, optimized contract, optimized script-smoke, top 10 slow tests, and full CI status. |
| QS-PR-CI-01 | Hosted PR feedback speed with explicit release evidence | An ordinary pull request updates code, tests, or validation contracts. | GitHub Actions hosted Windows runner with repository-approved .NET SDK and `pwsh`. | `ci-fast-required` runs build-producing restore/build, UI contract validation, direct Core/App/Windows tests, Corpus `Fast|Contract`, and Corpus `CorpusScript&Smoke`; release evidence and full closeout remain explicit lanes. | Workflow contract tests prove lane names, triggers, Windows/pwsh/SDK setup, command selection, and no default `ReleaseEvidence`; job summaries report selected tiers and durations or limitations. |

## 11. Risks and Technical Debt

- WinUI 3 and Shell integration rough edges may require targeted interop. Mitigation: isolate Windows adapters and capture Shell ownership in ADR 0002.
- File operation parity is broad and safety-sensitive. Mitigation: fixed compatibility corpus before release claims.
- Thumbnail and preview providers may block or fail. Mitigation: timeouts, concurrency limits, cancellation/ignore behavior, metadata fallback.
- Slow or unavailable drives can still consume worker resources. Mitigation: per-tab work scoping and cancellation.
- Benchmark numbers are only meaningful after corpus/harness exist. Mitigation: no public performance claims before harness and corpus.
- V1 excludes OS shell menu integration, which may disappoint users needing extension commands. Mitigation: explicit docs and post-V1 ADR/proposal gate.
- Diagnostics may accidentally reveal sensitive path data. Mitigation: allowed/prohibited diagnostic field contract, path classification by default, non-reversible per-installation fingerprints only when needed, local retention limits, and redacted export defaults.
- UI contract tooling may drift from app resources if it is treated as an orphan script. Mitigation: include the .NET tool in `VeloFile.sln`, validate static artifacts deterministically, and keep PowerShell responsible only for orchestration and baseline approval.
- Deterministic visual fixtures may be mistaken for product integration evidence. Mitigation: classify first-slice screenshots as row-presentation evidence and keep V1 integration/corpus/manual evidence requirements for real filesystem and Windows behavior.
- New visual styling may hurt virtualization or accessibility. Mitigation: keep the first slice to named resources/templates/styles, preserve row height stability, keep focus visible, and forbid synchronous filesystem or preview work during row rendering.
- Shell-wide visual evidence may become subjective or over-trusted. Mitigation: pair full-shell screenshots with additive token/scope validation, icon-resource invariants, screenshot sidecar checks, and behavior-preservation matrix evidence.
- High-DPI visual artifacts may be hard to automate consistently. Mitigation: protect touched high-DPI risk with static/resource tests, app tests, or explicit manual behavior notes; optional screenshots may support review when available.
- Deterministic fixture icon resources may diverge from later real Shell icon integration. Mitigation: classify vector fixture icons as visual baseline evidence only and keep real Shell icons as a later integration evidence path.
- Validation tiers may be misused to skip release evidence. Mitigation: keep `scripts/ci.ps1` as the first-slice broad closeout command, require explicit `ReleaseEvidence` commands, and record runtime evidence without presenting fast runs as release proof.
- Prepared-tool execution may hide wrapper isolation regressions. Mitigation: keep one common hermetic wrapper isolation test and minimal public script smoke tests for supported script families.
- Runtime reports may overfit to one machine. Mitigation: record command, filter, configuration, date, and environment assumptions, and treat local timing as evidence for the slice rather than a universal guarantee.
- Hosted fast PR CI may be mistaken for release readiness. Mitigation: stable lane names, job summaries, branch-protection handoff records, and release-evidence/full-closeout workflows keep evidence tiers explicit.
- Workflow YAML drift may silently weaken validation. Mitigation: static workflow contract tests parse committed workflow definitions and fail on missing lanes, wrong runner/shell/SDK setup, wrong command selection, or missing summary hooks.
- Runtime-summary generation may become duplicated across workflows. Mitigation: keep summary rendering and TRX slow-test extraction in a shared PowerShell helper with fixture-based tests.

## 12. Glossary

- **Adapter**: boundary that translates VeloFile service calls into Windows platform calls.
- **Built-in context menu**: VeloFile-owned menu with V1 commands only.
- **Compatibility corpus**: generated or fixed test data for Windows behavior parity.
- **Corpus validation tooling**: test harnesses and scripts that validate corpus contracts, public corpus wrappers, release-evidence reports, and test runtime evidence.
- **Prepared-tool manifest**: test-only metadata file in a prepared corpus tool root that proves the tool belongs to the current test-harness setup invocation.
- **Provider**: component that supplies content or data behind a stable application boundary, such as preview or metadata.
- **Shell-owned behavior**: behavior delegated to Windows Shell APIs instead of reimplemented in VeloFile.
- **UI contract tooling**: static validation and review-support tooling that checks repo-owned UI contracts against WinUI resources and optional visual-artifact metadata.
- **Visual baseline evidence**: reviewed screenshots and JSON sidecars used to compare first-slice UI presentation over time.
- **Shell visual-coherence artifacts**: optional reviewed full-shell screenshots, JSON sidecars, or manual notes across chosen shell states and effective-pixel profiles, used as soft-review context alongside behavior-preservation proof.
- **Validation tier**: accepted test category that tells contributors whether a test is intended for fast inner-loop, contract, smoke, release-evidence, benchmark, visual, or manual-evidence validation.
- **Hosted CI lane**: stable GitHub Actions job/check name with a defined trigger, environment, command selection, and summary contract.
- **Runtime summary**: GitHub Actions job-summary output that reports lane purpose, selected validation tiers, durations, slow-test details when available, and limitations when structured output is absent.
- **V1**: first public release scope defined by the approved V1 product scope spec.

## Next Artifacts

- Execution plan for [PR CI Validation Tiering](../../../specs/pr-ci-validation-tiering.md).
- Plan review for the PR CI validation tiering execution plan.
- Matching test spec for workflow contracts, runtime summaries, release-evidence preservation, and rollout evidence.

## Follow-on Artifacts

- [V1 Product Scope Execution Plan](../../plans/2026-05-04-v1-product-scope.md)
- [UI Design System and Shell Redesign Proposal](../../proposals/2026-05-11-ui-design-system-shell-redesign.md)
- [UI Design System and Shell Redesign Spec](../../../specs/ui-design-system-shell-redesign.md)
- [ADR 0009: UI Design Contracts, Static Validation, and Visual Fixtures](../../adr/0009-ui-design-contracts-static-validation-and-visual-fixtures.md)
- [UI Shell Visual Coherence Spec](../../../specs/ui-shell-visual-coherence.md)
- [ADR 0010: Shell Visual-Coherence Contracts and Evidence](../../adr/0010-shell-visual-coherence-contracts.md)
- [Test Runtime Optimization Proposal](../../proposals/2026-05-16-test-runtime-optimization.md)
- [Test Runtime Optimization Spec](../../../specs/test-runtime-optimization.md)
- [ADR 0011: Test Runtime Validation Tiers and Corpus Harness Optimization](../../adr/0011-test-runtime-validation-tiers-and-corpus-harness-optimization.md)
- [PR CI Validation Tiering Proposal](../../proposals/2026-05-18-pr-ci-validation-tiering.md)
- [PR CI Validation Tiering Spec](../../../specs/pr-ci-validation-tiering.md)
- [ADR 0012: Hosted PR CI Validation Tiers](../../adr/0012-hosted-pr-ci-validation-tiers.md)
- Architecture review for the UI design-system update completed on 2026-05-11 with status `approved` and no material findings.

## Readiness

Approved for execution planning by `architecture-review-r1` for the 2026-05-18 PR CI validation tiering amendment. The design keeps production App/Core/Windows behavior unchanged, preserves `scripts/ci.ps1` as broad closeout, separates hosted fast PR confidence from release evidence and full closeout, and does not authorize implementation before execution planning, plan-review, and a matching test spec.
