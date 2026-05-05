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

The Windows adapter now has a production recycle-capability classification seam. Known unsupported Recycle Bin targets, such as UNC paths, return `RecycleBinUnavailable` before any delete executor call. Ambiguous failures remain recoverable failures and do not offer permanent-delete fallback.

The app shell now retains a `FileOperationService`, wires Delete, Shift+Delete, and Rename command routes into it, exposes operation status text, and adds a confirmation surface for permanent delete. Rename is a complete two-stage shell route: the command enters visible rename state, Enter/button commit calls `CommitPendingRenameAsync`, and Escape/button cancel clears the pending rename without adapter calls.

Running operations now carry cancellable state when the adapter supports cancellation. The shell exposes a Cancel operation button for those operations and routes it to the retained in-flight cancellation token.

Completed mutating operations now refresh the originating tab's listing through the existing `FolderListingCoordinator`. Rename, Recycle Bin delete, and confirmed permanent delete update visible rows only after the operation completes. Failed, cancelled, and confirmation-waiting states preserve the current rows. The refresh is scoped to the tab/path where the operation started, so a late refresh cannot overwrite a newer navigation.

The corpus tool now supports `run-compat-corpus.ps1 -Scope safe-delete` by generating the operations profile and verifying scratch-only safe-delete fixtures.

## What Stayed Out

M9 still owns copy/move, conflict choices, partial batch continuation, and full undo execution. M8 records undo eligibility only: rename and Recycle Bin delete may be undo-eligible; permanent delete never is. M8 cancellation is limited to the visible in-flight cancel command and cancellation token route for the current rename/delete operation set.

## Tests

New tests cover:

- rename request routing and undo eligibility;
- Delete using Recycle Bin delete;
- Recycle Bin unavailable fallback requiring permanent-delete confirmation;
- Shift+Delete requiring explicit confirmation before adapter calls;
- cancelling confirmation without destructive work;
- permanent delete no-undo behavior;
- progress, failure, thrown adapter exceptions, and running state while work is pending;
- production Windows recycle-unavailable classification and ambiguous-failure classification;
- app shell command route and visible status/confirmation/cancel/rename surfaces;
- rename commit, cancel, and invalid-name recovery through the view-model shell route;
- in-flight operation cancellation through the retained token;
- visible row refresh after completed rename, Recycle Bin delete, and confirmed permanent delete;
- preservation of visible rows for failed, cancelled, and confirmation-waiting operation states;
- stale post-mutation refresh suppression after navigation;
- safe-delete corpus runner support.

## Validation

- `dotnet test VeloFile.sln -c Debug --filter Operations`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/run-compat-corpus.ps1 -Scope safe-delete -ScratchRoot <scratch-root>`
- `dotnet test tests/VeloFile.Corpus.Tests/VeloFile.Corpus.Tests.csproj -c Debug`
- `dotnet build VeloFile.sln -c Debug`
- `dotnet test tests/VeloFile.App.Tests/VeloFile.App.Tests.csproj -c Debug`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/ci.ps1`
