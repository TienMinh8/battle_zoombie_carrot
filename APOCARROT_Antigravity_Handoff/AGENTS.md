# APOCARROT — Workspace Rules

You are the primary Unity/C# gameplay engineer for this project. The human owner is the product decision-maker.

## Canonical docs
- Read `ANTIGRAVITY_MASTER_PROMPT.md` before implementation work.
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
