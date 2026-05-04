# M6 Fourth-Pass Code Review

## Outcome

Changes required.

## Findings

1. Blocker: the production file list had a bound control but no production data feed. `FileListSurface.ItemsSource` pointed at `AppShellViewModel.FileItems`, but `FileItems` remained empty unless tests injected rows directly.

2. Major: multi-selection output followed selected-item enumeration order instead of the current visible file-list order. Copy path/name needed to preserve the sorted or filtered view order, not WinUI selection order.

## Required Resolution

- Wire active-tab folder listing state into `AppShellViewModel.FileItems` through the existing listing/coordinator boundary.
- Prove startup/default active-tab listing, successful navigation listing refresh, active-tab switching, and shell selection/copy behavior from visible rows.
- Change selection mapping to accept current visible order and return selected rows in that order while ignoring stale selections.
