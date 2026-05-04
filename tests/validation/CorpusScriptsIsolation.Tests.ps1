[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $ScratchRoot
)

$ErrorActionPreference = "Stop"

function Get-RepoRoot {
    $current = Resolve-Path .
    while ($null -ne $current) {
        $candidate = Join-Path $current.Path "VeloFile.sln"
        if (Test-Path -LiteralPath $candidate) {
            return $current.Path
        }

        $parent = Split-Path -Parent $current.Path
        if ([string]::IsNullOrWhiteSpace($parent) -or $parent -eq $current.Path) {
            break
        }

        $current = Resolve-Path $parent
    }

    throw "Could not find repository root."
}

function Get-RepoSnapshot {
    param([string] $RepoRoot)

    $snapshot = @{}
    $repoPrefix = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]] @('\', '/')) + [System.IO.Path]::DirectorySeparatorChar

    Get-ChildItem -LiteralPath $RepoRoot -Recurse -Force -File |
        Where-Object { $_.FullName -notlike (Join-Path $RepoRoot ".git\*") } |
        ForEach-Object {
            $fullName = [System.IO.Path]::GetFullPath($_.FullName)
            if (!$fullName.StartsWith($repoPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "Repository snapshot path escaped the repository root."
            }

            $relative = $fullName.Substring($repoPrefix.Length)
            $snapshot[$relative] = "$($_.Length)|$($_.LastWriteTimeUtc.Ticks)"
        }

    return $snapshot
}

function Compare-RepoSnapshot {
    param(
        [hashtable] $Before,
        [hashtable] $After
    )

    $changes = New-Object System.Collections.Generic.List[string]

    foreach ($key in $Before.Keys) {
        if (!$After.ContainsKey($key)) {
            $changes.Add("deleted: $key")
        }
        elseif ($After[$key] -ne $Before[$key]) {
            $changes.Add("modified: $key")
        }
    }

    foreach ($key in $After.Keys) {
        if (!$Before.ContainsKey($key)) {
            $changes.Add("created: $key")
        }
    }

    return $changes
}

function Invoke-Checked {
    param([string[]] $Command)

    & $Command[0] @($Command | Select-Object -Skip 1)
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code ${LASTEXITCODE}: $($Command -join ' ')"
    }
}

$repoRoot = Get-RepoRoot
. (Join-Path $repoRoot "scripts\Invoke-CorpusTool.ps1")

$scratchFullPath = Resolve-VeloFileCorpusScratchRoot -ScratchRoot $ScratchRoot

if (Test-Path -LiteralPath $scratchFullPath) {
    Remove-Item -LiteralPath $scratchFullPath -Recurse -Force
}

$before = Get-RepoSnapshot -RepoRoot $repoRoot

Invoke-Checked @("powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", (Join-Path $repoRoot "scripts/generate-corpus.ps1"), "-Profile", "smoke", "-ScratchRoot", $scratchFullPath)
Invoke-Checked @("powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", (Join-Path $repoRoot "scripts/run-compat-corpus.ps1"), "-Scope", "smoke", "-ScratchRoot", $scratchFullPath)
Invoke-Checked @("powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", (Join-Path $repoRoot "scripts/run-preview-corpus.ps1"), "-ScratchRoot", $scratchFullPath)
Invoke-Checked @("powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", (Join-Path $repoRoot "scripts/run-benchmarks.ps1"), "-NonGating", "-ScratchRoot", $scratchFullPath)

$after = Get-RepoSnapshot -RepoRoot $repoRoot
$changes = Compare-RepoSnapshot -Before $before -After $after

if ($changes.Count -gt 0) {
    $changes | ForEach-Object { Write-Error $_ }
    throw "Corpus scripts created, modified, or deleted files under the repository."
}

if (!(Test-Path -LiteralPath (Join-Path $scratchFullPath ".velofile-tools\publish\VeloFile.Corpus\VeloFile.Corpus.dll"))) {
    throw "Published corpus tool was not found under the scratch-owned tool directory."
}

Write-Output "Corpus script isolation passed."
