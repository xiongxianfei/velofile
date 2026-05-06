[CmdletBinding()]
param(
    [ValidateSet("UnsignedLocal", "SignedRelease")]
    [string] $Mode = "UnsignedLocal",

    [ValidateSet("x64", "x86", "ARM64")]
    [string] $Platform = "x64",

    [string] $Configuration = "Release",

    [string] $Version = "0.1.0.0",

    [string] $OutputRoot = "artifacts/msix",

    [string] $SigningThumbprint = "",

    [string] $TimestampUrl = "https://timestamp.digicert.com",

    [switch] $SkipPublish,

    [switch] $DryRun
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "src/VeloFile.App/VeloFile.App.csproj"
$packageManifest = Join-Path $repoRoot "src/VeloFile.App/Package.appxmanifest"
$outputPath = Join-Path $repoRoot $OutputRoot
$publishPath = Join-Path $outputPath "publish-$Platform"
$metadataPath = Join-Path $outputPath "package-metadata.json"
$packagePath = Join-Path $outputPath "VeloFile_$Version`_$Platform.msix"

function Find-WindowsSdkTool {
    param([string] $ToolName)

    $command = Get-Command $ToolName -ErrorAction SilentlyContinue
    if ($command -and -not [string]::IsNullOrWhiteSpace($command.Source)) {
        return $command.Source
    }

    $programFilesX86 = ${env:ProgramFiles(x86)}
    if ([string]::IsNullOrWhiteSpace($programFilesX86)) {
        return $null
    }

    $kitsRoot = Join-Path $programFilesX86 "Windows Kits/10/bin"
    if (-not (Test-Path $kitsRoot)) {
        return $null
    }

    $architectureFolder = if ($Platform -eq "x86") { "x86" } else { "x64" }
    $tool = Get-ChildItem -Path $kitsRoot -Recurse -Filter $ToolName -ErrorAction SilentlyContinue |
        Where-Object { $_.DirectoryName -match [Regex]::Escape("\$architectureFolder") + "$" } |
        Sort-Object FullName -Descending |
        Select-Object -First 1

    if ($tool) {
        return $tool.FullName
    }

    return $null
}

function Convert-PlatformToManifestArchitecture {
    param([string] $PackagePlatform)

    if ($PackagePlatform -eq "ARM64") {
        return "arm64"
    }

    return $PackagePlatform
}

function Convert-PlatformToRuntimeIdentifier {
    param([string] $PackagePlatform)

    if ($PackagePlatform -eq "x86") {
        return "win-x86"
    }

    if ($PackagePlatform -eq "ARM64") {
        return "win-arm64"
    }

    return "win-x64"
}

function New-PackageAsset {
    param(
        [string] $Path,
        [int] $Width,
        [int] $Height
    )

    Add-Type -AssemblyName System.Drawing

    $directory = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $directory | Out-Null

    $bitmap = [System.Drawing.Bitmap]::new($Width, $Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.Clear([System.Drawing.Color]::FromArgb(20, 82, 140))
        $pen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(240, 245, 255), [Math]::Max(2, [Math]::Floor($Width / 24)))
        try {
            $margin = [Math]::Max(4, [Math]::Floor($Width / 8))
            $graphics.DrawRectangle($pen, $margin, $margin, $Width - ($margin * 2), $Height - ($margin * 2))
            $graphics.DrawLine($pen, $margin, [Math]::Floor($Height * 0.42), $Width - $margin, [Math]::Floor($Height * 0.42))
        }
        finally {
            $pen.Dispose()
        }

        $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

function Write-PackagingManifest {
    param(
        [string] $SourceManifest,
        [string] $DestinationManifest,
        [string] $ManifestVersion,
        [string] $PackagePlatform
    )

    [xml] $manifest = Get-Content -Raw -Path $SourceManifest
    $manifest.Package.Identity.Version = $ManifestVersion
    $manifest.Package.Identity.ProcessorArchitecture = Convert-PlatformToManifestArchitecture $PackagePlatform
    $application = $manifest.Package.Applications.Application
    $application.Executable = "VeloFile.App.exe"
    $application.EntryPoint = "Windows.FullTrustApplication"
    $manifest.Save($DestinationManifest)
}

function Invoke-NativeCommand {
    param(
        [string] $FilePath,
        [string[]] $Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command '$FilePath $($Arguments -join ' ')' failed with exit code $LASTEXITCODE."
    }
}

if (-not (Test-Path $packageManifest)) {
    throw "Package.appxmanifest is required for MSIX packaging."
}

if ($Mode -eq "SignedRelease" -and [string]::IsNullOrWhiteSpace($SigningThumbprint)) {
    throw "SignedRelease MSIX packaging requires -SigningThumbprint."
}

$runtimeIdentifier = Convert-PlatformToRuntimeIdentifier $Platform
$manifestArchitecture = Convert-PlatformToManifestArchitecture $Platform
$dryRunCommands = New-Object System.Collections.Generic.List[object]

New-Item -ItemType Directory -Force -Path $publishPath | Out-Null

$publishArguments = @(
    "publish",
    $projectPath,
    "-c",
    $Configuration,
    "-p:Platform=$Platform",
    "-p:WindowsPackageType=None",
    "-p:AppxPackageSigningEnabled=false",
    "-p:PackageVersion=$Version",
    "-r",
    $runtimeIdentifier,
    "-o",
    $publishPath)

$dryRunCommands.Add([ordered]@{ name = "dotnet publish"; executable = "dotnet"; arguments = $publishArguments }) | Out-Null

if (-not $SkipPublish -and -not $DryRun) {
    & dotnet @publishArguments

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE."
    }
}

Write-PackagingManifest `
    -SourceManifest $packageManifest `
    -DestinationManifest (Join-Path $publishPath "AppxManifest.xml") `
    -ManifestVersion $Version `
    -PackagePlatform $Platform

$makeAppxArguments = @("pack", "/d", $publishPath, "/p", $packagePath, "/o")
$dryRunCommands.Add([ordered]@{ name = "MakeAppx pack"; executable = "makeappx.exe"; arguments = $makeAppxArguments }) | Out-Null

if (-not $DryRun) {
    $makeAppx = Find-WindowsSdkTool "makeappx.exe"
    if ([string]::IsNullOrWhiteSpace($makeAppx)) {
        throw "MSIX packaging requires Windows SDK MakeAppx.exe."
    }

    New-PackageAsset -Path (Join-Path $publishPath "Assets/StoreLogo.png") -Width 50 -Height 50
    New-PackageAsset -Path (Join-Path $publishPath "Assets/Square150x150Logo.png") -Width 150 -Height 150
    New-PackageAsset -Path (Join-Path $publishPath "Assets/Square44x44Logo.png") -Width 44 -Height 44

    if (Test-Path $packagePath) {
        Remove-Item -LiteralPath $packagePath -Force
    }

    Invoke-NativeCommand -FilePath $makeAppx -Arguments $makeAppxArguments
}

if ($Mode -eq "SignedRelease") {
    $signArguments = @(
        "sign",
        "/fd", "SHA256",
        "/td", "SHA256",
        "/tr", $TimestampUrl,
        "/sha1", $SigningThumbprint,
        $packagePath)
    $dryRunCommands.Add([ordered]@{ name = "SignTool sign"; executable = "signtool.exe"; arguments = $signArguments }) | Out-Null

    if (-not $DryRun) {
        $signTool = Find-WindowsSdkTool "signtool.exe"
        if ([string]::IsNullOrWhiteSpace($signTool)) {
            throw "SignedRelease MSIX packaging requires Windows SDK SignTool.exe."
        }

        Invoke-NativeCommand -FilePath $signTool -Arguments $signArguments
    }
}

$metadata = [ordered]@{
    documentType = "velofileMsixPackageMetadata"
    schemaVersion = 1
    mode = $Mode
    unsignedLocalPackaging = ($Mode -eq "UnsignedLocal")
    msix = "MSIX"
    dryRun = [bool]$DryRun
    packagePath = $packagePath
    appProject = "src/VeloFile.App/VeloFile.App.csproj"
    packageManifest = "src/VeloFile.App/Package.appxmanifest"
    publishPath = $publishPath
    version = $Version
    platform = $Platform
    runtimeIdentifier = $runtimeIdentifier
    manifestArchitecture = $manifestArchitecture
    signingThumbprint = if ($Mode -eq "SignedRelease") { $SigningThumbprint } else { "" }
    signingRequiredForRelease = $true
    dryRunCommands = $dryRunCommands.ToArray()
    note = "Unsigned local packaging creates an unsigned MSIX. Signed release packaging creates the same MSIX and signs it with SignTool using the configured certificate thumbprint."
}

$metadata | ConvertTo-Json -Depth 5 | Set-Content -Path $metadataPath -Encoding UTF8

Write-Host "VeloFile $Mode MSIX package written to $packagePath"
Write-Host "VeloFile $Mode MSIX packaging metadata written to $metadataPath"

if ($Mode -eq "SignedRelease") {
    Write-Host "Signed release packaging requested with SigningThumbprint $SigningThumbprint"
}
