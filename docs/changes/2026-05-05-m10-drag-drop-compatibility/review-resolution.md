# M10 Review Resolution

## Findings Addressed

- Production WinUI drag/drop route was missing.
- Shortcut drops were advertised as allowed but rejected during commit.
- Path compatibility corpus results treated placeholder labels as passed evidence.
- Second-pass review found drag/drop extraction failures could escape the WinUI route.
- Second-pass review found path corpus fixture creation could still be counted as verified behavior.

## Resolution

- Wired `FileListSurface` as a WinUI drop target with drag-over, drag-leave, and drop handlers.
- Added `AppDragDropRoute` so handlers translate platform drag events into the same view-model operation path used by tests.
- Added shortcut creation as a file operation kind. Shortcut drops now create non-colliding `.lnk` files through the Windows Shell boundary and refresh the destination listing on completion.
- Added shortcut-payload gating so unsupported payloads cannot advertise a valid shortcut drop.
- Replaced path compatibility placeholder pass output with per-case results using `verified`, `skipped`, `unavailable`, or `failed` status and scratch-relative fixture references.
- Added drag/drop extraction exception boundaries in the app route and WinUI event handlers. Throwing extractors, inaccessible storage items, malformed paths, and mixed valid/invalid payloads now resolve to no-drop or recoverable drop failure without starting operations.
- Split path corpus evidence into fixture and behavior fields. Verified cases now require `behaviorVerifierInvoked = true` and `verifiedBehavior = true`; junction and reparse-loop cases use bounded Core recursive-search loop-detection evidence, while file path cases use Core listing evidence.
- Updated the corpus script helper to copy `VeloFile.Core` into the scratch-owned tool source before publishing so corpus behavior checks can use Core listing/search services without producing repo-side build output.

## Validation

- `dotnet test tests/VeloFile.Core.Tests/VeloFile.Core.Tests.csproj -c Debug --filter "DragDrop|Create_shortcuts"` passed 6 tests.
- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter DragDrop` passed 7 tests.
- `dotnet test tests/VeloFile.Windows.Tests/VeloFile.Windows.Tests.csproj -c Debug --filter "Create_shortcut|DragDrop"` passed 5 tests.
- `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug --filter Compatibility_and_preview_runners_validate_scope` passed 1 test.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test VeloFile.sln -c Debug --filter DragDrop` passed 5 Core, 7 App, and 2 Windows drag/drop tests; Corpus tests had no matching DragDrop filter.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope dragdrop -ScratchRoot <scratch-root>` passed.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope paths -ScratchRoot <scratch-root>` passed with per-case path outcomes.
- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter AppShellContractTests` passed 11 tests.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed restore, build with 0 warnings and 0 errors, and 232 tests across Windows, App, Core, and Corpus test assemblies.

Second-pass review-resolution validation:

- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug --filter DragDrop` passed 9 tests.
- `dotnet test tests/VeloFile.Windows.Tests/VeloFile.Windows.Tests.csproj -c Debug --filter OleDragDrop` passed 3 tests.
- `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug --filter Compatibility_and_preview_runners_validate_scope` passed 1 test.
- `dotnet build VeloFile.sln -c Debug` passed with 0 warnings and 0 errors.
- `dotnet test tests/VeloFile.Core.Tests/VeloFile.Core.Tests.csproj -c Debug --filter "DragDrop|Create_shortcuts"` passed 6 tests.
- `dotnet test tests/VeloFile.Windows.Tests/VeloFile.Windows.Tests.csproj -c Debug --filter "Create_shortcut|DragDrop|OleDragDrop"` passed 6 tests.
- `dotnet test VeloFile.sln -c Debug --filter DragDrop` passed 5 Core, 9 App, and 3 Windows drag/drop tests; Corpus tests had no matching DragDrop filter.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope dragdrop -ScratchRoot <scratch-root>` passed.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope paths -ScratchRoot <scratch-root>` passed with behavior-verifier evidence in path case results.
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` passed restore, build with 0 warnings and 0 errors, and 235 tests across Windows, App, Core, and Corpus test assemblies.
