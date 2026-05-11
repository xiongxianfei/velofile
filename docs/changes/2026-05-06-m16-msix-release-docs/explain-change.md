# M16 MSIX Release Path

M16 finishes the V1 packaging and documentation path without turning VeloFile into an Explorer replacement.

## What Changed

`src/VeloFile.App` now declares an MSIX package manifest while keeping normal Release publish unpackaged by default. The source manifest uses the standard MSIX token form for the app executable and entry point, declares WinUI package resources, and includes `runFullTrust` for the packaged desktop app path. It deliberately omits file type associations, Explorer context menu extensions, and app execution aliases.

`scripts/package-msix.ps1` now publishes the app, normalizes the manifest for the package layout, generates required package logo assets, creates an unsigned local `.msix` with Windows SDK `MakeAppx`, writes package metadata, and signs the package with Windows SDK `SignTool` when `-Mode SignedRelease` and a certificate thumbprint are supplied. The GitHub release workflow runs on Windows, verifies release readiness, builds the signed package, and uploads the MSIX plus metadata.

`scripts/verify-release-tag.ps1` verifies the release tag in a temporary isolated GPG home, imports only configured VeloFile release public keys, extracts the `VALIDSIG` fingerprint, and requires that fingerprint to match the configured approved release-key allowlist before the workflow can package or publish.

`scripts/release-verify.ps1` replaces the template release verifier with product-specific checks for the package manifest, no Explorer/file-association ownership, stable update channel docs, rollback/uninstall docs, V1 release notes, user differences docs, the release checklist, and release-tag verifier presence. The Bash wrapper now delegates to the PowerShell verifier.

The release and user docs now cover the stable update channel, signing identity, versioning policy, update cadence, install/update/rollback/uninstall, default extension display, `invoice.pdf.exe` visibility, per-application extension settings, built-in context menu scope, and the differences from File Explorer. README and SECURITY now point at the release path instead of stale foundation or placeholder security text.

## Safety Notes

Unsigned local packaging and signed release packaging are separate on purpose. Contributors can validate the package layout without release signing credentials. Release owners must supply the signing thumbprint through the Windows release workflow.

Rollback remains uninstalling the MSIX package. VeloFile does not replace File Explorer, does not register as the Windows shell, and does not take ownership of system file associations.

The manual install/update/rollback/uninstall matrix is recorded in `docs/release/release-checklist.md`. It was not executed in this local implementation run because there is no release signing certificate or previous signed package in the workspace.

Final verification also cleaned the release workflow ordering assertions so they use MSTest comparison helpers instead of generic boolean assertions. The test still proves trusted tag verification precedes packaging and GitHub release creation, but Debug builds no longer emit analyzer warnings for those assertions.

## Tests

`ReleasePackagingContractTests` prove the project declares the MSIX manifest, the manifest avoids Explorer replacement and file-association ownership, release scripts/workflow run Windows package checks, the release verifier executes, and release/user docs cover extension display, File Explorer differences, rollback, and checklist requirements.

Review-resolution tests also prove that the stable-channel signed Git tag claim is enforced through an isolated trusted-key verifier with full fingerprint allowlisting before packaging/release, the verifier accepts only `VALIDSIG` status output from an approved full fingerprint, x86/x64/ARM64 package inputs map to matching .NET RIDs and manifest architectures, and M16 change metadata links resolve to tracked files and Markdown anchors.

## Validation

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter ReleasePackagingContractTests`
- `dotnet publish src\VeloFile.App\VeloFile.App.csproj -c Release`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\package-msix.ps1`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\release-verify.ps1`
- `dotnet test VeloFile.sln -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
- Review resolution additionally ran the targeted signed-tag/RID/link tests, signed-release ARM64 dry-run packaging, `dotnet test VeloFile.sln -c Debug --filter Release`, and final CI.
- Trusted release-key remediation additionally ran the signed-tag workflow contract test, deterministic `git verify-tag --raw` status fixture tests for approved, unapproved, unsigned, missing-key, and empty/lightweight-style status, parsed `scripts\verify-release-tag.ps1`, reran `ReleasePackagingContractTests`, and reran `scripts\release-verify.ps1 -SkipPublish`.
- Full temp Git/GPG integration is intentionally deferred because the verifier's trust decision is covered by deterministic status fixtures. The release workflow contract test separately proves production invokes the verifier before packaging with isolated `GNUPGHOME` and trusted key configuration.
