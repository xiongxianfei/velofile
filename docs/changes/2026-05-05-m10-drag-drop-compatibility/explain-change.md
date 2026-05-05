# M10 Drag/Drop and Compatibility Corpus

M10 adds the first Windows-correct drag/drop contract and expands the compatibility corpus for risky path behavior. The implementation keeps drag/drop decisions in Core, Windows file-drop projection in the Windows boundary, and file mutation through the existing operation service.

Scope note: R81, R82, and I8 file-association Open/Open With behavior remains in M14. M10 implements R79-R80 drag/drop behavior and the path/drag-drop compatibility corpus work needed before later Explorer-parity validation.

## What Changed

`src/VeloFile.Core/DragDrop/` defines drop items, modifier flags, volume relationship, resolved actions, and a resolver for Explorer-style action selection. The resolver encodes the V1 contract: same-volume drops move by default, cross-volume drops copy by default, Ctrl copies, Shift moves, and Ctrl+Shift or Alt resolves to shortcut intent.

`AppShellViewModel` now exposes `CurrentDropAction`, `DropActionIndicatorVisible`, and `DropActionIndicatorText` so the shell can show the resolved action before the drop completes. The WinUI file list is a real drop target. Its handlers use an app-level drag/drop route and a Windows file-drop payload extractor, then update the view model during drag-over, clear it on drag-leave, and commit through the same operation boundary on drop.

Copy, move, and shortcut drops commit through `FileOperationService` against the active folder, then reuse the existing post-mutation listing refresh path. Shortcut drops create `.lnk` files through the Windows Shell boundary with non-colliding names.

`src/VeloFile.Windows/DragDrop/WindowsOleDragDropDataAdapter.cs` is the Windows boundary for file-drop payloads. It projects file and directory paths into Core `DropItem` values and rejects empty or unsupported payloads without leaking OLE details into Core.

Drag/drop now treats external payload extraction as an input boundary. Extractor exceptions, inaccessible storage items, malformed paths, and mixed valid/invalid payloads resolve to controlled no-drop or recoverable drop-failure state instead of escaping the WinUI event route.

`tools/VeloFile.Corpus/` now supports deterministic `dragdrop` and `pathological` profiles. `run-compat-corpus.ps1 -Scope dragdrop` writes a drag/drop result document with the expected modifier actions. `run-compat-corpus.ps1 -Scope paths` now writes per-case path compatibility results with `verified`, `skipped`, `unavailable`, `not-implemented`, or `failed` status. Verified path cases include separate fixture and behavior evidence fields, and fixture-only cases cannot count as verified behavior.

`docs/qa/m10-dragdrop-compatibility-checklist.md` records the manual cross-app checks for Explorer, browser, IDE, and Office payloads that are too brittle for stable CI automation.

## Tests

New tests cover:

- Core drag/drop action resolution for same-volume, cross-volume, Ctrl, Shift, Ctrl+Shift, empty items, missing targets, unsupported shortcut payloads, and root-based volume classification;
- App shell drop-action indicator state and production-shaped drag/drop route handling for copy, move, shortcut, modifier changes, unsupported payloads, and destination refresh;
- App shell and Windows adapter input-boundary tests for throwing extractors, malformed paths, and conservative rejection of mixed valid/invalid payloads;
- Windows file-drop projection from paths into Core drop items and rejection of empty/unknown payloads;
- Windows shortcut operation mapping, `.lnk` creation, target verification, and non-colliding shortcut names;
- Corpus generation for `dragdrop` and `pathological` profiles;
- Compatibility runner support for `dragdrop` and `paths` result documents, including a guard against placeholder or fixture-only path cases being counted as verified behavior.

## Validation

- `dotnet test tests/VeloFile.Core.Tests/VeloFile.Core.Tests.csproj -c Debug --filter DragDrop`
- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter DragDrop`
- `dotnet test tests/VeloFile.Windows.Tests/VeloFile.Windows.Tests.csproj -c Debug --filter DragDrop`
- `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug --filter Compatibility_and_preview_runners_validate_scope`
- `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug --filter Generate_placeholder_profiles_are_deterministic`
- `dotnet test VeloFile.sln -c Debug --filter DragDrop`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope dragdrop -ScratchRoot <scratch-root>`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope paths -ScratchRoot <scratch-root>`
- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter AppShellContractTests`
- `dotnet build VeloFile.sln -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
