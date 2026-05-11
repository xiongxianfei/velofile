# VeloFile Constitution

This document defines the durable rules for agentic development in VeloFile. It is governance, not a feature plan. Change it only when the project rules themselves need to change.

## Project Purpose

VeloFile is a fast, lightweight, open-source file explorer for Windows 10 and 11. It serves everyday Windows file browsing, developer project-folder workflows, and power users who need tabs, preview, keyboard flow, and clear Windows-native behavior.

VeloFile V1 MUST stay focused on the approved Windows desktop file-manager scope. It MUST NOT become an Explorer replacement, cross-platform file manager, plugin marketplace, global indexer, cloud sync client, or broad power-user suite without an approved change to the vision, spec, architecture, and release plan.

## Source Of Truth Order

When sources conflict, agents MUST follow this order:

1. Direct maintainer or user instruction in the current task.
2. `CONSTITUTION.md`.
3. Approved feature specifications in `specs/`.
4. Matching test specifications in `specs/`.
5. Approved architecture docs and ADRs under `docs/architecture/` and `docs/adr/`.
6. Active execution plans in `docs/plan.md` and `docs/plans/`.
7. Root agent guidance such as `AGENTS.md`.
8. Implementation code, tests, and local scripts.
9. Chat summaries, generated notes, or unapproved draft artifacts.

`VISION.md` defines product direction and SHOULD guide proposals and scope decisions, but it does not override this constitution or approved specs. If any source appears stale, missing, or contradictory, agents MUST call out the conflict instead of silently blending instructions.

Local `.codex/` files are personal agent tooling. They are ignored by Git and MUST NOT be treated as tracked repository governance.

## Spec-Driven Rules

Externally observable behavior MUST be specified before it is implemented. This includes UI behavior, file operations, persistence, diagnostics, release behavior, compatibility, security, privacy, public scripts, and data contracts.

A spec MUST define requirements, examples or edge cases, non-goals, compatibility expectations, failure behavior, and acceptance criteria. Every `MUST` in a spec SHOULD map to at least one concrete test in the matching test spec.

Agents MUST NOT code new product contracts from informal chat alone when the change affects users, release evidence, public tooling, or long-lived architecture. Bootstrap, typo, formatting, and mechanical documentation changes MAY use the fast lane when the contract is already obvious.

## Test-Driven Rules

Behavior changes SHOULD start with the smallest meaningful failing test. Bug fixes MUST add or update a regression test unless the test is impossible or disproportionate; the reason for skipping the test MUST be recorded.

Tests MUST exercise the production boundary that matters for the claim. View-model-only, fixture-only, header-only, placeholder, or synthetic tests MUST NOT be presented as evidence for product behavior through the app boundary.

Release-readiness reports MUST distinguish verified behavior from fixture setup, skipped cases, unavailable capabilities, not-implemented verifiers, and failures. They MUST NOT count skipped, unavailable, fixture-only, placeholder, synthetic, or infrastructure-only results as verified release evidence.

## Architecture Rules

VeloFile uses explicit layers:

- `src/VeloFile.App` owns the WinUI shell, view models, shell command routes, and user-facing state.
- `src/VeloFile.Core` owns product-neutral domain models, app services, orchestration, policies, and deterministic state transitions.
- `src/VeloFile.Windows` owns Windows Shell, Win32, WinRT, COM, OLE, process launch, file association, MSIX, and other OS integration boundaries.
- `tools/` owns corpus, benchmark, and release-support tooling.
- `tests/` owns contract, adapter, app-shell, corpus, and release-verification tests.

Core MUST NOT depend on WinUI, Windows Shell COM, registry access, process launch, or local machine state. App code SHOULD stay thin at Windows event boundaries and route product behavior through Core or explicit app services. Windows adapters MUST convert untrusted platform data into structured app results without leaking raw OS details into Core.

Cross-component changes SHOULD update the architecture document or ADRs when they introduce or change a boundary, data flow, persistence model, release mechanism, security posture, or compatibility policy.

## Security And Privacy Rules

Paths, filenames, preview text, terminal commands, persisted state, drag/drop payloads, shell data, diagnostics inputs, and external process results are untrusted.

