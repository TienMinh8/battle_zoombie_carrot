using System.Collections.Generic;
using UnityEngine;

namespace Apocarrot.Visuals
{
    public enum CharacterSpriteType
    {
        RabbitWarrior,
        ZombieCarrotBasic,
        ZombieCarrotFast,
        ZombieCarrotElite,
        KingCarrotBoss,
        CarrotDrone
    }

    /// <summary>
    /// Nhà máy tạo và quản lý bộ Sprite 2.5D cho tất cả các nhân vật trong Apocarrot:
    /// - Thỏ Hiệp Sĩ (Rabbit Warrior)
    /// - Quái Zombie Cà Rốt (Thường, Nhanh, Đột Biến)
    /// - Boss Cà Rốt Hoàng Đế (King Carrot)
    /// - Trợ thủ Carrot Drone
    /// Hỗ trợ load texture từ Resources nếu có hoặc tự động vẽ Sprite chuẩn sắc nét với đường viền (Outline) và bóng đổ (Cel-shading).
    /// </summary>
    public static class SpriteCharacterFactory
    {
        private static readonly Dictionary<CharacterSpriteType, Sprite> cachedSprites = new Dictionary<CharacterSpriteType, Sprite>();

        /// <summary>
        /// Tạo và gắn bộ hiển thị 2.5D Billboard Sprite vào nhân vật
        /// </summary>
        public static SpriteBillboard2D AttachCharacterSprite(GameObject targetObj, CharacterSpriteType type, Vector3 localOffset, float visualScale = 1.0f)
        {
            if (targetObj == null) return null;

            // Xóa các visual 3D cũ (Capsule, Cylinder) nếu có
            Transform existingVisual = targetObj.transform.Find("Visual_Root");
            if (existingVisual != null)
            {
                Object.Destroy(existingVisual.gameObject);
            }
            Transform existingBody = targetObj.transform.Find("Visual_Body");
            if (existingBody != null)
            {
                Object.Destroy(existingBody.gameObject);
            }

            // Tạo GameObject chứa Sprite 2.5D
            GameObject spriteObj = new GameObject("Visual_Sprite2D");
            spriteObj.transform.SetParent(targetObj.transform, false);
            spriteObj.transform.localPosition = localOffset;
            spriteObj.transform.localScale = Vector3.one * visualScale;

            SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrGenerateSprite(type);

            // Sắp xếp thứ tự hiển thị (Sorting order theo trục Y của nhân vật)
            sr.sortingOrder = 1;

            SpriteBillboard2D billboard = spriteObj.AddComponent<SpriteBillboard2D>();
            return billboard;
        }

        public static Sprite GetOrGenerateSprite(CharacterSpriteType type)
        {
            if (cachedSprites.TryGetValue(type, out Sprite existing) && existing != null)
            {
                return existing;
            }

            Sprite generated = null;
            switch (type)
            {
                case CharacterSpriteType.RabbitWarrior:
                    generated = TryLoadResourceSprite("UI/avatar_rabbit_warrior", 1.8f) ?? GenerateRabbitWarriorSprite();
                    break;
                case CharacterSpriteType.CarrotDrone:
                    generated = TryLoadResourceSprite("UI/pet_carrot_drone", 1.0f) ?? GenerateCarrotDroneSprite();
                    break;
                case CharacterSpriteType.ZombieCarrotBasic:
                    generated = GenerateZombieCarrotSprite(new Color(1f, 0.48f, 0.08f), new Color(0.2f, 0.75f, 0.28f), new Color(1f, 0.15f, 0.15f));
                    break;
                case CharacterSpriteType.ZombieCarrotFast:
                    generated = GenerateZombieTomatoSprite();
                    break;
                case CharacterSpriteType.ZombieCarrotElite:
                    generated = GenerateZombieCarrotSprite(new Color(0.72f, 0.2f, 0.85f), new Color(0.15f, 0.85f, 0.45f), new Color(1f, 0.85f, 0.1f));
                    break;
                case CharacterSpriteType.KingCarrotBoss:
                    generated = GenerateKingCarrotBossSprite();
                    break;
            }

            if (generated != null)
            {
                cachedSprites[type] = generated;
            }

            return generated;
        }

