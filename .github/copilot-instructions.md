# Copilot Workspace Instructions

## Skill Gate Policy (Mandatory)

For every user request, apply this decision gate before execution:

1. Detect whether any existing skill is relevant.
2. If relevant, load and use the skill first.
3. If no skill is relevant, proceed without creating a new skill.
4. Never create new skills by default.

## New Skill Creation Policy (Strict)

Create a new skill only when all conditions are true:

1. The workflow is expected to repeat (at least 3 times).
2. The workflow is multi-step and benefits from reusable guidance.
3. There are reusable assets (templates, scripts, checklists, or conventions).
4. The skill can be triggered by explicit keywords in description.
5. The new skill is not substantially overlapping with existing skills.

If any condition is false, do one of these instead:

- Use workspace instructions for always-on project rules.
- Use a prompt for one-off focused tasks.
- Use direct implementation for simple, single operations.

## Response Behavior for Skill Selection

Before implementation, briefly state:

1. Which skill(s) are selected.
2. Why they are selected.
3. Why new skill creation is not needed.

If new skill creation is requested, explicitly validate all five conditions above before creating files.

## Session Continuity Protocol (Required)

To reduce context loss across new sessions, maintain the files in `.ai-context/`:

1. `PROJECT_INDEX.md`: stable map of important folders, key entry points, and common commands.
2. `SESSION_HANDOFF.md`: latest concise handoff (what changed, decisions, validation status, next steps).

At the end of every non-trivial task, update `SESSION_HANDOFF.md` with:

1. Files changed.
2. Decisions and assumptions.
3. Validation run and results.
4. Next suggested action.

At the beginning of each new task, quickly read `PROJECT_INDEX.md` and `SESSION_HANDOFF.md` before making edits.

## Discovery Fast Path

When the user asks to continue work in a new session, use this order:

1. Read `.ai-context/SESSION_HANDOFF.md`.
2. Read `.ai-context/PROJECT_INDEX.md`.
3. Then scan only task-specific files.
