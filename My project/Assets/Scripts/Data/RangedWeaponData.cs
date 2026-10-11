using UnityEngine;

namespace Apocarrot.Data
{
    [CreateAssetMenu(fileName = "NewRangedWeapon", menuName = "Apocarrot/Combat/Ranged Weapon Data")]
    public class RangedWeaponData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string weaponId = "tomato_blaster";
        [SerializeField] private string weaponName = "Tomato Blaster";
        [SerializeField] private int level = 1;

        [Header("Combat Attributes")]
        [Tooltip("Sát thương mỗi viên đạn")]
        [SerializeField] private float damage = 22f;

        [Tooltip("Thời gian hồi giữa các lượt bắn (giây)")]
        [SerializeField] private float fireCooldown = 1.0f;

        [Tooltip("Tầm bắn quét mục tiêu tối đa (mét)")]
        [SerializeField] private float range = 8.5f;

        [Tooltip("Tốc độ bay của viên đạn (m/s)")]
        [SerializeField] private float projectileSpeed = 15.0f;

        [Tooltip("Số lượng viên đạn mỗi lượt bắn (1 cho cà chua, 3 cho nho chùm)")]
        [SerializeField] private int projectileCount = 1;

        [Tooltip("Góc tỏa hình quạt khi bắn nhiều viên (độ)")]
        [SerializeField] private float spreadAngle = 0f;

        [Header("Visual Presentation")]
        [SerializeField] private Color projectileColor = new Color(0.95f, 0.22f, 0.15f); // Đỏ cà chua
        [SerializeField] private Vector3 projectileScale = new Vector3(0.45f, 0.45f, 0.45f);
        [SerializeField] private string description = "Pháo cà chua tự động bắn viên đạn đỏ mọng tốc độ cao.";

        public string WeaponId => weaponId;
        public string WeaponName => weaponName;
        public int Level => level;
        public float Damage => damage;
        public float FireCooldown => fireCooldown;
        public float Range => range;
        public float ProjectileSpeed => projectileSpeed;
        public int ProjectileCount => projectileCount;
        public float SpreadAngle => spreadAngle;
        public Color ProjectileColor => projectileColor;
        public Vector3 ProjectileScale => projectileScale;
        public string Description => description;

        /// <summary>
        /// Tạo cấu hình Tomato Blaster (Lv.1): 1 viên đạn cà chua đỏ, tốc độ bay nhanh, bắn tập trung
        /// </summary>
        public static RangedWeaponData CreateTomatoBlaster(int level = 1)
        {
            var data = CreateInstance<RangedWeaponData>();
            data.weaponId = "tomato_blaster";
            data.weaponName = "Tomato Blaster";
            data.level = level;
            data.damage = 22f * level;
            data.fireCooldown = 0.95f;
            data.range = 9.0f;
            data.projectileSpeed = 16.0f;
            data.projectileCount = 1;
            data.spreadAngle = 0f;
            data.projectileColor = new Color(0.95f, 0.18f, 0.15f); // Đỏ cà chua
            data.projectileScale = new Vector3(0.45f, 0.45f, 0.45f);
            data.description = "Bắn đạn cà chua đỏ mọng trực diện, hồi chiêu nhanh.";
            return data;
        }

        /// <summary>
        /// Tạo cấu hình Grape Splitter (Lv.1): Chùm 3 viên đạn nho tím tỏa quạt 30 độ, diện rộng
        /// </summary>
        public static RangedWeaponData CreateGrapeSplitter(int level = 1)
        {
            var data = CreateInstance<RangedWeaponData>();
            data.weaponId = "grape_splitter";
            data.weaponName = "Grape Splitter";
            data.level = level;
            data.damage = 15f * level;
            data.fireCooldown = 1.75f;
            data.range = 8.0f;
            data.projectileSpeed = 12.0f;
            data.projectileCount = 3;
            data.spreadAngle = 32f;
            data.projectileColor = new Color(0.65f, 0.15f, 0.95f); // Tím nho
            data.projectileScale = new Vector3(0.35f, 0.35f, 0.35f);
            data.description = "Bắn chùm 3 viên đạn nho tím tỏa quạt góc rộng, sát thương đa mục tiêu.";
            return data;
        }
    }
}
