# ADR 0001: WinUI 3 with C# and Windows App SDK

## Status

accepted

## Context

VeloFile V1 must be Windows-native, MSIX-packaged, DPI-aware, contributor-friendly, and able to access Windows Shell, WinRT, and Win32 integration points.

## Decision

Use WinUI 3 via the Windows App SDK with C# as the primary V1 application stack. Use targeted P/Invoke, COM interop, WinRT APIs, or generated bindings where the managed surface is insufficient.

## Alternatives considered

- WPF: familiar, but weaker fit for modern Windows UI, mixed DPI, and MSIX-first direction.
- Raw Win32/C++: highest control, but higher contributor barrier and slower UI iteration.
- Electron/Tauri/web shell: conflicts with Windows-native and responsiveness goals.
- Avalonia/.NET MAUI: cross-platform abstraction where V1 needs direct Windows behavior.

## Consequences

- Contributors work mostly in C# and WinUI.
- Architecture must isolate Windows interop behind adapters.
- Some Shell integration rough edges will require ADR-backed interop decisions.

## Follow-up

Architecture review should verify that all Shell/Win32 access is behind adapter boundaries.
