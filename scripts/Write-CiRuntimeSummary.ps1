[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$LaneName,

    [string]$Trigger = $env:GITHUB_EVENT_NAME,

    [string[]]$SelectedCategory = @(),

    [string]$ReleaseEvidenceStatus = "unknown",

    [string]$CorpusScriptSmokeStatus = "unknown",

    [string]$FullCloseoutStatus = "unknown",

    [double]$TotalDurationSeconds = -1,

    [double]$BuildDurationSeconds = -1,

    [string[]]$TestProjectDuration = @(),

    [string[]]$TrxPath = @(),

    [string]$FailedCommand,

    [string]$FailedOutcome,

    [string]$SummaryPath = $env:GITHUB_STEP_SUMMARY
)

$ErrorActionPreference = "Stop"

function Protect-SummaryValue {
    param(
        [AllowNull()]
        [string]$Value
    )

    if ([string]::IsNullOrEmpty($Value)) {
        return ""
    }

    $protected = $Value
    $protected = [regex]::Replace($protected, '(?i)C:\\Users\\[^\s\|`"''<>]+', '[redacted-user-profile]')
    $protected = [regex]::Replace($protected, '(?i)--(?:secret|token|credential|password|certificate)(?:=|\s+)[^\s\|]+', '--[redacted-sensitive]')
    $protected = [regex]::Replace($protected, '(?i)\b(?:secret|token|credential|password|signing\s*material|signing-material|certificate)\b\s*[:=]\s*[^,\s\|;]+', '[redacted-sensitive]')

    return $protected
}

function Format-OptionalDuration {
    param([double]$Seconds)

    if ($Seconds -lt 0) {
        return "unavailable"
    }

    return [TimeSpan]::FromSeconds($Seconds).ToString("hh\:mm\:ss\.fff")
}

function Format-TimeSpanValue {
    param([TimeSpan]$Duration)

    return $Duration.ToString("hh\:mm\:ss\.fff")
}

function Read-SlowestTests {
    param([string[]]$Paths)

    $rows = New-Object System.Collections.Generic.List[object]
    $limitations = New-Object System.Collections.Generic.List[string]

    if ($Paths.Count -eq 0) {
        $limitations.Add("No TRX or equivalent structured output paths were provided.")
        return [pscustomobject]@{
            Rows = @()
            Limitations = @($limitations)
        }
    }

    foreach ($path in $Paths) {
        if (-not (Test-Path -LiteralPath $path)) {
            $limitations.Add("$(Split-Path -Leaf $path) (not found)")
            continue
        }

        try {
            [xml]$trx = Get-Content -LiteralPath $path -Raw
            $nodes = $trx.SelectNodes("//*[local-name()='UnitTestResult']")
            foreach ($node in $nodes) {
                $testName = $node.GetAttribute("testName")
                $durationText = $node.GetAttribute("duration")
                $duration = [TimeSpan]::Zero

                if ([string]::IsNullOrWhiteSpace($testName) -or -not [TimeSpan]::TryParse($durationText, [ref]$duration)) {
                    continue
                }

                $rows.Add([pscustomobject]@{
                    TestName = $testName
                    Duration = $duration
                    Source = (Split-Path -Leaf $path)
                })
            }
        }
        catch {
            $limitations.Add("$(Split-Path -Leaf $path) (parse failed)")
        }
    }

    return [pscustomobject]@{
        Rows = @($rows | Sort-Object -Property Duration -Descending | Select-Object -First 10)
        Limitations = @($limitations)
    }
}

function Get-TrxProjectName {
    param(
        [xml]$Trx,
        [string]$Path
    )

    $testMethod = $Trx.SelectSingleNode("//*[local-name()='TestMethod' and @codeBase]")
    if ($null -ne $testMethod) {
        $codeBase = $testMethod.GetAttribute("codeBase")
        if (-not [string]::IsNullOrWhiteSpace($codeBase)) {
            $testAssembly = Split-Path -Leaf $codeBase
            if (-not [string]::IsNullOrWhiteSpace($testAssembly)) {
                return [System.IO.Path]::GetFileNameWithoutExtension($testAssembly)
            }
        }
    }

    return [System.IO.Path]::GetFileNameWithoutExtension((Split-Path -Leaf $Path))
}

function Read-TestProjectDurationsFromTrx {
    param([string[]]$Paths)

    $durations = @{}

    foreach ($path in $Paths) {
        if (-not (Test-Path -LiteralPath $path)) {
            continue
        }

        try {
            [xml]$trx = Get-Content -LiteralPath $path -Raw
            $nodes = $trx.SelectNodes("//*[local-name()='UnitTestResult']")
            $projectName = Get-TrxProjectName -Trx $trx -Path $path
            $projectDuration = [TimeSpan]::Zero
            $hasDuration = $false

            foreach ($node in $nodes) {
                $durationText = $node.GetAttribute("duration")
                $duration = [TimeSpan]::Zero

                if ([TimeSpan]::TryParse($durationText, [ref]$duration)) {
                    $projectDuration = $projectDuration.Add($duration)
                    $hasDuration = $true
                }
            }

            if (-not $hasDuration -or [string]::IsNullOrWhiteSpace($projectName)) {
                continue
            }

            if (-not $durations.ContainsKey($projectName)) {
                $durations[$projectName] = [TimeSpan]::Zero
            }

            $durations[$projectName] = $durations[$projectName].Add($projectDuration)
        }
        catch {
            continue
        }
    }

    return @($durations.Keys | Sort-Object | ForEach-Object {
        [pscustomobject]@{
            Project = Protect-SummaryValue $_
            Duration = Format-TimeSpanValue $durations[$_]
        }
    })
}

