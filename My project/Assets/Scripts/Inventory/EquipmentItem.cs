using System;
using UnityEngine;

namespace Apocarrot.Inventory
{
    public enum EquipmentSlotType
    {
        Weapon = 0,    // Vũ khí chính (Carrot Blade, Tomato Blaster, Grape Cannon)
        Necklace = 1,  // Dây chuyền / Trợ thủ (Carrot Pendant)
        Gloves = 2,    // Găng tay / Hộp chiến binh (Warrior Gloves)
        Armor = 3,     // Áo giáp / Thân trên (Carrot Vest)
        Belt = 4,      // Thắt lưng (Leather Belt)
        Boots = 5      // Giày tốc độ (Swift Boots)
    }

    public enum EquipmentRarity
    {
        Common = 0,     // Xám / Trắng (Thường)
        Great = 1,      // Xanh lá (Tốt)
        Rare = 2,       // Xanh lam (Hiếm - như trong ảnh)
        Epic = 3,       // Tím (Sử thi)
        Legendary = 4   // Vàng cam (Huyền thoại)
    }

    /// <summary>
    /// Thực thể Trang bị cá nhân trong Balo tài khoản (Survivor.io / Archero style).
    /// Cộng dồn chỉ số ATK và HP cho Thỏ Chiến Binh, có thể nâng cấp (Fuse) và mang vào 6 ô trang bị.
    /// </summary>
    [Serializable]
    public class EquipmentItem
    {
        public string uid;
        public string templateId;
        public string displayName;
        public EquipmentSlotType slotType;
        public EquipmentRarity rarity;
        public int level = 1;
        public int baseAttack = 10;
        public int baseHealth = 50;
        public string description;

        public string Uid => uid;
        public string TemplateId => templateId;
        public string DisplayName => displayName;
        public EquipmentSlotType SlotType => slotType;
        public EquipmentRarity Rarity => rarity;
        public int Level => level;
        public string Description => description;

        public EquipmentItem()
        {
            uid = Guid.NewGuid().ToString("N");
        }

        public EquipmentItem(string templateId, string name, EquipmentSlotType slot, EquipmentRarity rarity, int level, int baseAtk, int baseHp, string desc = "")
        {
            this.uid = Guid.NewGuid().ToString("N");
            this.templateId = templateId;
            this.displayName = name;
            this.slotType = slot;
            this.rarity = rarity;
            this.level = Mathf.Max(1, level);
            this.baseAttack = baseAtk;
            this.baseHealth = baseHp;
            this.description = desc;
        }

        /// <summary>
        /// Tổng sát thương cộng thêm từ món đồ (nhân theo Phẩm chất và Cấp độ).
        /// </summary>
        public int TotalAttack
        {
            get
            {
                if (baseAttack <= 0) return 0;
                float rarityMult = 1.0f + (int)rarity * 0.45f;
                return Mathf.RoundToInt(baseAttack * rarityMult + (level - 1) * 8);
            }
        }

