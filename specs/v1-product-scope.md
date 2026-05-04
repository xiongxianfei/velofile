# V1 Product Scope

## Status

approved

## Related proposal

- [V1 Product Direction](../docs/proposals/2026-05-04-v1-product-direction.md)

## Goal and context

This spec defines the externally observable V1 behavior for VeloFile, a fast, lightweight, open-source file explorer for Windows 10 and Windows 11.

V1 focuses on daily file-management workflows: opening folders, navigating with tabs and sidebar locations, browsing large folders, filtering the current folder, recursive search, preview, common file operations, safe deletion, drag/drop, file association open, terminal launch, session restore, diagnostics, and MSIX distribution.

This spec intentionally does not define the internal implementation plan, class structure, or exact Windows API bindings. Architecture and ADRs will decide those details while preserving this behavioral contract.

## Glossary

- **Active tab**: the tab whose folder content, breadcrumb, filter, preview, and command state are currently shown.
- **Built-in context menu**: VeloFile's own context menu containing V1 commands. It does not enumerate third-party Shell extensions in V1.
- **Current folder**: the folder shown by the active tab.
- **Current-folder filter**: a non-recursive text filter that narrows visible items in the current folder only.
- **Diagnostics marker**: a local record that captures crash state, last user action, or preview-release triage evidence without sending data off-device by default.
- **First interactive frame**: the point at which the VeloFile window is visible, the active tab has rendered its first viewport, and input is accepted.
- **First viewport**: the initially visible set of rows or items in the file list.
- **OS shell menu integration**: exposing menu items from Windows Shell extensions or third-party Shell context menu handlers. This is out of V1 scope.
- **Permanent delete**: a delete action that bypasses the Recycle Bin.
- **Preview pane**: the optional pane that renders supported file content or metadata for the selected item.
- **Recycle Bin delete**: a delete action that sends the selected file or folder to the Windows Recycle Bin when the target supports it.
- **Recursive search**: an on-demand search that walks below the current folder and streams bounded results.
- **Reference corpus**: a deterministic generated file tree used for V1 compatibility and performance verification.

## Examples first

### Example E1: Open and browse a project folder

Given VeloFile is installed on Windows 11
When the user opens `D:\projects\velofile`
Then VeloFile shows the folder in the active tab, renders the first viewport, shows a breadcrumb path, and accepts navigation input.

### Example E2: Filter without recursive search

Given the active folder contains `README.md`, `report.pdf`, and `src`
When the user types `read` in the current-folder filter
Then the visible file list narrows to matching current-folder items such as `README.md`
And VeloFile does not start a recursive search or query a global file index.

### Example E3: Search recursively with cancellation

Given the active folder contains a deep source tree
When the user starts a recursive search for `config`
Then VeloFile streams matching results as they are found
And when the user cancels the search, no new results are added after cancellation completes.

### Example E4: Safe delete by default

Given one file is selected
When the user presses `Delete`
Then VeloFile sends the file to the Recycle Bin when supported
And VeloFile does not permanently delete the file.

### Example E5: Permanent delete requires confirmation

Given one file is selected
When the user presses `Shift+Delete`
Then VeloFile shows a distinct permanent-delete confirmation
And the file is permanently deleted only after the user confirms.

### Example E6: Restore session after restart

Given the user had ten tabs open, a selected active tab, per-tab history, sort state, and scroll positions
When VeloFile is closed and reopened
Then VeloFile restores the tabs, active tab, history, sort and view state, and scroll anchor where possible
And it does not restore selected files, filter text, recursive search results, clipboard contents, or in-flight file operations.

### Example E7: Preview unsupported file

Given the selected item is an unsupported file type
When the preview pane is open
Then VeloFile shows metadata and an unsupported preview state
And it does not show a generic error state for a normal unsupported file type.

### Example E8: Built-in context menu only

Given a file is selected
When the user opens the context menu
Then VeloFile shows its built-in V1 commands
And it does not enumerate or show OS Shell extension menu entries.

### Example E9: Recursive search reaches result cap

Given a recursive search matches more than 10,000 items
When VeloFile reaches 10,000 displayed results
Then VeloFile preserves the 10,000 displayed results and shows a visible "result limit reached" state
And the user can cancel, refine the query, or start a new query
And cancellation remains available before and after the cap is reached.

