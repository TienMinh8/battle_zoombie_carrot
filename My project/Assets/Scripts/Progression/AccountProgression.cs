using System;
using System.Collections.Generic;
using UnityEngine;
using Apocarrot.Data;
using Apocarrot.Loot;
using Apocarrot.Inventory;

namespace Apocarrot.Progression
{
    /// <summary>
    /// Quản lý tiến trình tài khoản Thỏ, XP, cấp độ, thành tựu và lưu trữ vĩnh viễn (Gate 7).
    /// </summary>
    public class AccountProgression : MonoBehaviour
    {
        public static AccountProgression Instance { get; private set; }

        public event Action OnAccountDataChanged;
        public event Action<int> OnRabbitLeveledUp; // (newLevel)
        public event Action<AchievementProgress> OnAchievementUnlocked;

        [SerializeField] private AccountData currentData;

        public AccountData Data => currentData;
        public int RabbitLevel => currentData != null ? currentData.RabbitLevel : 1;
        public int CurrentXP => currentData != null ? currentData.CurrentXP : 0;
        public int PermanentGold => currentData != null ? currentData.PermanentGold : 0;
        public int Energy => currentData != null ? currentData.CurrentEnergy : 5;
        public bool IsUnlimitedEnergy => currentData != null && currentData.UnlimitedEnergyForTesting;

        private int pendingRunXP = 0;
        private bool isRunProcessed = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadAccount();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadAccount()
        {
            currentData = SaveService.Load();
            if (currentData == null)
            {
                currentData = AccountData.CreateDefault();
            }

            SanitizeAccountData(currentData);
            SaveService.Save(currentData);

            pendingRunXP = 0;
            isRunProcessed = false;
            OnAccountDataChanged?.Invoke();
        }

        private void Start()
        {
            if (currentData != null && currentData.EquippedWeapon != null)
            {
                SyncWeaponToRunInventory(currentData.EquippedWeapon);
            }
        }

        private void SanitizeAccountData(AccountData data)
        {
            if (data == null) return;

            // Đảm bảo 6 ô trang bị trên người luôn hợp lệ
            if (data.EquippedWeapon == null || string.IsNullOrEmpty(data.EquippedWeapon.TemplateId) || string.IsNullOrEmpty(data.EquippedWeapon.DisplayName))
            {
                data.EquippedWeapon = EquipmentItem.CreateCarrotBlade(EquipmentRarity.Rare, 1);
            }
            if (data.EquippedNecklace == null || string.IsNullOrEmpty(data.EquippedNecklace.TemplateId) || string.IsNullOrEmpty(data.EquippedNecklace.DisplayName))
            {
                data.EquippedNecklace = EquipmentItem.CreateCarrotPendant(EquipmentRarity.Rare, 1);
            }
            if (data.EquippedGloves == null || string.IsNullOrEmpty(data.EquippedGloves.TemplateId) || string.IsNullOrEmpty(data.EquippedGloves.DisplayName))
            {
                data.EquippedGloves = EquipmentItem.CreateWarriorGloves(EquipmentRarity.Rare, 1);
            }
            if (data.EquippedArmor == null || string.IsNullOrEmpty(data.EquippedArmor.TemplateId) || string.IsNullOrEmpty(data.EquippedArmor.DisplayName))
            {
                data.EquippedArmor = EquipmentItem.CreateRabbitVest(EquipmentRarity.Rare, 1);
            }
            if (data.EquippedBelt == null || string.IsNullOrEmpty(data.EquippedBelt.TemplateId) || string.IsNullOrEmpty(data.EquippedBelt.DisplayName))
            {
                data.EquippedBelt = EquipmentItem.CreateLeatherBelt(EquipmentRarity.Rare, 1);
            }
            if (data.EquippedBoots == null || string.IsNullOrEmpty(data.EquippedBoots.TemplateId) || string.IsNullOrEmpty(data.EquippedBoots.DisplayName))
            {
                data.EquippedBoots = EquipmentItem.CreateSwiftBoots(EquipmentRarity.Rare, 1);
            }

            if (data.OwnedEquipments == null)
            {
                data.OwnedEquipments = new List<EquipmentItem>();
            }

            // Loại bỏ hoàn toàn các trang bị lỗi rỗng templateId hoặc displayName
            data.OwnedEquipments.RemoveAll(it => it == null || string.IsNullOrEmpty(it.TemplateId) || string.IsNullOrEmpty(it.DisplayName));

            // Loại bỏ các UID trùng lặp bất thường
            HashSet<string> seenUids = new HashSet<string>();
            List<EquipmentItem> cleanList = new List<EquipmentItem>();
            foreach (var it in data.OwnedEquipments)
            {
                if (!string.IsNullOrEmpty(it.Uid) && seenUids.Add(it.Uid))
                {
                    cleanList.Add(it);
                }
            }
            data.OwnedEquipments = cleanList;

            // Nếu túi trống rỗng, khởi tạo bộ mẫu mặc định 12 món
            if (data.OwnedEquipments.Count == 0)
            {
                data.InitializeDefaultEquipment();
            }
            // Giới hạn an toàn túi đồ tối đa 40 món để tránh tình trạng lag UI khi tạo quá nhiều GameObject
            else if (data.OwnedEquipments.Count > 40)
            {
                data.OwnedEquipments.RemoveRange(40, data.OwnedEquipments.Count - 40);
            }
        }

