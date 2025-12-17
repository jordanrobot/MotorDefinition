# Copilot Development Workflow for CurveEditor

## Purpose

This document describes how GitHub Copilot agents should use the planning and architecture artifacts in this repository when implementing new features, phases, or refactors.

The goal is to ensure that agents:
- Honor the product roadmap and phase plans.
- Implement behaviors consistent with functional requirements.
- Respect architectural decisions captured in ADRs.
- Use shared terminology from the terms and definitions.

## Required Reading for New Work

Before making non-trivial code or XAML changes, Copilot agents should load and review:

- Overall roadmap: `.github/planning/04-mvp-roadmap.md`.
- Relevant phase requirements: `.github/planning/phase-N-requirements.md` for the active phase.
- Relevant phase plan: `.github/planning/phase-N-plan.md` (if present).
- Phase-specific bug/notes log: `.github/planning/phase-N-bugs-and-notes.md` (if present).
- Terms and definitions: `.github/planning/00-terms-and-definitions.md`.
- Related ADRs referenced by the roadmap or phase docs (for example, motor property undo design, panel layout, file schema).

## Phase-Centric Workflow

For any new feature or phase-level work:

1. **Identify the active phase**
   - From the roadmap, determine which phase section covers the requested work (e.g., Phase 3: File Management).
   - Locate the corresponding phase requirements file, such as `.github/planning/phase-3-requirements.md`.

2. **Use the phase requirements as the source of truth**
   - Treat the `phase-N-requirements.md` file as the primary specification for user-visible behavior.
    - Use the headings and checklists in the phase requirements as the main guide for behavior.
    - When acceptance criteria or open questions have lightweight labels (e.g., `AC 3.0.1`, `Q 3.0.1`), use those labels when:
       - Planning implementation steps.
       - Writing tests.
       - Describing changes in commits or PRs.

3. **Consult ADRs for architectural decisions**
   - When the phase requirements or roadmap reference an ADR (e.g., undo/redo behavior, layout strategy, keyboard routing), load that ADR and follow its guidance.
   - If implementation work reveals that an ADR is incomplete or conflicts with requirements, note this in the appropriate phase bugs/notes file and propose an ADR update or a new ADR.

4. **Create or update a phase plan as needed**
   - For larger efforts, create or update `.github/planning/phase-N-plan.md` to:
     - Break requirements into implementation tasks.
     - Capture sequencing, dependencies, and technical notes.
     - Record any test strategy or tooling considerations.
   - Keep the plan focused on how we will meet the requirements, not on re-specifying behavior.

5. **Track bugs and follow-ups per phase**
   - Use `.github/planning/phase-N-bugs-and-notes.md` to log:
     - Known defects or regressions.
     - Questions about ambiguous requirements.
     - Future enhancements discovered during implementation.
   - Reference ADR IDs and any relevant acceptance criteria or question labels whenever possible.

6. **Keep terms consistent**
   - When introducing new UI labels or domain concepts, update `.github/planning/00-terms-and-definitions.md` so future agents and humans share the same vocabulary.

## When Requirements Are Missing or Ambiguous

If the active phase requirements do not clearly specify behavior:

1. Add a short "Open Questions" section to the relevant `phase-N-requirements.md` area with numbered questions (e.g., `Q 3.0.1`).
2. Use these question labels in code comments, TODOs, or bugs/notes to highlight areas that need clarification.
3. Prefer conservative, non-breaking behavior when forced to choose an implementation before the question is resolved.

## ADR Usage

- Use ADRs to capture cross-cutting or high-impact decisions (architecture, layout strategy, undo/redo model, schema changes, keyboard routing).
- For new decisions:
  - Add a new ADR under `.github/adr/adr-XXXX-some-title.md`.
  - Reference the ADR from the roadmap and from any affected `phase-N-requirements.md` or `phase-N-plan.md` documents.
- When implementing code governed by an ADR, avoid re-deciding the same question in code; if a change is necessary, propose an ADR update instead.

## Templates for Phase Requirements

To keep phase requirement documents consistent, use the phase requirements template when creating a new `phase-N-requirements.md` file:

- Template location: `.github/planning/templates/phase-requirements-template.md`.

Agents creating new phase requirement documents should:

1. Copy the template file to a new `phase-N-requirements.md`.
2. Replace `N` and section titles with the correct phase number and feature name.
3. Fill in Scope, Non-goals, Functional Requirements (as readable checklists), Acceptance Criteria (with light AC labels if helpful), and Open Questions (with light Q labels if helpful).
4. Link the new phase requirements document from the roadmap (`04-mvp-roadmap.md`) if it is not already referenced.

## Coding and Testing Expectations

When implementing features against these documents, agents should:

- Respect existing project conventions (naming, structure, patterns) unless an ADR specifies a new pattern.
- Prefer surgical changes focused on the active phase and requirements, avoiding unrelated refactors.
- Add or update tests where practical to exercise the specified behaviors, using requirement IDs in test names or comments when helpful.
- Run relevant tests after changes and note any failing tests related to the active phase in the phase bugs/notes doc.

This workflow is intended to make Copilot-driven changes predictable, traceable, and aligned with the evolving design of CurveEditor.
