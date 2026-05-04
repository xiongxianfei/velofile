# ADR 0003: Benchmark Corpus and Release Gates

## Status

accepted

## Context

V1 makes responsiveness central but cannot make public performance claims until the reference corpus and benchmark harness exist.

## Decision

Create a deterministic generated benchmark corpus and an app-level benchmark harness. Benchmark reports record environment data, run count, median, p95, and p99. Release gates use p95 targets from the approved spec.

Regressions over 10% at p95 require explicit ADR or release-note acknowledgement. Regressions over 25% at p95 block release unless a later accepted decision changes the policy.

## Alternatives considered

- Manual timing only: not repeatable.
- Unit-test-only performance checks: misses UI and app-process behavior.
- Median-only targets: hides tail latency that users feel.

## Consequences

- Benchmark harness is a first-class architecture component.
- Public release claims wait until corpus and harness exist.
- CI/release design must account for app-level benchmark evidence.

## Follow-up

Test-spec must map each performance target to corpus shape and measurement points.
