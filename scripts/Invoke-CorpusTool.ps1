[CmdletBinding()]
param()

$script:VeloFileCorpusScriptRoot = $PSScriptRoot
$script:VeloFileCorpusRepoRoot = Split-Path -Parent $script:VeloFileCorpusScriptRoot

function Normalize-VeloFilePath {
    param([Parameter(Mandatory = $true)][string] $Path)

    return [System.IO.Path]::GetFullPath($Path).TrimEnd([char[]] @('\', '/'))
}

function Test-VeloFileCorpusAbsolutePath {
    param([Parameter(Mandatory = $true)][string] $Path)

    return $Path -match '^(?:[A-Za-z]:[\\/]|\\\\)'
}

function Resolve-VeloFileCorpusScratchRoot {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string] $ScratchRoot
    )

    if ([string]::IsNullOrWhiteSpace($ScratchRoot)) {
        throw "A scratch root is required."
    }

    if (!(Test-VeloFileCorpusAbsolutePath -Path $ScratchRoot)) {
        throw "The scratch root must be an absolute path."
    }

    $fullPath = [System.IO.Path]::GetFullPath($ScratchRoot)
    $leaf = (Split-Path -Leaf $fullPath).ToLowerInvariant()

    if (!$leaf.Contains("velofile") -or !$leaf.Contains("corpus")) {
        throw "Refusing unsafe scratch root: path leaf must contain 'velofile' and 'corpus'."
    }

    $normalized = Normalize-VeloFilePath -Path $fullPath
    $rootPath = [System.IO.Path]::GetPathRoot($fullPath)

    if (![string]::IsNullOrWhiteSpace($rootPath) -and $normalized -ieq (Normalize-VeloFilePath -Path $rootPath)) {
        throw "Refusing unsafe scratch root: choose a dedicated VeloFile corpus workspace."
    }

    if ($normalized -ieq (Normalize-VeloFilePath -Path $script:VeloFileCorpusRepoRoot)) {
        throw "Refusing unsafe scratch root: choose a dedicated VeloFile corpus workspace."
    }

    if ($env:USERPROFILE -and $normalized -ieq (Normalize-VeloFilePath -Path $env:USERPROFILE)) {
        throw "Refusing unsafe scratch root: choose a dedicated VeloFile corpus workspace."
    }

    $markerPath = Join-Path $fullPath ".velofile-corpus-root"
    if (Test-Path -LiteralPath $fullPath) {
        $firstEntry = Get-ChildItem -LiteralPath $fullPath -Force | Select-Object -First 1
        if ($null -ne $firstEntry -and !(Test-Path -LiteralPath $markerPath)) {
            throw "Refusing unsafe scratch root: existing non-empty directory is not marked as a VeloFile corpus workspace."
        }
    }

    New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
    Set-Content -LiteralPath $markerPath -Value "VeloFile generated corpus scratch root." -Encoding UTF8

    return $fullPath
}

function Set-VeloFileEnvironmentValue {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Name,

        [AllowNull()]
        [string] $Value
    )

    if ($null -eq $Value) {
        Remove-Item -Path "Env:$Name" -ErrorAction SilentlyContinue
        return
    }

    Set-Item -Path "Env:$Name" -Value $Value
}

