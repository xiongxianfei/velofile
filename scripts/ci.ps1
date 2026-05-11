$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

dotnet --info
dotnet restore VeloFile.sln
dotnet build VeloFile.sln -c Debug --no-restore
dotnet run --project tools/VeloFile.UiContracts -- validate-tokens --contract docs/ui/tokens.v1.json --xaml-root tests/fixtures/ui-contracts/valid
dotnet test VeloFile.sln -c Debug --no-build
