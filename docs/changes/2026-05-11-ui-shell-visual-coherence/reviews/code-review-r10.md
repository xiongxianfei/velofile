# Code Review R10: M3 File-List Polish and Deterministic Fixture Icons

## Result

clean-with-notes

## Review Inputs

- Reviewed milestone: M3 File-List Polish and Deterministic Fixture Icons.
- Review surface: committed implementation in `343302f` plus the M3 visual-evidence deferral amendment in `61884a3`.
- Governing spec: `specs/ui-shell-visual-coherence.md`, including R22A, R28-R43, R66, AC16, and AC21.
- Test spec: `specs/ui-shell-visual-coherence.test.md`, including TSC013, TSC014, and TSC020 deferral handling.
- Plan: `docs/plans/2026-05-11-ui-shell-visual-coherence.md`, M3 milestone and 2026-05-17 M3 evidence-deferral amendment.
- Architecture: `docs/architecture/system/architecture.md` UI Design Contracts and `docs/adr/0010-shell-visual-coherence-contracts.md`.
- Evidence note: `docs/changes/2026-05-11-ui-shell-visual-coherence/visual-evidence/m3-shell-file-list-selected-focused.md`.

## Diff Summary

- Added deterministic fixture icon geometry resources in `src/VeloFile.App/Resources/Icons/VeloFile.FixtureIcons.xaml`.
- Added allowlisted file-list icon kinds and resource-backed geometry mapping in `src/VeloFile.App/Ui/FileListIconKind.cs` and `src/VeloFile.App/Ui/FileListIconGeometryConverter.cs`.
- Updated the file-list row template to render `IconKind` through `VfFileListFixtureIconTemplate` instead of placeholder-looking thumbnail text chips.
- Added a tokenized details-view header and maintained file-list selection, context menu, drag/drop, thumbnail, and preview routes.
- Activated the governed `fixture-icons` UI contract scope and added app/corpus tests for icon resources, fixture rows, and forbidden icon shapes/colors.
- Recorded M3 `shell-file-list-selected-focused` visual evidence as deferred to M8, with no M3 whole-shell visual acceptance claim.

## Findings

No blocking or required-change findings.

## Checklist Coverage

| Check | Result | Notes |
|---|---|---|
| Spec alignment | pass | R22A/AC21 are satisfied by the explicit M3 deferral note; R28-R43 are addressed by allowlisted icon kinds, vector resources, static fixture icon validation, and file-list row resource updates. |
| Test coverage | pass | `FileListResourceContractTests`, `UiFixtureRegistryTests`, `AppShellContractTests`, and `ShellVisualCoherenceContractTests` cover icon resources, fixture rows, forbidden icon chips/control paths, local icon colors, and behavior route preservation. |
| Edge cases | pass | Required icon kinds include folder, generic, PDF, image, text, markdown, spreadsheet, executable, and thumbnail fallback; hidden/protected, selected/focused, long-name, metadata-heavy, and empty-folder fixture states are represented. |
| Error handling | pass | Icon conversion falls back to `FileGeneric` when input or resource lookup is invalid; arbitrary fixture input cannot choose resource keys. |
| Architecture boundaries | pass | Changes stay in App resources/view models/test fixture support and UI contract tooling; Core/Windows behavior is not changed for M3. |
| Compatibility | pass | Existing file-list item source, selection mode, selection changed route, double-tap route, context flyout, drag/drop handlers, thumbnail state, and preview route remain wired. |
| Security/privacy | pass | Fixture paths use `C:\VeloFileFixture`; the M3 evidence note does not include real user paths or private screenshots. |
| Derived artifact currency | pass | `docs/ui/ui-contract-scopes.v1.json` and valid UI-contract fixtures include the active M3 icon/header resources. |
| Unrelated changes | pass | The reviewed M3 surface is limited to file-list/icon resources, tests, scope metadata, and required review/evidence records. |
| Validation evidence | pass | Implementation notes record full M3 validation and CI passing. Reviewer reran focused app tests, UI contract validation, valid fixture validation, app build, and project-level app/corpus filtered checks. A solution-level broad filter timed out after 5 minutes during review, then its relevant app/corpus coverage was rerun successfully by project. |

## Reviewer Validation

- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "FileListResourceContractTests|UiFixtureRegistryTests"`: passed, 21 tests.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root src\VeloFile.App\Resources --scopes docs\ui\ui-contract-scopes.v1.json --scope-root .`: passed.
- `dotnet test tests\VeloFile.App.Tests\VeloFile.App.Tests.csproj -c Debug --filter "UiContracts|FileList|AppShellContract|DragDrop|Preview"`: passed, 54 tests.
- `dotnet test tests\VeloFile.Corpus.Tests\VeloFile.Corpus.Tests.csproj -c Debug --filter "UiContracts"`: passed, 20 tests.
- `dotnet run --project tools\VeloFile.UiContracts -- validate-tokens --contract docs\ui\tokens.v1.json --xaml-root tests\fixtures\ui-contracts\valid --scopes tests\fixtures\ui-contracts\scopes.valid.json --scope-root tests\fixtures\ui-contracts\valid`: passed.
- `dotnet build src\VeloFile.App\VeloFile.App.csproj -c Debug`: passed with 0 warnings and 0 errors.
- `git diff --check`: passed.

The reviewer also attempted `dotnet test VeloFile.sln -c Debug --filter "UiContracts|FileList|AppShellContract|DragDrop|Preview"`; it timed out after 5 minutes. That timeout is not treated as passing evidence.

## No-Finding Rationale

No required-change finding is recorded because the M3 implementation satisfies the approved deferral path and the file-list/icon contract without expanding into M4-M8 behavior. The implementation removes governed placeholder chip rendering, uses deterministic vector resources through an allowlisted enum mapping, keeps public file-list behavior routes wired, and records that full-shell visual acceptance for `shell-file-list-selected-focused` remains deferred to M8.

## Residual Risk

- `shell-file-list-selected-focused` has not been accepted as full-shell visual evidence. This is permitted for M3 only by R22A/AC21 and remains a blocking M8/final-closeout requirement.

## Next Stage

Close M3 and proceed to `implement` M4 Command Band Visual Coherence.