        /// <summary>
        /// Tổng lượng máu cộng thêm từ món đồ (nhân theo Phẩm chất và Cấp độ).
        /// </summary>
        public int TotalHealth
        {
            get
            {
                if (baseHealth <= 0) return 0;
                float rarityMult = 1.0f + (int)rarity * 0.45f;
                return Mathf.RoundToInt(baseHealth * rarityMult + (level - 1) * 45);
            }
        }

        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case EquipmentRarity.Common:
                    return new Color(0.68f, 0.72f, 0.78f); // Xám bạc
                case EquipmentRarity.Great:
                    return new Color(0.22f, 0.85f, 0.38f); // Xanh lá
                case EquipmentRarity.Rare:
                    return new Color(0.18f, 0.62f, 0.95f); // Xanh lam (như ảnh)
                case EquipmentRarity.Epic:
                    return new Color(0.72f, 0.32f, 0.92f); // Tím
                case EquipmentRarity.Legendary:
                    return new Color(1.0f, 0.75f, 0.15f);  // Vàng cam
                default:
                    return Color.white;
            }
        }

        public string GetRarityName()
        {
            switch (rarity)
            {
                case EquipmentRarity.Common: return "Thường (Common)";
                case EquipmentRarity.Great: return "Tốt (Great)";
                case EquipmentRarity.Rare: return "Hiếm (Rare)";
                case EquipmentRarity.Epic: return "Sử thi (Epic)";
                case EquipmentRarity.Legendary: return "Huyền thoại (Legendary)";
                default: return "Thường";
            }
        }

        public string GetSlotIcon()
        {
            switch (slotType)
            {
                case EquipmentSlotType.Weapon: return "KIẾM";
                case EquipmentSlotType.Necklace: return "DÂY";
                case EquipmentSlotType.Gloves: return "GĂNG";
                case EquipmentSlotType.Armor: return "GIÁP";
                case EquipmentSlotType.Belt: return "ĐAI";
                case EquipmentSlotType.Boots: return "ỦNG";
                default: return "ĐỒ";
            }
        }

        public string GetSlotBadge()
        {
            switch (slotType)
            {
                case EquipmentSlotType.Weapon: return "◆ VŨ KHÍ";
                case EquipmentSlotType.Necklace: return "◆ DÂY CHUYỀN";
                case EquipmentSlotType.Gloves: return "◆ GĂNG TAY";
                case EquipmentSlotType.Armor: return "◆ ÁO GIÁP";
                case EquipmentSlotType.Belt: return "◆ THẮT LƯNG";
                case EquipmentSlotType.Boots: return "◆ ỦNG CHIẾN";
                default: return "◆ TRANG BỊ";
            }
        }

        public string GetPrimaryStatText()
        {
            if (TotalAttack > 0 && TotalHealth > 0)
            {
                return $"+{TotalAttack} ATK | +{TotalHealth} HP";
            }
            if (TotalAttack > 0)
            {
                return $"+{TotalAttack} ATK";
            }
            if (TotalHealth > 0)
            {
                return $"+{TotalHealth} HP";
            }
            return $"Lv.{Level}";
        }

        public string GetSlotName()
        {
            switch (slotType)
            {
                case EquipmentSlotType.Weapon: return "Vũ khí";
                case EquipmentSlotType.Necklace: return "Dây chuyền";
                case EquipmentSlotType.Gloves: return "Găng tay";
                case EquipmentSlotType.Armor: return "Áo giáp";
                case EquipmentSlotType.Belt: return "Thắt lưng";
                case EquipmentSlotType.Boots: return "Giày chạy";
                default: return "Trang bị";
            }
        }

        /// <summary>
        /// Điều kiện Fuse (Ghép): Cùng TemplateId, cùng Rarity, chưa đạt phẩm tối đa.
        /// </summary>
        public bool CanFuseWith(EquipmentItem other)
        {
            if (other == null || other.uid == this.uid) return false;
            return this.templateId == other.templateId && 
                   this.rarity == other.rarity && 
                   this.rarity < EquipmentRarity.Legendary;
        }

        /// <summary>
        /// Tạo ra trang bị mới sau khi Ghép (tăng 1 bậc Phẩm chất, giữ cấp độ cao hơn).
        /// </summary>
        public EquipmentItem CreateFusedVersion(EquipmentItem other)
        {
            int nextRarity = Mathf.Min((int)EquipmentRarity.Legendary, (int)this.rarity + 1);
            int newLevel = Mathf.Max(this.level, other.level);
            return new EquipmentItem(this.templateId, this.displayName, this.slotType, (EquipmentRarity)nextRarity, newLevel, this.baseAttack, this.baseHealth, this.description);
        }

        public EquipmentItem Clone()
        {
            var copy = new EquipmentItem(this.templateId, this.displayName, this.slotType, this.rarity, this.level, this.baseAttack, this.baseHealth, this.description);
            return copy;
        }

        // ========================== FACTORY HELPER ==========================
        public static EquipmentItem CreateCarrotBlade(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("wpn_carrot_blade", "Thanh Kiếm Cà Rốt", EquipmentSlotType.Weapon, r, lvl, 45, 0, "Lưỡi kiếm bén ngót từ tinh hoa củ cà rốt hoàng gia.");
        }

        public static EquipmentItem CreateTomatoBlaster(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("wpn_tomato_blaster", "Súng Cà Chua Siêu Âm", EquipmentSlotType.Weapon, r, lvl, 40, 0, "Súng bắn đạn cà chua tự động ngắm mục tiêu tầm xa.");
        }

        public static EquipmentItem CreateGrapeCannon(EquipmentRarity r = EquipmentRarity.Great, int lvl = 1)
        {
            return new EquipmentItem("wpn_grape_cannon", "Đại Bác Chùm Nho", EquipmentSlotType.Weapon, r, lvl, 35, 0, "Khai hỏa chùm đạn nho càn quét diện rộng.");
        }

        public static EquipmentItem CreateCarrotPendant(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("neck_carrot_pendant", "Mặt Dây Cà Rốt Ngọc", EquipmentSlotType.Necklace, r, lvl, 25, 120, "Dây chuyền cổ vật tiếp thêm linh lực và may mắn.");
        }

        public static EquipmentItem CreateWarriorGloves(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("glove_warrior", "Hộp Găng Cơ Giới", EquipmentSlotType.Gloves, r, lvl, 28, 80, "Găng bọc thép tăng tốc độ vung đòn và lực đánh.");
        }

        public static EquipmentItem CreateRabbitVest(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("arm_rabbit_vest", "Áo Giáp Thỏ Chiến", EquipmentSlotType.Armor, r, lvl, 0, 480, "Bộ giáp bảo hộ siêu nhẹ bảo vệ thỏ trước nanh vuốt quái vật.");
        }

        public static EquipmentItem CreateLeatherBelt(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("belt_leather", "Thắt Lưng Thợ Săn", EquipmentSlotType.Belt, r, lvl, 10, 260, "Đai thắt lưng bền bỉ giúp di chuyển vững vàng.");
        }

        public static EquipmentItem CreateSwiftBoots(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("boot_swift", "Ủng Chiến Cao Cổ", EquipmentSlotType.Boots, r, lvl, 12, 220, "Ủng tăng tốc giúp Thỏ lướt gió né đòn chớp nhoáng.");
        }
    }
}
