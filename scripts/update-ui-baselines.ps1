param(
    [Parameter(Mandatory = $true)]
    [string]$Suite,

    [string]$Profile,

    [switch]$AllReviewed,

    [string]$ReviewId,

    [string]$RepositoryRoot
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $scriptRoot = if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) { Split-Path -Parent $MyInvocation.MyCommand.Path } else { $PSScriptRoot }
    $RepositoryRoot = (Resolve-Path (Join-Path $scriptRoot "..")).Path
}

if ([string]::IsNullOrWhiteSpace($ReviewId)) {
    Write-Error "ReviewId is required when updating UI baselines."
    exit 2
}

if ($Suite -ne "winui") {
    Write-Error "Unsupported UI baseline suite '$Suite'. Expected 'winui'."
    exit 2
}

if (-not $AllReviewed -and [string]::IsNullOrWhiteSpace($Profile)) {
    Write-Error "Profile is required unless -AllReviewed is supplied."
    exit 2
}

$visualRoot = Join-Path $RepositoryRoot "tests\visual"
$currentSuiteRoot = Join-Path (Join-Path $visualRoot "current") $Suite
$baselineSuiteRoot = Join-Path (Join-Path $visualRoot "baselines") $Suite

if (-not (Test-Path -LiteralPath $currentSuiteRoot -PathType Container)) {
    Write-Error "No current screenshots found for suite '$Suite'. Expected current screenshots under $currentSuiteRoot."
    exit 3
}

$profiles = @()
if ($AllReviewed) {
    $profiles = Get-ChildItem -LiteralPath $currentSuiteRoot -Directory | Select-Object -ExpandProperty Name
} else {
    $profiles = @($Profile)
}

if ($profiles.Count -eq 0) {
    Write-Error "No current screenshots found for suite '$Suite'."
    exit 3
}

$updatedCount = 0
foreach ($profileName in $profiles) {
    $currentProfileRoot = Join-Path $currentSuiteRoot $profileName
    if (-not (Test-Path -LiteralPath $currentProfileRoot -PathType Container)) {
        Write-Error "No current screenshots found for profile '$profileName'. Expected $currentProfileRoot."
        exit 3
    }

    $pngFiles = @(Get-ChildItem -LiteralPath $currentProfileRoot -Filter "*.png" -File)
    if ($pngFiles.Count -eq 0) {
        Write-Error "No current screenshots found for profile '$profileName'."
        exit 3
    }

    $baselineProfileRoot = Join-Path $baselineSuiteRoot $profileName
    New-Item -ItemType Directory -Force -Path $baselineProfileRoot | Out-Null

    foreach ($png in $pngFiles) {
        $screenName = [System.IO.Path]::GetFileNameWithoutExtension($png.Name)
        $currentSidecar = Join-Path $currentProfileRoot ($screenName + ".json")
        if (-not (Test-Path -LiteralPath $currentSidecar -PathType Leaf)) {
            Write-Error "Current screenshot '$($png.Name)' is missing JSON sidecar '$currentSidecar'."
            exit 4
        }

        Copy-Item -LiteralPath $png.FullName -Destination (Join-Path $baselineProfileRoot $png.Name) -Force

        $json = Get-Content -LiteralPath $currentSidecar -Raw | ConvertFrom-Json
        $json | Add-Member -NotePropertyName reviewId -NotePropertyValue $ReviewId -Force
        $json | Add-Member -NotePropertyName updatedAtUtc -NotePropertyValue ([DateTimeOffset]::UtcNow.ToString("O")) -Force
        $json | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath (Join-Path $baselineProfileRoot ($screenName + ".json")) -Encoding UTF8
        $updatedCount++
    }
}

Write-Host "Updated $updatedCount $Suite baseline screenshot(s) for review '$ReviewId'."