### Example E10: Preview limits and timeout states

Given the selected image is larger than 100 MB or decodes above 8192 by 8192 pixels
When the preview pane attempts to show it
Then VeloFile skips content preview and shows metadata only.

Given preview generation takes longer than its budget
When image decode exceeds 2 seconds, text read and encoding detection exceeds 1 second, PDF first-page render exceeds 3 seconds, or thumbnail generation exceeds 500 ms per item
Then VeloFile shows the corresponding failed or fallback state without blocking navigation.

### Example E11: Terminal discovery default order

Given Windows Terminal, PowerShell 7, Windows PowerShell, Command Prompt, Git Bash, and WSL distributions are available
When VeloFile chooses the default terminal target
Then the default order is Windows Terminal, PowerShell 7, Windows PowerShell, then Command Prompt
And Git Bash and WSL distributions are selectable only when discovered or explicitly chosen.

### Example E12: Missing path during session restore

Given a restored tab path no longer exists
When VeloFile restores the session
Then the tab remains visible, displays the missing path, offers a close-tab action, and is not silently removed.

## Requirements

### Product boundary

R1. VeloFile V1 MUST run as a Windows 10/11 desktop file explorer that can be launched side by side with Windows File Explorer.

R2. VeloFile V1 MUST NOT register itself as a full global Explorer replacement.

R3. VeloFile V1 release builds MUST be distributed as a signed MSIX package through a documented stable update channel that includes a published release source, signing identity, versioning policy, update cadence expectation, and rollback/uninstall instructions.

R4. VeloFile V1 MUST NOT provide cross-platform runtime support, cloud sync, P2P sync, FTP/SFTP browsing, archive-as-filesystem browsing, folder synchronization, duplicate finding, AI file classification, a plugin marketplace, a full scripting language, or OS shell menu integration.

### Navigation and file list

R5. The UI MUST let users open a folder from launch, a typed or pasted path, breadcrumb navigation, sidebar locations, recent locations, favorites, and drive entries.

R6. The breadcrumb/path bar MUST let users jump to a visible segment, switch to raw path editing, and paste a path to navigate.

R7. The file list MUST render folders with virtualization so only the visible working set is rendered in the primary view.

R8. The file list MUST support details, large-icons, and list view modes in V1.

R9. Folder navigation MUST render the first viewport before thumbnail and nonessential metadata work completes.

R10. Thumbnail and metadata work MUST be cancellable or ignorable when the user navigates away, switches tabs, or changes selection.

R11. A slow, unavailable, removable, or network-backed location in one tab MUST NOT block interaction with other already-open tabs.

R12. Empty folders MUST render a clear empty state without treating the folder as an error.

R13. Invalid, unavailable, or access-denied paths MUST show a user-visible failure state that preserves the previous valid tab state when possible.

### Sidebar

R14. The sidebar MUST expose pinned favorites, recent locations capped at 20 entries, and drives.

R15. Users MUST be able to add and remove pinned favorites.

R16. Users MUST be able to dismiss individual recent locations.

R17. Drive entries SHOULD show a free-space hint when the information is available without delaying navigation.

### Tabs and session restore

R18. Tabs MUST support open, close, reorder, duplicate, and reopen closed tab.

R19. Each tab MUST keep its own back and forward navigation history.

R20. VeloFile MUST support keyboard-driven tab switching.

R21. Session restore MUST restore tab paths, active tab index, per-tab history, sort state, view mode, window position and size, monitor target when still available, sidebar state, and scroll position when the stored scroll anchor still exists.

R22. Session restore MUST anchor scroll by the first visible item name, not by pixel offset alone.

R23. If a stored monitor no longer exists, VeloFile MUST restore the window onto an available primary or current monitor.

R24. Session restore MUST NOT restore selected files, per-tab filter text, recursive search query/results, clipboard contents, authentication state, or in-flight file operations.

R25. Session state MUST use a versioned schema, ignore unknown fields, and fall back per field when a field is corrupt or cannot be migrated.

R26. Session writes MUST protect against partial-write corruption by writing recoverable state in a way that a crash during persistence does not destroy the last valid session.

R27. After a crash, VeloFile MUST detect local crash markers and offer a way to start fresh if restored state appears to trigger repeated failure.

