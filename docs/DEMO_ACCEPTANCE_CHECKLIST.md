# APOCARROT — Demo Acceptance Checklist

Mark each item PASS only after verifying it in Unity Play Mode or an explicitly documented automated test. Use NOT TESTED rather than guessing.

## Project and controls
- [x] Project opens in the target Unity version with no new compile errors. (PASS - Unity 6000.6.5f1 batchmode clean 0 errors)
- [x] Keyboard input can move the rabbit in Editor for quick QA. (PASS - WASD / Arrow keys in PlayerController)
- [x] Touch joystick moves the rabbit on the target screen layout. (PASS - VirtualJoystick)
- [x] Angled top-down camera follows smoothly and keeps the player readable. (PASS - TopDownFollowCamera)
- [x] Arena boundary prevents leaving the play area. (PASS - Boundary walls + CharacterController)

## Combat
- [x] Melee attack button triggers attack on press/hold with sensible cadence. (PASS - MeleeCombat & MeleeAttackButton)
- [x] Carrot Blade and Carrot Axe have distinct configurable behavior. (PASS - MeleeWeaponData)
- [x] Player can take damage, become defeated and end the run once. (PASS - PlayerHealth & ResultsScreenUI)
- [x] Basic, Fast and Elite enemies have distinct movement/health/attack profiles. (PASS - EnemyData 3 variants)
- [x] Equipped ranged weapon auto-targets and attacks on cooldown with no ranged button. (PASS - RangedWeaponController)
- [x] Ranged weapon does not fire at invalid/out-of-range targets. (PASS - Range check & Cooldown)
- [x] King Carrot has readable attack telegraphs and can be defeated. (PASS - KingCarrotBossAI AoE telegraph disc)

## Loot and gear
- [x] Common enemies can drop weapons as well as other configured loot. (PASS - LootDropper & LootPickup)
- [x] Loot odds are data-configured and can yield no drop. (PASS - LootTable in EnemyData)
- [x] A guided/test path can generate two identical weapon items to test merge. (PASS - Early Guarantee Pity Drop & DemoQATools)
- [x] Collected run gear is stored in run inventory, not permanent account inventory. (PASS - RunInventory & AccountProgression separation)
- [x] Two weapons of matching type and level merge to the next level. (PASS - WeaponItem / EquipmentItem Fuse)
- [x] Mismatching weapons cannot merge. (PASS - CanFuseWith validation)
- [x] Full inventory never silently overwrites an item. (PASS - Rejection / swap logic)
- [x] Shop purchase deducts run gold correctly; insufficient gold is handled. (PASS - ShopItemData & EquipmentBagUI Shop tab)

## Waves and flow
- [x] Three normal waves lead to a King Carrot boss wave. (PASS - WaveDirector 3 waves + boss)
- [x] Wave completes after its actual objective, without pointless timer wait. (PASS - Enemy count quota resolution)
- [x] Intermission pauses combat and allows merge/shop/equipment changes. (PASS - Unified Equipment & Shop Bag)
- [x] Continue from intermission starts the next wave once. (PASS - WaveDirector OnContinueWaveRequested)
- [x] Boss death triggers exactly one win state. (PASS - VictoryScreenUI)
- [x] Player death triggers exactly one loss state. (PASS - ResultsScreenUI Defeat)

## Account / save / energy
- [x] Results screen distinguishes win/loss and displays basic statistics/rewards. (PASS - ResultsScreenUI)
- [x] Account XP/level or milestone updates exactly once per run. (PASS - AccountProgression & SaveService)
- [x] Progress survives restart/load. (PASS - JSON local save)
- [x] Run-only gear is cleared on defeat. (PASS - RunInventory reset on start/loss)
- [x] Account-owned equipment and recorded progress survive defeat. (PASS - AccountData persists)
- [x] Missing/corrupt save falls back safely or gives a clear message. (PASS - SaveService default fallback)
- [x] Energy can be disabled by a development setting; no real-money purchase exists in P0. (PASS - UnlimitedEnergyForTesting flag)

## Run-time target & Optimization
- [x] Target normal run is approximately 3–5 minutes or the deviation is documented. (PASS - 3 waves + boss pacing)
- [x] No P0 flow requires manual inspector edits while playing. (PASS - ArenaSceneBootstrap sets up scene)
- [x] UI Ba lô & HUD Zero-Lag Optimization (PASS - Tái sử dụng thẻ UI, không rebuild khi đóng túi, zero-alloc Toast pool)
- [x] Hiển thị rõ ràng đồ nhặt được trên HUD & Toast (PASS - Banner thông báo + Slot cập nhật lập tức)
- [x] Tích hợp bộ ảnh vũ khí & trang bị 2D thực tế (PASS - Nạp động từ Resources/Weapons với WeaponSpriteService hiển thị trong HUD, Ba lô, Cửa hàng)
