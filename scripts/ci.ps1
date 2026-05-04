$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

dotnet --info
dotnet restore VeloFile.sln
dotnet build VeloFile.sln -c Debug --no-restore
dotnet test VeloFile.sln -c Debug --no-build
