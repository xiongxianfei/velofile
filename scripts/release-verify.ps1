[CmdletBinding()]
param(
    [switch] $SkipPublish
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot

function Assert-File {
    param([string] $RelativePath)

    $path = Join-Path $repoRoot $RelativePath
    if (-not (Test-Path $path)) {
        throw "Required release artifact '$RelativePath' is missing."
    }

    return $path
}

function Assert-Contains {
    param(
        [string] $RelativePath,
        [string] $Expected
    )

    $path = Assert-File $RelativePath
    $content = Get-Content -Raw -Path $path
    if ($content.IndexOf($Expected, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Release artifact '$RelativePath' must contain '$Expected'."
    }
}

$packageManifest = Assert-File "src/VeloFile.App/Package.appxmanifest"
Assert-File "scripts/package-msix.ps1" | Out-Null
Assert-File "docs/release/stable-update-channel.md" | Out-Null
Assert-File "docs/release/install-rollback.md" | Out-Null
Assert-File "docs/release/v1-release-notes.md" | Out-Null
Assert-File "docs/release/release-checklist.md" | Out-Null
Assert-File "docs/user/differences-from-file-explorer.md" | Out-Null

[xml] $manifest = Get-Content -Raw -Path $packageManifest
$identity = $manifest.Package.Identity
if ($identity.Name -ne "VeloFile") {
    throw "Package identity must be VeloFile."
}

if ([string]::IsNullOrWhiteSpace($identity.Publisher) -or -not $identity.Publisher.StartsWith("CN=", [StringComparison]::Ordinal)) {
    throw "Package publisher must name a signing certificate subject."
}

$manifestText = Get-Content -Raw -Path $packageManifest
foreach ($forbidden in @("FileTypeAssociation", "windows.fileExplorerContextMenus", "AppExecutionAlias")) {
    if ($manifestText.IndexOf($forbidden, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "Package manifest must not contain '$forbidden'."
    }
}

Assert-Contains "docs/release/stable-update-channel.md" "published release source"
Assert-Contains "docs/release/stable-update-channel.md" "signing identity"
Assert-Contains "docs/release/stable-update-channel.md" "versioning policy"
Assert-Contains "docs/release/stable-update-channel.md" "update cadence"
Assert-Contains "docs/release/stable-update-channel.md" "rollback"
Assert-Contains "docs/release/install-rollback.md" "Uninstalling VeloFile is the rollback path"
Assert-Contains "docs/release/install-rollback.md" "Explorer remains available"
Assert-Contains "docs/release/v1-release-notes.md" "file extensions are shown by default"
Assert-Contains "docs/release/v1-release-notes.md" "invoice.pdf.exe"
Assert-Contains "docs/release/v1-release-notes.md" "per-application"
Assert-Contains "docs/user/differences-from-file-explorer.md" "OS shell extension"
Assert-Contains "docs/user/differences-from-file-explorer.md" "does not replace File Explorer"
Assert-Contains "docs/release/release-checklist.md" "Uninstall"
Assert-Contains "docs/release/release-checklist.md" "file associations"

if (-not $SkipPublish) {
    & (Join-Path $repoRoot "scripts/package-msix.ps1") -Mode UnsignedLocal -SkipPublish:$false
}

Write-Host "Release verification passed."