        public void SaveAccount()
        {
            if (currentData != null)
            {
                SaveService.Save(currentData);
                OnAccountDataChanged?.Invoke();
            }
        }

        public void StartNewRun()
        {
            pendingRunXP = 0;
            isRunProcessed = false;
        }

        /// <summary>
        /// Ghi nhận khi một quái bị hạ gục trong trận
        /// </summary>
        public void RecordEnemyKilled(EnemyVariantType variant)
        {
            if (currentData == null) return;

            currentData.TotalEnemiesKilled++;

            int xpGain = 10;
            switch (variant)
            {
                case EnemyVariantType.Fast: xpGain = 15; break;
                case EnemyVariantType.Elite: xpGain = 35; break;
                case EnemyVariantType.Basic: default: xpGain = 10; break;
            }

            pendingRunXP += xpGain;

            UpdateAchievementProgress("first_kill", 1);
            UpdateAchievementProgress("carrot_slayer_20", 1);
        }

        public void RecordWaveCleared(int waveNumber)
        {
            if (waveNumber == 1)
            {
                UpdateAchievementProgress("wave_1_clear", 1);
            }
        }

        public void RecordWeaponMerged(int newLevel)
        {
            if (newLevel >= 2)
            {
                UpdateAchievementProgress("first_merge", 1);
            }
        }

        public void RecordBossDefeated()
        {
            if (currentData == null) return;

            currentData.TotalBossesDefeated++;
            pendingRunXP += 200; // Thưởng 200 XP khi hạ King Carrot!
            UpdateAchievementProgress("king_carrot_slayer", 1);
        }

        /// <summary>
        /// Hoàn tất lượt chơi và cập nhật phần thưởng tài khoản (chỉ gọi đúng 1 lần khi kết thúc màn).
        /// </summary>
        public void FinalizeRun(bool won, int runKills, int runGold, out int earnedXP, out int convertedGold, out List<string> newAchievements)
        {
            newAchievements = new List<string>();

            if (isRunProcessed || currentData == null)
            {
                earnedXP = 0;
                convertedGold = 0;
                return;
            }

            isRunProcessed = true;
            currentData.TotalRunsPlayed++;

            if (won)
            {
                currentData.TotalRunsWon++;
                pendingRunXP += 150; // Thưởng bonus thắng màn
                // Quy tắc kinh tế: Chuyển 50% số vàng thu được trong run thành Vàng Tài Khoản vĩnh viễn
                convertedGold = Mathf.FloorToInt(runGold * 0.5f);
                currentData.PermanentGold += convertedGold;
            }
            else
            {
                convertedGold = 0;
                // Nếu chế độ Năng Lượng thật được bật -> Trừ 1 năng lượng khi thua
                if (!currentData.UnlimitedEnergyForTesting)
                {
                    currentData.CurrentEnergy = Mathf.Max(0, currentData.CurrentEnergy - 1);
                }
            }

            earnedXP = pendingRunXP;
            AddXP(earnedXP);

            SaveAccount();
        }

