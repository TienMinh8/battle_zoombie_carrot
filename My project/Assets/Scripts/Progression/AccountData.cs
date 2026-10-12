using System;
using System.Collections.Generic;
using UnityEngine;
using Apocarrot.Inventory;

namespace Apocarrot.Progression
{
    [Serializable]
    public class AchievementProgress
    {
        public string Id;
        public string Title;
        public string Description;
        public int CurrentProgress;
        public int TargetProgress;
        public bool IsUnlocked;
        public int RewardAccountGold;
        public int RewardAccountXP;

        public AchievementProgress(string id, string title, string description, int target, int goldReward, int xpReward)
        {
            Id = id;
            Title = title;
            Description = description;
            CurrentProgress = 0;
            TargetProgress = target;
            IsUnlocked = false;
            RewardAccountGold = goldReward;
            RewardAccountXP = xpReward;
        }
    }

    /// <summary>
    /// Cấu trúc lưu trữ dữ liệu tài khoản lâu dài (Gate 7).
    /// Lưu trữ cấp độ, tài nguyên, thành tựu và Hệ thống 6 Ô Trang Bị + Túi Đồ Vĩnh Viễn.
    /// </summary>
    [Serializable]
    public class AccountData
    {
        public int SaveVersion = 2;

        [Header("Rabbit Progression")]
        public string RabbitName = "Rabbit Warrior";
        public int RabbitLevel = 1;
        public int CurrentXP = 0;
        public int PermanentGold = 150;
        public int PermanentGems = 50;

        [Header("Lifetime Stats")]
        public int TotalRunsPlayed = 0;
        public int TotalRunsWon = 0;
        public int TotalEnemiesKilled = 0;
        public int TotalBossesDefeated = 0;

        [Header("Energy System (Default Dev Flag = Unlimited)")]
        public bool UnlimitedEnergyForTesting = true;
        public int CurrentEnergy = 20;
        public int MaxEnergy = 20;
        public long LastEnergyRechargeTimestamp = 0;

        [Header("Achievements / Milestones")]
        public List<AchievementProgress> Achievements = new List<AchievementProgress>();

        [Header("Equipped Slots (8 Ô Trang Bị Chuẩn: Cận Chiến, Tầm Xa, Giáp, Đồ Phụ Trợ)")]
        public EquipmentItem EquippedWeapon;       // ⚔️ Cận Chiến (Carrot Blade)
        public EquipmentItem EquippedRangedWeapon; // 🏹 Tầm Xa (Tomato Blaster / Grape Cannon)
        public EquipmentItem EquippedNecklace;     // 📿 Dây Chuyền (Carrot Pendant)
        public EquipmentItem EquippedGloves;       // 🥊 Găng Tay (Warrior Gloves)
        public EquipmentItem EquippedArmor;        // 🛡️ Áo Giáp (Rabbit Vest)
        public EquipmentItem EquippedBelt;         // 🥋 Thắt Lưng (Leather Belt)
        public EquipmentItem EquippedBoots;        // 👢 Giày Chạy (Swift Boots)
        public EquipmentItem EquippedBadge;        // 🥕 Huy Hiệu / Bảo Vật (Carrot Royal Badge)

        [Header("Bag Inventory (Túi đồ chứa các trang bị sở hữu)")]
        public List<EquipmentItem> OwnedEquipments = new List<EquipmentItem>();

        public static AccountData CreateDefault()
        {
            var data = new AccountData();
            data.InitializeDefaultAchievements();
            data.InitializeDefaultEquipment();
            return data;
        }

        public void InitializeDefaultAchievements()
        {
            Achievements.Clear();
            Achievements.Add(new AchievementProgress("first_kill", "Khai Hỏa", "Hạ gục Zombie Carrot đầu tiên", 1, 20, 50));
            Achievements.Add(new AchievementProgress("wave_1_clear", "Vượt Ải Khởi Đầu", "Vượt qua Wave 1 lần đầu", 1, 30, 80));
            Achievements.Add(new AchievementProgress("carrot_slayer_20", "Thợ Săn Cà Rốt", "Hạ gục tổng cộng 20 Zombie Carrot", 20, 50, 150));
            Achievements.Add(new AchievementProgress("first_merge", "Bậc Thầy Ghép Đồ", "Ghép nâng cấp vũ khí lên Lv.2 lần đầu", 1, 40, 100));
            Achievements.Add(new AchievementProgress("king_carrot_slayer", "Hạ Gục Chúa Tể", "Đánh bại King Carrot và cứu lấy đấu trường", 1, 100, 300));
        }

