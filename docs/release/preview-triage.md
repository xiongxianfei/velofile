# Preview Release Triage Policy

M15 preview release promotion is blocked when crash, hang, diagnostics, or benchmark evidence exceeds the thresholds below. These thresholds apply to preview builds before public performance claims are made.

## Blocking Thresholds

- Crash threshold: two or more crash markers for the same last-action category in a preview triage window blocks promotion.
- Hang threshold: one confirmed shell hang, stuck operation, or unresponsive preview generation report blocks promotion until triaged.
- Diagnostics threshold: any diagnostic export that contains raw file contents, terminal command text, secrets, usernames, or unredacted working directories blocks promotion.
- Benchmark threshold: p95 regressions above 10% require explicit acknowledgement in release notes; p95 regressions above 25% block promotion unless there is an explicit exception.

## Exception Path

An explicit exception must name the affected requirement, the evidence artifact, the reason the release can proceed, and the owner accepting the risk. Skipped or unavailable compatibility cases are not counted as verified evidence without a documented waiver.

## Evidence Rules

Promotion evidence must come from local-only diagnostic logs, benchmark reports with environment metadata, compatibility corpus reports, and the manual accessibility evidence tracked in `docs/release/accessibility-checklist.md`. The release owner must preserve the triage threshold used for the decision with the preview release notes.
