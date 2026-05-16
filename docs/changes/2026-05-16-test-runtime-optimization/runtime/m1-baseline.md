# M1 Baseline Runtime Evidence

## Evidence type

accepted pre-optimization measurement

## Measurement

- Date recorded: 2026-05-16
- Command: `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug`
- Configuration: Debug
- Filter: none
- Result: passed
- Test count: 37
- Duration: about `5 m 49 s`

## Source

This baseline is the accepted measurement from the 2026-05-16 learn/proposal session that motivated the test runtime optimization initiative.

## Interpretation

The measurement is local review evidence, not a universal runtime guarantee. The slow path was attributed to PowerShell wrapper execution, scratch source copying, repeated `dotnet publish`, and assembly-wide serialization.

## M1 use

M1 records this accepted baseline before category migration and local command documentation. Optimized runtime measurements are deferred to later milestones after the contract/script split and prepared-tool harness exist.