        public void AddXP(int amount)
        {
            if (currentData == null || amount <= 0) return;

            currentData.CurrentXP += amount;
            int xpRequired = AccountData.GetXPRequiredForLevel(currentData.RabbitLevel);

            while (currentData.CurrentXP >= xpRequired)
            {
                currentData.CurrentXP -= xpRequired;
                currentData.RabbitLevel++;
                currentData.PermanentGold += 50; // Thưởng 50 vàng tài khoản mỗi lần lên cấp Thỏ!

                LootToastManager.ShowWorldToast(
                    Vector3.up * 3f,
                    $"🌟 THỎ ĐẠT CẤP {currentData.RabbitLevel}! +50 VÀNG TK!",
                    new Color(1f, 0.85f, 0.2f)
                );

                OnRabbitLeveledUp?.Invoke(currentData.RabbitLevel);
                xpRequired = AccountData.GetXPRequiredForLevel(currentData.RabbitLevel);
            }

            OnAccountDataChanged?.Invoke();
        }

        private void UpdateAchievementProgress(string achievementId, int amount)
        {
            if (currentData == null || currentData.Achievements == null) return;

            var ach = currentData.Achievements.Find(a => a.Id == achievementId);
            if (ach == null || ach.IsUnlocked) return;

            ach.CurrentProgress = Mathf.Min(ach.TargetProgress, ach.CurrentProgress + amount);
            if (ach.CurrentProgress >= ach.TargetProgress)
            {
                ach.IsUnlocked = true;
                currentData.PermanentGold += ach.RewardAccountGold;
                AddXP(ach.RewardAccountXP);

                LootToastManager.ShowWorldToast(
                    Vector3.up * 2.5f,
                    $"🏆 THÀNH TỰU: {ach.Title}! (+{ach.RewardAccountGold}g, +{ach.RewardAccountXP} XP)",
                    new Color(0.2f, 0.95f, 0.4f)
                );

                OnAchievementUnlocked?.Invoke(ach);
            }
        }

        // ========================== EQUIPMENT & FUSE MANAGEMENT ==========================
        public void EquipItem(EquipmentItem item)
        {
            if (currentData == null || item == null) return;

            EquipmentItem currentEquipped = currentData.GetEquippedItem(item.SlotType);
            currentData.OwnedEquipments.Remove(item);

            if (currentEquipped != null)
            {
                currentData.OwnedEquipments.Add(currentEquipped);
            }

            currentData.SetEquippedItem(item.SlotType, item);

            // Đồng bộ trực tiếp vũ khí vào RunInventory và các component chiến đấu của Player
            if (item.SlotType == EquipmentSlotType.Weapon)
            {
                SyncWeaponToRunInventory(item);
            }

            SaveAccount();
            Debug.Log($"[AccountProgression] Đã trang bị: {item.DisplayName} ({item.GetSlotName()})");
        }

        public void SyncWeaponToRunInventory(EquipmentItem item)
        {
            if (item == null || RunInventory.Instance == null) return;

            string id = item.TemplateId != null ? item.TemplateId.ToLower() : "";
            if (id.Contains("tomato"))
            {
                RunInventory.Instance.EquipRanged(WeaponItem.CreateTomatoBlaster(item.Level));
            }
            else if (id.Contains("grape"))
            {
                RunInventory.Instance.EquipRanged(new WeaponItem("grape_splitter", "Đại Bác Nho", WeaponType.Ranged, item.Level, new Color(0.65f, 0.15f, 0.95f)));
            }
            else if (id.Contains("axe"))
            {
                RunInventory.Instance.EquipMelee(new WeaponItem("carrot_axe", "Rìu Cà Rốt", WeaponType.Melee, item.Level, new Color(1.0f, 0.45f, 0.1f)));
            }
            else
            {
                RunInventory.Instance.EquipMelee(WeaponItem.CreateCarrotBlade(item.Level));
            }
        }

