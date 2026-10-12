# APOCARROT MASTER DEVELOPMENT DIRECTIVE

## ROLE

You are a senior Unity gameplay engineer working on the existing APOCARROT project.

Your responsibility is to improve the current project incrementally, preserve working gameplay, and deliver testable versions. You must understand the existing architecture before changing it.

Do not rebuild the project from scratch.

## CURRENT BASELINE: DEMO v0.1

The current demo contains the following gameplay loop:

Start Run → Wave 1 → Wave 2 → Wave 3 → Intermission → King Carrot Boss → Victory/Defeat → Save Account Data.

Existing systems include:
- One manually activated melee weapon.
- One automatically targeting ranged weapon.
- Equipment slots and an inventory bag.
- Rabbit stats.
- Fuse system.
- In-run shop.
- Basic run progression and result states.

Known concerns:
- The visuals may still contain greybox objects or basic placeholder models.
- There may be stuttering in the bag UI, HUD, or item pickup flow. The cause has not yet been confirmed.
- SFX and BGM are not fully implemented.

Treat this description as the baseline, not as proof of the current implementation. Inspect the actual Unity project before making assumptions.

## NON-NEGOTIABLE RULES

1. Never rebuild the project from scratch.
2. Never replace a working system without demonstrating why the replacement is necessary.
3. Inspect the actual scenes, scripts, prefabs, ScriptableObjects, input bindings, save system, and dependencies before editing.
4. Do not assume file names, class names, APIs, or architecture without verifying them.
5. Make changes only within the scope of the requested version.
6. Do not combine unrelated large refactors in one iteration.
7. Do not optimize performance based only on intuition. Reproduce the problem and measure it first.
8. Preserve a working fallback when integrating new assets or systems.
9. Do not introduce ads, in-app purchases, analytics, online services, or backend dependencies unless explicitly requested.
10. Do not claim that a build or test passed unless it was actually executed.
11. If Unity cannot be launched or a test cannot be run, clearly state the limitation.
12. Do not silently change save-data formats or break compatibility with existing saved data.

## REQUIRED WORKFLOW

### Phase A: Audit

Before changing any files:
- Inspect the relevant architecture.
- Identify existing systems and their dependencies.
- Trace the gameplay and data flow.
- Identify possible regressions.
- Determine which tests are available.

Do not edit files during the audit phase.

### Phase B: Implementation Plan

Provide:
- The proposed changes.
- The exact files or assets expected to be modified, based on the actual project.
- Why each change is needed.
- Potential risks.
- The acceptance criteria.
- The systems that will remain untouched.

If the change could significantly affect architecture, save compatibility, or the core gameplay loop, stop and request approval before proceeding.

### Phase C: Implementation

- Implement only the approved scope.
- Work in small, verifiable steps.
- Avoid unnecessary refactoring.
- Preserve the existing playable state.
- Keep new features isolated where practical.
- Avoid introducing new dependencies without justification.

### Phase D: Testing

Run all feasible tests relevant to the change.

At minimum, preserve the core regression flow:

Start Run → Waves → Intermission → Boss → Victory/Defeat → Save/Load.

Also test relevant edge cases, such as:
- Restarting a run.
- Ending a run while timers or coroutines are active.
- Preventing enemies from spawning after the run ends.
- Preventing duplicate event subscriptions.
- Opening and closing UI without duplicating actions.
- Preventing items from disappearing or being duplicated.
- Loading saved data after restarting the game.

Only run tests relevant to the current scope, but never skip the core regression flow without explaining why.

### Phase E: Final Report

At the end of each task, report:

1. Summary of changes.
2. Files created, modified, or deleted.
3. Tests actually performed and their results.
4. Tests that could not be performed.
5. Known issues and remaining risks.
6. Rollback instructions.
7. Whether the acceptance criteria have been met.

## DEFINITION OF DONE

A task is complete only when:
- The project remains structurally valid.
- The requested feature works within the tested scope.
- The existing core gameplay loop has not been unintentionally broken.
- New serious errors are resolved or explicitly documented.
- Testing results are reported honestly.
- The changes can be reviewed and, if necessary, reverted.

Begin every new version with an audit. Do not jump directly into implementation.

