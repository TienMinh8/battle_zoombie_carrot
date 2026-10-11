using System;
using System.Collections.Generic;
using UnityEngine;
using Apocarrot.Combat;
using Apocarrot.Gameplay;

namespace Apocarrot.Inventory
{
    /// <summary>
    /// Quản lý túi đồ và kinh tế trong lượt chơi (Run-only Inventory).
    /// Tuân thủ quy tắc GDD & Roadmap:
    /// - Tách biệt túi đồ trong lượt chơi với tài sản vĩnh viễn của tài khoản.
    /// - Khi người chơi chết, Run Inventory bị xóa sạch.
    /// - Chứa: Vàng trong run, 1 vũ khí cận chiến đang trang bị, 1 vũ khí tầm xa đang trang bị,
    ///   và 4 ô ba lô dự trữ (tổng cộng 6 ô đồ).
    /// - Hỗ trợ cơ chế ghép vũ khí: 2 vũ khí cùng loại & cùng cấp ghép thành 1 vũ khí cấp kế tiếp (Gate 5).
    /// </summary>
    public class RunInventory : MonoBehaviour
    {
        public static RunInventory Instance { get; private set; }

        public const int MAX_BACKPACK_SLOTS = 4;

        [Header("Run Currency")]
        [SerializeField] private int runGold = 0;

        [Header("Equipped Weapons")]
        [SerializeField] private WeaponItem equippedMelee;
        [SerializeField] private WeaponItem equippedRanged;

        [Header("Backpack Storage")]
        [SerializeField] private List<WeaponItem> backpack = new List<WeaponItem>();

        public event Action<int> OnGoldChanged;
        public event Action OnInventoryChanged;

        public int RunGold => runGold;
        public WeaponItem EquippedMelee => equippedMelee;
        public WeaponItem EquippedRanged => equippedRanged;
        public IReadOnlyList<WeaponItem> Backpack => backpack;
        public bool IsBackpackFull => backpack.Count >= MAX_BACKPACK_SLOTS;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Khởi tạo trang bị mặc định ban đầu của Rabbit Warrior
            if (equippedMelee == null)
            {
                equippedMelee = WeaponItem.CreateCarrotBlade(1);
            }
            if (equippedRanged == null)
            {
                equippedRanged = WeaponItem.CreateTomatoBlaster(1);
            }
        }

        private void Start()
        {
            // Lắng nghe khi nhân vật chết -> dọn sạch Run Inventory
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied += HandlePlayerDied;
            }

            SyncActiveWeaponsWithCombat();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Đồng bộ chỉ số vũ khí đang trang bị vào các component chiến đấu của Rabbit Warrior
        /// </summary>
        public void SyncActiveWeaponsWithCombat()
        {
            if (equippedMelee != null)
            {
                MeleeCombat melee = GetComponent<MeleeCombat>() ?? FindFirstObjectByType<MeleeCombat>();
                if (melee != null)
                {
                    melee.EquipWeapon(equippedMelee.ToMeleeData());
                }
            }

            if (equippedRanged != null)
            {
                RangedWeaponController ranged = GetComponent<RangedWeaponController>() ?? FindFirstObjectByType<RangedWeaponController>();
                if (ranged != null)
                {
                    ranged.EquipRangedWeapon(equippedRanged.ToRangedData());
                }
            }
        }

