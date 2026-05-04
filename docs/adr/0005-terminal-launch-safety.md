# ADR 0005: Terminal Launch Safety

## Status

accepted

## Context

V1 includes Open terminal here for developer workflows. Folder names are untrusted and may contain shell metacharacters.

## Decision

Terminal launch is always explicit. Discovery must not block app launch. Supported targets are Windows Terminal, PowerShell 7, Windows PowerShell, Command Prompt, Git Bash, and WSL distributions. Default order is Windows Terminal, PowerShell 7, Windows PowerShell, then Command Prompt; Git Bash and WSL are selectable when discovered but do not outrank default targets unless chosen by the user.

Launch treats folder paths as structured process data or working directory. VeloFile does not build shell commands by concatenating folder paths into command text.

## Alternatives considered

- `cmd /c` launcher: high injection risk.
- Auto-launch terminal on navigation: surprising and out of scope.
- Only support one terminal: weaker developer workflow.

## Consequences

- Terminal launch adapter must own quoting/process-launch rules.
- Terminal failures are user-visible and leave browsing state unchanged.
- Diagnostic logs record selected terminal identity but not full command text.

## Follow-up

Test-spec must include shell-metacharacter paths and unavailable terminal targets.
