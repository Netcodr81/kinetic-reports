# Testing and Diagnostics

Status: Draft

## Scope
Defines parity, regression, and diagnostics strategy for Visual migration.

## Decisions
- Run dual-mode tests (Legacy and Visual) in CI.
- Require parity checks for protected samples.

## Open Questions
- Golden artifact storage policy for HTML and PDF outputs.

## Non-goals
- Full visual interaction testing in phase 1.

## Implementation Notes
- Include page count, element count, and bounds-delta checks.
- Surface parity results in sample viewer diagnostics.
