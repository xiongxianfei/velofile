# M5 First-Pass Code Review

## Status

Changes requested.

## Findings

1. Blocker: WinUI shell controls and keyboard accelerators were static-only and did not route into navigation commands.
2. Blocker: App launch created a hardcoded workspace and did not read durable session/settings/favorites/recent-location state or call session restore.
3. Major: Empty session restore could create a zero-tab workspace whose `ActiveTab` access threw during safe-default launch.

## Required Resolution

- Introduce a command surface that every shell navigation entry point uses.
- Add a launch composition path that reads durable local state, applies crash/session restore policy, and creates the shell view model from restored state.
- Enforce the V1 invariant that a workspace always has at least one safe active tab.
- Add regression tests for command entry points, app startup restore, missing-location restore, crash start-fresh state, and empty session restore.
