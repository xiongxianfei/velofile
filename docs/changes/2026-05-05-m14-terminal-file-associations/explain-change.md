# M14 Terminal Launch And File Associations

M14 wires the existing shell verbs for Open, Open With, and Open terminal here to production app and Windows boundaries. The change keeps launch behavior explicit and treats paths as data, so command execution does not depend on shell command string construction.

## What Changed

`src/VeloFile.Core/Terminal/` adds terminal target models, default-order discovery, launch results, working-directory validation, and a launch service. Discovery sorts Windows Terminal, PowerShell 7, Windows PowerShell, and Command Prompt ahead of optional Git Bash and WSL targets. A selected optional target can still become the launch target when the user chooses it.

`src/VeloFile.Windows/Terminal/` discovers Windows terminal executables and WSL distributions behind a fakeable probe. `WindowsTerminalProcessLauncher` maps a selected terminal target to structured process start data: executable path, working directory, and separate arguments where a terminal requires them. It never builds a command line by concatenating the folder path into command text.

`src/VeloFile.Core/FileAssociations/` and `src/VeloFile.Windows/ShellExecute/` add Open and Open With launch boundaries. The Windows adapter uses ShellExecute-style process starts for default association Open and the `openas` verb for Open With, with `ModifySystemAssociations` always false.

`AppShellViewModel` now handles `Open`, `OpenWith`, and `OpenTerminalHere` in the existing built-in command route. It exposes launch status text for recoverable failures, keeps browsing state unchanged on launch failures, and persists the selected terminal target through the settings payload.

`MainWindow.xaml` binds a shell-visible launch status line, routes double-click to Open, and exposes a lazy terminal target selector. Opening the selector triggers terminal discovery, so app construction and startup do not synchronously probe terminal installations.

## Safety Notes

Terminal diagnostics record the selected terminal identity, result state, and exact controlled failure reason code. They do not record the active path, raw command text, or a concatenated launch command. Terminal reason codes are centralized in `TerminalLaunchReasonCodes` and fed into the diagnostic reason-code allowlist so drift cannot silently redact known terminal failures.

File association launch requests go through the Windows adapter with `UseShellExecute = true`; Open With uses `openas`. The app does not write association settings.

## Tests

Core tests cover terminal ordering, optional target selection, discovery failure fallback, structured working-directory launch, missing terminal, inaccessible folder, terminal diagnostic reason-code serialization with redaction, Open/Open With request shape, and broken association failures.

App tests cover the production command route for Open, Open With, Open terminal here, terminal target selection, selected-terminal settings persistence, user-visible failures, and startup not invoking terminal discovery.

Windows tests cover discovery projection, structured terminal process start requests, process-launch failures, ShellExecute Open, ShellExecute Open With, and association-launch failures.

## Validation

- `dotnet test VeloFile.sln -c Debug --filter Terminal`
- `dotnet test VeloFile.sln -c Debug --filter "Terminal|Diagnostics"` (19 Core, 7 App, and 3 Windows tests after the diagnostic reason-code allowlist resolution)
- `dotnet test VeloFile.sln -c Debug --filter FileAssociations`
- `dotnet build VeloFile.sln -c Debug`
- `dotnet test VeloFile.sln -c Debug --no-build`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\ci.ps1`
