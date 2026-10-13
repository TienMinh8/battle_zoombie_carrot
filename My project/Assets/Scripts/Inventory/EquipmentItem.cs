using System;
using UnityEngine;

namespace Apocarrot.Inventory
{
    public enum EquipmentSlotType
    {
        Weapon = 0,        // Vũ khí cận chiến (Carrot Blade, Carrot Axe)
        Necklace = 1,      // Dây chuyền (Carrot Pendant)
        Gloves = 2,        // Găng tay (Warrior Gloves)
        Armor = 3,         // Áo giáp (Rabbit Vest)
        Belt = 4,          // Thắt lưng (Leather Belt)
        Boots = 5,         // Giày tốc độ (Swift Boots)
        RangedWeapon = 6,  // Vũ khí tầm xa (Tomato Blaster, Grape Cannon)
        Badge = 7          // Huy hiệu bảo hộ (Carrot Royal Badge)
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
        /// Tổng sát thương cộng thêm từ món đồ (cân bằng tỉ lệ, tránh lạm phát chỉ số đột biến).
        /// Phẩm chất: Common (1.0x), Great (1.15x), Rare (1.30x), Epic (1.45x), Legendary (1.60x).
        /// </summary>
        public int TotalAttack
        {
            get
            {
                if (baseAttack <= 0) return 0;
                float rarityMult = 1.0f + (int)rarity * 0.15f;
                return Mathf.RoundToInt(baseAttack * rarityMult + (level - 1) * 2);
            }
        }

        /// <summary>
        /// Tổng lượng máu cộng thêm từ món đồ (cân bằng tỉ lệ).
        /// </summary>
        public int TotalHealth
        {
            get
            {
                if (baseHealth <= 0) return 0;
                float rarityMult = 1.0f + (int)rarity * 0.15f;
                return Mathf.RoundToInt(baseHealth * rarityMult + (level - 1) * 8);
            }
        }

        public int AttackBonus => baseAttack;
        public int HealthBonus => baseHealth;
        public int ArmorBonus => slotType == EquipmentSlotType.Armor || slotType == EquipmentSlotType.Belt ? Mathf.RoundToInt(TotalHealth * 0.05f) : 0;
        public float AttackSpeedBonus => slotType == EquipmentSlotType.Gloves ? 0.10f : 0f;
        public float MoveSpeedBonus => slotType == EquipmentSlotType.Boots ? 0.10f : 0f;

        public int GetTotalAttackBonus() => TotalAttack;
        public int GetTotalHealthBonus() => TotalHealth;

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
                case EquipmentSlotType.RangedWeapon: return "SÚNG";
                case EquipmentSlotType.Necklace: return "DÂY";
                case EquipmentSlotType.Gloves: return "GĂNG";
                case EquipmentSlotType.Armor: return "GIÁP";
                case EquipmentSlotType.Belt: return "ĐAI";
                case EquipmentSlotType.Boots: return "ỦNG";
                case EquipmentSlotType.Badge: return "HUY HIỆU";
                default: return "ĐỒ";
            }
        }

        public string GetSlotBadge()
        {
            switch (slotType)
            {
                case EquipmentSlotType.Weapon: return "◆ CẬN CHIẾN";
                case EquipmentSlotType.RangedWeapon: return "◆ TẦM XA";
                case EquipmentSlotType.Necklace: return "◆ DÂY CHUYỀN";
                case EquipmentSlotType.Gloves: return "◆ GĂNG TAY";
                case EquipmentSlotType.Armor: return "◆ ÁO GIÁP";
                case EquipmentSlotType.Belt: return "◆ THẮT LƯNG";
                case EquipmentSlotType.Boots: return "◆ ỦNG CHIẾN";
                case EquipmentSlotType.Badge: return "◆ HUY HIỆU";
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
                case EquipmentSlotType.Weapon: return "Cận Chiến";
                case EquipmentSlotType.RangedWeapon: return "Tầm Xa";
                case EquipmentSlotType.Necklace: return "Dây Chuyền";
                case EquipmentSlotType.Gloves: return "Găng Tay";
                case EquipmentSlotType.Armor: return "Áo Giáp";
                case EquipmentSlotType.Belt: return "Thắt Lưng";
                case EquipmentSlotType.Boots: return "Giày Chạy";
                case EquipmentSlotType.Badge: return "Huy Hiệu";
                default: return "Trang Bị";
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
            return new EquipmentItem("wpn_carrot_blade", "Thanh Kiếm Cà Rốt", EquipmentSlotType.Weapon, r, lvl, 7, 0, "Lưỡi kiếm cận chiến bén ngót vung chém quét mục tiêu.");
        }

        public static EquipmentItem CreateTomatoBlaster(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("wpn_tomato_blaster", "Súng Cà Chua Siêu Âm", EquipmentSlotType.RangedWeapon, r, lvl, 6, 0, "Vũ khí tầm xa: tự động ngắm bắn đạn cà chua liên thanh.");
        }

        public static EquipmentItem CreateGrapeCannon(EquipmentRarity r = EquipmentRarity.Great, int lvl = 1)
        {
            return new EquipmentItem("wpn_grape_cannon", "Đại Bác Chùm Nho", EquipmentSlotType.RangedWeapon, r, lvl, 5, 0, "Vũ khí tầm xa: khai hỏa chùm đạn nho càn quét diện rộng.");
        }

        public static EquipmentItem CreateCarrotPendant(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("neck_carrot_pendant", "Mặt Dây Cà Rốt Ngọc", EquipmentSlotType.Necklace, r, lvl, 3, 25, "Dây chuyền cổ vật tiếp thêm linh lực và may mắn.");
        }

        public static EquipmentItem CreateWarriorGloves(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("glove_warrior", "Hộp Găng Cơ Giới", EquipmentSlotType.Gloves, r, lvl, 3, 15, "Găng bọc thép tăng tốc độ vung đòn và lực đánh.");
        }

        public static EquipmentItem CreateRabbitVest(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("arm_rabbit_vest", "Áo Giáp Thỏ Chiến", EquipmentSlotType.Armor, r, lvl, 0, 80, "Bộ giáp bảo hộ siêu nhẹ bảo vệ thỏ trước nanh vuốt quái vật.");
        }

        public static EquipmentItem CreateLeatherBelt(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("belt_leather", "Thắt Lưng Thợ Săn", EquipmentSlotType.Belt, r, lvl, 1, 40, "Đai thắt lưng bền bỉ giúp di chuyển vững vàng.");
        }

        public static EquipmentItem CreateSwiftBoots(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("boot_swift", "Ủng Chiến Cao Cổ", EquipmentSlotType.Boots, r, lvl, 1, 30, "Ủng tăng tốc giúp Thỏ lướt gió né đòn chớp nhoáng.");
        }

        public static EquipmentItem CreateCarrotBadge(EquipmentRarity r = EquipmentRarity.Rare, int lvl = 1)
        {
            return new EquipmentItem("badge_royal_carrot", "Huy Hiệu Cà Rốt Vàng", EquipmentSlotType.Badge, r, lvl, 2, 40, "Huân chương hoàng gia thỏ bảo hộ và gia tăng sĩ khí.");
        }
    }
}
