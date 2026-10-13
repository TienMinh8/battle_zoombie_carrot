using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Apocarrot.Inventory;

namespace Apocarrot.UI
{
    /// <summary>
    /// Dịch vụ quản lý và sinh Sprite cho toàn bộ hệ thống UI Stylized của APOCARROT.
    /// Hỗ trợ cả nạp ảnh Resource, nạp tệp trực tiếp và sinh procedural vector sprite (bo góc, gradient, viền 3D).
    /// </summary>
    public static class UISpriteService
    {
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        // ========================== 1. ARTWORK SPRITES ==========================

        public static Sprite GetAvatarSprite() => LoadOrFallback("avatar_rabbit_warrior", () => CreateCircle(128, new Color(0.2f, 0.4f, 0.8f), Color.white, 4));
        public static Sprite GetPetDroneSprite() => LoadOrFallback("pet_carrot_drone", () => CreateCircle(128, new Color(1f, 0.5f, 0.1f), Color.yellow, 4));
        public static Sprite GetChestGoldenSprite() => LoadOrFallback("chest_golden_stageclear", () => CreateRoundedBox(160, 140, 20, new Color(0.85f, 0.65f, 0.15f), Color.white, 4));
        public static Sprite GetChestPurpleSprite() => LoadOrFallback("chest_royal_purple", () => CreateRoundedBox(160, 140, 20, new Color(0.6f, 0.2f, 0.8f), new Color(1f, 0.85f, 0.3f), 4));
        public static Sprite GetLogoSprite() => LoadOrFallback("logo_apocarrot", () => CreateRoundedBox(300, 100, 16, new Color(1f, 0.5f, 0.1f), Color.white, 3));
        public static Sprite GetPlayButtonSprite() => LoadOrFallback("btn_play_glow", () => CreateGradientBox(240, 80, 40, new Color(1f, 0.75f, 0.2f), new Color(0.95f, 0.45f, 0.05f), new Color(1f, 0.9f, 0.5f), 4));
        public static Sprite GetAttackButtonSprite() => LoadOrFallback("btn_attack_sword", () => CreateCircle(130, new Color(0.95f, 0.4f, 0.05f), new Color(1f, 0.85f, 0.3f), 6));

