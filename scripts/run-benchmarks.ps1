[CmdletBinding()]
param(
    [switch] $NonGating,

    [Parameter(Mandatory = $true)]
    [Alias("Root")]
    [string] $ScratchRoot
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "Invoke-CorpusTool.ps1")

$arguments = @("benchmarks")

if ($NonGating) {
    $arguments += "--non-gating"
}

exit (Invoke-VeloFileCorpusTool -ScratchRoot $ScratchRoot -CommandArguments $arguments)
