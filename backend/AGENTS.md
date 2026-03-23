# Backend Agent Entry Guide

Use this file when opening only the `backend` folder.

Primary backend policy file:

- `.github/copilot-instructions.md`

Backend continuity files:

- `.ai-context/PROJECT_INDEX.md`
- `.ai-context/SESSION_HANDOFF.md`

Startup sequence for a new backend session:

1. Read `.ai-context/SESSION_HANDOFF.md`.
2. Read `.ai-context/PROJECT_INDEX.md`.
3. Read `.github/copilot-instructions.md`.
4. Then inspect task-specific backend files.
