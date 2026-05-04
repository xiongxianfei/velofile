# M2 Validation Tooling Change Explanation

## Scope

M2 creates the validation surfaces that later milestones depend on before they use generated corpora:

- `tools/VeloFile.Corpus`, a small console tool that owns corpus generation, deterministic placeholder profiles, runner dispatch, scratch-root validation, and benchmark report stub output.
- `scripts/generate-corpus.ps1`.
- `scripts/run-compat-corpus.ps1`.
- `scripts/run-preview-corpus.ps1`.
- `scripts/run-benchmarks.ps1`.
- `tests/VeloFile.Corpus.Tests`.
- `docs/release/benchmark-baseline.md`.

## Safety Boundary

Every corpus command requires an explicit absolute scratch root. The wrappers publish and execute the corpus tool from `<scratch-root>/.velofile-tools`, including MSBuild output, NuGet caches, `DOTNET_CLI_HOME`, and temp directories. The tool refuses drive roots, the repository root, the user profile, path leaves that do not identify a VeloFile corpus workspace, and existing non-empty directories that do not contain the `.velofile-corpus-root` marker.

All generated files are created under the scratch root. The marker allows repeat runs in the same generated workspace while preventing accidental writes into arbitrary user folders.

## Runner Behavior

The `smoke`, `operations`, `preview`, `search`, and `large-folder` corpus profiles are deterministic placeholders and safe for repeated runs. Each profile writes under `<scratch-root>/corpora/<profile>`. The compatibility and preview runners support only smoke scope in M2. Other scopes fail fast with an explicit "not implemented" state so later milestones cannot accidentally treat missing validation as passing.

The benchmark runner is non-gating in M2. It emits the required report shape and environment fields, but timing values remain null until M15 implements real benchmark measurements and release gating.

## Tests

The M2 tests were added before the scripts and tool existed. The initial corpus test run failed because the scripts were missing, then passed after the tool and scripts were implemented.

## Validation

M2 validation passed with:

- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug`
- `dotnet test VeloFile.sln -c Debug --filter Corpus`
- `powershell -NoProfile -ExecutionPolicy Bypass -File tests/validation/CorpusScriptsIsolation.Tests.ps1 -ScratchRoot <scratch-root>`
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile smoke -ScratchRoot <scratch-root>`
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile operations -ScratchRoot <scratch-root>`
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile preview -ScratchRoot <scratch-root>`
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile search -ScratchRoot <scratch-root>`
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/generate-corpus.ps1 -Profile large-folder -ScratchRoot <scratch-root>`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope smoke -ScratchRoot <scratch-root>`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-preview-corpus.ps1 -ScratchRoot <scratch-root>`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-benchmarks.ps1 -NonGating -ScratchRoot <scratch-root>`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`

Full CI passed 9 tests across 4 test assemblies.