        /// <summary>
        /// Cộng vàng trong trận đấu
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            runGold += amount;
            OnGoldChanged?.Invoke(runGold);
            Debug.Log($"[RunInventory] +{amount} Vàng! Tổng hiện có: {runGold}");
        }

        /// <summary>
        /// Tiêu vàng (dùng cho shop ở Gate 5)
        /// </summary>
        public bool TrySpendGold(int amount)
        {
            if (amount <= 0 || runGold < amount) return false;
            runGold -= amount;
            OnGoldChanged?.Invoke(runGold);
            return true;
        }

        /// <summary>
        /// Thêm một vũ khí nhặt được vào ba lô dự trữ
        /// </summary>
        public bool TryAddWeapon(WeaponItem weapon)
        {
            if (weapon == null) return false;

            if (backpack.Count >= MAX_BACKPACK_SLOTS)
            {
                Debug.LogWarning($"[RunInventory] Ba lô đã đầy ({MAX_BACKPACK_SLOTS}/{MAX_BACKPACK_SLOTS})! Không thể nhặt thêm {weapon.DisplayName}.");
                return false;
            }

            backpack.Add(weapon);
            OnInventoryChanged?.Invoke();
            Debug.Log($"[RunInventory] Đã thêm {weapon.DisplayName} (Lv.{weapon.Level}) vào ba lô! Ô còn trống: {MAX_BACKPACK_SLOTS - backpack.Count}");
            return true;
        }

        /// <summary>
        /// Đổi vị trí món đồ trong ba lô với vũ khí đang trang bị tương ứng (Cận chiến hoặc Tầm xa)
        /// </summary>
        public bool SwapWithEquipped(int backpackIndex)
        {
            if (backpackIndex < 0 || backpackIndex >= backpack.Count) return false;

            WeaponItem bagItem = backpack[backpackIndex];
            if (bagItem.WeaponType == Data.WeaponType.Melee)
            {
                WeaponItem temp = equippedMelee;
                equippedMelee = bagItem;
                backpack[backpackIndex] = temp;
            }
            else
            {
                WeaponItem temp = equippedRanged;
                equippedRanged = bagItem;
                backpack[backpackIndex] = temp;
            }

            SyncActiveWeaponsWithCombat();
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Ghép 2 vũ khí bên trong Ba Lô (cùng loại, cùng cấp) -> 1 vũ khí cấp kế tiếp
        /// </summary>
        public bool TryMergeBackpackItems(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= backpack.Count) return false;
            if (indexB < 0 || indexB >= backpack.Count) return false;
            if (indexA == indexB) return false;

            WeaponItem itemA = backpack[indexA];
            WeaponItem itemB = backpack[indexB];

            if (!itemA.CanMergeWith(itemB)) return false;

            WeaponItem upgraded = itemA.GetMergedVersion();

            // Xóa món ở chỉ số lớn hơn trước để tránh sai lệch index
            int higherIdx = Mathf.Max(indexA, indexB);
            int lowerIdx = Mathf.Min(indexA, indexB);

            backpack.RemoveAt(higherIdx);
            backpack[lowerIdx] = upgraded;

            Progression.AccountProgression.Instance?.RecordWeaponMerged(upgraded.Level);
            OnInventoryChanged?.Invoke();
            Debug.Log($"[RunInventory] Ghép thành công: {upgraded.DisplayName} đã nâng lên Cấp {upgraded.Level}!");
            return true;
        }

        /// <summary>
        /// Ghép món đồ trong Ba Lô với vũ khí đang trang bị (nếu cùng loại và cùng cấp)
        /// </summary>
        public bool TryMergeWithEquipped(int backpackIndex)
        {
            if (backpackIndex < 0 || backpackIndex >= backpack.Count) return false;

            WeaponItem bagItem = backpack[backpackIndex];

            if (bagItem.WeaponType == Data.WeaponType.Melee && equippedMelee != null && equippedMelee.CanMergeWith(bagItem))
            {
                equippedMelee = equippedMelee.GetMergedVersion();
                backpack.RemoveAt(backpackIndex);
                SyncActiveWeaponsWithCombat();
                Progression.AccountProgression.Instance?.RecordWeaponMerged(equippedMelee.Level);
                OnInventoryChanged?.Invoke();
                Debug.Log($"[RunInventory] Ghép thành công: Trang bị cận chiến {equippedMelee.DisplayName} đã lên Cấp {equippedMelee.Level}!");
                return true;
            }
            else if (bagItem.WeaponType == Data.WeaponType.Ranged && equippedRanged != null && equippedRanged.CanMergeWith(bagItem))
            {
                equippedRanged = equippedRanged.GetMergedVersion();
                backpack.RemoveAt(backpackIndex);
                SyncActiveWeaponsWithCombat();
                Progression.AccountProgression.Instance?.RecordWeaponMerged(equippedRanged.Level);
                OnInventoryChanged?.Invoke();
                Debug.Log($"[RunInventory] Ghép thành công: Trang bị tầm xa {equippedRanged.DisplayName} đã lên Cấp {equippedRanged.Level}!");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tự động tìm và ghép cặp vũ khí đầu tiên thỏa mãn điều kiện
        /// </summary>
        public bool AutoMergeAny()
        {
            // 1. Thử ghép trong ba lô với vũ khí đang trang bị
            for (int i = 0; i < backpack.Count; i++)
            {
                if (TryMergeWithEquipped(i)) return true;
            }

            // 2. Thử ghép giữa các món trong ba lô
            for (int i = 0; i < backpack.Count; i++)
            {
                for (int j = i + 1; j < backpack.Count; j++)
                {
                    if (TryMergeBackpackItems(i, j)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra xem hiện tại có thể thực hiện ghép bất kỳ cặp vũ khí nào không
        /// </summary>
        public bool CanMergeAny()
        {
            // Kiểm tra ba lô với vũ khí đang trang bị
            for (int i = 0; i < backpack.Count; i++)
            {
                if (backpack[i].CanMergeWith(equippedMelee) || backpack[i].CanMergeWith(equippedRanged))
                    return true;
            }

            // Kiểm tra giữa các món trong ba lô
            for (int i = 0; i < backpack.Count; i++)
            {
                for (int j = i + 1; j < backpack.Count; j++)
                {
                    if (backpack[i].CanMergeWith(backpack[j]))
                        return true;
                }
            }

            return false;
        }

        public void DiscardBackpackItem(int index)
        {
            if (index >= 0 && index < backpack.Count)
            {
                WeaponItem removed = backpack[index];
                backpack.RemoveAt(index);
                OnInventoryChanged?.Invoke();
                Debug.Log($"[RunInventory] Đã vứt bỏ {removed.DisplayName} Lv.{removed.Level}.");
            }
        }

        public void EquipMelee(WeaponItem melee)
        {
            equippedMelee = melee;
            SyncActiveWeaponsWithCombat();
            OnInventoryChanged?.Invoke();
        }

        public void EquipRanged(WeaponItem ranged)
        {
            equippedRanged = ranged;
            SyncActiveWeaponsWithCombat();
            OnInventoryChanged?.Invoke();
        }

        private void HandlePlayerDied()
        {
            ClearRunData();
        }

        public void ClearRunData()
        {
            runGold = 0;
            backpack.Clear();
            equippedMelee = WeaponItem.CreateCarrotBlade(1);
            equippedRanged = WeaponItem.CreateTomatoBlaster(1);
            SyncActiveWeaponsWithCombat();
            OnGoldChanged?.Invoke(runGold);
            OnInventoryChanged?.Invoke();
            Debug.Log("[RunInventory] Run kết thúc: Đã xóa toàn bộ tài nguyên tạm thời của lượt chơi.");
        }

        public void ResetInventory() => ClearRunData();
    }
}
