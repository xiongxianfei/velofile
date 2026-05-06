# Differences from File Explorer

VeloFile V1 is a side-by-side file explorer. It does not replace File Explorer, does not register as the Windows shell, and does not take ownership of system file associations.

## File Extensions

VeloFile shows file extensions by default. This is intentional.

Keeping extensions visible makes names such as `invoice.pdf.exe` easier to inspect because the executable extension remains visible. This is not a complete security guarantee; it is a clarity default.

The setting is per-application. Hiding known extensions in VeloFile changes only VeloFile. It does not change File Explorer, and File Explorer settings do not change VeloFile.

## Built-In Context Menu

VeloFile V1 has a built-in context menu for V1 commands such as Open, Open with, Cut, Copy, Paste, Rename, Delete, Properties, Copy path, Copy name, and Open terminal here.

V1 does not expose OS shell extension menu entries and does not host third-party Shell extension handlers. Commands installed by cloud storage tools, archive tools, source-control tools, or device utilities may still be available in File Explorer but absent in VeloFile V1.

## Explorer Remains Available

VeloFile installs side by side. File Explorer remains available for workflows that depend on Shell extension commands or global Explorer integration.
