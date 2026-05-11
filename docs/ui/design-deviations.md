# UI Design Deviations

This document records intentional deviations from reference design material when VeloFile chooses a different production UI because it is clearer, more accessible, more Windows-native, more performant, easier to maintain, or better aligned with V1 behavior.

`hifi-design/` is reference material only. It is not the production UI source of truth.

## Status values

- proposed
- accepted
- temporary
- rejected

## Entry template

```markdown
## <Deviation title>

- Reference pattern:
- VeloFile decision:
- Reason:
  - accessibility
  - Windows-native behavior
  - performance
  - maintainability
  - V1 behavior preservation
  - no direct WinUI equivalent
- User impact:
- Verification:
- Status:
```

No meaningful first-slice deviations have been introduced yet because M1 adds contracts and static validation only. Implementation slices that change visual behavior must add entries here when they intentionally differ from reference material.
