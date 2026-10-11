# APOCARROT — Unity Playable Demo Roadmap

This roadmap is the working order for the coding agent. Each gate should be small enough to inspect and test. Do not proceed on top of compile errors from the previous gate.

## Gate 0 — Environment bootstrap and project audit (no destructive edits)

**Tasks**
- First detect whether Unity Hub, Unity Editor and a Unity project are present. Never assume they exist.
- If Unity Hub/Editor are missing, guide the owner through official installation; do not create scripts in an arbitrary folder and do not install software without approval. Stop until the owner confirms the Editor is ready.
- If Unity exists but project does not, guide the owner to create `APOCARROT` in Unity Hub using Universal 3D (URP) if available, otherwise 3D Core. The project location must be explicitly chosen by the owner.
- Once the project exists, verify `Assets/`, `Packages/`, `ProjectSettings/`, `ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json`.
- Inspect scenes, Assets structure, input packages, scripts, prefabs, tags/layers, materials and Git status.
- Identify usable assets and missing 3D rigs/animations.
- Create `docs/PROJECT_AUDIT.md` noting setup status, exact Unity version, template/render pipeline, input stack, Console blockers and project state.

**Exit criteria**
- Unity project opens in Editor and its structure is understood.
- Existing user work is preserved.
- Known blockers are listed separately from non-blocking missing art.

## Gate 1 — Playable greybox arena and movement

**Tasks**
- Create or adapt one arena scene with floor, visible boundary, lighting and simple props.
- Add a placeholder Rabbit Warrior using primitives or available assets.
- Configure angled 3D top-down follow camera; camera tuning should be adjustable in Inspector.
- Add movement input: keyboard/arrow keys for Editor testing plus a basic on-screen virtual joystick for touch. Reuse the project's already-installed input approach where practical.
- Clamp movement to arena and keep facing direction consistent with movement.
- Show a minimal HUD with health and a placeholder wave label.

**Exit criteria**
- In Play Mode, user can move around the arena using keyboard; touch control is wired and usable at target device size.
- Camera follows smoothly, player stays visible and arena boundaries work.
- No compile errors. No unrelated systems added.

## Gate 2 — Health, melee combat and enemy damage

**Tasks**
- Player health, enemy health, damage events and death states.
- One Zombie Carrot placeholder enemy that follows and attacks at readable range/intervals.
- Melee attack input, attack cadence/cooldown, hit detection and damage window; prevent one press from registering accidental repeated hits.
- Add Carrot Blade and Carrot Axe as two different melee data configurations. If no matching animations exist, use readable temporary attack motion/hit arcs.
- Add health feedback: hit flash/animation placeholder, health bars or readable HP feedback, player hurt/death state.

**Exit criteria**
- Player can attack, kill an enemy and take damage/die.
- Blade and Axe feel measurably different through configurable cadence/range/damage.
- Hits occur once per valid attack window and damage is not applied repeatedly by overlapping colliders.

## Gate 3 — Auto-ranged weapons and cooldown

**Tasks**
- Implement one ranged weapon controller for the equipped ranged slot, target selection, max range, cooldown and projectile/hit resolution.
- Add Tomato Blaster and Grape Splitter as separate configurations/behaviors.
- Only fire when a valid target exists and cooldown has completed. No ranged attack button.
- Use pooling only if it adds clear value; otherwise keep projectile lifecycle simple and safe.

**Exit criteria**
- The ranged weapon automatically attacks a valid enemy after cooldown.
- No-target state does not waste a shot/cooldown unless deliberately configured.
- The two weapons are visibly and mechanically distinct.

## Gate 4 — Enemy variants, loot and run inventory

