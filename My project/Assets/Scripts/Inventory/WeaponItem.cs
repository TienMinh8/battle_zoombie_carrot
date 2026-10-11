using System;
using UnityEngine;
using Apocarrot.Data;

namespace Apocarrot.Inventory
{
    [Serializable]
    public class WeaponItem
    {
        public const int MAX_WEAPON_LEVEL = 3;

        [SerializeField] private string weaponId;
        [SerializeField] private string displayName;
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private int level = 1;
        [SerializeField] private Color itemColor = Color.white;

        public string WeaponId => weaponId;
        public string DisplayName => displayName;
        public WeaponType WeaponType => weaponType;
        public int Level => level;
        public Color ItemColor => itemColor;

        public WeaponItem(string id, string name, WeaponType type, int lvl, Color color)
        {
            weaponId = id;
            displayName = name;
            weaponType = type;
            level = Mathf.Clamp(lvl, 1, MAX_WEAPON_LEVEL);
            itemColor = color;
        }

        public WeaponItem Clone()
        {
            return new WeaponItem(weaponId, displayName, weaponType, level, itemColor);
        }

        /// <summary>
        /// Kiểm tra hai vũ khí có thỏa mãn điều kiện ghép (cùng loại, cùng cấp, chưa đạt cấp tối đa)
        /// </summary>
        public bool CanMergeWith(WeaponItem other)
        {
            if (other == null) return false;
            return weaponId == other.weaponId && level == other.level && level < MAX_WEAPON_LEVEL;
        }

        /// <summary>
        /// Tạo ra vũ khí cấp kế tiếp sau khi ghép thành công
        /// </summary>
        public WeaponItem GetMergedVersion()
        {
            return new WeaponItem(weaponId, displayName, weaponType, level + 1, itemColor);
        }

        public MeleeWeaponData ToMeleeData()
        {
            if (weaponId == "carrot_axe")
            {
                return MeleeWeaponData.CreateCarrotAxe(level);
            }
            return MeleeWeaponData.CreateCarrotBlade(level);
        }

        public RangedWeaponData ToRangedData()
        {
            if (weaponId == "grape_splitter")
            {
                return RangedWeaponData.CreateGrapeSplitter(level);
            }
            return RangedWeaponData.CreateTomatoBlaster(level);
        }

        public string GetStatsSummary()
        {
            if (weaponType == WeaponType.Melee)
            {
                var data = ToMeleeData();
                return $"Sát thương: {data.Damage} | Cooldown: {data.AttackCooldown:0.00}s | Tầm: {data.AttackRange}m";
            }
            else
            {
                var data = ToRangedData();
                return $"Sát thương: {data.Damage} | Đạn: {data.ProjectileCount} | Tầm: {data.Range}m";
            }
        }

        public static WeaponItem FromMeleeData(MeleeWeaponData data)
        {
            return new WeaponItem(data.WeaponId, data.WeaponName, WeaponType.Melee, data.Level, data.SlashColor);
        }

        public static WeaponItem FromRangedData(RangedWeaponData data)
        {
            return new WeaponItem(data.WeaponId, data.WeaponName, WeaponType.Ranged, data.Level, data.ProjectileColor);
        }

        public static WeaponItem CreateCarrotBlade(int level = 1)
        {
            return new WeaponItem("carrot_blade", "Carrot Blade", WeaponType.Melee, level, new Color(0.3f, 0.95f, 1f));
        }

        public static WeaponItem CreateCarrotAxe(int level = 1)
        {
            return new WeaponItem("carrot_axe", "Carrot Axe", WeaponType.Melee, level, new Color(1.0f, 0.45f, 0.1f));
        }

        public static WeaponItem CreateTomatoBlaster(int level = 1)
        {
            return new WeaponItem("tomato_blaster", "Tomato Blaster", WeaponType.Ranged, level, new Color(0.95f, 0.18f, 0.15f));
        }

        public static WeaponItem CreateGrapeSplitter(int level = 1)
        {
            return new WeaponItem("grape_splitter", "Grape Splitter", WeaponType.Ranged, level, new Color(0.65f, 0.15f, 0.95f));
        }
    }
}
