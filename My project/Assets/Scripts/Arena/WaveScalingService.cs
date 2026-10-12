using System.Collections.Generic;
using UnityEngine;
using Apocarrot.Data;

namespace Apocarrot.Arena
{
    /// <summary>
    /// Dịch vụ Thuật Toán Tính Toán và Tự Sinh Cấp Độ Quái (Monster & Wave Scaling Service).
    /// Giải quyết triệt để vấn đề quái quá ít / quá yếu hoặc vũ khí quá mạnh:
    /// 1. Công thức Máu (HP Multiplier) tự động tăng 25% mỗi cấp Wave:
    ///    HP(W) = BaseHP * (1.0 + (W - 1) * 0.25)
    /// 2. Công thức Sát Thương (ATK Multiplier):
    ///    ATK(W) = BaseATK * (1.0 + (W - 1) * 0.12)
    /// 3. Thuật toán phân bổ số lượng quái theo nhịp điệu (Pacing & Wave Density):
    ///    Wave 1 (10 con) -> Wave 2 (16 con) -> Wave 3 (24 con) -> Wave 4 (Boss 1,200 HP + Đệ tử).
    /// 4. Hỗ trợ tự sinh Wave vô tận (Infinite / Endless Mode) cho các cấp độ cao hơn.
    /// </summary>
    public static class WaveScalingService
    {
        /// <summary>
        /// Hệ số máu theo Wave: Wave 1 = 1.0x, Wave 2 = 1.25x, Wave 3 = 1.50x, Wave 4 = 1.75x...
        /// </summary>
        public static float GetHealthMultiplier(int waveNumber)
        {
            if (waveNumber <= 1) return 1.0f;
            return 1.0f + (waveNumber - 1) * 0.25f;
        }

        /// <summary>
        /// Hệ số sát thương theo Wave: tăng 12% mỗi Wave để tăng độ nguy hiểm
        /// </summary>
        public static float GetDamageMultiplier(int waveNumber)
        {
            if (waveNumber <= 1) return 1.0f;
            return 1.0f + (waveNumber - 1) * 0.12f;
        }

        /// <summary>
        /// Hệ số tốc độ di chuyển nhẹ (tối đa 1.2x để không bị quá nhanh)
        /// </summary>
        public static float GetSpeedMultiplier(int waveNumber)
        {
            if (waveNumber <= 1) return 1.0f;
            return Mathf.Min(1.20f, 1.0f + (waveNumber - 1) * 0.04f);
        }

        /// <summary>
        /// Tạo EnemyData tự động scale theo Wave Level
        /// </summary>
        public static EnemyData CreateScaledEnemyData(EnemyVariantType type, int waveNumber)
        {
            EnemyData baseData;
            switch (type)
            {
                case EnemyVariantType.Fast:
                    baseData = EnemyData.CreateFast();
                    break;
                case EnemyVariantType.Elite:
                    baseData = EnemyData.CreateElite();
                    break;
                case EnemyVariantType.Basic:
                default:
                    baseData = EnemyData.CreateBasic();
                    break;
            }

            float hpMult = GetHealthMultiplier(waveNumber);
            float dmgMult = GetDamageMultiplier(waveNumber);
            float spdMult = GetSpeedMultiplier(waveNumber);

            // Clone và scale chỉ số
            var scaledData = ScriptableObject.CreateInstance<EnemyData>();
            
            // Sao chép và áp dụng hệ số toán học
            SetDataFields(scaledData, baseData, hpMult, dmgMult, spdMult, waveNumber);

            return scaledData;
        }

        private static void SetDataFields(EnemyData target, EnemyData source, float hpMult, float dmgMult, float spdMult, int wave)
        {
            // Sử dụng reflection hoặc gán giá trị hợp lệ
            var t = typeof(EnemyData);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            t.GetField("variantType", flags)?.SetValue(target, source.VariantType);
            t.GetField("enemyName", flags)?.SetValue(target, $"{source.EnemyName} (W{wave})");
            t.GetField("maxHealth", flags)?.SetValue(target, Mathf.Round(source.MaxHealth * hpMult));
            t.GetField("attackDamage", flags)?.SetValue(target, Mathf.Round(source.AttackDamage * dmgMult));
            t.GetField("moveSpeed", flags)?.SetValue(target, source.MoveSpeed * spdMult);
            t.GetField("attackRange", flags)?.SetValue(target, source.AttackRange);
            t.GetField("windupDuration", flags)?.SetValue(target, source.WindupDuration);
            t.GetField("attackCooldown", flags)?.SetValue(target, source.AttackCooldown);
            t.GetField("scaleMultiplier", flags)?.SetValue(target, source.ScaleMultiplier);
            t.GetField("bodyColor", flags)?.SetValue(target, source.BodyColor);
            t.GetField("leafColor", flags)?.SetValue(target, source.LeafColor);
            t.GetField("eyeColor", flags)?.SetValue(target, source.EyeColor);
            t.GetField("goldDropChance", flags)?.SetValue(target, source.GoldDropChance);
            t.GetField("minGold", flags)?.SetValue(target, source.MinGold);
            t.GetField("maxGold", flags)?.SetValue(target, source.MaxGold);
            t.GetField("weaponDropChance", flags)?.SetValue(target, source.WeaponDropChance);
            t.GetField("healDropChance", flags)?.SetValue(target, source.HealDropChance);
            t.GetField("healAmount", flags)?.SetValue(target, source.HealAmount);
        }

