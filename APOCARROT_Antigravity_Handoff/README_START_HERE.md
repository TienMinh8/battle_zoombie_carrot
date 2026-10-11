# Start here — APOCARROT + Antigravity

This folder is a **handoff pack for an AI coding agent**, not a Unity project by itself. It gives Antigravity the product rules, current scope, art references and a staged plan for building the first playable demo.

## Files

- `ANTIGRAVITY_MASTER_PROMPT.md` — copy/paste this into the first Antigravity conversation. It tells the agent what role to play and what to do first.
- `AGENTS.md` — persistent workspace rules. Current Antigravity docs state that it discovers project-level `AGENTS.md` or `GEMINI.md` files as rules. Keep this file in the Unity project root.
- `docs/APOCARROT_GDD_Demo_Plan_v0_1.md` and `.docx` — full product/design specification.
- `docs/DECISION_LOG.md` — locked decisions, provisional defaults and open owner decisions.
- `docs/IMPLEMENTATION_ROADMAP.md` — staged engineering plan and gate acceptance criteria.
- `docs/BACKLOG.md` — ideas deliberately excluded from the first demo.
- `art_reference/` — reference pictures only, not production-ready rigged 3D assets.

## If you already have a Unity project

1. Make a backup or commit current work in Git before changing anything.
2. Copy `AGENTS.md` into the root of your existing Unity project.
3. Copy `ANTIGRAVITY_MASTER_PROMPT.md` into the same root.
4. Copy the `docs/` and `art_reference/` folders into the root. Do not overwrite newer project documents without comparing them first.
5. Open the actual Unity project root as the Antigravity workspace. The workspace root should contain `Assets/`, `Packages/` and `ProjectSettings/`.
6. In Antigravity, paste the content of `ANTIGRAVITY_MASTER_PROMPT.md` into a new task (or tell it to read the file), and ask it to begin with Gate 0 audit. After auditing, it should start Gate 1 if there is no blocker.

## If you don't have a Unity project yet

Create an empty Unity 3D project in Unity Hub using a stable installed Unity version. Open that project in Antigravity, then copy the root prompt/rules and the `docs/` and `art_reference/` folders into it. Do not place the entire handoff pack in `Assets/`; the documentation/rule files belong at the project root. Reference images may stay in `art_reference/` until you decide to import them into Unity.

## First message to Antigravity

Paste `ANTIGRAVITY_MASTER_PROMPT.md`, then add:

> Start with Gate 0. Inspect the actual Unity workspace and existing files before editing. Report the Unity version, input setup, scene/assets state and blockers. If the project is valid, proceed to Gate 1 (playable greybox arena, top-down angled camera, keyboard testing and mobile joystick). Preserve existing work, do not expand scope, and verify what you can actually run.

## Important

- This is a specification/handoff package. It does not itself contain a playable game binary or finished 3D models.
- The Rabbit Warrior PNG and logo are visual references, not animated 3D characters. Use greybox models until rigged models/animations are ready.
- Keep the first demo small and playable. Any nonessential new idea goes into `docs/BACKLOG.md` instead of being implemented immediately.
