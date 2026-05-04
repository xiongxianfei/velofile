# ADR 0004: Preview Provider Boundaries

## Status

accepted

## Context

V1 preview supports common images, bounded text/code, and PDF first-page rendering. It must avoid heavyweight dependencies, avoid UI stalls, fail safely, and never modify source files.

## Decision

Use internal preview provider boundaries with uniform terminal states: loading, success, unsupported, failed. Providers are cancellable or ignorable, bounded by timeout and size limits, and fall back to metadata.

V1 provider set:

- Images through Windows image capabilities with 100 MB and 8192 by 8192 decoded-pixel caps.
- Text/code by reading at most 1 MB and skipping content preview above 100 MB.
- PDF first-page rendering with user-driven page navigation and metadata-only fallback above 500 MB.
- Thumbnails with 500 ms per item and at most 4 concurrent operations.

## Alternatives considered

- Rich third-party preview engines: dependency and security surface too large for V1.
- Reuse Shell preview handlers: risks third-party code and broader hosting behavior.
- No preview in V1: loses core product value.

## Consequences

- New preview types can be added later without changing UI shell contracts.
- Unsupported is a normal state, not an error.
- Provider tests must prove source files are not modified.

## Follow-up

Architecture review should confirm provider boundaries are not tied to UI controls.