        /// <summary>
        /// Tạo danh sách các Wave được cân bằng chuẩn xác về nhịp độ sinh quái (Pacing Algorithm)
        /// </summary>
        public static List<WaveData> GenerateBalancedWaves()
        {
            var waves = new List<WaveData>();

            // ================= WAVE 1: BẦY CÀ RỐT KHỞI ĐẦU (10 con) =================
            // Nhịp: 4 con đầu trận -> 3 con tiếp theo -> 3 con đợt cuối
            var w1 = new WaveData
            {
                WaveNumber = 1,
                WaveTitle = "WAVE 1: BẦY CÀ RỐT ĐÓI KHÁT",
                WaveDescription = "Tiêu diệt 10 Zombie Carrot cơ bản, làm quen nhịp vung kiếm và thu thập chiến lợi phẩm!",
                IsBossWave = false
            };
            w1.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 4, 0.5f));
            w1.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 3, 4.0f));
            w1.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 3, 7.5f));
            waves.Add(w1);

            // ================= WAVE 2: ĐỘT BIẾN TỐC ĐỘ (16 con) =================
            // Nhịp: 4 Basic + 2 Fast -> 4 Fast lao nhanh -> 4 Basic + 2 Fast
            var w2 = new WaveData
            {
                WaveNumber = 2,
                WaveTitle = "WAVE 2: CÀ RỐT BIẾN DỊ TỐC ĐỘ",
                WaveDescription = "Bầy quái 16 con! Cẩn thận quái đỏ di chuyển cực nhanh, giữ cự ly và chém đúng nhịp!",
                IsBossWave = false
            };
            w2.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 4, 0.5f));
            w2.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, 2, 2.0f));
            w2.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, 4, 5.0f));
            w2.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 4, 7.5f));
            w2.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, 2, 9.0f));
            waves.Add(w2);

            // ================= WAVE 3: TINH NHUỆ & SINH TỒN ÁP LỰC (24 con) =================
            // Nhịp quái ùa vào đông đúc, có quái Tinh Nhuệ khổng lồ
            var w3 = new WaveData
            {
                WaveNumber = 3,
                WaveTitle = "WAVE 3: BẦY TINH NHUỆ XÂM LĂNG",
                WaveDescription = "24 quái tràn ngập đấu trường! Áp lực sinh tồn đỉnh điểm trước khi Chúa Tể xuất hiện!",
                IsBossWave = false
            };
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 5, 0.5f));
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, 3, 2.0f));
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Elite, 1, 4.0f)); // Elite 1
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 4, 6.0f));
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, 4, 7.5f));
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Elite, 1, 9.0f)); // Elite 2
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 4, 11.0f));
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, 2, 12.5f));
            waves.Add(w3);

            // ================= WAVE 4: BOSS CHÚA TỂ KING CARROT (1 Boss + Đệ tử hỗ trợ) =================
            var w4 = new WaveData
            {
                WaveNumber = 4,
                WaveTitle = "WAVE 4: CHÚA TỂ CÀ RỐT KING CARROT 👑",
                WaveDescription = "Trận chiến sinh tử! Né chiêu dậm đất AoE (Ground Slam) và tiêu diệt King Carrot!",
                IsBossWave = true
            };
            w4.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 2, 2.0f));
            w4.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, 2, 8.0f));
            w4.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 2, 16.0f));
            waves.Add(w4);

            return waves;
        }

        /// <summary>
        /// Thuật toán sinh tự động cho các Wave cấp cao hơn (Endless Mode)
        /// </summary>
        public static WaveData GenerateEndlessWave(int waveNumber)
        {
            int totalEnemies = 18 + (waveNumber - 4) * 6;
            var wave = new WaveData
            {
                WaveNumber = waveNumber,
                WaveTitle = $"WAVE {waveNumber}: CƠN ÁC MỘNG VÔ TẬN",
                WaveDescription = $"Đợt quái cấp {waveNumber}! Đấu trường tràn ngập {totalEnemies} kẻ thù hung hãn!",
                IsBossWave = (waveNumber % 4 == 0)
            };

            int basicCount = Mathf.RoundToInt(totalEnemies * 0.5f);
            int fastCount = Mathf.RoundToInt(totalEnemies * 0.35f);
            int eliteCount = Mathf.Max(1, totalEnemies - basicCount - fastCount);

            wave.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, basicCount / 2, 0.5f));
            wave.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, fastCount / 2, 2.5f));
            wave.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Elite, eliteCount, 5.0f));
            wave.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, basicCount - (basicCount / 2), 7.5f));
            wave.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, fastCount - (fastCount / 2), 9.5f));

            return wave;
        }
    }
}