**Tasks**
- Add Basic, Fast and Elite enemies with configurable HP, speed, attack and reward profile.
- Add loot pickup objects and loot table data for run gold, weapon drops and healing; rare material can be represented if needed but do not add extra currency systems without a clear role.
- Common enemies must have a non-zero chance to drop a weapon. Use weighted/data-driven probabilities.
- Add a controlled early drop/tutorial guarantee so the player can obtain a duplicate weapon pair and learn merging even if RNG is unlucky.
- Add `RunInventory` separated from permanent account inventory. Pickups from the current run should not accidentally become account-owned items.
- Define what a full inventory does: safe initial default is show a replace/discard choice in intermission, not silently delete or overwrite gear.

**Exit criteria**
- Defeating enemies can spawn pickups; player can collect them and see them represented in run inventory.
- Random outcomes are repeatable under a test seed if practical.
- On player death, run inventory is cleared while pre-owned account items remain.

## Gate 5 — Weapon merge and intermission shop

**Tasks**
- Implement merge validation: same weapon ID/type + same level only; consume two and create one at next level.
- Add a simple intermission screen that pauses combat and shows equipped melee, equipped ranged, reserve inventory, current run gold, merge action, shop items and Continue button.
- Add a small shop with configurable stock/prices. Shopping spends run gold only.
- If the player inventory is full, support explicit replace/discard behavior.
- Upgraded level should visibly or mechanically change at least one property. Keep first demo to a few levels (e.g. Lv.1–Lv.3).

**Exit criteria**
- A duplicate pair merges correctly and cannot be merged if type/level differs.
- Purchased item deducts correct run gold and enters run inventory.
- Continue resumes the next wave and prevents combat running behind an open intermission UI.

## Gate 6 — Wave director and King Carrot boss

**Tasks**
- Create a data-driven `WaveDirector` with three normal waves and one boss wave.
- Spawn batches/quotas for Basic, Fast and Elite enemies. Stop spawning according to wave config; wave completion checks remaining relevant enemies and pending spawns.
- End a normal wave when its configured objective is met; don't wait for a timer when all required enemies are dead.
- Implement King Carrot with HP, contact/melee attack, at least one ground telegraph AoE attack, damage resolution and a low-HP pressure phase.
- Keep attack telegraphs readable and avoid cheap unavoidable hits.
- Transition into intermission between waves; use longer preparation before boss if necessary.

**Exit criteria**
- Full run can be played through all normal waves into boss wave.
- Defeating King Carrot causes a single, deterministic win transition.
- If player dies at any wave, run stops and win/boss state cannot also fire.

## Gate 7 — Account progression, results and local save

**Tasks**
- Implement run states explicitly: menu/ready, combat, intermission, win, loss, results.
- Results screen reports outcome, basic kills/waves stats and earned account XP/rewards.
- Add AccountProgression: account XP/level and at least one achievement/milestone.
- Add safe local save/load with defaults, version field and error handling.
- Do not confuse run gear/run gold with permanent account-owned gear/currency.
- Add an energy subsystem behind a dev flag; default disabled during tests. No store payments or IAP.

**Exit criteria**
- Winning/loss updates the correct values once; repeated scene load does not duplicate rewards.
- Account level/progression persists after application/Play Mode restart.
- Loss clears run-only gear but never clears pre-owned account inventory or recorded achievements.

## Gate 8 — Full demo integration and QA

**Tasks**
- Test fresh install/save state, full win, early loss, loss at boss, no target for ranged weapon, full inventory, no loot drop, duplicate merge, failed purchase due to insufficient gold, repeat run, missing save and corrupt save fallback.
- Tune the run target to approximately 3–5 minutes. Record deviations and reasons.
- Fix all P0 compile errors and blockers; check UI at the target resolution and safe area.
- Complete `docs/DEMO_ACCEPTANCE_CHECKLIST.md` with pass/fail and evidence.

**Exit criteria**
- Every mandatory P0 criterion in GDD section 14 is marked PASS or explicitly blocked with evidence; no claim of completion while a required flow is untested.

## Working cadence

At the start of a gate: state the intended scope in 3–5 bullets. Implement that gate, not the whole game. At the end: run available verification, summarize changed files and tests, then stop at a clear checkpoint. If a new idea appears, put it in backlog first.