        public void UnequipItem(EquipmentSlotType slot)
        {
            if (currentData == null) return;
            EquipmentItem currentEquipped = currentData.GetEquippedItem(slot);
            if (currentEquipped != null)
            {
                currentData.SetEquippedItem(slot, null);
                currentData.OwnedEquipments.Add(currentEquipped);
                SaveAccount();
                Debug.Log($"[AccountProgression] Đã tháo trang bị: {currentEquipped.DisplayName}");
            }
        }

        public bool FuseItems(EquipmentItem itemA, EquipmentItem itemB, out EquipmentItem fusedResult)
        {
            fusedResult = null;
            if (currentData == null || itemA == null || itemB == null) return false;
            if (!itemA.CanFuseWith(itemB)) return false;

            fusedResult = itemA.CreateFusedVersion(itemB);
            currentData.OwnedEquipments.Remove(itemA);
            currentData.OwnedEquipments.Remove(itemB);
            currentData.OwnedEquipments.Add(fusedResult);

            UpdateAchievementProgress("first_merge", 1);
            SaveAccount();
            Apocarrot.Audio.AudioManager.Instance?.PlayMergeSuccess();
            Debug.Log($"[AccountProgression] Ghép thành công: {fusedResult.DisplayName} phẩm {fusedResult.GetRarityName()}!");
            return true;
        }

        public int AutoFuseAll()
        {
            if (currentData == null || currentData.OwnedEquipments == null) return 0;
            int fuseCount = 0;
            bool foundPair = true;

            while (foundPair)
            {
                foundPair = false;
                for (int i = 0; i < currentData.OwnedEquipments.Count; i++)
                {
                    var a = currentData.OwnedEquipments[i];
                    for (int j = i + 1; j < currentData.OwnedEquipments.Count; j++)
                    {
                        var b = currentData.OwnedEquipments[j];
                        if (a.CanFuseWith(b))
                        {
                            var fused = a.CreateFusedVersion(b);
                            currentData.OwnedEquipments.RemoveAt(j);
                            currentData.OwnedEquipments.RemoveAt(i);
                            currentData.OwnedEquipments.Add(fused);
                            fuseCount++;
                            foundPair = true;
                            break;
                        }
                    }
                    if (foundPair) break;
                }
            }

            if (fuseCount > 0)
            {
                UpdateAchievementProgress("first_merge", fuseCount);
                SaveAccount();
            }
            return fuseCount;
        }

        public void AddEquipment(EquipmentItem item)
        {
            if (currentData == null || item == null) return;
            if (currentData.OwnedEquipments == null) currentData.OwnedEquipments = new List<EquipmentItem>();
            currentData.OwnedEquipments.Insert(0, item); // Đưa lên đầu túi đồ để người chơi thấy ngay lập tức!
            SaveAccount();
            Debug.Log($"[AccountProgression] Đã thêm trang bị mới vào Balo: {item.DisplayName} ({item.GetRarityName()})");
        }

        public void SetUnlimitedEnergy(bool unlimited)
        {
            if (currentData != null)
            {
                currentData.UnlimitedEnergyForTesting = unlimited;
                SaveAccount();
            }
        }

        public void ResetAllProgression()
        {
            SaveService.ResetSave();
            currentData = AccountData.CreateDefault();
            SaveService.Save(currentData);
            pendingRunXP = 0;
            isRunProcessed = false;
            OnAccountDataChanged?.Invoke();
            Debug.Log("[AccountProgression] Đã thiết lập lại toàn bộ tiến trình tài khoản về ban đầu.");
        }
    }
}