function Format-TestProjectDuration {
    param([string]$Value)

    $parts = $Value.Split("=", 2)
    if ($parts.Count -ne 2 -or [string]::IsNullOrWhiteSpace($parts[0]) -or [string]::IsNullOrWhiteSpace($parts[1])) {
        return $null
    }

    return [pscustomobject]@{
        Project = Protect-SummaryValue $parts[0].Trim()
        Duration = Protect-SummaryValue $parts[1].Trim()
    }
}

function Expand-SemicolonList {
    param([string[]]$Values)

    return @($Values | ForEach-Object {
        if (-not [string]::IsNullOrWhiteSpace($_)) {
            $_ -split ';' | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        }
    })
}

if ([string]::IsNullOrWhiteSpace($SummaryPath)) {
    $SummaryPath = Join-Path (Get-Location) "ci-runtime-summary.md"
}

$summaryDirectory = Split-Path -Parent $SummaryPath
if (-not [string]::IsNullOrWhiteSpace($summaryDirectory)) {
    New-Item -ItemType Directory -Path $summaryDirectory -Force | Out-Null
}

$selectedCategoryValues = Expand-SemicolonList $SelectedCategory
$testProjectDurationValues = Expand-SemicolonList $TestProjectDuration
$trxPathValues = Expand-SemicolonList $TrxPath
$slowTests = Read-SlowestTests -Paths $trxPathValues
$lines = New-Object System.Collections.Generic.List[string]

$selectedCategories = if ($selectedCategoryValues.Count -gt 0) {
    ($selectedCategoryValues | ForEach-Object { Protect-SummaryValue $_ }) -join ", "
}
else {
    "none recorded"
}

$lines.Add("# CI Runtime Summary")
$lines.Add("")
$lines.Add("- Lane: $(Protect-SummaryValue $LaneName)")
$lines.Add("- Trigger: $(Protect-SummaryValue $Trigger)")
$lines.Add("- Selected categories: $selectedCategories")
$lines.Add("- ReleaseEvidence: $(Protect-SummaryValue $ReleaseEvidenceStatus)")
$lines.Add("- CorpusScript Smoke: $(Protect-SummaryValue $CorpusScriptSmokeStatus)")
$lines.Add("- Full closeout: $(Protect-SummaryValue $FullCloseoutStatus)")

if (-not [string]::IsNullOrWhiteSpace($FailedCommand)) {
    $lines.Add("- Failed command: $(Protect-SummaryValue $FailedCommand)")
}

if (-not [string]::IsNullOrWhiteSpace($FailedOutcome)) {
    $lines.Add("- Command outcome: $(Protect-SummaryValue $FailedOutcome)")
}

$lines.Add("")
$lines.Add("## Durations")
$lines.Add("")
$lines.Add("- Total job duration: $(Format-OptionalDuration $TotalDurationSeconds)")
$lines.Add("- Build duration: $(Format-OptionalDuration $BuildDurationSeconds)")
$lines.Add("")
$lines.Add("## Test Project Durations")
$lines.Add("")

$projectRows = @($testProjectDurationValues | ForEach-Object { Format-TestProjectDuration $_ } | Where-Object { $_ -ne $null })
if ($projectRows.Count -eq 0) {
    $projectRows = @(Read-TestProjectDurationsFromTrx -Paths $trxPathValues)
}

if ($projectRows.Count -eq 0) {
    $lines.Add("No per test project duration data available.")
}
else {
    $lines.Add("| Project | Duration |")
    $lines.Add("|---|---:|")
    foreach ($row in $projectRows) {
        $lines.Add("| $($row.Project) | $($row.Duration) |")
    }
}

$lines.Add("")
$lines.Add("## Slowest Tests")
$lines.Add("")

if ($slowTests.Rows.Count -eq 0) {
    $lines.Add("Structured test output: unavailable")
    foreach ($limitation in $slowTests.Limitations) {
        $lines.Add("- $(Protect-SummaryValue $limitation)")
    }
    $lines.Add("No slow-test details available.")
}
else {
    $lines.Add("Structured test output: TRX")
    $lines.Add("")
    $lines.Add("| Rank | Test | Duration | Source |")
    $lines.Add("|---:|---|---:|---|")

    $rank = 1
    foreach ($row in $slowTests.Rows) {
        $lines.Add("| $rank | $(Protect-SummaryValue $row.TestName) | $(Format-TimeSpanValue $row.Duration) | $(Protect-SummaryValue $row.Source) |")
        $rank++
    }

    foreach ($limitation in $slowTests.Limitations) {
        $lines.Add("- $(Protect-SummaryValue $limitation)")
    }
}

Set-Content -LiteralPath $SummaryPath -Value $lines -Encoding utf8
Write-Host "Wrote CI runtime summary to $SummaryPath"
