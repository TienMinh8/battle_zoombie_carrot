using System;
using System.Collections.Generic;
using UnityEngine;

namespace Apocarrot.Data
{
    [Serializable]
    public class EnemySpawnGroup
    {
        public EnemyVariantType Type;
        public int Count;
        public float DelayBeforeSpawn; // Giây chờ trước khi đợt quái này xuất hiện

        public EnemySpawnGroup(EnemyVariantType type, int count, float delay = 0f)
        {
            Type = type;
            Count = count;
            DelayBeforeSpawn = delay;
        }
    }

    /// <summary>
    /// Dữ liệu cấu hình cho từng Wave trong trận đấu (Gate 6).
    /// </summary>
    [Serializable]
    public class WaveData
    {
        public int WaveNumber;
        public string WaveTitle;
        public string WaveDescription;
        public bool IsBossWave;
        public List<EnemySpawnGroup> SpawnGroups = new List<EnemySpawnGroup>();

        public int TotalEnemies
        {
            get
            {
                int total = 0;
                foreach (var g in SpawnGroups) total += g.Count;
                if (IsBossWave) total += 1; // Tính thêm King Carrot
                return total;
            }
        }

        public static List<WaveData> CreateDemoWaves()
        {
            var waves = new List<WaveData>();

            // Wave 1: Quái cơ bản (6 con chia làm 2 đợt: 3 con đầu trận, 3 con sau 4 giây)
            var w1 = new WaveData
            {
                WaveNumber = 1,
                WaveTitle = "WAVE 1: BẦY CÀ RỐT ĐÓI KHÁT",
                WaveDescription = "Tiêu diệt bầy Zombie Carrot cơ bản và thu thập chiến lợi phẩm!",
                IsBossWave = false
            };
            w1.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 3, 0.5f));
            w1.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 3, 5.0f));
            waves.Add(w1);

            // Wave 2: Quái cơ bản + Quái nhanh (8 con: 4 Basic + 4 Fast)
            var w2 = new WaveData
            {
                WaveNumber = 2,
                WaveTitle = "WAVE 2: CÀ RỐT BIẾN DỊ TỐC ĐỘ",
                WaveDescription = "Cẩn thận! Quái nhanh màu tím có tốc độ di chuyển cực cao!",
                IsBossWave = false
            };
            w2.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 3, 0.5f));
            w2.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, 2, 2.0f));
            w2.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, 2, 6.0f));
            w2.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 1, 7.0f));
            waves.Add(w2);

            // Wave 3: Quái hỗn hợp + Quái tinh nhuệ Elite (10 con: 4 Basic, 4 Fast, 2 Elite)
            var w3 = new WaveData
            {
                WaveNumber = 3,
                WaveTitle = "WAVE 3: BẦY TINH NHUỆ XÂM LĂNG",
                WaveDescription = "Quái tinh nhuệ khổng lồ xuất hiện với lượng máu và sát thương lớn!",
                IsBossWave = false
            };
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 2, 0.5f));
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, 3, 2.0f));
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Elite, 1, 4.0f));
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Fast, 1, 7.0f));
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 2, 8.0f));
            w3.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Elite, 1, 10.0f));
            waves.Add(w3);

            // Wave 4: Boss Wave - King Carrot
            var w4 = new WaveData
            {
                WaveNumber = 4,
                WaveTitle = "WAVE 4: CHÚA TỂ CÀ RỐT KING CARROT 👑",
                WaveDescription = "Trận chiến sinh tử! Né chiêu dậm đất AoE và tiêu diệt King Carrot!",
                IsBossWave = true
            };
            // 2 đệ tử quái hỗ trợ tạo cơ hội rơi máu và tương tác
            w4.SpawnGroups.Add(new EnemySpawnGroup(EnemyVariantType.Basic, 2, 1.5f));
            waves.Add(w4);

            return waves;
        }
    }
}