### Filtering and search

R28. The current-folder filter MUST narrow the active file list by substring match against item names.

R29. The current-folder filter MUST be non-recursive.

R30. Applying the current-folder filter MUST NOT start recursive search or depend on Windows Search indexing.

R31. Clearing the current-folder filter MUST restore the unfiltered visible list for the current folder.

R32. Recursive search MUST be explicitly started by the user.

R33. Recursive search MUST walk from the current folder, stream results progressively, stop adding results after 10,000 results per query by default, show a "result limit reached" state when the cap is reached, and remain cancellable.

R34. Recursive search MUST report permission failures and skipped locations without aborting the entire search when continuing is possible.

R35. V1 MAY omit glob syntax. If glob syntax is included in V1, its supported patterns MUST be documented before release.

### File operations and safety

R36. VeloFile MUST support copy, move, rename, delete to Recycle Bin, and permanent delete.

R37. File operations MUST run without blocking tab switching, navigation in unaffected tabs, or cancellation UI.

R38. Pressing `Delete` or choosing the normal delete command MUST use Recycle Bin delete when the target supports it.

R39. Permanent delete MUST require a distinct user gesture and explicit confirmation.

R40. VeloFile MUST NOT permanently delete data without explicit user confirmation.

R41. Name collisions during copy or move MUST offer skip, replace, and keep-both choices.

R42. Batch operation conflicts MUST queue or pause for resolution without silently aborting the entire batch when other items can continue.

R43. File operations MUST show visible progress, completion, cancellation, and failure status.

R44. File operations SHOULD support undo for the most recent move, rename, and Recycle Bin delete within the current session.

R45. Undo MUST NOT be offered for permanent delete.

R46. Operations on long paths, junctions, symlinks, and reparse points MUST have defined compatibility behavior in the V1 compatibility corpus before release.

### Context menu, commands, and shortcuts

R47. V1 MUST provide a built-in context menu for the V1 command set.

R48. The built-in context menu MUST include core verbs for Open, Open with, Cut, Copy, Paste when applicable, Rename, Delete, Properties, Copy path, Copy name, and Open terminal here when applicable.

R49. V1 MUST NOT expose OS shell extension menu entries or enumerate third-party Shell context menu handlers.

R50. Keyboard selection MUST support single selection, multi-selection, `Ctrl+A`, `Escape` to clear selection, arrow-key focus movement, `Shift` range extension, and `Ctrl` toggle behavior.

R51. V1 MUST support keyboard commands for Enter to open, `F2` rename, `Delete` Recycle Bin delete, `Shift+Delete` permanent delete with confirmation, `F5` refresh, Backspace parent navigation, `Ctrl+Shift+C` copy absolute path, and `Ctrl+Shift+N` copy name.

R52. Copy path MUST place the absolute Windows path of the selected item or items onto the clipboard.

R53. Copy name MUST place only the selected item name or names onto the clipboard.

### Terminal integration

R54. V1 MUST provide an explicit Open terminal here command for the current folder.

R55. V1 MUST support discovery of Windows Terminal, PowerShell 7, Windows PowerShell, Command Prompt, Git Bash, and WSL distributions. If multiple targets are available, the default order MUST be Windows Terminal, PowerShell 7, Windows PowerShell, then Command Prompt. Git Bash and WSL distributions MUST be selectable discovered targets when available, but MUST NOT outrank the four default targets unless the user explicitly chooses them.

R56. Terminal discovery MUST NOT block app launch.

R57. Terminal launch MUST treat folder paths as data, not as command fragments.

R58. VeloFile MUST NOT launch a terminal automatically on navigation.

R59. If the selected terminal cannot be found or the current folder cannot be used as a working directory, VeloFile MUST show a user-visible error and leave the file browsing state unchanged.

### Preview and details

R60. V1 SHOULD provide a preview pane toggle with a keyboard path.

R61. Supported preview types MUST include common images, bounded text/code files, and PDF first-page rendering.

R62. Image preview MUST skip content preview and show metadata only when the input file is over 100 MB or decoded dimensions exceed 8192 by 8192 pixels.

R63. Text/code preview MUST read at most the first 1 MB and show a truncation indicator when the file continues beyond the previewed prefix.

R64. Text/code files over 100 MB MUST show metadata only.

