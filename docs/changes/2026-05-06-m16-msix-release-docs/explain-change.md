# M16 MSIX Release Path

M16 finishes the V1 packaging and documentation path without turning VeloFile into an Explorer replacement.

## What Changed

`src/VeloFile.App` now declares an MSIX package manifest while keeping normal Release publish unpackaged by default. The source manifest uses the standard MSIX token form for the app executable and entry point, declares WinUI package resources, and includes `runFullTrust` for the packaged desktop app path. It deliberately omits file type associations, Explorer context menu extensions, and app execution aliases.

`scripts/package-msix.ps1` now publishes the app, normalizes the manifest for the package layout, generates required package logo assets, creates an unsigned local `.msix` with Windows SDK `MakeAppx`, writes package metadata, and signs the package with Windows SDK `SignTool` when `-Mode SignedRelease` and a certificate thumbprint are supplied. The GitHub release workflow runs on Windows, verifies release readiness, builds the signed package, and uploads the MSIX plus metadata.

`scripts/release-verify.ps1` replaces the template release verifier with product-specific checks for the package manifest, no Explorer/file-association ownership, stable update channel docs, rollback/uninstall docs, V1 release notes, user differences docs, and the release checklist. The Bash wrapper now delegates to the PowerShell verifier.

The release and user docs now cover the stable update channel, signing identity, versioning policy, update cadence, install/update/rollback/uninstall, default extension display, `invoice.pdf.exe` visibility, per-application extension settings, built-in context menu scope, and the differences from File Explorer. README and SECURITY now point at the release path instead of stale foundation or placeholder security text.

## Safety Notes

Unsigned local packaging and signed release packaging are separate on purpose. Contributors can validate the package layout without release signing credentials. Release owners must supply the signing thumbprint through the Windows release workflow.

Rollback remains uninstalling the MSIX package. VeloFile does not replace File Explorer, does not register as the Windows shell, and does not take ownership of system file associations.

The manual install/update/rollback/uninstall matrix is recorded in `docs/release/release-checklist.md`. It was not executed in this local implementation run because there is no release signing certificate or previous signed package in the workspace.

## Tests

`ReleasePackagingContractTests` prove the project declares the MSIX manifest, the manifest avoids Explorer replacement and file-association ownership, release scripts/workflow run Windows package checks, the release verifier executes, and release/user docs cover extension display, File Explorer differences, rollback, and checklist requirements.

## Validation

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter ReleasePackagingContractTests`
- `dotnet publish src\VeloFile.App\VeloFile.App.csproj -c Release`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\package-msix.ps1`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\release-verify.ps1`
- `dotnet test VeloFile.sln -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
