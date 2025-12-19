## Phase {{phase}} Subtasks: {{title}} (Agent Execution Checklist)

### Purpose
- Provide a PR-sliceable task list for implementing Phase {{phase}} with minimal rework.
- Make it easy for agents to validate each acceptance criterion incrementally.

### Execution Rules (Mandatory)

- Treat this file as the single source of truth for work tracking.
- Do not start a PR section until the prior PR section is complete.
- When a task is completed, mark it as `[x]` immediately.
- A PR section is not complete until:
  - All tasks are checked `[x]`, AND
  - The "Done when" criteria are satisfied.
- Do not add “nice-to-haves” that are not listed in this file or the phase requirements.

### Scope Reminder (Phase {{phase}})
- {{scope_item_1}}
- {{scope_item_2}}
- Do not implement out-of-scope Phase {{phase}} items.

### Key Files (Expected touch points)
- UI layout: [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml)
- UI code-behind: [src/CurveEditor/Views/MainWindow.axaml.cs](src/CurveEditor/Views/MainWindow.axaml.cs)
- View model commands/state: [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)
- Persistence helpers (if relevant): [src/CurveEditor/Behaviors/PanelLayoutPersistence.cs](src/CurveEditor/Behaviors/PanelLayoutPersistence.cs)

Replace or extend this list for the phase (add services, models, tests, ADRs as needed).

### Acceptance Criteria (Phase {{phase}})

List the phase ACs here. Every PR should clearly map to one or more ACs.

- AC {{phase}}.1: {{ac_1}}
- AC {{phase}}.2: {{ac_2}}
- AC {{phase}}.3: {{ac_3}}

### State Model Summary (Target)

Describe the target runtime state and persistence for this phase. Keep it precise and implementation-oriented.

- Core descriptors/config (optional):
  - `Id` (stable)
  - `DisplayName`
  - `Default*` values
- Runtime state:
  - {{runtime_state_item_1}}
  - {{runtime_state_item_2}}
- Persistence:
  - {{persistence_item_1}}
  - {{persistence_item_2}}

Notes:
- {{note_1}}
- {{note_2}}

Defaults (first run / no persisted state):
- {{default_1}}
- {{default_2}}

### Agent Notes (Migration Guidance)

Use this section to explain how to migrate from existing code patterns without maintaining two competing sources of truth.

- Current implementation already has: {{existing_state_1}}, {{existing_state_2}}.
- Phase {{phase}} should migrate behavior to a single source of truth: {{new_source_of_truth}}.
- Keep existing commands/menus/shortcuts where possible; re-implement handlers to target the new state.
- If legacy persistence keys exist, define a migration fallback strategy.

### Implementation Notes (to avoid known pitfalls)

- {{pitfall_1}}
- {{pitfall_2}}

---

## [ ] PR 0: Preparation (no behavior change)

### Tasks
- [ ] Add/update shared types needed for Phase {{phase}}.
- [ ] Decide stable IDs/keys/names (do not rename after merging).

Required hygiene:
- [ ] Ensure no user-visible behavior changes in this PR.

### Done when
- Build passes.
- No user-visible behavior changes.

### Files
- {{file_1}}
- {{file_2}}

---

## [ ] PR 1: Core plumbing + persisted state

### Tasks
- [ ] Introduce core models/registries/state holders.
- [ ] Add persisted settings fields:
  - [ ] `{{KeyPrefix}}.{{Setting1}}`
  - [ ] `{{KeyPrefix}}.{{Setting2}}`
- [ ] Extend persistence helpers as needed.
- [ ] Add safe fallbacks for invalid persisted values.
- [ ] Add logging for persistence load/parse failures (recover with defaults; log once per failure).

Required hygiene:
- [ ] Add or update unit tests for the new state/persistence behavior where an existing pattern exists.

### Done when
- App can start, persist values, and restore them on restart (even if not yet wired to UI).
- Meets the relevant acceptance criteria for persistence safety.

### Files
- {{file_1}}
- {{file_2}}

### Quick manual test
1. Launch app.
2. Close app.
3. Confirm no exceptions and logs are clean.
4. Change state, restart, verify round-trip.

---

## [ ] PR 2: UI shell (wiring, no feature conversion yet)

### Tasks
- [ ] Add the UI shell needed for the phase (e.g., panel bar, toolbar, status affordance).
- [ ] Bind UI to the new state.
- [ ] Implement user actions -> state changes.

Required hygiene:
- [ ] Ensure UI changes do not break existing shortcuts, undo/redo, or persistence behavior outside this phase.

### Done when
- UI appears correctly.
- No overlap with content.
- Basic interaction works.

### Files
- {{file_1}}
- {{file_2}}

### Quick manual test
1. Launch.
2. Verify UI appears.
3. Trigger each interaction and confirm the right state changes.

---

## [ ] PR 3: Convert feature area A

### Tasks
- [ ] Convert {{feature_area_A}} to be driven by the new state.
- [ ] Ensure persistence/restore works.
- [ ] Ensure no regressions in unrelated areas.

Required hygiene:
- [ ] Add focused unit tests if conversion changes non-trivial logic.

### Done when
- {{feature_area_A}} works end-to-end.
- Relevant acceptance criteria pass.

---

## [ ] PR 4: Convert feature area B

### Tasks
- [ ] Convert {{feature_area_B}} to be driven by the new state.
- [ ] Ensure persistence/restore works.

Required hygiene:
- [ ] Add focused unit tests if conversion changes non-trivial logic.

### Done when
- {{feature_area_B}} works end-to-end.

---

## [ ] PR 5: Wire menus and shortcuts (if applicable)

### Tasks
- [ ] Update menus to reflect and control the new state.
- [ ] Update existing keybindings to target the new commands/state.
- [ ] Ensure menu checkmarks / enabled states are accurate.

Required hygiene:
- [ ] Confirm shortcut definitions remain centralized (e.g., defined on MainWindow) per the project’s shortcut policy.

### Done when
- Keyboard shortcuts behave consistently with the UI.
- No regressions in shortcut policy.

---

## [ ] PR 6: Hardening and final validation

### Tasks
- [ ] Ensure persistence never “learns” invalid/zero sizes.
- [ ] Ensure invalid persisted values fail safely (no user-facing errors; log once).
- [ ] Ensure responsiveness feels instant.

Required hygiene:
- [ ] Run the most relevant automated tests and keep the build clean.

### Done when
- All acceptance criteria for Phase {{phase}} pass in a manual validation pass.

### Final manual validation script (AC-driven)
1. {{validation_step_1}}
2. {{validation_step_2}}
3. {{validation_step_3}}

### Sign-off checklist

- [ ] All tasks across all PR sections are checked `[x]`.
- [ ] All acceptance criteria listed above have a verification step (test or manual script).
- [ ] No out-of-scope features were implemented.
