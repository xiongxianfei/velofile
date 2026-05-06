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

function Get-MarkdownAnchors {
    param([string] $Path)

    $anchors = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
    foreach ($line in Get-Content -Path $Path) {
        if ($line -notmatch '^(#{1,6})\s+(.+?)\s*$') {
            continue
        }

        $anchor = $Matches[2].Trim().ToLowerInvariant()
        $anchor = [Regex]::Replace($anchor, '[^\w\s-]', '')
        $anchor = [Regex]::Replace($anchor, '\s+', '-').Trim('-')
        if ($anchor.Length -gt 0) {
            $anchors.Add($anchor) | Out-Null
        }
    }

    return $anchors
}

function Read-YamlListValues {
    param(
        [string[]] $Lines,
        [string] $Key
    )

    $values = New-Object 'System.Collections.Generic.List[string]'
    $inSection = $false
    foreach ($line in $Lines) {
        if ($line -match '^\S') {
            $inSection = ($line -eq "$Key`:")
            continue
        }

        if (-not $inSection) {
            continue
        }

        if ($line -match '^\s+-\s+(.+?)\s*$') {
            $values.Add($Matches[1]) | Out-Null
        }
    }

    return $values
}

function Assert-RepoRelativeLink {
    param(
        [string] $SourcePath,
        [string] $Reference
    )

    if ([string]::IsNullOrWhiteSpace($Reference)) {
        throw "Change metadata in '$SourcePath' contains an empty reference."
    }

    if ([IO.Path]::IsPathRooted($Reference) -or $Reference.Contains("..")) {
        throw "Change metadata reference '$Reference' in '$SourcePath' must be repo-relative."
    }

    $parts = $Reference.Split('#', 2)
    $relativePath = $parts[0]
    $targetPath = Join-Path $repoRoot $relativePath
    if (-not (Test-Path $targetPath)) {
        throw "Change metadata reference '$Reference' in '$SourcePath' points to a missing path."
    }

    if ($parts.Count -eq 2) {
        if (-not (Test-Path $targetPath -PathType Leaf)) {
            throw "Change metadata reference '$Reference' in '$SourcePath' points to an anchor on a non-file path."
        }

        $anchors = Get-MarkdownAnchors $targetPath
        if (-not $anchors.Contains($parts[1])) {
            throw "Change metadata reference '$Reference' in '$SourcePath' points to a missing Markdown anchor."
        }
    }
}

function Test-IsPathReference {
    param([string] $Reference)

    return $Reference.Contains("/") -or $Reference.Contains("\") -or $Reference.EndsWith(".md", [StringComparison]::OrdinalIgnoreCase)
}

function Assert-ChangeMetadataLinks {
    $changeRoot = Join-Path $repoRoot "docs/changes"
    if (-not (Test-Path $changeRoot)) {
        return
    }

    foreach ($changeFile in Get-ChildItem -Path $changeRoot -Recurse -Filter change.yaml) {
        $lines = Get-Content -Path $changeFile.FullName
        foreach ($reference in (Read-YamlListValues -Lines $lines -Key "architecture")) {
            if (Test-IsPathReference $reference) {
                Assert-RepoRelativeLink -SourcePath $changeFile.FullName -Reference $reference
            }
        }

        foreach ($reference in (Read-YamlListValues -Lines $lines -Key "files")) {
            Assert-RepoRelativeLink -SourcePath $changeFile.FullName -Reference $reference
        }
    }
}

$packageManifest = Assert-File "src/VeloFile.App/Package.appxmanifest"
Assert-File "scripts/package-msix.ps1" | Out-Null
Assert-File "scripts/verify-release-tag.ps1" | Out-Null
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
Assert-ChangeMetadataLinks

if (-not $SkipPublish) {
    & (Join-Path $repoRoot "scripts/package-msix.ps1") -Mode UnsignedLocal -SkipPublish:$false
}

Write-Host "Release verification passed."
