# M1 Foundation Change Explanation

## Scope

M1 establishes the first buildable VeloFile product foundation:

- A Windows-first solution with `VeloFile.App`, `VeloFile.Core`, and `VeloFile.Windows`.
- A minimal WinUI 3 / Windows App SDK app shell.
- MSTest smoke tests for product identity, bootstrapping, app project shape, and the Windows integration boundary.
- Real Windows CI commands in `scripts/ci.ps1`, a Bash wrapper, and the GitHub workflow.
- Repository docs updated so contributors can build and test the actual solution instead of the template.

This change does not implement file browsing, Shell operations, persistence, diagnostics, preview, search, tabs, packaging, or benchmark corpora. Those remain assigned to later milestones in the active plan.

## Test-First Evidence

The first M1 tests were added before production files:

- `tests/VeloFile.Core.Tests/FoundationSmokeTests.cs`
- `tests/VeloFile.Windows.Tests/WindowsBoundarySmokeTests.cs`
- `tests/VeloFile.App.Tests/AppProjectSmokeTests.cs`

The initial `dotnet test VeloFile.sln -c Debug --no-restore` run failed for the expected reasons: missing `VeloFile.Core.Foundation`, missing `VeloFile.Windows.Foundation`, and missing WinUI app project/XAML files. Production scaffolding was then added to satisfy those tests.

## Design Choices

The app shell is intentionally small. It proves the selected V1 stack can restore, build, test, and launch a WinUI surface without pulling feature behavior into M1.

`VeloFile.Core` owns product-neutral startup identity and bootstrapping. `VeloFile.Windows` owns the Windows integration boundary marker so later Shell, Win32, WinRT, and OLE work has a clear assembly boundary before implementation begins.

`VeloFile.App` references core state and renders a single visible window. It does not directly own file operations, persistence, diagnostics, or Shell interop.

## Validation

M1 validation passed with:

- `dotnet --info`
- `dotnet restore VeloFile.sln`
- `dotnet build VeloFile.sln -c Debug`
- `dotnet test VeloFile.sln -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1` as a fallback route before `pwsh` was installed.
- A launch smoke that started `VeloFile.App.exe`, confirmed it stayed alive for 2 seconds, then stopped it.

`dotnet build` completed with 0 warnings and 0 errors. `dotnet test` passed 5 tests across 3 test assemblies. `scripts/select-validation.py` is not present in M1, so selector-based validation is recorded as unavailable rather than skipped silently.
