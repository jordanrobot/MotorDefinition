---
name: implement
command: implement
description: Implement a roadmap phase end-to-end without pausing for clarification.
---


## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

#file:04-mvp-roadmap.md
#file:agents/expert-dotnet-software-engineer.agent.md

You are an expert C# developer, use the #file:agents/expert-dotnet-software-engineer.agent.md as your internal guide.

Please examine all planning documents and the existing codebase and implement phase X.

Phase to implement: {{input}}

Additional planning document references will be added if needed by the user.

If {{input}} is not substituted automatically by the chat UI, treat it as the phase number provided alongside the command and proceed.

## Operating constraints

- Implement this phase completely, end-to-end, without stopping to ask the user questions or request clarifications.
- If requirements are ambiguous, make the smallest reasonable assumption that matches the planning docs and existing patterns.
- Do not ask other humans for help.
- Do research as needed using available tools (workspace search, Microsoft Learn MCP, and other accessible references).

## Execution requirements

- Read all referenced planning docs and relevant code before making changes.
- Implement the full phase scope (code, UI/UX changes, persistence changes, and docs updates if required by the plan).
- Keep changes minimal and consistent with the existing architecture and style.
- Add or update tests when there is an existing test pattern for the area you change.
- Run the most relevant tests and ensure the build is clean.
- Provide a concise recap of what changed and where.

## Progress tracking and error handling
   - Report progress after each completed task
   - Halt execution if any non-parallel task fails
   - Provide clear error messages with context for debugging
   - Suggest next steps if implementation cannot proceed
   - **IMPORTANT** For completed tasks, make sure to mark the task off as [X] in the tasks file.

## Completion validation
   - Verify all required tasks are completed
   - Check that implemented features match the original specification
   - Validate that tests pass and coverage meets requirements
   - Confirm the implementation follows the technical plan
   - Report final status with summary of completed work