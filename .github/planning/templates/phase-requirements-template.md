# Phase N.X Functional Requirements: <Feature Name>

## Scope (Phase N.X)

- Briefly describe what this phase or sub-phase covers.
- Focus on user-visible behavior and any major technical outcomes.

## Non-goals (Phase N.X)

- List behaviors or areas explicitly out of scope for this phase.
- Mention related features that will be handled in other phases.

## Dependencies

- Other phases or features that must exist or be stable.
- ADRs that govern this work (e.g., ADR-0003 for undo/redo, layout ADRs, schema ADRs).

## Functional Requirements

Describe the behaviors as readable checklists. IDs are not required for each bullet; use clear headings and grouping instead.

- [ ] Short, testable statement of required behavior.
- [ ] Another requirement.
- [ ] ...

### Acceptance Criteria (Phase N.X)

Define how we know this phase is "done" from a behavior perspective.

- AC N.X.1: Criteria tied to key behaviors in this phase.
- AC N.X.2: Another criterion.

## Open Questions (Phase N.X)

Capture design or behavior questions that still need a decision. Use lightweight labels (e.g., `Q N.X.1`) if you need to reference them from code or bugs.

- Q N.X.1: Example question about behavior or scope.
- Q N.X.2: Another question.

Once resolved, update this section (or move notes into an ADR) and adjust requirements accordingly.

## Related Planning Artifacts (Optional)

After requirements are drafted and accepted, it is often helpful to create additional planning artifacts:

- `.github/planning/phase-N-plan.md`: A concise implementation strategy and sequencing plan.
- `.github/planning/phase-N-tasks.md`: A detailed, PR-sliceable execution checklist for agents (include file touch points, “done when” criteria, and a validation script mapped to acceptance criteria).
