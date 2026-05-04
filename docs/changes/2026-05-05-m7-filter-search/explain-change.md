# M7 Filter and Search Change Explanation

## Why This Change Exists

M7 implements the approved V1 scope for current-folder filtering and explicit recursive search.

The governing requirements are:

- R28-R31: current-folder filtering narrows only the active folder list, uses substring matching, clears back to the unfiltered list, and must not depend on recursive or indexed search.
- R32-R35: recursive search starts only from an explicit user action, walks below the current folder, streams bounded results, remains cancellable, reports skipped locations, and avoids undocumented glob behavior.
- AC4-AC5: filter/search behavior must be user-visible, bounded, and cancellable.

## What Changed

`CurrentFolderFilterService` applies literal, case-insensitive substring matching to the current visible `ListedFileItem` rows. It treats shell/search metacharacters as ordinary text and has no dependency on recursive search or Windows Search.

`RecursiveSearchService` walks folders through the existing `IFolderEntrySource` boundary. It streams result updates, reports skipped locations, skips reparse-point directories, and emits a result-limit state when the configured cap is reached.

`AppShellViewModel` now keeps the unfiltered active listing separately from the filtered visible file rows. The WinUI shell has a current-folder filter box, explicit recursive search box/button, cancel button, and visible search status text.

## Tests

The M7 tests cover:

- current-folder literal substring filtering;
- clearing a filter to restore visible order;
- metacharacters treated as literal filter text;
- recursive search streaming and result cap;
- cancellation before the cap and after the cap;
- access-denied skipped locations;
- reparse-point loop avoidance;
- app-shell filter/search wiring and state updates.

## Boundaries

This milestone does not implement Windows Search integration, glob syntax, content search, ranking, or durable search state. Those remain outside V1 or outside M7.