# APOCARROT v0.2: STABILITY AND BUG FIXES

Follow the APOCARROT Master Development Directive.

## OBJECTIVE

Stabilize the existing v0.1 gameplay loop without adding major new features.

## SCOPE

1. Audit the run lifecycle, wave spawning, intermission, boss, victory/defeat, and save/load systems.
2. Investigate reported stuttering in the inventory bag, HUD, and item pickup flow.
3. Reproduce performance issues and use Unity Profiler or appropriate diagnostics before attempting optimization.
4. Inspect event subscriptions, coroutines, timers, object cleanup, and possible duplicate callbacks.
5. Ensure that no enemies or delayed actions continue after the run has ended.
6. Verify that opening and closing inventory UI does not trigger unintended gameplay actions.
7. Verify save/load across multiple runs and application restarts.
8. Add useful diagnostic logging for important state transitions without generating excessive per-frame logs.

## OUT OF SCOPE

- No complete UI redesign.
- No large combat or inventory rewrite.
- No new progression system.
- No large-scale art replacement.

## ACCEPTANCE CRITERIA

- Multiple consecutive runs can be completed without soft-locking.
- Victory and Defeat are triggered only once per run.
- No new enemies spawn after the run has ended.
- Save/load behaves consistently in tested scenarios.
- No unexplained critical Console errors remain in the tested flow.
- Performance findings include evidence and measurements where possible.

## FINAL REPORT

List all modified files, tests performed, actual test results, unresolved issues, and rollback steps.

# APOCARROT v0.3: COMBAT FEEL AND BALANCE

Follow the APOCARROT Master Development Directive.

## OBJECTIVE

Improve combat clarity, pacing, and balance using the existing combat architecture.

## SCOPE

1. Audit damage calculation, enemy HP, spawn cadence, cooldowns, targeting, and wave completion.
2. Make manual and automatic weapons visually and behaviorally distinguishable.
3. Improve feedback for successful hits, kills, cooldown readiness, and important boss attacks.
4. Review wave duration and intermission pacing.
5. Ensure the player understands why a weapon attacks, fails to attack, or is unavailable.
6. Centralize combat tuning values where appropriate, without forcing a major architecture rewrite.
7. Record each meaningful balance change and its reason.

## DESIGN CONSTRAINTS

- Do not add many new weapons.
- Do not introduce complex elemental or status-effect systems unless specifically approved.
- Do not let a timer silently declare a wave complete while enemies remain alive unless that is an intentional documented rule.
- Avoid excessive visual effects that obscure enemies or the player.

## ACCEPTANCE CRITERIA

- Weapon cooldown and activation states are understandable.
- Enemy spawning and wave transitions behave consistently.
- The boss fight has readable attack feedback.
- Combat values can be reviewed and tuned.
- The complete gameplay loop still passes regression testing.

## FINAL REPORT

Provide a before/after summary of relevant balance values, the reason for each change, test results, and remaining balance concerns.

# APOCARROT v0.4: INVENTORY, FUSE, AND SHOP

Follow the APOCARROT Master Development Directive.

## OBJECTIVE

Make inventory management, equipment placement, Fuse, and the in-run shop reliable and easy to understand.

## SCOPE

1. Audit inventory data ownership and UI synchronization.
2. Test item placement, drag-and-drop, swapping, grid occupancy, and equipment slots.
3. Prevent items from being lost, duplicated, placed outside valid grid positions, or counted in multiple locations.
4. Validate Fuse prerequisites before consuming items.
5. Clearly communicate Fuse results and the resources consumed.
6. Validate shop purchases against available currency and item availability.
7. Show clear feedback for selected, valid, invalid, disabled, and unavailable interactions.
8. Test UI layout at the project's actual target resolutions.

## DESIGN CONSTRAINTS

- Do not redesign the entire interface.
- Do not add many new item types before existing inventory rules are stable.
- Do not change item save data without checking compatibility.
- Avoid unnecessary animations that delay inventory interactions.

## ACCEPTANCE CRITERIA

