#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Apocarrot.Arena;
using Apocarrot.Inventory;
using Apocarrot.Progression;

namespace Apocarrot.EditorTools
{
    /// <summary>
    /// Công cụ hỗ trợ QA và kiểm thử toàn diện vòng lặp game (Gate 8).
    /// Hỗ trợ kiểm thử các tình huống: save mới, nhảy đến boss, nạp vàng test shop, ghép đồ, v.v.
    /// </summary>
    public static class DemoQATools
    {
        [MenuItem("Apocarrot/QA/1. Xóa Dữ Liệu Save (Fresh Install Test)")]
        public static void ResetSaveData()
        {
            SaveService.ResetSave();
            if (AccountProgression.Instance != null)
            {
                AccountProgression.Instance.ResetAllProgression();
            }
            Debug.Log("[QA] Đã xóa toàn bộ save data! Kiểm thử trạng thái cài đặt mới thành công.");
        }

        [MenuItem("Apocarrot/QA/2. Nạp 200 Vàng Run (Test Shop & Insufficient Gold)")]
        public static void GrantRunGold()
        {
            if (RunInventory.Instance != null)
            {
                RunInventory.Instance.AddGold(200);
                Debug.Log("[QA] Đã cộng 200 Vàng trong Run để kiểm thử Cửa Hàng.");
            }
            else
            {
                Debug.LogWarning("[QA] Hãy bấm Play trước khi nạp vàng test!");
            }
        }

        [MenuItem("Apocarrot/QA/3. Cấp Cặp Vũ Khí Trùng Nhau (Test Duplicate Merge)")]
        public static void GrantDuplicateWeapons()
        {
            if (RunInventory.Instance != null)
            {
                RunInventory.Instance.TryAddWeapon(WeaponItem.CreateCarrotBlade(1));
                RunInventory.Instance.TryAddWeapon(WeaponItem.CreateCarrotBlade(1));
                Debug.Log("[QA] Đã thêm 2 Carrot Blade Lv.1 vào ba lô để test ghép đồ!");
            }
            else
            {
                Debug.LogWarning("[QA] Hãy bấm Play trước khi cấp vũ khí!");
            }
        }

        [MenuItem("Apocarrot/QA/4. Nhảy Ngay Đến Wave 4 (King Carrot Boss)")]
        public static void JumpToBossWave()
        {
            if (WaveDirector.Instance != null)
            {
                // Dọn dẹp quái cũ và bắt đầu Wave 4 (index 3)
                foreach (var z in Object.FindObjectsByType<Enemies.ZombieCarrotAI>(FindObjectsInactive.Exclude))
                {
                    Object.Destroy(z.gameObject);
                }
                WaveDirector.Instance.StartWave(3);
                Debug.Log("[QA] Đã chuyển ngay lập tức đến Wave 4: King Carrot Boss!");
            }
            else
            {
                Debug.LogWarning("[QA] Hãy bấm Play trước khi nhảy wave!");
            }
        }

        [MenuItem("Apocarrot/QA/5. Đổi Trạng Thái Năng Lượng (Bật/Tắt Test Mode)")]
        public static void ToggleEnergyMode()
        {
            if (AccountProgression.Instance != null)
            {
                bool current = AccountProgression.Instance.IsUnlimitedEnergy;
                AccountProgression.Instance.SetUnlimitedEnergy(!current);
                Debug.Log($"[QA] Đã chuyển Năng Lượng Vô Hạn thành: {!current}");
            }
            else
            {
                Debug.LogWarning("[QA] Hãy bấm Play trước khi đổi trạng thái năng lượng!");
            }
        }

        [MenuItem("Apocarrot/QA/6. Cấp 1000 Vàng + 100 Gems + 50 Scrap (Meta Resources)")]
        public static void GrantMetaResources()
        {
            if (AccountProgression.Instance != null)
            {
                AccountProgression.Instance.AddGold(1000);
                AccountProgression.Instance.AddGems(100);
                AccountProgression.Instance.AddScrap(50);
                Debug.Log("[QA] Đã cấp +1000 Vàng, +100 Gems, +50 Scrap cho tài khoản!");
            }
            else
            {
                Debug.LogWarning("[QA] Hãy bấm Play trước khi cấp tài nguyên!");
            }
        }

        [MenuItem("Apocarrot/QA/7. Mở Thử 1 Rương Boss (Test Boss Chest Unbox)")]
        public static void TestOpenBossChest()
        {
            var reward = Loot.ChestService.OpenChest(Loot.ChestType.Boss);
            if (AccountProgression.Instance != null && reward != null)
            {
                AccountProgression.Instance.AddEquipment(reward.Item);
                AccountProgression.Instance.AddGold(reward.BonusGold);
                AccountProgression.Instance.AddScrap(reward.BonusScrap);
                UI.HomeHubUI.Instance?.ShowChestRewardPopup(reward);
                Debug.Log($"[QA] Mở Rương Boss thành công: {reward.Item.DisplayName} ({reward.Item.GetRarityName()})!");
            }
        }

        [MenuItem("Apocarrot/QA/8. Chuyển Về Sảnh Chính (Return to Home Hub)")]
        public static void ReturnToHomeHub()
        {
            if (WaveDirector.Instance != null)
            {
                WaveDirector.Instance.ReturnToHome();
                Debug.Log("[QA] Đã chuyển về Sảnh Chính (Home Hub)!");
            }
        }
    }
}
#endif