        private static Sprite TryLoadResourceSprite(string resourcePath, float targetHeightUnits)
        {
            Texture2D tex = Resources.Load<Texture2D>(resourcePath);
            if (tex != null)
            {
                float pixelsPerUnit = tex.height / targetHeightUnits;
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.15f), pixelsPerUnit);
            }
            return null;
        }

        // ========================== THỦ TỤC VẼ SPRITE 2D SẮC NÉT ==========================

        /// <summary>
        /// Tạo Sprite Thỏ Hiệp Sĩ (Rabbit Warrior): Tai thỏ dài, mặt ngầu, khăn choàng xanh lá, thanh kiếm cà rốt
        /// </summary>
        public static Sprite GenerateRabbitWarriorSprite()
        {
            int w = 128, h = 160;
            Texture2D tex = CreateEmptyTexture(w, h);

            Color outline = new Color(0.12f, 0.15f, 0.22f);
            Color furWhite = new Color(0.96f, 0.96f, 0.98f);
            Color furShade = new Color(0.82f, 0.85f, 0.90f);
            Color earPink = new Color(0.98f, 0.65f, 0.72f);
            Color scarfGreen = new Color(0.18f, 0.78f, 0.32f);
            Color eyeDark = new Color(0.1f, 0.1f, 0.15f);

            // 1. Tai thỏ (Trái & Phải)
            DrawFilledEllipse(tex, 44, 126, 11, 28, furWhite, outline, 2);
            DrawFilledEllipse(tex, 44, 126, 6, 20, earPink, Color.clear, 0);

            DrawFilledEllipse(tex, 84, 126, 11, 28, furWhite, outline, 2);
            DrawFilledEllipse(tex, 84, 126, 6, 20, earPink, Color.clear, 0);

            // 2. Thân thỏ
            DrawFilledEllipse(tex, 64, 46, 26, 32, furShade, outline, 3);
            DrawFilledEllipse(tex, 64, 48, 24, 30, furWhite, Color.clear, 0);

            // 3. Đầu thỏ
            DrawFilledEllipse(tex, 64, 88, 30, 26, furWhite, outline, 3);

            // 4. Mắt Thỏ (Hiệp sĩ ngầu)
            DrawFilledCircle(tex, 52, 92, 5, eyeDark);
            DrawFilledCircle(tex, 76, 92, 5, eyeDark);
            DrawFilledCircle(tex, 54, 94, 2, Color.white); // Đốm sáng mắt
            DrawFilledCircle(tex, 78, 94, 2, Color.white);

            // Mũi hồng
            DrawFilledCircle(tex, 64, 84, 3, earPink);

            // 5. Khăn choàng xanh lá (Scarf)
            DrawFilledEllipse(tex, 64, 72, 28, 9, scarfGreen, outline, 2);
            DrawFilledEllipse(tex, 82, 60, 9, 16, scarfGreen, outline, 2); // Đuôi khăn bay

            // 6. Kiếm cà rốt bên hông
            DrawFilledEllipse(tex, 96, 62, 5, 22, new Color(1f, 0.5f, 0.1f), outline, 2); // Lưỡi kiếm cam
            DrawFilledCircle(tex, 96, 84, 6, new Color(0.2f, 0.8f, 0.3f)); // Chuôi kiếm lá xanh

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.12f), 64f);
        }

        /// <summary>
        /// Tạo Sprite Quái Zombie Cà Rốt: Dáng quả cà rốt hung tợn, mắt phát sáng, lá trên đầu
        /// </summary>
        public static Sprite GenerateZombieCarrotSprite(Color carrotColor, Color leafColor, Color eyeColor)
        {
            int w = 110, h = 150;
            Texture2D tex = CreateEmptyTexture(w, h);

            Color outline = new Color(0.15f, 0.1f, 0.08f);

            // 1. Chùm lá trên đầu
            DrawFilledEllipse(tex, 55, 126, 8, 20, leafColor, outline, 2);
            DrawFilledEllipse(tex, 40, 122, 7, 16, leafColor, outline, 2);
            DrawFilledEllipse(tex, 70, 122, 7, 16, leafColor, outline, 2);

            // 2. Thân củ cà rốt (Hình thuôn nhọn dần xuống dưới)
            for (int y = 14; y < 114; y++)
            {
                float progress = (float)(y - 14) / 100f; // 0 ở đáy, 1 ở đỉnh
                float rx = Mathf.Lerp(8f, 32f, Mathf.Sqrt(progress));

                int cx = 55;
                int minX = Mathf.RoundToInt(cx - rx);
                int maxX = Mathf.RoundToInt(cx + rx);

                for (int x = minX; x <= maxX; x++)
                {
                    if (x == minX || x == maxX || y == 14)
                    {
                        tex.SetPixel(x, y, outline);
                    }
                    else
                    {
                        // Đổ bóng cel-shading trái/phải
                        float shade = (float)(x - minX) / (maxX - minX);
                        Color c = Color.Lerp(carrotColor * 0.75f, carrotColor, shade);
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            // 3. Mắt quái vật Zombie phát sáng
            DrawFilledCircle(tex, 44, 76, 6, eyeColor);
            DrawFilledCircle(tex, 66, 76, 6, eyeColor);
            DrawFilledCircle(tex, 44, 76, 2, Color.white);
            DrawFilledCircle(tex, 66, 76, 2, Color.white);

            // 4. Miệng quái vật răng cưa lởm chởm
            DrawFilledEllipse(tex, 55, 54, 14, 5, outline, Color.clear, 0);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.12f), 64f);
        }

        /// <summary>
        /// Tạo Sprite Quái Cà Chua (Zombie Tomato Fast): Tròn, tốc độ nhanh, mắt tinh quái
        /// </summary>
        public static Sprite GenerateZombieTomatoSprite()
        {
            int w = 110, h = 110;
            Texture2D tex = CreateEmptyTexture(w, h);

            Color outline = new Color(0.2f, 0.05f, 0.05f);
            Color redBody = new Color(0.95f, 0.22f, 0.18f);
            Color redShade = new Color(0.72f, 0.12f, 0.10f);
            Color greenLeaves = new Color(0.22f, 0.85f, 0.32f);

            // 1. Thân quả cà chua tròn
            DrawFilledCircle(tex, 55, 48, 38, outline);
            DrawFilledCircle(tex, 55, 48, 36, redShade);
            DrawFilledCircle(tex, 58, 52, 33, redBody);

            // 2. Cuống lá cà chua
            DrawFilledEllipse(tex, 55, 86, 16, 7, greenLeaves, outline, 2);
            DrawFilledCircle(tex, 55, 92, 5, greenLeaves);

            // 3. Đôi mắt tinh ranh sắc lẹm
            DrawFilledEllipse(tex, 42, 56, 7, 5, Color.yellow, outline, 2);
            DrawFilledEllipse(tex, 68, 56, 7, 5, Color.yellow, outline, 2);
            DrawFilledCircle(tex, 44, 56, 2, Color.black);
            DrawFilledCircle(tex, 70, 56, 2, Color.black);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.12f), 64f);
        }

        /// <summary>
        /// Tạo Sprite Boss King Carrot: Cà rốt khổng lồ, vương miện vàng, hồng ngọc, chùm lá hoàng đế
        /// </summary>
        public static Sprite GenerateKingCarrotBossSprite()
        {
            int w = 180, h = 240;
            Texture2D tex = CreateEmptyTexture(w, h);

            Color outline = new Color(0.12f, 0.08f, 0.05f);
            Color kingOrange = new Color(0.98f, 0.44f, 0.05f);
            Color crownGold = new Color(1.0f, 0.84f, 0.0f);
            Color rubyRed = new Color(0.95f, 0.05f, 0.18f);
            Color leafGreen = new Color(0.15f, 0.75f, 0.25f);

            // 1. Thân khổng lồ
            for (int y = 20; y < 165; y++)
            {
                float progress = (float)(y - 20) / 145f;
                float rx = Mathf.Lerp(14f, 54f, Mathf.Sqrt(progress));

                int cx = 90;
                int minX = Mathf.RoundToInt(cx - rx);
                int maxX = Mathf.RoundToInt(cx + rx);

                for (int x = minX; x <= maxX; x++)
                {
                    if (x == minX || x == maxX || y == 20)
                    {
                        tex.SetPixel(x, y, outline);
                    }
                    else
                    {
                        float shade = (float)(x - minX) / (maxX - minX);
                        Color c = Color.Lerp(kingOrange * 0.75f, kingOrange, shade);
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            // 2. Chùm lá hoàng đế
            DrawFilledEllipse(tex, 90, 205, 12, 30, leafGreen, outline, 2);
            DrawFilledEllipse(tex, 65, 195, 10, 26, leafGreen, outline, 2);
            DrawFilledEllipse(tex, 115, 195, 10, 26, leafGreen, outline, 2);

            // 3. Vương Miện Hoàng Đế (Crown 👑)
            DrawFilledBox(tex, 60, 165, 60, 20, crownGold, outline, 2);
            DrawFilledCircle(tex, 62, 185, 7, crownGold); // Chóp trái
            DrawFilledCircle(tex, 90, 190, 9, crownGold); // Chóp giữa cao
            DrawFilledCircle(tex, 118, 185, 7, crownGold); // Chóp phải

            // Viên hồng ngọc giữa vương miện
            DrawFilledCircle(tex, 90, 175, 6, rubyRed);
            DrawFilledCircle(tex, 92, 177, 2, Color.white);

            // 4. Mắt quỷ đỏ rực
            DrawFilledCircle(tex, 72, 115, 9, rubyRed);
            DrawFilledCircle(tex, 108, 115, 9, rubyRed);
            DrawFilledCircle(tex, 72, 115, 3, Color.yellow);
            DrawFilledCircle(tex, 108, 115, 3, Color.yellow);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.1f), 64f);
        }

        /// <summary>
        /// Tạo Sprite Trợ thủ Carrot Drone: Con thoi cam, kính phi công xanh neon, cánh quạt vàng
        /// </summary>
        public static Sprite GenerateCarrotDroneSprite()
        {
            int w = 96, h = 96;
            Texture2D tex = CreateEmptyTexture(w, h);

            Color outline = new Color(0.1f, 0.12f, 0.15f);
            Color bodyOrange = new Color(1f, 0.52f, 0.12f);
            Color gogglesCyan = new Color(0.15f, 0.88f, 1.0f);
            Color propYellow = new Color(0.98f, 0.95f, 0.25f);

            // 1. Thân drone
            DrawFilledEllipse(tex, 48, 42, 28, 22, bodyOrange, outline, 2);

            // 2. Kính phi công xanh neon
            DrawFilledEllipse(tex, 48, 44, 22, 10, gogglesCyan, outline, 2);
            DrawFilledCircle(tex, 44, 46, 3, Color.white); // Đốm phản chiếu kính

            // 3. Cánh quạt quay trên đỉnh
            DrawFilledBox(tex, 45, 64, 6, 8, outline, Color.clear, 0); // Trục
            DrawFilledEllipse(tex, 48, 72, 36, 5, propYellow, outline, 2); // Cánh quạt ngang

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.35f), 64f);
        }

        // ========================== LOOT PICKUP SPRITES ==========================

        public static Sprite GenerateCoinSprite()
        {
            int size = 64;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color outline = new Color(0.45f, 0.32f, 0.05f);
            Color goldBase = new Color(1.0f, 0.84f, 0.05f);
            Color goldShine = new Color(1.0f, 0.95f, 0.45f);

            DrawFilledCircle(tex, 32, 32, 26, outline);
            DrawFilledCircle(tex, 32, 32, 24, goldBase);
            DrawFilledCircle(tex, 32, 32, 17, outline);
            DrawFilledCircle(tex, 32, 32, 15, goldShine);

            // Chữ C biểu tượng Carrot coin ở tâm
            DrawFilledBox(tex, 28, 26, 8, 12, outline, Color.clear, 0);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        }

        public static Sprite GenerateHeartSprite()
        {
            int size = 64;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color outline = new Color(0.1f, 0.4f, 0.15f);
            Color greenHeart = new Color(0.2f, 0.95f, 0.35f);

            // Hai nửa bán cầu trên của trái tim
            DrawFilledCircle(tex, 23, 38, 14, outline);
            DrawFilledCircle(tex, 41, 38, 14, outline);
            DrawFilledCircle(tex, 23, 38, 12, greenHeart);
            DrawFilledCircle(tex, 41, 38, 12, greenHeart);

            // Đáy nhọn trái tim
            for (int y = 10; y <= 38; y++)
            {
                float t = (float)(y - 10) / 28f;
                int hw = Mathf.RoundToInt(Mathf.Lerp(2f, 22f, t));
                for (int x = 32 - hw; x <= 32 + hw; x++)
                {
                    tex.SetPixel(x, y, (x == 32 - hw || x == 32 + hw || y == 10) ? outline : greenHeart);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        }

        /// <summary>
        /// Gắn visual 2.5D Billboard Sprite cho vật phẩm rơi dưới đất
        /// </summary>
        public static SpriteBillboard2D AttachLootSprite(GameObject targetObj, Sprite sprite, float scale = 0.65f)
        {
            if (targetObj == null || sprite == null) return null;

            GameObject spriteObj = new GameObject("Visual_LootSprite2D");
            spriteObj.transform.SetParent(targetObj.transform, false);
            spriteObj.transform.localPosition = Vector3.zero;
            spriteObj.transform.localScale = Vector3.one * scale;

            SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 0;

            SpriteBillboard2D billboard = spriteObj.AddComponent<SpriteBillboard2D>();
            return billboard;
        }

        // ========================== TIỆN ÍCH VẼ SHAPE ==========================

        private static Texture2D CreateEmptyTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] clear = new Color[w * h];
            tex.SetPixels(clear);
            return tex;
        }

        private static void DrawFilledCircle(Texture2D tex, int cx, int cy, int r, Color color)
        {
            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    if (x < 0 || x >= tex.width || y < 0 || y >= tex.height) continue;
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static void DrawFilledEllipse(Texture2D tex, int cx, int cy, int rx, int ry, Color fillColor, Color outlineColor, int outlineWidth)
        {
            int minX = Mathf.Max(0, cx - rx - outlineWidth);
            int maxX = Mathf.Min(tex.width - 1, cx + rx + outlineWidth);
            int minY = Mathf.Max(0, cy - ry - outlineWidth);
            int maxY = Mathf.Min(tex.height - 1, cy + ry + outlineWidth);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = (float)(x - cx) / rx;
                    float dy = (float)(y - cy) / ry;
                    float dInner = dx * dx + dy * dy;

                    if (dInner <= 1.0f)
                    {
                        tex.SetPixel(x, y, fillColor);
                    }
                    else if (outlineWidth > 0)
                    {
                        float dxOut = (float)(x - cx) / (rx + outlineWidth);
                        float dyOut = (float)(y - cy) / (ry + outlineWidth);
                        if (dxOut * dxOut + dyOut * dyOut <= 1.0f)
                        {
                            tex.SetPixel(x, y, outlineColor);
                        }
                    }
                }
            }
        }

        private static void DrawFilledBox(Texture2D tex, int x, int y, int w, int h, Color fillColor, Color outlineColor, int outlineWidth)
        {
            for (int py = y - outlineWidth; py < y + h + outlineWidth; py++)
            {
                for (int px = x - outlineWidth; px < x + w + outlineWidth; px++)
                {
                    if (px < 0 || px >= tex.width || py < 0 || py >= tex.height) continue;
                    bool isOutline = px < x || px >= x + w || py < y || py >= y + h;
                    if (isOutline && outlineWidth > 0)
                    {
                        tex.SetPixel(px, py, outlineColor);
                    }
                    else if (!isOutline)
                    {
                        tex.SetPixel(px, py, fillColor);
                    }
                }
            }
        }
    }
}
