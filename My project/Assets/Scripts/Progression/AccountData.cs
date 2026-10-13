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
        public bool IsClaimed;
        public int RewardAccountGold;
        public int RewardAccountXP;
        public int RewardAccountGems;
        public int RewardAccountScrap;

        public AchievementProgress() { }

        public AchievementProgress(string id, string title, string description, int target, int goldReward, int xpReward, int gemsReward = 5, int scrapReward = 2)
        {
            Id = id;
            Title = title;
            Description = description;
            CurrentProgress = 0;
            TargetProgress = target;
            IsUnlocked = false;
            IsClaimed = false;
            RewardAccountGold = goldReward;
            RewardAccountXP = xpReward;
            RewardAccountGems = gemsReward;
            RewardAccountScrap = scrapReward;
        }
    }

    [Serializable]
    public class BestiaryEntry
    {
        public string MonsterId;
        public string MonsterName;
        public string Role;
        public int Encounters;
        public int Defeated;
        public string Description;
        public string ThreatLevel;

        public BestiaryEntry() { }
        public BestiaryEntry(string id, string name, string role, string desc, string threat)
        {
            MonsterId = id;
            MonsterName = name;
            Role = role;
            Description = desc;
            ThreatLevel = threat;
            Encounters = 0;
            Defeated = 0;
        }
    }

    /// <summary>
    /// Cấu trúc lưu trữ dữ liệu tài khoản lâu dài (Gate 7 + Meta Expansion).
    /// Lưu trữ cấp độ, tài nguyên, thành tựu, Sổ tay quái vật (Bestiary) và Hệ thống Trang Bị vĩnh viễn.
    /// </summary>
    [Serializable]
    public class AccountData
    {
        public int SaveVersion = 3;

        [Header("Rabbit Progression")]
        public string RabbitName = "Rabbit Warrior";
        public int RabbitLevel = 1;
        public int CurrentXP = 0;
        public int PermanentGold = 150;
        public int PermanentGems = 50;
        public int PermanentScrap = 20; // Mảnh phân giải ngoài trận (Carrot Scrap / Essence)

        [Header("Lifetime Stats")]
        public int TotalRunsPlayed = 0;
        public int TotalRunsWon = 0;
        public int TotalLevelsCompleted = 0;
        public int TotalEnemiesKilled = 0;
        public int TotalBossesDefeated = 0;

        [Header("Energy System (Default Dev Flag = Unlimited)")]
        public bool UnlimitedEnergyForTesting = true;
        public int CurrentEnergy = 20;
        public int MaxEnergy = 20;
        public long LastEnergyRechargeTimestamp = 0;

        [Header("Achievements / Milestones")]
        public List<AchievementProgress> Achievements = new List<AchievementProgress>();

        [Header("Bestiary (Sổ Tay Quái Vật)")]
        public List<BestiaryEntry> Bestiary = new List<BestiaryEntry>();

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
            data.InitializeDefaultBestiary();
            data.InitializeDefaultEquipment();
            return data;
        }

        public void InitializeDefaultBestiary()
        {
            Bestiary.Clear();
            Bestiary.Add(new BestiaryEntry(
                "basic",
                "Zombie Cà Rốt Cơ Bản",
                "Cận Chiến Phổ Thông",
                "Thây ma đột biến cấp thấp, tấn công cào cấu bầy đàn. Dễ bị đẩy lùi khi trúng đòn chém.",
                "Thấp (Cấp 1)"
            ));
            Bestiary.Add(new BestiaryEntry(
                "fast",
                "Zombie Cà Rốt Tốc Độ",
                "Đột Kích Chớp Nhoáng",
                "Thân hình nhỏ gọn, lướt nhanh áp sát và đổi hướng khó lường. Cần ưu tiên tiêu diệt từ xa.",
                "Trung Bình (Cấp 2)"
            ));
            Bestiary.Add(new BestiaryEntry(
                "elite",
                "Zombie Cà Rốt Tinh Nhuệ",
                "Đấu Sĩ Giáp Nặng",
                "Chiến binh đột biến khổng lồ có lớp vỏ gai dày, lượng máu cao và đòn đập tạ gây choáng.",
                "Cao (Cấp 3)"
            ));
            Bestiary.Add(new BestiaryEntry(
                "king_carrot",
                "Chúa Tể King Carrot",
                "Boss Hoàng Gia Tối Thượng",
                "Chúa tể cai quản khu vườn đột biến. Sở hữu kỹ năng Dậm Đất Rung Chuyển AoE cực kỳ nguy hiểm.",
                "Cực Nguy Hiểm (Boss)"
            ));
        }

        public void RecordBestiaryEncounter(string monsterId)
        {
            if (Bestiary == null) Bestiary = new List<BestiaryEntry>();
            var entry = Bestiary.Find(b => b.MonsterId == monsterId);
            if (entry != null)
            {
                entry.Encounters++;
            }
        }

        public void RecordBestiaryKill(string monsterId)
        {
            if (Bestiary == null) Bestiary = new List<BestiaryEntry>();
            var entry = Bestiary.Find(b => b.MonsterId == monsterId);
            if (entry != null)
            {
                entry.Defeated++;
            }
        }

        /// <summary>
        /// Phân giải trang bị không dùng để nhận Mảnh Nguyên Liệu Scrap (Mục 6 tài liệu)
        /// Tỉ lệ: Common = 1, Great = 2, Rare = 3, Epic = 8, Legendary = 20.
        /// </summary>
        public bool SalvageEquipment(EquipmentItem item, out int scrapGained)
        {
            scrapGained = 0;
            if (item == null || OwnedEquipments == null || !OwnedEquipments.Contains(item)) return false;

            switch (item.Rarity)
            {
                case EquipmentRarity.Common: scrapGained = 1; break;
                case EquipmentRarity.Great: scrapGained = 2; break;
                case EquipmentRarity.Rare: scrapGained = 3; break;
                case EquipmentRarity.Epic: scrapGained = 8; break;
                case EquipmentRarity.Legendary: scrapGained = 20; break;
                default: scrapGained = 1; break;
            }

            OwnedEquipments.Remove(item);
            PermanentScrap += scrapGained;
            return true;
        }

        /// <summary>
        /// Nâng cấp trang bị ngoài trận bằng Vàng + Scrap
        /// </summary>
        public bool UpgradeEquipment(EquipmentItem item, out string message)
        {
            if (item == null)
            {
                message = "Trang bị không hợp lệ!";
                return false;
            }

            int goldCost = item.Level * 60;
            int scrapCost = item.Level * 2;

            if (PermanentGold < goldCost)
            {
                message = $"Thiếu vàng! Cần {goldCost} Vàng (Hiện có: {PermanentGold})";
                return false;
            }
            if (PermanentScrap < scrapCost)
            {
                message = $"Thiếu Scrap! Cần {scrapCost} Scrap (Hiện có: {PermanentScrap})";
                return false;
            }

            PermanentGold -= goldCost;
            PermanentScrap -= scrapCost;
            item.level++;
            message = $"✨ Nâng cấp thành công {item.DisplayName} lên Cấp {item.Level}!";
            return true;
        }

        public bool ClaimAchievement(string achievementId, out int goldGained, out int gemsGained, out int scrapGained, out int xpGained)
        {
            goldGained = 0;
            gemsGained = 0;
            scrapGained = 0;
            xpGained = 0;

            if (Achievements == null) return false;
            var ach = Achievements.Find(a => a.Id == achievementId);
            if (ach == null || !ach.IsUnlocked || ach.IsClaimed) return false;

            ach.IsClaimed = true;
            goldGained = ach.RewardAccountGold;
            gemsGained = ach.RewardAccountGems;
            scrapGained = ach.RewardAccountScrap;
            xpGained = ach.RewardAccountXP;

            PermanentGold += goldGained;
            PermanentGems += gemsGained;
            PermanentScrap += scrapGained;
            CurrentXP += xpGained;

            // Kiểm tra thăng cấp Thỏ
            int reqXP = GetXPRequiredForLevel(RabbitLevel);
            while (CurrentXP >= reqXP)
            {
                CurrentXP -= reqXP;
                RabbitLevel++;
                reqXP = GetXPRequiredForLevel(RabbitLevel);
            }

            return true;
        }

        public void InitializeDefaultAchievements()
        {
            Achievements.Clear();
            Achievements.Add(new AchievementProgress("first_kill", "Khai Hỏa", "Hạ gục Zombie Carrot đầu tiên", 1, 50, 50, 5, 2));
            Achievements.Add(new AchievementProgress("wave_1_clear", "Vượt Ải Khởi Đầu", "Vượt qua Wave 1 lần đầu", 1, 80, 80, 10, 3));
            Achievements.Add(new AchievementProgress("carrot_slayer_20", "Thợ Săn Cà Rốt", "Hạ gục tổng cộng 20 Zombie Carrot", 20, 150, 150, 15, 5));
            Achievements.Add(new AchievementProgress("first_merge", "Bậc Thầy Ghép Đồ", "Ghép nâng cấp vũ khí lên Lv.2 lần đầu", 1, 100, 100, 10, 4));
            Achievements.Add(new AchievementProgress("king_carrot_slayer", "Hạ Gục Chúa Tể", "Đánh bại King Carrot và cứu lấy đấu trường", 1, 300, 300, 30, 10));
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