R65. PDF preview MUST render the first page initially and render later pages only after user navigation.

R66. PDFs over 500 MB MUST show metadata only.

R67. Preview generation MUST use these timeout budgets: image decode 2 s, text read and encoding detection 1 s, PDF first-page render 3 s, and thumbnail generation 500 ms per item with no more than 4 concurrent thumbnail operations.

R68. Preview generation MUST clear the previous preview when loading a new selected item and show a loading state after 200 ms.

R69. Preview terminal states MUST distinguish loading, success, unsupported, and failed.

R70. Failed preview states MUST include a user-visible reason when the reason is known, such as timeout, access denied, corrupt file, or decode error.

R71. Preview generation MUST NOT modify the source file.

R72. The details pane or metadata fallback MUST show file size, timestamps, attributes, and type when available.

### Visibility and Explorer parity

R73. V1 MUST expose persistent visibility controls for hidden files, protected operating-system files, and file extensions.

R74. V1 MUST show file extensions by default.

R75. Users MUST be able to hide known file extensions within VeloFile without changing Windows File Explorer settings.

R76. Protected operating-system files MUST remain hidden by default.

R77. Enabling protected operating-system files SHOULD require a first-use confirmation because it can expose dangerous system files.

R78. Hidden or protected files shown because of visibility toggles SHOULD be visually distinguishable from ordinary files.

R79. Drag/drop behavior MUST follow Windows copy, move, and shortcut modifier conventions for same-volume and cross-volume drops.

R80. Drop targets MUST show the resolved action before the drop completes.

R81. Double-click and Open MUST respect user-configured Windows file associations.

R82. VeloFile MUST provide Open with behavior without modifying system file associations.

R83. VeloFile MUST support per-monitor DPI behavior with sharp text and icons on mixed-DPI monitor setups.

### Diagnostics and preview readiness

R84. Preview builds MUST record local crash markers.

R85. Preview builds MUST record local last-action markers sufficient to identify whether the last action was navigation, preview generation, file operation, search, terminal launch, or another high-level workflow.

R86. Preview builds MUST write diagnostic logs for failures relevant to V1 release readiness, including navigation failures, file-operation failures, preview failures, search cancellation/failure, terminal launch failures, and session restore fallback.

R87. Diagnostic logs MUST NOT include file contents, secrets, terminal command text beyond the selected terminal identity, or clipboard contents.

R88. V1 preview promotion MUST define a triage threshold for crash or hang reports that blocks promotion when exceeded.

R89. Any post-V1 preview that enables OS shell menu integration MUST add shell-menu last-action markers before the integration is enabled.

### Packaging, docs, and release notes

R90. V1 release notes MUST document that file extensions are shown by default.

R91. The extension-display release note MUST explain the `invoice.pdf.exe` safety case and state that the setting is per-application.

R92. V1 documentation MUST include a "Differences from File Explorer" page or section covering extension display and the absence of OS shell menu integration in V1.

R93. V1 MUST provide rollback by uninstalling the MSIX without taking ownership of Explorer replacement behavior or system file associations.

## Inputs and outputs

### User inputs

- Folder paths from launch, path bar, paste, sidebar, favorites, recent locations, and drives.
- Mouse, touchpad, keyboard, context menu, toolbar, preview pane, drag/drop, and tab interactions.
- File selections and multi-selections.
- File operation choices: copy, move, rename, delete, permanent delete confirmation, conflict resolution, cancel, and undo where supported.
- Filter text and recursive search query.
- Terminal selection and Open terminal here command.
- Visibility settings for hidden files, protected operating-system files, and file extensions.
- Session restore choice after crash recovery when offered.

### System inputs

- Windows file system state, including permissions, long paths, junctions, symlinks, reparse points, removable drives, mapped drives, and unavailable locations.
- Windows file associations and Open with registrations.
- Windows drag/drop data from Explorer, browsers, IDEs, Office, and VeloFile.
- Thumbnail, icon, image, text, and PDF preview sources.
- MSIX install, update, and uninstall state.

### User-visible outputs

- Window, tabs, breadcrumb/path bar, sidebar, file list, filter/search surfaces, preview/details pane, context menu, dialogs, progress surfaces, and error states.
- Clipboard contents for copy path and copy name.
- Launched terminal process or user-visible launch error.
- Operation progress, cancellation, completion, conflict, undo, and failure states.
- Release notes and documentation for V1 differences from File Explorer.

