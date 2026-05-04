[CmdletBinding()]
param(
    [string] $Profile = "smoke",

    [Parameter(Mandatory = $true)]
    [string] $Root
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "tools/VeloFile.Corpus/VeloFile.Corpus.csproj"

dotnet run --project $project -- generate --profile $Profile --root $Root
exit $LASTEXITCODE