        private static Sprite LoadOrFallback(string assetName, Func<Sprite> fallbackGenerator)
        {
            if (cache.TryGetValue(assetName, out Sprite s) && s != null) return s;

            // 1. Thử Resources.Load
            Sprite loaded = Resources.Load<Sprite>($"UI/{assetName}");
            if (loaded == null)
            {
                Texture2D tex = Resources.Load<Texture2D>($"UI/{assetName}");
                if (tex != null)
                {
                    loaded = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }

            // 2. Thử nạp từ Application.dataPath nếu chạy trong Editor
            if (loaded == null)
            {
                string pathJpg = Path.Combine(Application.dataPath, "Resources", "UI", $"{assetName}.jpg");
                string pathPng = Path.Combine(Application.dataPath, "Resources", "UI", $"{assetName}.png");
                string finalPath = File.Exists(pathJpg) ? pathJpg : (File.Exists(pathPng) ? pathPng : null);

                if (finalPath != null)
                {
                    try
                    {
                        byte[] bytes = File.ReadAllBytes(finalPath);
                        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        if (tex.LoadImage(bytes))
                        {
                            tex.filterMode = FilterMode.Bilinear;
                            loaded = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                        }
                    }
                    catch (Exception) { }
                }
            }

            // 3. Fallback generator nếu không tìm thấy tệp
            if (loaded == null && fallbackGenerator != null)
            {
                loaded = fallbackGenerator();
            }

            if (loaded != null) cache[assetName] = loaded;
            return loaded;
        }

        // ========================== 2. PROCEDURAL STYLIZED SPRITES ==========================

        /// <summary>
        /// Tạo thẻ hoặc nền bo góc với màu nền và viền (Border)
        /// </summary>
        public static Sprite CreateRoundedBox(int width, int height, int radius, Color fill, Color border, int borderWidth = 3)
        {
            string key = $"rect_{width}_{height}_{radius}_{fill}_{border}_{borderWidth}";
            if (cache.TryGetValue(key, out Sprite s) && s != null) return s;

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[width * height];

            float r2 = radius * radius;
            float innerR = Mathf.Max(0, radius - borderWidth);
            float innerR2 = innerR * innerR;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    int cx = x < radius ? radius : (x >= width - radius ? width - radius - 1 : x);
                    int cy = y < radius ? radius : (y >= height - radius ? height - radius - 1 : y);

                    float distSq = (x - cx) * (x - cx) + (y - cy) * (y - cy);

                    if (distSq > r2)
                    {
                        pixels[index] = Color.clear;
                    }
                    else if (borderWidth > 0 && (distSq >= innerR2 || x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth))
                    {
                        pixels[index] = border;
                    }
                    else
                    {
                        pixels[index] = fill;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Tạo nút bấm / thẻ có dải màu chuyển Gradient từ trên xuống dưới kèm viền nổi 3D
        /// </summary>
        public static Sprite CreateGradientBox(int width, int height, int radius, Color topColor, Color bottomColor, Color border, int borderWidth = 3)
        {
            string key = $"grad_{width}_{height}_{radius}_{topColor}_{bottomColor}_{border}_{borderWidth}";
            if (cache.TryGetValue(key, out Sprite s) && s != null) return s;

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[width * height];

            float r2 = radius * radius;
            float innerR = Mathf.Max(0, radius - borderWidth);
            float innerR2 = innerR * innerR;

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height;
                Color currentFill = Color.Lerp(bottomColor, topColor, t);

                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    int cx = x < radius ? radius : (x >= width - radius ? width - radius - 1 : x);
                    int cy = y < radius ? radius : (y >= height - radius ? height - radius - 1 : y);

                    float distSq = (x - cx) * (x - cx) + (y - cy) * (y - cy);

                    if (distSq > r2)
                    {
                        pixels[index] = Color.clear;
                    }
                    else if (borderWidth > 0 && (distSq >= innerR2 || x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth))
                    {
                        pixels[index] = border;
                    }
                    else
                    {
                        pixels[index] = currentFill;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Tạo Sprite hình tròn (cho Avatar, Nút bấm, Icon)
        /// </summary>
        public static Sprite CreateCircle(int diameter, Color fill, Color border, int borderWidth = 3)
        {
            string key = $"circle_{diameter}_{fill}_{border}_{borderWidth}";
            if (cache.TryGetValue(key, out Sprite s) && s != null) return s;

            Texture2D tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[diameter * diameter];

            float radius = diameter * 0.5f;
            float r2 = radius * radius;
            float innerR = radius - borderWidth;
            float innerR2 = innerR * innerR;

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x - radius + 0.5f;
                    float dy = y - radius + 0.5f;
                    float d2 = dx * dx + dy * dy;

                    int index = y * diameter + x;
                    if (d2 > r2)
                    {
                        pixels[index] = Color.clear;
                    }
                    else if (borderWidth > 0 && d2 >= innerR2)
                    {
                        pixels[index] = border;
                    }
                    else
                    {
                        pixels[index] = fill;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), 100f);
            cache[key] = sprite;
            return sprite;
        }

        // ========================== 3. UI STYLE PRESETS ==========================

        public static Color DarkCardBg => new Color(0.10f, 0.12f, 0.19f, 0.95f);      // Slate Dark #1A1F30
        public static Color DarkCardBorder => new Color(0.18f, 0.22f, 0.35f, 1.0f);   // Slate Border #2E3859
        public static Color PillBg => new Color(0.08f, 0.10f, 0.15f, 0.90f);          // Dark pill container
        public static Color OrangeGlow => new Color(1.0f, 0.60f, 0.10f);              // Primary carrot orange
        public static Color GoldYellow => new Color(1.0f, 0.84f, 0.20f);              // Accent gold

        /// <summary>
        /// Khung thẻ trang bị theo phẩm chất (Common, Great, Rare, Epic, Legendary)
        /// </summary>
        public static Sprite GetRarityCardSlot(EquipmentRarity rarity, int size = 80)
        {
            Color rarityBorder;
            Color rarityBg;

            switch (rarity)
            {
                case EquipmentRarity.Legendary:
                    rarityBorder = new Color(1.0f, 0.65f, 0.10f); // Cam vàng
                    rarityBg = new Color(0.28f, 0.18f, 0.08f, 0.95f);
                    break;
                case EquipmentRarity.Epic:
                    rarityBorder = new Color(0.72f, 0.30f, 0.98f); // Tím neon
                    rarityBg = new Color(0.22f, 0.12f, 0.30f, 0.95f);
                    break;
                case EquipmentRarity.Rare:
                    rarityBorder = new Color(0.22f, 0.55f, 1.0f);  // Xanh lam
                    rarityBg = new Color(0.10f, 0.18f, 0.32f, 0.95f);
                    break;
                case EquipmentRarity.Great:
                    rarityBorder = new Color(0.20f, 0.85f, 0.35f); // Xanh lục
                    rarityBg = new Color(0.10f, 0.25f, 0.15f, 0.95f);
                    break;
                case EquipmentRarity.Common:
                default:
                    rarityBorder = new Color(0.50f, 0.55f, 0.65f); // Xám bạc
                    rarityBg = new Color(0.15f, 0.17f, 0.22f, 0.95f);
                    break;
            }

            return CreateRoundedBox(size, size, 14, rarityBg, rarityBorder, 3);
        }

        /// <summary>
        /// Nút bấm bo tròn màu chuẩn theo thiết kế (Orange CTA, Cyan Equip, Grey Salvage)
        /// </summary>
        public static Sprite GetActionButtonSprite(string buttonType, int width = 160, int height = 50)
        {
            switch (buttonType.ToLower())
            {
                case "play":
                case "start":
                    return CreateGradientBox(width, height, height / 2, new Color(1f, 0.75f, 0.15f), new Color(0.95f, 0.45f, 0.05f), new Color(1f, 0.9f, 0.5f), 3);
                case "equip":
                    return CreateGradientBox(width, height, 12, new Color(0.2f, 0.7f, 1f), new Color(0.1f, 0.45f, 0.85f), new Color(0.6f, 0.9f, 1f), 2);
                case "salvage":
                    return CreateGradientBox(width, height, 12, new Color(0.35f, 0.4f, 0.5f), new Color(0.2f, 0.25f, 0.32f), new Color(0.5f, 0.55f, 0.65f), 2);
                case "upgrade":
                case "openchest":
                case "confirm":
                    return CreateGradientBox(width, height, 14, new Color(1f, 0.78f, 0.2f), new Color(0.95f, 0.55f, 0.05f), new Color(1f, 0.92f, 0.55f), 3);
                default:
                    return CreateRoundedBox(width, height, 12, DarkCardBg, DarkCardBorder, 2);
            }
        }
    }
}
