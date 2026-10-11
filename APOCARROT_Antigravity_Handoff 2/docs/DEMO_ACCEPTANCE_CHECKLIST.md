# APOCARROT — Demo Acceptance Checklist

Mark each item PASS only after verifying it in Unity Play Mode or an explicitly documented automated test. Use NOT TESTED rather than guessing.

## Project and controls
- [ ] Project opens in the target Unity version with no new compile errors.
- [ ] Keyboard input can move the rabbit in Editor for quick QA.
- [ ] Touch joystick moves the rabbit on the target screen layout.
- [ ] Angled top-down camera follows smoothly and keeps the player readable.
- [ ] Arena boundary prevents leaving the play area.

## Combat
- [ ] Melee attack button triggers attack on press/hold with sensible cadence.
- [ ] Carrot Blade and Carrot Axe have distinct configurable behavior.
- [ ] Player can take damage, become defeated and end the run once.
- [ ] Basic, Fast and Elite enemies have distinct movement/health/attack profiles.
- [ ] Equipped ranged weapon auto-targets and attacks on cooldown with no ranged button.
- [ ] Ranged weapon does not fire at invalid/out-of-range targets.
- [ ] King Carrot has readable attack telegraphs and can be defeated.

## Loot and gear
- [ ] Common enemies can drop weapons as well as other configured loot.
- [ ] Loot odds are data-configured and can yield no drop.
- [ ] A guided/test path can generate two identical weapon items to test merge.
- [ ] Collected run gear is stored in run inventory, not permanent account inventory.
- [ ] Two weapons of matching type and level merge to the next level.
- [ ] Mismatching weapons cannot merge.
- [ ] Full inventory never silently overwrites an item.
- [ ] Shop purchase deducts run gold correctly; insufficient gold is handled.

## Waves and flow
- [ ] Three normal waves lead to a King Carrot boss wave.
- [ ] Wave completes after its actual objective, without pointless timer wait.
- [ ] Intermission pauses combat and allows merge/shop/equipment changes.
- [ ] Continue from intermission starts the next wave once.
- [ ] Boss death triggers exactly one win state.
- [ ] Player death triggers exactly one loss state.

## Account / save / energy
- [ ] Results screen distinguishes win/loss and displays basic statistics/rewards.
- [ ] Account XP/level or milestone updates exactly once per run.
- [ ] Progress survives restart/load.
- [ ] Run-only gear is cleared on defeat.
- [ ] Account-owned equipment and recorded progress survive defeat.
- [ ] Missing/corrupt save falls back safely or gives a clear message.
- [ ] Energy can be disabled by a development setting; no real-money purchase exists in P0.

## Run-time target
- [ ] Target normal run is approximately 3–5 minutes or the deviation is documented.
- [ ] No P0 flow requires manual inspector edits while playing.
- [ ] No known P0 issue is hidden by claiming the demo is complete.
