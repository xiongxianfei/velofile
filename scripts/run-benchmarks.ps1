[CmdletBinding()]
param(
    [switch] $NonGating,

    [Parameter(Mandatory = $true)]
    [string] $Root
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "tools/VeloFile.Corpus/VeloFile.Corpus.csproj"
$arguments = @("benchmarks", "--root", $Root)

if ($NonGating) {
    $arguments += "--non-gating"
}

dotnet run --project $project -- @arguments
exit $LASTEXITCODE