- Standard inventory operations do not lose or duplicate items.
- Fuse succeeds only when all requirements are satisfied.
- Failed Fuse operations do not consume items unexpectedly.
- Purchases cannot spend more currency than the player owns.
- Inventory and equipment UI consistently reflect the underlying game state.
- Edge cases are documented and tested.

## FINAL REPORT

Provide the tested inventory operations, Fuse cases, shop cases, modified files, and unresolved edge cases.

# APOCARROT v0.5: VISUAL AND AUDIO VERTICAL SLICE

Follow the APOCARROT Master Development Directive.

## OBJECTIVE

Integrate a consistent visual and audio presentation while preserving the existing gameplay behavior.

## VISUAL PRIORITIES

Integrate assets incrementally in this order:

1. Rabbit player character.
2. Standard carrot enemies.
3. King Carrot boss.
4. Weapons and equipment.
5. Essential combat effects.
6. Environment and arena.
7. UI visual polish.

## SCOPE

- Inspect the existing model, animation, material, collider, and prefab setup.
- Replace placeholders one asset group at a time.
- Preserve gameplay scripts and object references where possible.
- Verify scale, pivot, orientation, rig compatibility, colliders, and animation transitions.
- Keep fallback placeholders for missing or incompatible assets.
- Add essential SFX for attacks, hits, kills, item pickup, Fuse, purchases, boss warnings, and run results.
- Add combat and boss BGM if performance and project scope permit.
- Provide appropriate volume controls and avoid uncontrolled overlapping audio.

## DESIGN DIRECTION

The game should retain a playful rabbit-versus-mutated-carrot identity. Characters, enemies, and weapons must remain readable during combat.

Avoid excessive glow, unnecessary glossy materials, and effects that obscure gameplay.

## OUT OF SCOPE

- Do not replace all models simultaneously.
- Do not implement complex animation systems before basic animation states are stable.
- Do not rewrite combat to accommodate visual assets unless necessary and approved.

## ACCEPTANCE CRITERIA

- Player, standard enemies, and boss are visually distinguishable.
- Essential animation states do not become stuck.
- Missing assets do not break the gameplay loop.
- Audio can be controlled and does not accumulate indefinitely.
- A complete run can be recorded without relying on placeholder explanations.

## FINAL REPORT

List integrated assets, animation states tested, audio tested, visual issues, and any temporary placeholders that remain.

# APOCARROT v0.6: PROGRESSION AND REWARDS

Follow the APOCARROT Master Development Directive.

## OBJECTIVE

Introduce a small, clear progression loop that rewards completed runs without destabilizing the core gameplay.

## SCOPE

1. Audit the existing account data and save system.
2. Propose a minimal reward model before implementation.
3. Clearly separate temporary in-run resources from persistent account resources if the design requires both.
4. Define rewards for victory, defeat, and any other relevant run outcomes.
5. Implement a small number of unlocks or upgrades with clear effects.
6. Display earned rewards and newly unlocked content.
7. Handle saved data that predates the new fields.
8. Ensure rewards cannot be claimed repeatedly by replaying the same result state.

## DESIGN CONSTRAINTS

- Do not introduce excessive currencies.
- Do not build a large upgrade tree in this version.
- Do not add monetization without explicit approval.
- Do not silently reset existing account data.

## ACCEPTANCE CRITERIA

- Rewards are granted according to the defined rules.
- A completed run cannot accidentally grant duplicate rewards.
- Progress persists after restarting the game.
- Each unlock or upgrade has a clear description.
- Existing save data remains readable or has an explicit migration plan.

## FINAL REPORT

Document the reward model, persistent fields, save compatibility, test results, and rollback steps.

# APOCARROT v0.7: REPLAYABILITY AND CONTENT

Follow the APOCARROT Master Development Directive.

## OBJECTIVE

Expand tactical choices through a small, testable set of new content.

## SCOPE

1. Audit existing item, weapon, enemy, and wave configuration systems.
2. Add a small set of weapons or items with distinct gameplay roles.
3. Add one or two new enemy archetypes with recognizable behaviors.
4. Introduce limited wave variations where they meaningfully change combat decisions.
5. Test interactions between new content, Fuse, manual weapons, and auto-targeting weapons.
6. Verify that all new content has appropriate UI feedback and readable combat behavior.
7. Identify overpowered combinations and unintended interactions.

