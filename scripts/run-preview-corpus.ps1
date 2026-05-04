[CmdletBinding()]
param(
    [string] $Scope = "smoke",

    [Parameter(Mandatory = $true)]
    [string] $Root
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "tools/VeloFile.Corpus/VeloFile.Corpus.csproj"

dotnet run --project $project -- preview --scope $Scope --root $Root
exit $LASTEXITCODE
