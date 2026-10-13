using System;
using UnityEngine;
using Apocarrot.Inventory;
using Apocarrot.Progression;

namespace Apocarrot.Loot
{
    public enum ChestType
    {
        Normal = 0, // Rương Thường (Vượt màn thường / Hàng ngày)
        Elite = 1,  // Rương Tinh Anh (Thử thách khó / Gems)
        Boss = 2    // Rương Boss (Hạ gục King Carrot / Cột mốc)
    }

    /// <summary>
    /// Thực thể phần thưởng sau khi mở Rương (Mục 5 tài liệu)
    /// </summary>
    public class ChestReward
    {
        public EquipmentItem Item;
        public int BonusGold;
        public int BonusScrap;
        public ChestType SourceType;

        public string GetRewardTitle()
        {
            switch (SourceType)
            {
                case ChestType.Boss: return "👑 RƯƠNG CHÚA TỂ (BOSS CHEST)";
                case ChestType.Elite: return "🏆 RƯƠNG TINH ANH (ELITE CHEST)";
                case ChestType.Normal: default: return "📦 RƯƠNG CHIẾN TRANH (WOOD CHEST)";
            }
        }
    }

    /// <summary>
    /// Dịch vụ mở rương và tạo trang bị ngẫu nhiên theo bảng xác suất (Section 5.3 Loot Table)
    /// </summary>
    public static class ChestService
    {
        public static ChestReward OpenChest(ChestType type)
        {
            EquipmentRarity rarity = DetermineRarity(type);
            EquipmentItem item = GenerateRandomEquipment(rarity);

            int bonusGold = 20;
            int bonusScrap = 1;

            switch (type)
            {
                case ChestType.Boss:
                    bonusGold = 120;
                    bonusScrap = 5;
                    break;
                case ChestType.Elite:
                    bonusGold = 60;
                    bonusScrap = 2;
                    break;
                case ChestType.Normal:
                default:
                    bonusGold = 25;
                    bonusScrap = 1;
                    break;
            }

            if (AccountProgression.Instance != null && AccountProgression.Instance.Data != null)
            {
                var data = AccountProgression.Instance.Data;
                data.OwnedEquipments.Insert(0, item);
                data.PermanentGold += bonusGold;
                data.PermanentScrap += bonusScrap;
                AccountProgression.Instance.SaveAccount();
            }

            return new ChestReward
            {
                Item = item,
                BonusGold = bonusGold,
                BonusScrap = bonusScrap,
                SourceType = type
            };
        }

        /// <summary>
        /// Bảng xác suất độ hiếm chuẩn theo Mục 5.3 trong tài liệu:
        /// Normal: 70% Common, 25% Great, 4% Rare, 1% Epic
        /// Elite: 40% Great, 45% Rare, 13% Epic, 2% Legendary
        /// Boss: 20% Great, 45% Rare, 28% Epic, 7% Legendary
        /// </summary>
        public static EquipmentRarity DetermineRarity(ChestType type)
        {
            float roll = UnityEngine.Random.value * 100f; // 0.0 to 100.0

            switch (type)
            {
                case ChestType.Boss:
                    // 7% Legendary, 28% Epic, 45% Rare, 20% Great
                    if (roll < 7f) return EquipmentRarity.Legendary;
                    if (roll < 35f) return EquipmentRarity.Epic;
                    if (roll < 80f) return EquipmentRarity.Rare;
                    return EquipmentRarity.Great;

                case ChestType.Elite:
                    // 2% Legendary, 13% Epic, 45% Rare, 40% Great
                    if (roll < 2f) return EquipmentRarity.Legendary;
                    if (roll < 15f) return EquipmentRarity.Epic;
                    if (roll < 60f) return EquipmentRarity.Rare;
                    return EquipmentRarity.Great;

                case ChestType.Normal:
                default:
                    // 1% Epic, 4% Rare, 25% Great, 70% Common
                    if (roll < 1f) return EquipmentRarity.Epic;
                    if (roll < 5f) return EquipmentRarity.Rare;
                    if (roll < 30f) return EquipmentRarity.Great;
                    return EquipmentRarity.Common;
            }
        }

        public static EquipmentItem GenerateRandomEquipment(EquipmentRarity rarity)
        {
            int choice = UnityEngine.Random.Range(0, 9);
            switch (choice)
            {
                case 0: return EquipmentItem.CreateCarrotBlade(rarity, 1);
                case 1: return EquipmentItem.CreateTomatoBlaster(rarity, 1);
                case 2: return EquipmentItem.CreateGrapeCannon(rarity, 1);
                case 3: return EquipmentItem.CreateCarrotPendant(rarity, 1);
                case 4: return EquipmentItem.CreateWarriorGloves(rarity, 1);
                case 5: return EquipmentItem.CreateRabbitVest(rarity, 1);
                case 6: return EquipmentItem.CreateLeatherBelt(rarity, 1);
                case 7: return EquipmentItem.CreateSwiftBoots(rarity, 1);
                case 8: default: return EquipmentItem.CreateCarrotBadge(rarity, 1);
            }
        }
    }
}
