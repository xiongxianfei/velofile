# VeloFile

<!-- vision:start -->
VeloFile is a fast, lightweight, open-source file explorer for Windows 10 and 11. It is for everyday browsing and file management when Windows File Explorer feels slow, unpredictable, or buried under legacy behavior.

VeloFile chooses a narrow, Windows-native daily workflow instead of becoming a full power-user suite. It keeps hot paths like folder navigation, current-folder filtering, tabs, preview, and common file operations responsive while still respecting Windows expectations.

It serves everyday Windows users who want reliable browsing, developers who live in project folders and terminals, and power users who value tabs, preview, keyboard flow, and clear behavior.

See [VISION.md](VISION.md) for goals, non-goals, and falsifiability.
<!-- vision:end -->

VeloFile V1 is currently in foundation work. The repository now contains the initial WinUI 3 / Windows App SDK app shell, core and Windows boundary projects, MSTest smoke tests, and Windows CI entry point.

## Requirements

- Windows 10 or Windows 11.
- .NET SDK 10.x.
- Visual Studio 2022 with WinUI / Windows App SDK C# tooling.

## Build And Test

```powershell
dotnet restore VeloFile.sln
dotnet build VeloFile.sln -c Debug
dotnet test VeloFile.sln -c Debug
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1
```

The GitHub CI workflow runs on Windows with `pwsh` and calls `scripts/ci.ps1`. Local Windows PowerShell can run the same script as a fallback when PowerShell 7 is unavailable.

## Project Layout

```text
src/VeloFile.App       WinUI desktop app shell
src/VeloFile.Core      Product-neutral core foundation
src/VeloFile.Windows   Windows integration boundary
tests/                 MSTest smoke and contract tests
specs/                 Approved product and test specifications
docs/                  Plans, architecture, ADRs, and change notes
```

## License

VeloFile is licensed under the MIT license.
