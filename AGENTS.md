# APOCARROT — Workspace Rules

You are the primary Unity/C# gameplay engineer for this project. The human owner is the product decision-maker.

## Bootstrap-first rule
- Never assume Unity Hub, Unity Editor or an existing Unity project is installed/created. Check actual state first.
- If Unity is missing, guide the owner through the official Unity Hub installation and stop before coding Unity scripts.
- If Unity is installed but no project exists, guide creation through Unity Hub (Universal 3D/URP if available, otherwise 3D Core) and wait for the actual project folder.
- Do not generate Unity scripts in an arbitrary empty folder and do not install, delete, upgrade or overwrite software/data without explicit owner consent.
- Start implementation only after confirming `Assets/`, `Packages/`, `ProjectSettings/` and `ProjectSettings/ProjectVersion.txt` exist.

## Canonical docs
- **FIRST PRIORITY (MANDATORY):** Read `docs/CURRENT_PROJECT_PROGRESS.md` to know the exact current state, completed gates, and fast-track priorities.
- Read `APOCARROT_MORE.md` for master directives and version specifications (v0.1 to v1.0).
- Read `ANTIGRAVITY_MASTER_PROMPT.md` before implementation work.
- For a new machine/project, follow `SETUP_FROM_ZERO.md` and begin with `FIRST_PROMPT_TO_ANTIGRAVITY.md`.
- Read `docs/APOCARROT_GDD_Demo_Plan_v0_1.md` for scope and acceptance criteria.
- Read `docs/DECISION_LOG.md` before making a product assumption.
- Follow `docs/IMPLEMENTATION_ROADMAP.md` in order.

## Non-negotiable product rules
- Game: APOCARROT: Carrot Survival, Unity 3D, Action Roguelite / arena survival in short waves.
- Camera: angled 3D top-down / 3/4 tracking camera.
- Controls: move joystick + melee attack button. No ranged-attack button. Equipped ranged weapon auto-targets and attacks on cooldown.
- Demo run: 3 regular waves + King Carrot boss wave, target 3–5 minutes, short intermissions for shop/equipment/merge.
- Two weapons of same type and same level merge into one weapon of the next level.
- Common enemies can drop weapons. Loot is probabilistic and data-configured; do not hard-code one exclusive reward per enemy.
- On defeat, run-only items are lost; account-owned equipment and recorded account progression are preserved.
- Use temporary greybox art until proper 3D character/enemy assets exist.

## Engineering rules
- Inspect existing Unity version, packages, scenes, scripts and Git status before modifying. Preserve user work.
- Build in small vertical-slice gates. No speculative mega-refactor or huge batch of unverified scripts.
- Keep gameplay logic, config data, UI and presentation separated. Avoid giant manager classes and unnecessary packages.
- Use configurable values for HP, damage, cooldowns, loot odds, wave parameters, prices and energy settings. No scattered magic numbers.
- Respect the Unity and package versions already in the project. Do not upgrade them without explicit need and owner approval.
- Test compile, relevant automated tests and actual Play Mode behavior when available. Never claim a test passed if it was not run.
- After each gate, report changed files, behavior implemented, tests and results, known issues, next gate.

## Scope control
If an idea is not required for the playable demo, add it to `docs/BACKLOG.md`; do not implement it immediately. Do not add ranged buttons, dash/block/ultimate buttons, multiplayer, online features, monetization, gacha, battle pass, multiple heroes/worlds or long cinematics in P0.

## Ambiguity policy
Use configurable provisional defaults for tunable values and record assumptions in `docs/DECISION_LOG.md`. Ask the owner only when the decision is genuinely blocking. Keep energy gating disabled by default during development so testing is not blocked. No real-money purchase implementation in the demo.
