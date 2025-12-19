---
name: plan
command: plan
description: Create or update a phase implementation plan document (no code changes).
agent: "#file:../agents/plan.agent.md"
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

#file:04-mvp-roadmap.md
#file:planning/phase-2-plan.md
#file:templates/plan-template.md
#file:agents/plan.agent.md

You are a planning agent, use the #file:agents/plan.agent.md as your internal guide.

Please refer to `04-mvp-roadmap.md`, any user-provided requirements files (USER INPUT), and the codebase, then generate an implementation plan document named using the format `phase-X.x-plan.md`, where X.x is the phase identifier.

Phase to plan: {{input}}

If {{input}} is not substituted automatically by the chat UI, treat it as the phase identifier provided alongside the command and proceed.

## Output requirements

- Create or update a single plan document at `.github/planning/phase-{{input}}-plan.md`.
- Follow the formatting and general style of `phase-2-plan.md`.
- Use the structure from `.github/templates/plan-template.md`.
- Do not implement code changes in this step; planning only.

## Planning requirements

- Start by briefly summarizing: scope, non-goals, and acceptance criteria for the phase.
- Identify the most relevant files/components in the current codebase.
- Provide an incremental step-by-step plan with checklists.
- Call out risks, edge cases, and mitigation steps.
- Include testing strategy (unit tests where there is an existing pattern; otherwise manual validation steps).

## Clarifications policy

- Ask up to 3 clarifying questions only if required to produce a coherent plan.
- Otherwise, make the smallest reasonable assumptions, and record them explicitly under "Assumptions and Constraints".
