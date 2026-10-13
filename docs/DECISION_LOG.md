# APOCARROT — Decision Log

Status meanings:
- **LOCKED:** already agreed; do not change without explicit owner approval.
- **PROVISIONAL:** use as a configurable demo default and adjust through playtesting.
- **OPEN:** still needs owner decision before commercial release; do not block the prototype unless technically unavoidable.

## Locked decisions

| Topic | Decision |
|---|---|
| Game identity | APOCARROT: Carrot Survival; 2D Action Roguelite / Arena Survival (Top-Down), wave-based combat, loot and weapon merging (Pivoted from 3D to 2D by Owner on 2026-10-13). |
| Player character | Rabbit Warrior, based on art references (2D Sprites). |
| Main enemy/boss | Zombie Carrot variants; King Carrot is the demo boss (2D Sprites). |
| Camera | 2D top-down / 3/4 angled camera (Orthographic) following the rabbit. Not side-scroller. |
| Player controls | Movement joystick + active melee attack button. |
| Ranged weapons | No ranged-attack button. Equipped ranged weapon auto-targets and attacks when its own cooldown finishes. |
| Active weapons | One equipped melee weapon and one equipped ranged weapon participate in combat at a time; other weapons are run-inventory reserve. |
| Wave structure | Three regular waves followed by a King Carrot boss wave. A wave should finish when its completion condition is met, not wait needlessly for a timer. |
| Intermission | Pause combat between waves for merge, shop and equipment changes. |
| Loot | Regular enemies can drop weapons; rewards are randomized with configured probability. Include a guided early duplicate drop/pity to teach merging. |
| Merge | Two weapons of same type and same level merge into one of that type at the next level. |
| Loss | Run-only gear is lost on defeat. Pre-owned account items, account level and recorded achievements persist. |
| Account progression | Account-level rabbit XP, level milestones and achievements are long-term and saved locally. |
| Art availability | Transitioning from 3D greybox primitives to 2D Sprites using agent-sprite-forge pipeline. |
| Scope | No multiplayer, monetization, ads, gacha, battle pass or complex live ops in P0. |
| Dimension Pivot | **LOCKED (2026-10-13):** Transitioned from 3D to 2D Top-Down (XY plane) with SpriteRenderer to solve asset production bottlenecks while preserving 100% core gameplay, merge systems, and UI progression. |

## Provisional values for first playable build

These are tuning defaults, not final balance. Store them in config assets rather than hard-coding them.

| Topic | Provisional default |
|---|---|
| Run length | Target 3–5 minutes. |
| Wave cadence | 3 normal waves + 1 boss wave; approximate timing from GDD, tune by playtest. |
| Inventory | 6 slots total as initial test value; change after the merge/shop flow is tested. |
| Starting loadout | One Carrot Blade + one Tomato Blaster, unless the GDD scene setup requires another safe initial item. |
| Weapon list | Melee: Carrot Blade, Carrot Axe. Ranged auto: Tomato Blaster, Grape Splitter. |
| Test art | Primitive/greybox player, enemies, projectiles and loot pickups with clear silhouettes/colors. |
| Landscape/mobile orientation | **PROVISIONAL only** based on the supplied 16:9 gameplay camera reference. Keep UI layout adaptable; do not spend time polishing orientation until owner confirms. |
| Drop odds | Use the provisional table in GDD; make it data-driven. Provide a tutorial/pity rule for one early duplicate pair so merge can be tested reliably. |
| Energy | Implement a switchable subsystem if feasible, but `energyEnabled = false` by default in dev/test. Failure energy cost can be verified once toggled. Do not implement real-money purchases. |
| Remaining run gold on win | Prototype default: spendable only during a run; convert leftover gold to account currency only if this rule is enabled/configured. Keep the policy configurable and mark for owner review. |

## Open owner decisions

1. Final target orientation/aspect ratio: landscape or portrait.
2. What happens to unspent run gold after a victory (convert to account gold, partially convert, or expire).
3. Exact account XP reward on loss and whether every win consumes energy (current direction: failure costs energy; success-cost rule is not explicitly locked).
4. Final energy cap/recharge duration and UI wording. Real-money energy purchases are outside P0.
5. Final inventory slot count, item prices, loot odds, weapon stats and wave timings after hands-on testing.

## Change-control rule

Before implementing any new feature, label it `P0 blocker`, `backlog`, or `out of scope`. If it is not a P0 blocker, add it to `docs/BACKLOG.md`. Any change to a LOCKED decision must be proposed to the owner with: reason, impact on existing features, acceptance criteria affected, and what will be delayed or removed in exchange. Do not silently mutate this log.