        /// <summary>
        /// Tạo bộ trang bị mẫu đầy đủ 8 ô và túi đồ phong phú giống ảnh tham chiếu
        /// </summary>
        public void InitializeDefaultEquipment()
        {
            // Trang bị mặc định trên người (Rare / Hiếm - màu xanh lam như ảnh)
            EquippedWeapon = EquipmentItem.CreateCarrotBlade(EquipmentRarity.Rare, 1);
            EquippedRangedWeapon = EquipmentItem.CreateTomatoBlaster(EquipmentRarity.Rare, 1);
            EquippedNecklace = EquipmentItem.CreateCarrotPendant(EquipmentRarity.Rare, 1);
            EquippedGloves = EquipmentItem.CreateWarriorGloves(EquipmentRarity.Rare, 1);
            EquippedArmor = EquipmentItem.CreateRabbitVest(EquipmentRarity.Rare, 1);
            EquippedBelt = EquipmentItem.CreateLeatherBelt(EquipmentRarity.Rare, 1);
            EquippedBoots = EquipmentItem.CreateSwiftBoots(EquipmentRarity.Rare, 1);
            EquippedBadge = EquipmentItem.CreateCarrotBadge(EquipmentRarity.Rare, 1);

            // Các món đồ sẵn trong túi (bao gồm các món cùng loại để thử nghiệm Fuse)
            OwnedEquipments.Clear();
            // Vũ khí dự phòng
            OwnedEquipments.Add(EquipmentItem.CreateTomatoBlaster(EquipmentRarity.Rare, 1));
            OwnedEquipments.Add(EquipmentItem.CreateGrapeCannon(EquipmentRarity.Great, 1));
            OwnedEquipments.Add(EquipmentItem.CreateCarrotBlade(EquipmentRarity.Rare, 1)); // Cặp để Fuse với kiếm chính!

            // Giáp và ủng dự phòng
            OwnedEquipments.Add(EquipmentItem.CreateRabbitVest(EquipmentRarity.Rare, 1)); // Cặp để Fuse với áo giáp!
            OwnedEquipments.Add(EquipmentItem.CreateSwiftBoots(EquipmentRarity.Rare, 1)); // Cặp để Fuse với giày!
            OwnedEquipments.Add(EquipmentItem.CreateLeatherBelt(EquipmentRarity.Great, 1));
            OwnedEquipments.Add(EquipmentItem.CreateWarriorGloves(EquipmentRarity.Great, 1));
            OwnedEquipments.Add(EquipmentItem.CreateCarrotPendant(EquipmentRarity.Great, 1));

            // Thêm các món khác để lưới túi đồ trông dày dặn và phong phú như ảnh
            OwnedEquipments.Add(EquipmentItem.CreateTomatoBlaster(EquipmentRarity.Great, 1));
            OwnedEquipments.Add(EquipmentItem.CreateGrapeCannon(EquipmentRarity.Rare, 1));
            OwnedEquipments.Add(EquipmentItem.CreateSwiftBoots(EquipmentRarity.Great, 1));
            OwnedEquipments.Add(EquipmentItem.CreateLeatherBelt(EquipmentRarity.Rare, 1));
        }

        public EquipmentItem GetEquippedItem(EquipmentSlotType slot)
        {
            switch (slot)
            {
                case EquipmentSlotType.Weapon: return EquippedWeapon;
                case EquipmentSlotType.RangedWeapon: return EquippedRangedWeapon;
                case EquipmentSlotType.Necklace: return EquippedNecklace;
                case EquipmentSlotType.Gloves: return EquippedGloves;
                case EquipmentSlotType.Armor: return EquippedArmor;
                case EquipmentSlotType.Belt: return EquippedBelt;
                case EquipmentSlotType.Boots: return EquippedBoots;
                case EquipmentSlotType.Badge: return EquippedBadge;
                default: return null;
            }
        }

        public void SetEquippedItem(EquipmentSlotType slot, EquipmentItem item)
        {
            switch (slot)
            {
                case EquipmentSlotType.Weapon: EquippedWeapon = item; break;
                case EquipmentSlotType.RangedWeapon: EquippedRangedWeapon = item; break;
                case EquipmentSlotType.Necklace: EquippedNecklace = item; break;
                case EquipmentSlotType.Gloves: EquippedGloves = item; break;
                case EquipmentSlotType.Armor: EquippedArmor = item; break;
                case EquipmentSlotType.Belt: EquippedBelt = item; break;
                case EquipmentSlotType.Boots: EquippedBoots = item; break;
                case EquipmentSlotType.Badge: EquippedBadge = item; break;
            }
        }

        public int GetTotalEquipmentAttack()
        {
            int total = 0;
            if (EquippedWeapon != null) total += EquippedWeapon.TotalAttack;
            if (EquippedRangedWeapon != null) total += EquippedRangedWeapon.TotalAttack;
            if (EquippedNecklace != null) total += EquippedNecklace.TotalAttack;
            if (EquippedGloves != null) total += EquippedGloves.TotalAttack;
            if (EquippedArmor != null) total += EquippedArmor.TotalAttack;
            if (EquippedBelt != null) total += EquippedBelt.TotalAttack;
            if (EquippedBoots != null) total += EquippedBoots.TotalAttack;
            if (EquippedBadge != null) total += EquippedBadge.TotalAttack;
            return total;
        }

        public int GetTotalEquipmentHealth()
        {
            int total = 0;
            if (EquippedWeapon != null) total += EquippedWeapon.TotalHealth;
            if (EquippedRangedWeapon != null) total += EquippedRangedWeapon.TotalHealth;
            if (EquippedNecklace != null) total += EquippedNecklace.TotalHealth;
            if (EquippedGloves != null) total += EquippedGloves.TotalHealth;
            if (EquippedArmor != null) total += EquippedArmor.TotalHealth;
            if (EquippedBelt != null) total += EquippedBelt.TotalHealth;
            if (EquippedBoots != null) total += EquippedBoots.TotalHealth;
            if (EquippedBadge != null) total += EquippedBadge.TotalHealth;
            return total;
        }

        public static int GetXPRequiredForLevel(int level)
        {
            return Mathf.Max(100, level * 120);
        }
    }
}