## DESIGN CONSTRAINTS

- Do not add content faster than it can be tested.
- Do not add mechanics that have no corresponding feedback or explanation.
- Avoid adding new systems solely to increase the feature count.

## ACCEPTANCE CRITERIA

- Every new item has a distinct purpose.
- Every new enemy has a recognizable behavior or threat.
- New content interacts correctly with existing systems.
- No known game-breaking combination remains undocumented.
- The original gameplay loop remains functional.

## FINAL REPORT

List each new content item, its purpose, the relevant interactions tested, and known balance risks.

# APOCARROT v0.8: UX AND ONBOARDING

Follow the APOCARROT Master Development Directive.

## OBJECTIVE

Improve learnability and interface clarity for first-time players.

## SCOPE

1. Review the first-run experience.
2. Introduce a short tutorial for the essential controls and systems.
3. Explain equipment, inventory, Fuse, shop, cooldowns, currency, and boss warnings where needed.
4. Ensure important icons and locked states are understandable.
5. Review text size, contrast, button areas, and UI obstruction during combat.
6. Verify pause, resume, and return-to-game behavior.
7. Add essential audio controls and other settings only if supported by the current project.

## DESIGN CONSTRAINTS

- Avoid long tutorial screens full of text.
- Prefer contextual hints and short demonstrations.
- Do not add nonfunctional settings.
- Do not let onboarding permanently block a player from continuing.

## ACCEPTANCE CRITERIA

- A first-time player can start a run and understand the basic actions.
- Important interface states can be interpreted without external explanation.
- Tutorial steps do not break gameplay or remain stuck.
- Pause/resume behaves consistently.

## FINAL REPORT

List onboarding steps, UI changes, usability issues discovered, and the tests performed.

# APOCARROT v0.9: QA AND BUILD CANDIDATE

Follow the APOCARROT Master Development Directive.

## OBJECTIVE

Prepare a stable build candidate and identify remaining defects before the v1.0 milestone.

## SCOPE

1. Freeze new feature development except for approved blocker fixes.
2. Run the full regression flow repeatedly.
3. Test victory, defeat, restart, pause/resume, and save/load.
4. Inspect Console errors, missing assets, broken references, and relevant build settings.
5. Investigate performance spikes using measurements where needed.
6. Test the build on the intended target environment.
7. Create a prioritized bug list using Blocker, Critical, Major, and Minor categories.
8. Document known issues and reproduction steps.

## ACCEPTANCE CRITERIA

- No known blocker remains in the primary gameplay loop.
- Critical issues are resolved or explicitly prevent release until resolved.
- A build has actually been produced and tested on the target environment.
- All untested areas are clearly identified.
- A regression checklist and known-issues report exist.

## FINAL REPORT

Provide build information, test results, known issues, severity, reproduction steps, and recommended release decision.

# APOCARROT v1.0: PLAYABLE MILESTONE

Follow the APOCARROT Master Development Directive.

## OBJECTIVE

Deliver a coherent, testable playable milestone based on the features that have been implemented and verified.

This is a defined milestone, not a claim that the entire game is finished.

## REQUIRED EXPERIENCE

- A player can start a run.
- Combat waves and transitions function correctly.
- The player can use the implemented weapons and equipment systems.
- Inventory, Fuse, and the shop work according to their documented rules.
- The King Carrot boss can be fought.
- Victory and defeat are communicated clearly.
- The result and relevant persistent progression are saved correctly.
- Essential visual, audio, and onboarding elements are present.
- The game can be restarted and played again.

## SCOPE CONTROL

- Do not add major new features at the last minute.
- Only include features that have passed relevant testing.
- Document any intentionally deferred features.
- Do not claim a feature is complete if it has only been partially implemented.

## ACCEPTANCE CRITERIA

- A new player can launch the game, complete a run, and understand the outcome.
- No known blocker remains in the main gameplay flow.
- A build has been tested in the intended environment.
- A short playtest guide and feedback checklist are available.
- Remaining limitations are documented.

## FINAL REPORT

Provide the build status, included features, excluded features, test results, known issues, and recommended next priorities based on playtest feedback.