function Invoke-VeloFileCorpusTool {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string] $ScratchRoot,

        [Parameter(Mandatory = $true)]
        [string[]] $CommandArguments
    )

    $safeRoot = Resolve-VeloFileCorpusScratchRoot -ScratchRoot $ScratchRoot
    $toolsRoot = Join-Path $safeRoot ".velofile-tools"
    $repoToolSource = Join-Path $script:VeloFileCorpusRepoRoot "tools\VeloFile.Corpus"
    $scratchToolSource = Join-Path $toolsRoot "source\VeloFile.Corpus"
    $repoCoreSource = Join-Path $script:VeloFileCorpusRepoRoot "src\VeloFile.Core"
    $scratchCoreSource = Join-Path $toolsRoot "src\VeloFile.Core"
    $project = Join-Path $scratchToolSource "VeloFile.Corpus.csproj"
    $publishDir = Join-Path $toolsRoot "publish\VeloFile.Corpus"
    $binBase = (Join-Path $toolsRoot "bin") + [System.IO.Path]::DirectorySeparatorChar
    $objBase = (Join-Path $toolsRoot "obj\VeloFile.Corpus\base") + [System.IO.Path]::DirectorySeparatorChar
    $objIntermediate = (Join-Path $toolsRoot "obj\VeloFile.Corpus\intermediate") + [System.IO.Path]::DirectorySeparatorChar
    $nugetPackages = Join-Path $toolsRoot "nuget\packages"
    $nugetHttpCache = Join-Path $toolsRoot "nuget\http-cache"
    $nugetPluginsCache = Join-Path $toolsRoot "nuget\plugins-cache"
    $dotnetCliHome = Join-Path $toolsRoot "dotnet-cli-home"
    $tempRoot = Join-Path $toolsRoot "temp"

    @(
        $toolsRoot,
        $publishDir,
        $binBase,
        $objBase,
        $objIntermediate,
        $nugetPackages,
        $nugetHttpCache,
        $nugetPluginsCache,
        $dotnetCliHome,
        $tempRoot,
        (Split-Path -Parent $scratchToolSource),
        (Split-Path -Parent $scratchCoreSource)
    ) | ForEach-Object {
        New-Item -ItemType Directory -Path $_ -Force | Out-Null
    }

    if (Test-Path -LiteralPath $scratchToolSource) {
        Remove-Item -LiteralPath $scratchToolSource -Recurse -Force
    }

    if (Test-Path -LiteralPath $scratchCoreSource) {
        Remove-Item -LiteralPath $scratchCoreSource -Recurse -Force
    }

    New-Item -ItemType Directory -Path $scratchToolSource -Force | Out-Null
    Get-ChildItem -LiteralPath $repoToolSource -Force |
        Where-Object { $_.Name -notin @("bin", "obj") } |
        Copy-Item -Destination $scratchToolSource -Recurse -Force

    New-Item -ItemType Directory -Path $scratchCoreSource -Force | Out-Null
    Get-ChildItem -LiteralPath $repoCoreSource -Force |
        Where-Object { $_.Name -notin @("bin", "obj") } |
        Copy-Item -Destination $scratchCoreSource -Recurse -Force

    $environmentNames = @(
        "NUGET_PACKAGES",
        "NUGET_HTTP_CACHE_PATH",
        "NUGET_PLUGINS_CACHE_PATH",
        "DOTNET_CLI_HOME",
        "TEMP",
        "TMP",
        "DOTNET_CLI_TELEMETRY_OPTOUT",
        "DOTNET_ADD_GLOBAL_TOOLS_TO_PATH",
        "DOTNET_NOLOGO",
        "DOTNET_SKIP_FIRST_TIME_EXPERIENCE"
    )

    $previousEnvironment = @{}
    foreach ($name in $environmentNames) {
        $previousEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, "Process")
    }

    try {
        Set-VeloFileEnvironmentValue -Name "NUGET_PACKAGES" -Value $nugetPackages
        Set-VeloFileEnvironmentValue -Name "NUGET_HTTP_CACHE_PATH" -Value $nugetHttpCache
        Set-VeloFileEnvironmentValue -Name "NUGET_PLUGINS_CACHE_PATH" -Value $nugetPluginsCache
        Set-VeloFileEnvironmentValue -Name "DOTNET_CLI_HOME" -Value $dotnetCliHome
        Set-VeloFileEnvironmentValue -Name "TEMP" -Value $tempRoot
        Set-VeloFileEnvironmentValue -Name "TMP" -Value $tempRoot
        Set-VeloFileEnvironmentValue -Name "DOTNET_CLI_TELEMETRY_OPTOUT" -Value "1"
        Set-VeloFileEnvironmentValue -Name "DOTNET_ADD_GLOBAL_TOOLS_TO_PATH" -Value "0"
        Set-VeloFileEnvironmentValue -Name "DOTNET_NOLOGO" -Value "1"
        Set-VeloFileEnvironmentValue -Name "DOTNET_SKIP_FIRST_TIME_EXPERIENCE" -Value "1"

        $publishArguments = @(
            "publish",
            $project,
            "-c",
            "Debug",
            "-o",
            $publishDir,
            "-p:BaseOutputPath=$binBase",
            "-p:BaseIntermediateOutputPath=$objBase",
            "-p:IntermediateOutputPath=$objIntermediate",
            "-p:RestorePackagesPath=$nugetPackages",
            "-p:UseSharedCompilation=false",
            "--nologo"
        )

        & dotnet @publishArguments 2>&1 | Out-Host
        $publishExitCode = $LASTEXITCODE
        if ($publishExitCode -ne 0) {
            return $publishExitCode
        }

        $toolDll = Join-Path $publishDir "VeloFile.Corpus.dll"
        $toolArguments = @($CommandArguments) + @("--root", $safeRoot)

        Push-Location $safeRoot
        try {
            & dotnet $toolDll @toolArguments 2>&1 | Out-Host
            return $LASTEXITCODE
        }
        finally {
            Pop-Location
        }
    }
    finally {
        foreach ($name in $environmentNames) {
            Set-VeloFileEnvironmentValue -Name $name -Value $previousEnvironment[$name]
        }
    }
}