### Data outputs

- Versioned local session state.
- Local settings state.
- Local crash markers, last-action markers, diagnostic logs, and benchmark reports.
- No remote telemetry is required by this spec.

## State and invariants

I1. Normal delete defaults to Recycle Bin delete where supported.

I2. Permanent delete is never performed without explicit confirmation.

I3. Preview generation never modifies the source file.

I4. Current-folder filter and recursive search remain separate workflows.

I5. OS shell menu integration and third-party Shell extension hosting are not part of V1.

I6. Session restore never restores selected files, filter text, recursive search results, clipboard contents, authentication state, or in-flight file operations.

I7. Hidden/system/extension visibility settings persist as settings, not as per-session-only state.

I8. File association open respects Windows user defaults and does not modify associations.

I9. A failed path, preview, search, terminal launch, or operation must not corrupt the current valid browsing state.

I10. Diagnostic artifacts remain local unless a separate future user-consented reporting feature is specified.

## Error and boundary behavior

- Invalid path: show a clear error and keep the previous valid tab state when possible.
- Access denied: show access-denied status for the affected folder, item, search branch, preview, or operation without crashing.
- Missing path on session restore: keep the tab visible with a recoverable missing-location state, display the missing path, and offer a close-tab action. V1 MUST NOT silently skip or remove the tab.
- Offline drive or sleeping removable device: show pending or unavailable state for the affected tab while other tabs remain interactive.
- Empty folder: show empty state.
- Large folder: render first viewport before full enumeration or thumbnails complete.
- Long path: handle according to the compatibility corpus and show a clear error if Windows denies the operation.
- Reparse point loop during search: avoid unbounded recursion and report skipped loops.
- Copy/move conflict: show conflict choices and preserve unresolved work until the user chooses.
- Delete to Recycle Bin unsupported: tell the user that Recycle Bin delete is unavailable and require explicit permanent-delete confirmation before destructive fallback.
- Preview timeout: show failed preview with timeout reason and metadata fallback.
- Unsupported preview: show unsupported state and metadata, not an error state.
- Terminal missing: show a launch error and keep browsing state unchanged.
- Session file corrupt: restore valid fields and fall back only for invalid fields.
- Crash on previous run: record and read local crash marker, then offer recovery behavior defined by session restore requirements.

## Compatibility and migration

C1. V1 MUST target Windows 10 and Windows 11.

C2. V1 MUST install side by side with Windows File Explorer.

C3. V1 MUST respect Windows file associations for Open and Open with behavior.

C4. V1 MUST NOT modify global file associations as part of normal Open or Open with behavior.

C5. V1 MUST handle local, removable, mapped, and unavailable locations with defined user-visible states.

C6. V1 session and settings data MUST be versioned from the first release.

C7. Unknown fields in session or settings data MUST be ignored during read.

C8. Failed migrations MUST fall back per field where possible and log the fallback.

C9. MSIX uninstall MUST leave Windows Explorer and system file associations usable without repair.

C10. Post-V1 OS shell menu integration requires a separate proposal or ADR and is not a compatibility promise of V1.

## Observability

O1. V1 MUST provide local diagnostics for preview and release readiness.

O2. Diagnostics MUST include local crash markers, last-action markers, diagnostic logs, and benchmark reports.

O3. Last-action markers MUST identify high-level workflow categories, not file contents.

O4. Diagnostic logs MUST include enough context to triage path, operation, preview, search, terminal, session, and packaging failures without recording file contents.

O5. Benchmark reports MUST include OS build, hardware class, CPU, RAM, storage type, Windows Search state, antivirus state when observable, DPI configuration, run count, median, p95, and p99.

O6. Preview-release promotion MUST document the triage threshold used for crash or hang blockers.

O7. User-visible status MUST exist for long-running operations, recursive search, preview loading/failure, session recovery, and terminal launch failure.

## Security and privacy

S1. VeloFile MUST treat file and folder names as untrusted input.

S2. Terminal launch MUST NOT construct shell commands by concatenating folder paths into command text.

