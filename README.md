# VeloFile

<!-- vision:start -->
VeloFile is a fast, lightweight, open-source file explorer for Windows 10 and 11. It is for everyday browsing and file management when Windows File Explorer feels slow, unpredictable, or buried under legacy behavior.

VeloFile chooses a narrow, Windows-native daily workflow instead of becoming a full power-user suite. It keeps hot paths like folder navigation, current-folder filtering, tabs, preview, and common file operations responsive while still respecting Windows expectations.

It serves everyday Windows users who want reliable browsing, developers who live in project folders and terminals, and power users who value tabs, preview, keyboard flow, and clear behavior.

See [VISION.md](VISION.md) for goals, non-goals, and falsifiability.
<!-- vision:end -->

VeloFile V1 is implemented behind a Windows App SDK app shell with Core and Windows boundary projects, MSTest contract coverage, compatibility tooling, CI, and release packaging checks. Release builds are distributed as side-by-side MSIX packages and do not replace Windows File Explorer.

## User And Release Documentation

- [Differences from File Explorer](docs/user/differences-from-file-explorer.md)
- [Install, rollback, and uninstall](docs/release/install-rollback.md)
- [Stable update channel](docs/release/stable-update-channel.md)
- [V1 release notes](docs/release/v1-release-notes.md)

## Requirements

- Windows 10 or Windows 11.
- .NET SDK 10.x.
- Visual Studio 2022 with WinUI / Windows App SDK C# tooling.

## Run Locally

From a Windows developer shell at the repository root:

```powershell
dotnet restore VeloFile.sln
dotnet build src\VeloFile.App\VeloFile.App.csproj -c Debug -p:Platform=x64
dotnet run --project src\VeloFile.App\VeloFile.App.csproj -c Debug -p:Platform=x64
```

You can also open `VeloFile.sln` in Visual Studio 2022, set `VeloFile.App` as the startup project, choose `x64`, and press F5.

The local Debug run is unpackaged. Use the release commands below when you need an MSIX package.

## Build And Test

```powershell
dotnet restore VeloFile.sln
dotnet build VeloFile.sln -c Debug
dotnet test VeloFile.sln -c Debug
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1
```

The GitHub PR workflow runs on Windows with `pwsh`. Ordinary PRs run `ci-fast-required` for fast PR confidence, and the active default-branch ruleset requires that check. Broad closeout validation is no longer part of the default PR/push workflow.

### Focused Validation Tiers

Use the focused tiers for local feedback after the relevant projects are built:

```powershell
dotnet test VeloFile.sln -c Debug --no-build --filter "TestCategory=Fast|TestCategory=Contract"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --no-build --filter "TestCategory=Contract"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=CorpusScript&TestCategory=Smoke"
dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "TestCategory=ReleaseEvidence"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1
```

`--no-build` commands assume the relevant projects have already been built. Use `scripts\ci.ps1` for broad milestone closeout and review gates; focused tiers are for intentional local feedback, not a replacement for full validation.

### Hosted CI Tiers

- `ci-fast-required`: ordinary PR confidence. It restores, builds, validates UI contracts, runs Core/App/Windows tests directly, runs Corpus `Fast|Contract`, and runs Corpus `CorpusScript&Smoke`. Its summary reports `ReleaseEvidence: not run in this lane`, `CorpusScript Smoke: run`, and `Full closeout: not run`.
- `ci-release-evidence`: manual, scheduled, release, and merge-queue release-evidence validation. It is not the default ordinary PR gate and remains part of release readiness.
- `ci-full-closeout`: manual full closeout workflow that invokes `scripts/ci.ps1`. Full closeout remains available for milestone and release gates.

Fast PR confidence is not release readiness. Use `ci-release-evidence`, `ci-full-closeout`, local `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`, or another accepted release gate before claiming release readiness. The rollback path is to make the broad closeout check required again and leave `ci-fast-required` optional.

## Release Verification

```powershell
dotnet publish src/VeloFile.App/VeloFile.App.csproj -c Release
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/package-msix.ps1
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/release-verify.ps1
```

Unsigned local packaging creates an unsigned MSIX under `artifacts/msix/`. Signed release packaging runs through the Windows release workflow with a release-owner signing certificate thumbprint.

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

VeloFile is licensed under Apache-2.0.
