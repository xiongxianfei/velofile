[CmdletBinding()]
param(
    [switch] $NonGating,

    [Parameter(Mandatory = $true)]
    [Alias("Root")]
    [string] $ScratchRoot,

    [int] $RunCount = 5,

    [string] $AppExecutablePath = "",

    [string] $AppArguments = "",

    [int] $AppTimeoutMs = 5000
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "Invoke-CorpusTool.ps1")

$arguments = @("benchmarks")

if ($NonGating) {
    $arguments += "--non-gating"
}

$arguments += "--run-count"
$arguments += $RunCount.ToString()

if (-not [string]::IsNullOrWhiteSpace($AppExecutablePath)) {
    $arguments += "--app-executable"
    $arguments += $AppExecutablePath
}

if (-not [string]::IsNullOrWhiteSpace($AppArguments)) {
    $arguments += "--app-arguments"
    $arguments += $AppArguments
}

$arguments += "--app-timeout-ms"
$arguments += $AppTimeoutMs.ToString()

exit (Invoke-VeloFileCorpusTool -ScratchRoot $ScratchRoot -CommandArguments $arguments)
