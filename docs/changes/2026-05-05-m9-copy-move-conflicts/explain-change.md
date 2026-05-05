# M9 Copy/Move, Conflicts, Progress, Cancellation, and Undo Eligibility

M9 extends the M8 operation boundary from rename/delete into copy and move. The goal is to keep file mutations behind the same auditable Core service and Windows adapter path while adding visible conflict and undo-eligibility behavior.

## What Changed

`src/VeloFile.Core/Operations/` now includes `Copy` and `Move` operation kinds, target-directory requests, conflict choices, a conflict-required adapter result, and a `WaitingForConflict` operation state. `FileOperationService` validates selected items and target directories, propagates cancellable execution, records progress/state, and resumes a pending conflict with Skip, Replace, or Keep both.

Undo eligibility remains deliberately narrow. Copy never creates an undo-eligible state. Move, rename, and Recycle Bin delete can report undo eligibility only when the adapter reports support. Permanent delete remains non-undoable.

`src/VeloFile.Windows/Shell/WindowsShellFileOperationAdapter.cs` now maps Core copy/move requests into Windows shell-operation intents. A collision probe detects same-name destination conflicts before execution. The Visual Basic shell executor handles Skip, Replace, and Keep both choices. Skip now skips only targets whose destination exists, so unaffected batch items continue.

`AppShellViewModel` stages Copy and Cut selections as pending file transfers. Paste runs copy/move against the active folder, shows operation state, and refreshes the target listing after successful completion. If a conflict appears, the shell keeps the original paste target across navigation and refreshes that target after the user resolves the conflict.

`MainWindow.xaml` and code-behind add a small conflict panel with Skip, Replace, and Keep both routes into the view model. The app contract tests assert those routes are present.

The corpus tool now treats `run-compat-corpus.ps1 -Scope operations` as a real M9 compatibility gate. It generates the operations profile and verifies copy, move, collision, rename, delete, and partial-batch placeholder fixtures inside the scratch root.

## Tests

New tests cover:

- Core copy/move request routing, target-directory validation, progress/cancellation state, conflict pause/resume, and undo eligibility;
- App shell Copy/Cut/Paste routes, conflict visibility, conflict resolution, original-target refresh after navigation, and copy/move listing refresh;
- Windows copy/move request mapping, collision classification, Replace routing, and Skip behavior that preserves unaffected batch items;
- XAML/code-behind conflict panel routes for Skip, Replace, and Keep both;
- operations corpus runner support and fixture result shape.

## Validation

- `dotnet test VeloFile.sln -c Debug --filter Operations`
- `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope operations -ScratchRoot <scratch-root>`
- `dotnet build VeloFile.sln -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