S3. Diagnostic logs MUST NOT store file contents, clipboard contents, secrets, authentication tokens, or full terminal command text.

S4. VeloFile MUST NOT persist network authentication state in session restore.

S5. Permanent delete MUST require explicit confirmation even when invoked by keyboard.

S6. Showing file extensions by default MUST be documented as a safety-oriented divergence from File Explorer defaults.

S7. Preview providers MUST fail closed to metadata-only fallback when type, size, timeout, or permission boundaries are exceeded.

S8. V1 MUST NOT host third-party Shell extension menu handlers.

## Accessibility and UX

A11Y1. Every V1 navigation, selection, file operation, filter/search action, tab action, preview toggle, and context-menu command MUST have a keyboard path.

A11Y2. Focus state MUST remain visible during keyboard navigation.

A11Y3. Selection, rename, delete confirmation, conflict resolution, progress, preview state, and error surfaces MUST be reachable and understandable without a mouse.

A11Y4. Text and icons MUST remain legible on mixed-DPI monitor setups.

A11Y5. Dialogs and confirmations MUST state destructive consequences before the user confirms.

A11Y6. Loading, unsupported, failed, and empty states MUST be visually distinct.

A11Y7. The UI SHOULD avoid restoring selected files on launch because restored selections can confuse users and assistive technology.

## Performance expectations

P1. Performance targets apply to p95 on the documented baseline machine class after the reference corpus and benchmark harness exist.

P2. Cold launch to first interactive frame SHOULD be at or below 1500 ms.

P3. Warm launch to first interactive frame SHOULD be at or below 600 ms.

P4. Small folder switch with warm cache SHOULD be at or below 50 ms.

P5. Medium folder switch with warm cache SHOULD be at or below 150 ms.

P6. Large folder switch with warm cache SHOULD render the first viewport at or below 400 ms.

P7. Current-folder filter latency on the medium corpus SHOULD be at or below 30 ms from keystroke to filtered first viewport.

P8. Recursive search SHOULD stream the first result at or below 200 ms on the deep-tree corpus.

P9. Recursive search SHOULD reach the 1,000-result milestone at or below 2 s on the deep-tree corpus.

P10. Built-in context menu open SHOULD be at or below 50 ms.

P11. Tab switch to rendered viewport SHOULD be at or below 80 ms.

P12. Session restore for a 10-tab session SHOULD reach first interactive frame at or below 1500 ms.

P13. Large-folder sustained scroll SHOULD maintain 60 fps frame timing under continuous input on the benchmark corpus.

P14. Regressions over 10% at p95 SHOULD require explicit ADR or release-note acknowledgement.

P15. Regressions over 25% at p95 SHOULD block release unless a later accepted proposal or ADR changes the benchmark policy.

P16. V1 MUST NOT make public release performance claims until the benchmark harness and reference corpus exist.

## Edge cases

EC1. Folder path exists but user lacks read permission.

EC2. Folder path disappears while active.

EC3. Folder contains 100,000 items.

EC4. Folder contains names differing only by case on a case-insensitive volume.

EC5. Folder contains very long paths.

EC6. Search encounters a reparse-point loop.

EC7. Search encounters access-denied subfolders.

EC8. User switches tabs during thumbnail generation.

EC9. User changes selection during preview generation.

EC10. Preview file is corrupt.

EC11. Preview file exceeds size or decode limits.

EC12. Delete to Recycle Bin is unavailable for the target.

EC13. Copy or move collides with existing names.

EC14. User cancels a batch operation after some items have completed.

EC15. VeloFile crashes after a session background write begins.

EC16. Stored session references a monitor that no longer exists.

EC17. Stored scroll anchor item was deleted or renamed.

EC18. User opens terminal from a path containing shell metacharacters.

EC19. User opens a file whose default app is missing or broken.

EC20. User expects Shell extension menu commands that are intentionally absent in V1.

EC21. User enables protected operating-system files for the first time.

EC22. MSIX update changes session schema.

EC23. Removable drive sleeps or disconnects during navigation.

EC24. Drag/drop source comes from Explorer, browser, IDE, or Office.

EC25. Drag/drop modifier changes copy/move/shortcut action before release.

EC26. Recursive search reaches the 10,000-result cap.

EC27. Preview generation exceeds the 200 ms loading-state threshold but completes before its timeout.

