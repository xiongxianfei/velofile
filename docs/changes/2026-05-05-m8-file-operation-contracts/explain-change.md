# M8 File Operation Contracts, Safe Delete, and Rename

M8 adds the first file-operation safety boundary. The goal is not to implement every file operation yet; it is to ensure rename, normal delete, and permanent delete can only flow through an auditable operation service and Windows shell adapter boundary.

## What Changed

`src/VeloFile.Core/Operations/` now defines the operation contracts: operation kind, request targets, progress, terminal state, permanent-delete confirmation, adapter result, and undo eligibility. `FileOperationService` owns the policy:

- rename calls the adapter with a single target and target name;
- normal delete calls the adapter as `RecycleBinDelete`;
- unsupported Recycle Bin delete transitions to a permanent-delete confirmation state instead of deleting;
- Shift+Delete creates confirmation state before any adapter call;
- cancelled confirmation does not call the adapter;
- confirmed permanent delete is marked non-undoable;
- adapter failures become visible failed state.

`src/VeloFile.Windows/Shell/` adds the Windows boundary. The mapper rejects unconfirmed permanent delete and makes the delete disposition explicit: Recycle Bin versus permanent. The adapter runs the Windows file-system/Shell-facing calls on a worker task so the app command path is not tied to blocking file-operation work.

The app shell now retains a `FileOperationService`, wires Delete, Shift+Delete, and Rename command routes into it, exposes operation status text, and adds a confirmation surface for permanent delete. Rename is intentionally two-stage in this milestone: the command marks the selected item as pending rename, and `CommitPendingRenameAsync` supplies the target name.

The corpus tool now supports `run-compat-corpus.ps1 -Scope safe-delete` by generating the operations profile and verifying scratch-only safe-delete fixtures.

## What Stayed Out

M9 still owns copy/move, conflict choices, partial batch continuation, cancellation after partial completion, and full undo execution. M8 records undo eligibility only: rename and Recycle Bin delete may be undo-eligible; permanent delete never is.

## Tests

New tests cover:

- rename request routing and undo eligibility;
- Delete using Recycle Bin delete;
- Recycle Bin unavailable fallback requiring permanent-delete confirmation;
- Shift+Delete requiring explicit confirmation before adapter calls;
- cancelling confirmation without destructive work;
- permanent delete no-undo behavior;
- progress, failure, thrown adapter exceptions, and running state while work is pending;
- Windows shell-operation request mapping;
- app shell command route and visible status/confirmation surfaces;
- safe-delete corpus runner support.

## Validation

- `dotnet test VeloFile.sln -c Debug --filter Operations`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope safe-delete -ScratchRoot <scratch-root>`
- `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug`
- `dotnet build VeloFile.sln -c Debug`
- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
