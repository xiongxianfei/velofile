# VeloFile Vision

## Pitch

VeloFile is a fast, lightweight, open-source file explorer for Windows 10 and 11. It is for everyday browsing and file management when Windows File Explorer feels slow, unpredictable, or buried under legacy behavior.

## What makes this different

VeloFile chooses a narrow, Windows-native daily workflow instead of becoming a full power-user suite. It keeps hot paths like folder navigation, current-folder filtering, tabs, preview, and common file operations responsive while still respecting Windows expectations such as Recycle Bin, file associations, drag and drop, thumbnails, long paths, DPI, and terminal integration.

## Who it is for

VeloFile serves everyday Windows users who want reliable browsing, developers who live in project folders and terminals, and power users who value tabs, preview, keyboard flow, and clear behavior.

## Who it is not for

It is not for users seeking a cross-platform file manager, a cloud sync client, a global desktop indexer, a dual-pane commander, an FTP/SFTP client, or a plugin marketplace in the initial product.

## What it commits to

VeloFile commits to responsiveness, safe file operations, Windows compatibility, maintainable open-source boundaries, and enough extensibility that later features can be added without rewriting the core.

## What it refuses to be

VeloFile refuses to become bloated in V1. It does not host third-party Shell extensions in process, replace Explorer globally, add AI classification, build cloud or P2P sync, or ship large unrelated features before the core browsing, finding, preview, tab, and file-operation workflows are solid.

## What would prove this wrong

The vision is wrong if VeloFile cannot beat Explorer in common navigation and filtering workflows, cannot keep destructive operations safe by default, cannot match key Windows behaviors users rely on, or requires broad feature creep before it is useful.

## Open questions

The first architecture decision needs to choose the Windows UI/runtime stack and Shell integration strategy that can meet the performance and compatibility goals.