## Non-goals

- Cross-platform runtime.
- Full Explorer replacement mode.
- OS shell menu integration or third-party Shell extension hosting.
- Cloud sync, P2P sync, FTP/SFTP, folder synchronization, duplicate finding, archive-as-filesystem browsing, AI classification, or plugin marketplace.
- Custom global file indexer.
- Content search or indexed search.
- Office document preview, media playback, audio/video scrubbing, RAW image preview, or archive preview.
- Fully remappable shortcuts.
- Embedded terminal pane.
- Dual-pane view, batch rename, tagging, customizable toolbar, color labels, or theme engine beyond light/dark.
- Durable cross-session undo or durable resumable file operations.
- Telemetry upload or crash-report upload without a separate user-consented specification.

## Acceptance criteria

AC1. On Windows 10 and Windows 11, VeloFile installs as a side-by-side MSIX app and does not replace Explorer.

AC2. A user can open a folder, navigate by breadcrumb, switch tabs, use sidebar locations, and return through tab history.

AC3. A generated large folder renders its first viewport without rendering every item as a UI element.

AC4. Current-folder filtering narrows the visible list without recursive search or Windows Search dependency.

AC5. Recursive search streams results, stops adding results after 10,000 results per query by default, preserves the 10,000 displayed results, shows a visible "result limit reached" state, reports skipped permission failures, and cancels on request before or after the cap is reached.

AC6. Delete sends supported targets to Recycle Bin; permanent delete requires `Shift+Delete` or equivalent distinct gesture plus confirmation.

AC7. Copy/move conflicts offer skip, replace, keep both, and batch conflict handling.

AC8. Built-in context menu includes V1 core verbs and does not show OS Shell extension commands.

AC9. Open, Open with, drag/drop, long-path, and file-operation behavior pass the fixed Windows compatibility corpus.

AC10. Preview pane handles image, text/code, and PDF examples plus unsupported, failed, timeout, oversized, and access-denied cases, including the 100 MB image cap, 8192 by 8192 decoded-image cap, 1 MB text prefix, 100 MB text skip threshold, 500 MB PDF skip threshold, configured timeout budgets, four-thumbnail concurrency cap, immediate previous-preview clearing, and 200 ms loading state.

AC11. Preview generation does not modify the previewed file.

AC12. Open terminal here launches only on explicit user command and treats folder paths as data.

AC13. Session restore restores included fields, keeps missing-path tabs visible with the missing path and a close-tab action, and does not restore selection, filter text, search results, clipboard, authentication, or in-flight operations.

AC14. Hidden, protected operating-system file, and extension visibility settings persist; extensions are shown by default.

AC15. Benchmark harness and reference corpus produce reports with median, p95, p99, environment details, and release-gating status.

AC16. V1 release builds use a documented stable update channel with published release source, signing identity, versioning policy, update cadence expectation, and rollback/uninstall instructions.

AC17. Preview diagnostics include local crash markers, last-action markers, diagnostic logs, and triage thresholds.

AC18. V1 documentation and release notes explain extension display default and absence of OS shell menu integration.

AC19. Accessibility checks verify keyboard paths, focus visibility, readable destructive confirmations, and distinct loading/unsupported/failed/empty states.

## Open questions

None at spec level. Architecture and ADRs still need to choose implementation details for the Windows UI/runtime stack, Shell API bindings, file-operation service boundaries, benchmark harness design, preview provider structure, terminal discovery, session persistence, Explorer parity, and diagnostic triage policy.

## Next artifacts

- `spec-review` for this draft spec.
- `specs/v1-product-scope.test.md` after spec review.
- Architecture artifact covering UI/runtime, Shell integration, file-operation boundaries, persistence, diagnostics, packaging, and performance strategy.
- ADRs for UI/runtime choice, Shell API ownership, benchmark corpus and harness, preview provider boundaries, terminal launch safety, session restore model, Explorer parity policy, and diagnostic triage policy.
- Execution plan under `docs/plans/` after spec, test spec, and architecture are stable enough to sequence.

## Follow-on artifacts

- [VeloFile System Architecture](../docs/architecture/system/architecture.md)

## Readiness

Approved and ready for architecture, test-spec, and planning. Implementation details are intentionally delegated to the architecture package and ADRs.
