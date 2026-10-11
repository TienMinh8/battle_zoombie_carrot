using System;
using UnityEngine;

namespace Apocarrot.Inventory
{
    public enum ShopItemType
    {
        Weapon,
        Heal
    }

    [Serializable]
    public class ShopItemData
    {
        public string ItemId;
        public string Title;
        public string Description;
        public int Price;
        public ShopItemType ItemType;
        public WeaponItem Weapon;
        public float HealAmount;
        public Color ThemeColor;

        public ShopItemData(string id, string title, string desc, int price, ShopItemType type, WeaponItem weapon, float heal, Color color)
        {
            ItemId = id;
            Title = title;
            Description = desc;
            Price = price;
            ItemType = type;
            Weapon = weapon;
            HealAmount = heal;
            ThemeColor = color;
        }

        public static ShopItemData[] GetDefaultStock()
        {
            return new ShopItemData[]
            {
                new ShopItemData(
                    "shop_blade",
                    "Carrot Blade Lv.1",
                    "Kiếm cà rốt chém nhanh (Mua để ghép lên Lv.2)",
                    35,
                    ShopItemType.Weapon,
                    WeaponItem.CreateCarrotBlade(1),
                    0,
                    new Color(0.3f, 0.95f, 1f)
                ),
                new ShopItemData(
                    "shop_axe",
                    "Carrot Axe Lv.1",
                    "Rìu nặng sát thương khủng quét góc rộng",
                    45,
                    ShopItemType.Weapon,
                    WeaponItem.CreateCarrotAxe(1),
                    0,
                    new Color(1.0f, 0.45f, 0.1f)
                ),
                new ShopItemData(
                    "shop_tomato",
                    "Tomato Blaster Lv.1",
                    "Pháo cà chua tự động bắn mục tiêu gần",
                    40,
                    ShopItemType.Weapon,
                    WeaponItem.CreateTomatoBlaster(1),
                    0,
                    new Color(0.95f, 0.18f, 0.15f)
                ),
                new ShopItemData(
                    "shop_grape",
                    "Grape Splitter Lv.1",
                    "Bắn chùm 3 viên nho tím tỏa quạt đa mục tiêu",
                    50,
                    ShopItemType.Weapon,
                    WeaponItem.CreateGrapeSplitter(1),
                    0,
                    new Color(0.65f, 0.15f, 0.95f)
                ),
                new ShopItemData(
                    "shop_snack",
                    "Cà Rốt Tiếp Tế (+35 HP)",
                    "Hồi phục 35 HP ngay lập tức cho Thỏ",
                    25,
                    ShopItemType.Heal,
                    null,
                    35f,
                    new Color(0.25f, 0.95f, 0.4f)
                )
            };
        }
    }
}
