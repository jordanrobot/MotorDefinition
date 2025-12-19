---
name: tasks
command: tasks
description: Generate a PR-sliceable task checklist for a phase (no code changes).
agent: "#file:../agents/expert-dotnet-software-engineer.agent.md"
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

#file:04-mvp-roadmap.md
#file:templates/tasks-template.md
#file:agents/expert-dotnet-software-engineer.agent.md

You are an expert C# developer, use the #file:agents/expert-dotnet-software-engineer.agent.md as your internal guide.

Task: Using the current repository (codebase + tests) and the user-provided planning documents (requirements + plan + any ADR references), generate a detailed, PR-sliceable task checklist to implement the requested phase.

Phase to generate tasks for: {{input}}

The user will provide the relevant phase requirements/plan documents via attachments or #file references in the chat input.

If {{input}} is not substituted automatically by the chat UI, treat it as the phase identifier provided alongside the command and proceed.

## Output requirements

- Create or update a single tasks document at `.github/planning/phase-{{input}}-tasks.md`.
- Use the structure and execution rules from `.github/templates/tasks-template.md`.
- Ensure the checklist is PR-sliceable (sequential PR sections), and each PR has:
  - a clear goal,
  - a concrete task checklist,
  - "Done when" criteria,
  - impacted file list,
  - and a quick manual test.
- Do not implement code changes in this step; tasks only.

## Task generation requirements

- The task list must cover every feature/requirement in the phase requirements document and align with the phase plan.
- The task list must be grounded in the existing codebase:
  - call out the likely files to change,
  - identify migration steps from existing patterns,
  - include persistence keys where relevant,
  - and explicitly note any prerequisites required by the current architecture.
- Include a dedicated section for acceptance criteria and an AC-driven validation script.
- Be explicit about risks and edge cases (add checklist items for mitigations).

## Clarifications policy

- Ask up to 3 clarifying questions only if required to produce a coherent tasks document.
- Otherwise, make the smallest reasonable assumptions, and record them in the tasks file under "Assumptions and Constraints".
