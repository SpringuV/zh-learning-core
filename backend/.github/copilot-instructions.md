# Backend Workspace Instructions

This file exists so AI assistants still load clear rules when only the `backend` folder is opened in Visual Studio or VS Code.

## Skill Gate (Required)

For every request:

1. Detect whether an existing skill is relevant.
2. If relevant, load and use that skill first.
3. If no skill is relevant, continue without creating a new skill.
4. Do not create new skills by default.

Create a new skill only if all are true:

1. The workflow is expected to repeat at least 3 times.
2. The workflow is multi-step and benefits from reusable guidance.
3. Reusable assets exist (templates, scripts, checklists, conventions).
4. Trigger keywords can be defined clearly in the description.
5. The skill does not substantially overlap existing skills.

## Backend Scope

- Scope: .NET backend only (`backend/src`, `backend/tests`, solution file in `backend`).
- Do not treat frontend or Python services as part of backend changes unless explicitly requested.

## Architecture and Editing Rules

- Preserve module boundaries under `backend/src/module`.
- Prefer minimal, targeted edits; avoid broad refactors unless requested.
- Do not edit generated artifacts in `bin` or `obj`.
- Keep public APIs and contracts stable unless the task requires a breaking change.
- If a breaking change is needed, call it out explicitly in the summary.

## Validation Before Finishing

Run relevant commands for touched backend projects:

1. `dotnet restore`
2. `dotnet build`
3. `dotnet test`

If any command cannot be run, explain why and list what was not validated.

## Response Expectations

Before implementation, briefly state:

1. Which skill(s) are selected.
2. Why they are selected.
3. Why creating a new skill is not needed.

After implementation, provide:

1. Files changed.
2. What behavior changed.
3. Validation commands run and outcomes.

## Session Continuity (Backend)

To keep context in new sessions when only `backend` is open, maintain `backend/.ai-context/`:

1. `PROJECT_INDEX.md`: backend module map, solution entry points, and common commands.
2. `SESSION_HANDOFF.md`: latest backend handoff summary.

After each non-trivial backend task, update `SESSION_HANDOFF.md` with:

1. Files changed.
2. Architecture or contract decisions.
3. Validation run (`dotnet restore`, `dotnet build`, `dotnet test`) and outcomes.
4. Next recommended backend step.

At the start of new backend tasks, read `backend/.ai-context/PROJECT_INDEX.md` and `backend/.ai-context/SESSION_HANDOFF.md` first.
