# VeloFile V1 Release Notes

VeloFile V1 is a Windows-native file explorer focused on daily local file-management workflows: tabs, sidebar navigation, current-folder filtering, recursive search, preview/details, common file operations, terminal launch, session restore, and local diagnostics.

## Packaging

V1 ships as a signed MSIX package from the stable update channel. It installs side by side with Windows File Explorer and does not register VeloFile as a global Explorer replacement.

## File Extensions

In VeloFile, file extensions are shown by default. This is a deliberate V1 difference from many File Explorer configurations.

The safety case is clarity: a name such as `invoice.pdf.exe` remains visibly executable because the `.exe` extension is shown. This does not make a file safe by itself, and users should still evaluate files carefully.

The extension visibility setting is per-application. Changing it in VeloFile does not change File Explorer, and changing File Explorer does not change VeloFile.

## Context Menu Scope

VeloFile V1 uses a built-in context menu for V1 commands. It does not host OS shell extension menu entries or third-party context menu handlers.

## Rollback

Rollback is uninstalling the MSIX package and installing an earlier signed release if needed. Explorer remains available and system file associations remain owned by Windows and user defaults.
