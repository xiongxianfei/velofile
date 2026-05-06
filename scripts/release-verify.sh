#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
script_file="$repo_root/scripts/release-verify.ps1"

if command -v pwsh >/dev/null 2>&1; then
  powershell_bin="pwsh"
elif command -v powershell.exe >/dev/null 2>&1; then
  powershell_bin="powershell.exe"
elif command -v powershell >/dev/null 2>&1; then
  powershell_bin="powershell"
elif [[ -x "/c/Program Files/PowerShell/7/pwsh.exe" ]]; then
  powershell_bin="/c/Program Files/PowerShell/7/pwsh.exe"
elif [[ -x "/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe" ]]; then
  powershell_bin="/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe"
elif [[ -x "/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe" ]]; then
  powershell_bin="/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe"
else
  echo "PowerShell is required to run VeloFile release verification."
  exit 1
fi

if command -v wslpath >/dev/null 2>&1 && [[ "$powershell_bin" == *powershell.exe ]]; then
  script_file="$(wslpath -w "$script_file")"
elif command -v cygpath >/dev/null 2>&1 && [[ "$powershell_bin" == *powershell.exe ]]; then
  script_file="$(cygpath -w "$script_file")"
fi

"$powershell_bin" -NoProfile -ExecutionPolicy Bypass -File "$script_file" "$@"
