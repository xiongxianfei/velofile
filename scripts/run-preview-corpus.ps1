[CmdletBinding()]
param(
    [string] $Scope = "smoke",

    [Parameter(Mandatory = $true)]
    [Alias("Root")]
    [string] $ScratchRoot
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "Invoke-CorpusTool.ps1")

exit (Invoke-VeloFileCorpusTool -ScratchRoot $ScratchRoot -CommandArguments @("preview", "--scope", $Scope))
