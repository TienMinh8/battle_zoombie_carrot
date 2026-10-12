using System.Collections.Generic;
using UnityEngine;
using Apocarrot.Inventory;

namespace Apocarrot.Inventory
{
    /// <summary>
    /// Dịch vụ quản lý và ánh xạ Sprite 2D vũ khí & trang bị từ thư mục Resources/Weapons.
    /// Hỗ trợ nạp động hình ảnh chân thực cho từng cấp độ (Lv.1, Lv.2, Lv.3) và tự động tạo Sprite từ Texture2D.
    /// </summary>
    public static class WeaponSpriteService
    {
        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Lấy Sprite cho trang bị (EquipmentItem) dựa theo SlotType và Level
        /// </summary>
        public static Sprite GetEquipmentSprite(EquipmentItem item)
        {
            if (item == null) return null;
            return GetSpriteByTemplate(item.TemplateId, item.SlotType, item.Level);
        }

        /// <summary>
        /// Lấy Sprite cho vũ khí trong Run (WeaponItem)
        /// </summary>
        public static Sprite GetWeaponItemSprite(WeaponItem item)
        {
            if (item == null) return null;
            return GetSpriteByTemplate(item.WeaponId, EquipmentSlotType.Weapon, item.Level);
        }

        /// <summary>
        /// Lấy Sprite cho vật phẩm Cửa Hàng (ShopItemData)
        /// </summary>
        public static Sprite GetShopItemSprite(ShopItemData item)
        {
            if (item == null) return null;
            if (item.ItemType == ShopItemType.Heal)
            {
                return LoadSprite("potion1") ?? LoadSprite("cake1");
            }
            if (item.Weapon != null)
            {
                return GetWeaponItemSprite(item.Weapon);
            }
            return LoadSprite("sword1");
        }

        /// <summary>
        /// Ánh xạ từ Template ID và Level sang tên tệp ảnh Sprite tương ứng
        /// </summary>
        public static Sprite GetSpriteByTemplate(string templateId, EquipmentSlotType slot, int level)
        {
            int lvl = Mathf.Clamp(level, 1, 3);
            string id = string.IsNullOrEmpty(templateId) ? "" : templateId.ToLower();

            string spriteName;

            // 1. Phân loại theo Vũ Khí (Weapon)
            if (slot == EquipmentSlotType.Weapon || id.Contains("weapon") || id.Contains("blade") || id.Contains("axe") || id.Contains("blaster") || id.Contains("cannon") || id.Contains("splitter"))
            {
                if (id.Contains("axe"))
                {
                    spriteName = lvl >= 3 ? "axe3" : "axe2";
                }
                else if (id.Contains("tomato") || id.Contains("blaster"))
                {
                    spriteName = $"colt{lvl}";
                }
                else if (id.Contains("grape") || id.Contains("cannon") || id.Contains("splitter"))
                {
                    spriteName = $"crossbow{lvl}";
                }
                else if (id.Contains("dagger") || id.Contains("knife"))
                {
                    spriteName = $"Knife{lvl}";
                }
                else if (id.Contains("shuriken"))
                {
                    spriteName = $"Shuriken{lvl}";
                }
                else
                {
                    // Mặc định: Kiếm chém Carrot Blade
                    spriteName = $"sword{lvl}";
                }
            }
            // 2. Phân loại Giáp (Armor / Vest)
            else if (slot == EquipmentSlotType.Armor || id.Contains("armor") || id.Contains("vest"))
            {
                spriteName = $"Helmet{lvl}";
            }
            // 3. Phân loại Găng Tay (Gloves)
            else if (slot == EquipmentSlotType.Gloves || id.Contains("glove"))
            {
                spriteName = $"glove{lvl}";
            }
            // 4. Phân loại Ủng (Boots)
            else if (slot == EquipmentSlotType.Boots || id.Contains("boot"))
            {
                spriteName = $"icePickaxe{lvl}";
            }
            // 5. Phân loại Dây Chuyền / Phù Chú (Necklace / Pendant)
            else if (slot == EquipmentSlotType.Necklace || id.Contains("pendant") || id.Contains("necklace"))
            {
                spriteName = $"hourglass{lvl}";
            }
            // 6. Phân loại Thắt Lưng / Khiên (Belt / Shield)
            else if (slot == EquipmentSlotType.Belt || id.Contains("belt"))
            {
                spriteName = lvl >= 3 ? "shield_dragon1" : (lvl >= 2 ? "shield1_skin_police" : "shield_1141");
            }
            else
            {
                spriteName = $"sword{lvl}";
            }

            Sprite sp = LoadSprite(spriteName);
            if (sp == null)
            {
                // Fallback nếu không thấy file cụ thể
                sp = LoadSprite($"sword{lvl}") ?? LoadSprite("sword1");
            }
            return sp;
        }

        /// <summary>
        /// Nạp Sprite từ Resources/Weapons/{name} và cache lại
        /// Hỗ trợ cả trường hợp file được import là Sprite hoặc Texture2D thông thường
        /// </summary>
        public static Sprite LoadSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;

            if (spriteCache.TryGetValue(spriteName, out Sprite cached) && cached != null)
            {
                return cached;
            }

            // Thử nạp dưới dạng Sprite trực tiếp
            Sprite sp = Resources.Load<Sprite>($"Weapons/{spriteName}");

            // Nếu Texture chưa được set textureType là Sprite trong Unity, nạp dưới dạng Texture2D và tạo Sprite
            if (sp == null)
            {
                Texture2D tex = Resources.Load<Texture2D>($"Weapons/{spriteName}");
                if (tex != null)
                {
                    sp = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );
                    sp.name = spriteName;
                }
            }

            if (sp != null)
            {
                spriteCache[spriteName] = sp;
            }

            return sp;
        }
    }
}