Diagnostics MUST be local-only by default. Raw paths, filenames, usernames, terminal commands, preview content, credentials, secrets, and local profile details MUST NOT appear in serialized diagnostics, release reports, or public artifacts unless an approved spec explicitly permits it.

Secrets MUST NOT be committed. Release signing keys, GPG private material, certificate passwords, tokens, and similar credentials MUST stay in trusted CI secrets or local secure stores. Release integrity checks MUST verify approved identities, not only the presence of a tag, certificate, or signature.

## Compatibility Rules

VeloFile V1 targets Windows 10 and Windows 11. Windows-native behavior SHOULD be delegated to Windows Shell or platform APIs where correctness depends on OS semantics, including Recycle Bin delete, file associations, drag/drop, thumbnails/icons, long paths, shortcuts, DPI, and MSIX packaging.

Compatibility evidence MUST use real fixtures or explicit manual evidence where automation is not practical. Unsupported environment cases MUST be reported as skipped or unavailable with controlled reasons, not as passed.

Breaking changes to schemas, settings, session state, release metadata, corpus output, diagnostics, or public scripts MUST include migration, compatibility handling, or an approved deprecation decision.

## Verification Rules

Agents MUST run the smallest relevant verification first, then expand as risk requires. Before claiming branch or release readiness, agents SHOULD run:

```powershell
dotnet --info
dotnet restore VeloFile.sln
dotnet build VeloFile.sln -c Debug
dotnet test VeloFile.sln -c Debug
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1
```

Docs-only changes MAY use focused validation such as link checks, `rg` drift checks, `git diff --check`, or release-verification scripts when full CI would not add evidence. The final response MUST name the commands actually run and MUST NOT imply CI or hosted checks passed unless they did.

If verification fails, the failure MUST be fixed, explicitly deferred with rationale, or reported as a blocker. Agents MUST NOT hide analyzer warnings, skipped tests, partial runs, or environment limitations.

## Review Rules

Use proposal and proposal-review for unclear product direction, scope tradeoffs, or major options. Use spec and spec-review before behavior-changing implementation. Use architecture and architecture-review for cross-component, security, performance, persistence, release, or long-lived design changes. Use plan and plan-review for risky or multi-step execution. Use code-review after implementation and verify before PR handoff.

Review findings marked blocker or major MUST be resolved, downgraded with an approved rationale, or carried as explicit release-blocking risk. Agents MUST NOT close a review by proving only that scaffolding exists when the finding requires product behavior through a real boundary.

## Documentation Rules

`AGENTS.md` is concise operating guidance. `CONSTITUTION.md` owns durable governance. `VISION.md` owns product direction. `docs/plan.md` indexes execution plans; plan bodies live under `docs/plans/`. Specs live under `specs/`. Architecture and ADRs live under `docs/architecture/` and `docs/adr/`. Change records live under `docs/changes/`.

Behavior changes MUST update relevant specs, test specs, architecture docs, release docs, user docs, examples, or change records in the same change when those artifacts are affected.

Unapproved ideas and future work SHOULD be recorded in proposals, issues, the draft table in `docs/plan.md`, or explicit follow-up sections in change records. Agents MUST NOT rely on deleted, renamed, or untracked documents as source-of-truth artifacts.

## Agent Behavior Rules

Agents MUST keep diffs scoped, preserve user changes, prefer existing scripts and patterns, and avoid unrelated refactors. They MUST state assumptions and conflicts, especially when source-of-truth artifacts disagree.

Agents MUST NOT claim success without evidence, fabricate test results, silently skip required lifecycle stages, or present synthetic measurements as release evidence. They MUST NOT revert user changes unless explicitly asked.

When a task is interrupted or resumed, agents MUST re-check the latest user request and current working tree before continuing.

## Fast-Lane Exceptions

Small, low-risk changes MAY skip the full lifecycle when they do not alter product behavior, public contracts, architecture, release evidence, security, or compatibility. Examples include typo fixes, comment cleanup, formatting-only edits, narrow documentation corrections, and mechanical link updates.

Fast-lane work still MUST preserve source-of-truth order, keep changes scoped, run an appropriate focused validation, and report what was and was not verified.
