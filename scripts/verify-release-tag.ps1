[CmdletBinding()]
param(
    [string] $Tag = $env:GITHUB_REF_NAME,

    [string] $VerifyStatusFile = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Normalize-ReleaseFingerprints {
    param([string] $Fingerprints)

    $normalized = New-Object 'System.Collections.Generic.List[string]'
    foreach ($entry in ($Fingerprints -split "[,`n`r]+")) {
        $compact = ($entry -replace '\s+', '').Trim().ToUpperInvariant()
        if ([string]::IsNullOrWhiteSpace($compact)) {
            continue
        }

        if ($compact -notmatch '^[0-9A-F]+$' -or ($compact.Length % 40) -ne 0) {
            throw "VELOFILE_RELEASE_GPG_FINGERPRINTS must contain full 40-character hexadecimal fingerprints."
        }

        for ($index = 0; $index -lt $compact.Length; $index += 40) {
            $fingerprint = $compact.Substring($index, 40)
            if ($fingerprint -notmatch '^[0-9A-F]{40}$') {
                throw "VELOFILE_RELEASE_GPG_FINGERPRINTS must contain full 40-character hexadecimal fingerprints."
            }

            $normalized.Add($fingerprint) | Out-Null
        }
    }

    if ($normalized.Count -eq 0) {
        throw "VELOFILE_RELEASE_GPG_FINGERPRINTS must include at least one approved release signing fingerprint."
    }

    return $normalized.ToArray()
}

if ([string]::IsNullOrWhiteSpace($env:VELOFILE_RELEASE_GPG_FINGERPRINTS)) {
    throw "VELOFILE_RELEASE_GPG_FINGERPRINTS is required to verify signed release tags."
}

$runnerTemp = if ([string]::IsNullOrWhiteSpace($env:RUNNER_TEMP)) { [IO.Path]::GetTempPath() } else { $env:RUNNER_TEMP }
$allowedFingerprints = Normalize-ReleaseFingerprints $env:VELOFILE_RELEASE_GPG_FINGERPRINTS

if ([string]::IsNullOrWhiteSpace($VerifyStatusFile)) {
    if ([string]::IsNullOrWhiteSpace($Tag)) {
        throw "GITHUB_REF_NAME is required for release tag verification."
    }

    if ([string]::IsNullOrWhiteSpace($env:VELOFILE_RELEASE_GPG_PUBLIC_KEYS)) {
        throw "VELOFILE_RELEASE_GPG_PUBLIC_KEYS is required to verify signed release tags."
    }

    $gpgHome = Join-Path $runnerTemp ("velofile-release-gnupg-" + [Guid]::NewGuid().ToString("N"))
    New-Item -ItemType Directory -Force -Path $gpgHome | Out-Null

    $isLinuxVariable = Get-Variable -Name IsLinux -ErrorAction SilentlyContinue
    $isMacOsVariable = Get-Variable -Name IsMacOS -ErrorAction SilentlyContinue
    $isLinux = $isLinuxVariable -and [bool]$isLinuxVariable.Value
    $isMacOs = $isMacOsVariable -and [bool]$isMacOsVariable.Value
    if ($isLinux -or $isMacOs) {
        chmod 700 $gpgHome
    }

    $env:GNUPGHOME = $gpgHome

    $publicKeyPath = Join-Path $runnerTemp ("velofile-release-public-keys-" + [Guid]::NewGuid().ToString("N") + ".asc")
    $env:VELOFILE_RELEASE_GPG_PUBLIC_KEYS | Out-File -FilePath $publicKeyPath -Encoding ascii -NoNewline

    gpg --batch --import $publicKeyPath
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to import configured VeloFile release public keys."
    }

    $verifyOutputPath = Join-Path $runnerTemp ("velofile-release-tag-verify-" + [Guid]::NewGuid().ToString("N") + ".txt")
    git verify-tag --raw $Tag 2> $verifyOutputPath
    if ($LASTEXITCODE -ne 0) {
        throw "Release tag was not verified with a valid signature."
    }

    $status = Get-Content $verifyOutputPath -Raw
}
else {
    if (-not (Test-Path $VerifyStatusFile -PathType Leaf)) {
        throw "VerifyStatusFile must point to an existing git verify-tag --raw status file."
    }

    $status = Get-Content $VerifyStatusFile -Raw
}

$validSigLine = ($status -split "`n") | Where-Object { $_ -match '^\[GNUPG:\]\s+VALIDSIG\s+' } | Select-Object -First 1
if (-not $validSigLine) {
    throw "Release tag was not verified with a valid signature."
}

$verifiedFingerprint = (($validSigLine -split '\s+')[2]).Trim().ToUpperInvariant()
if ($verifiedFingerprint -notmatch '^[0-9A-F]{40}$') {
    throw "Release tag verification did not return a full signing fingerprint."
}

if ($allowedFingerprints -notcontains $verifiedFingerprint) {
    throw "Release tag signer fingerprint is not in the allowed release-key set."
}

Write-Host "Release tag signature verified with trusted fingerprint $verifiedFingerprint"
