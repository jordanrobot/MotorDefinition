## Curve Editor Phase {{phase}} Plan

### Status

Draft

**Related ADRs**

- ADR-XXXX Title (`docs/adr/adr-XXXX-some-title.md`)

### Goal

- State the user-visible outcome for this phase.
- Keep scope aligned with `04-mvp-roadmap.md` and any user-provided requirements.

### Scope

- In scope:
  - [ ] Item 1
  - [ ] Item 2
- Out of scope:
  - [ ] Item 1
  - [ ] Item 2

### Assumptions and Constraints

- Assumption 1
- Constraint 1 (performance, UX, persistence, platform)

### Current Baseline (What exists today)

- Relevant files/components:
  - `src/CurveEditor/Views/...`
  - `src/CurveEditor/ViewModels/...`
  - `src/CurveEditor/Services/...`
- Existing patterns to follow (reference ADRs and current implementations):
  - Persistence
  - Keyboard shortcuts
  - Undo/redo (if relevant)

### Proposed Design

#### 1) Data / State Model

- New/updated types:
  - `TypeName`: purpose
- State ownership:
  - View model responsibilities
  - View (code-behind) responsibilities

#### 2) UI / UX

- Layout changes:
  - Where it lives (e.g., `MainWindow.axaml`)
- User interactions:
  - What the user does
  - What changes in response

#### 3) Persistence

- Keys and storage:
  - `MainWindow.SomeKey`
- Migration strategy (if legacy keys/settings exist):
  - ( ) Read old -> write new
  - ( ) Backward compatible fallback for one release

#### 4) Error Handling and Logging

- What should be logged (and what should not)
- Failure mode behavior (safe defaults, no user-facing crashes)

### Implementation Steps (Incremental)

#### Step 1: Preparation (no behavior change)

- [ ] Add/adjust shared types
- [ ] Add scaffolding needed for later steps

**Done when**

- Build passes
- No user-visible behavior change

#### Step 2: Core plumbing

- [ ] Implement core state/types
- [ ] Wire persistence load/save

**Done when**

- App starts cleanly
- New state round-trips on restart

#### Step 3: UI wiring

- [ ] Implement UI shell
- [ ] Bind UI to state and commands

**Done when**

- Feature is usable end-to-end

#### Step 4: Hardening

- [ ] Edge cases handled
- [ ] Performance is acceptable

**Done when**

- Acceptance criteria satisfied

### Acceptance Criteria

- [ ] AC {{phase}}.1: ...
- [ ] AC {{phase}}.2: ...

### Testing Strategy

- Unit tests:
  - [ ] New tests for state/persistence behavior
- Manual validation script:
  - [ ] Step 1
  - [ ] Step 2

### Follow-on Work and TODOs

- [ ] Deferred improvements / polish
- [ ] Future-phase prerequisites discovered during implementation
