using UnityEngine;

namespace Apocarrot.Data
{
    public enum EnemyVariantType
    {
        Basic,
        Fast,
        Elite
    }

    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Apocarrot/Enemies/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private EnemyVariantType variantType = EnemyVariantType.Basic;
        [SerializeField] private string enemyName = "Zombie Carrot";

        [Header("Base Stats")]
        [SerializeField] private float maxHealth = 45f;
        [SerializeField] private float moveSpeed = 2.8f;
        [SerializeField] private float attackDamage = 12f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float windupDuration = 0.35f;
        [SerializeField] private float attackCooldown = 1.2f;

        [Header("Visual Feedback")]
        [SerializeField] private float scaleMultiplier = 1.0f;
        [SerializeField] private Color bodyColor = new Color(0.95f, 0.42f, 0.08f); // Cam Zombie
        [SerializeField] private Color leafColor = new Color(0.12f, 0.75f, 0.22f); // Lá xanh
        [SerializeField] private Color eyeColor = Color.red;

        [Header("Loot Configuration")]
        [Range(0f, 1f)]
        [SerializeField] private float goldDropChance = 0.20f;
        [SerializeField] private int minGold = 10;
        [SerializeField] private int maxGold = 20;

        [Range(0f, 1f)]
        [SerializeField] private float weaponDropChance = 0.08f;

        [Range(0f, 1f)]
        [SerializeField] private float healDropChance = 0.03f;
        [SerializeField] private float healAmount = 25f;

        public EnemyVariantType VariantType => variantType;
        public string EnemyName => enemyName;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float AttackDamage => attackDamage;
        public float AttackRange => attackRange;
        public float WindupDuration => windupDuration;
        public float AttackCooldown => attackCooldown;
        public float ScaleMultiplier => scaleMultiplier;
        public Color BodyColor => bodyColor;
        public Color LeafColor => leafColor;
        public Color EyeColor => eyeColor;

        public float GoldDropChance => goldDropChance;
        public int MinGold => minGold;
        public int MaxGold => maxGold;
        public float WeaponDropChance => weaponDropChance;
        public float HealDropChance => healDropChance;
        public float HealAmount => healAmount;

        public static EnemyData CreateBasic()
        {
            var data = CreateInstance<EnemyData>();
            data.variantType = EnemyVariantType.Basic;
            data.enemyName = "Zombie Carrot (Cơ Bản)";
            data.maxHealth = 45f;
            data.moveSpeed = 2.6f;
            data.attackDamage = 15f;
            data.attackRange = 1.5f;
            data.windupDuration = 0.35f;
            data.attackCooldown = 1.2f;
            data.scaleMultiplier = 1.0f;
            data.bodyColor = new Color(0.95f, 0.42f, 0.08f); // Cam chuẩn
            data.leafColor = new Color(0.12f, 0.75f, 0.22f);
            data.eyeColor = Color.red;

            data.goldDropChance = 0.65f;
            data.minGold = 10;
            data.maxGold = 25;
            data.weaponDropChance = 0.35f; // 35% cơ hội rơi trang bị
            data.healDropChance = 0.15f;
            data.healAmount = 25f;
            return data;
        }

        public static EnemyData CreateFast()
        {
            var data = CreateInstance<EnemyData>();
            data.variantType = EnemyVariantType.Fast;
            data.enemyName = "Zombie Carrot (Tốc Độ)";
            data.maxHealth = 24f; // Máu giấy
            data.moveSpeed = 4.4f; // Chạy nhanh
            data.attackDamage = 12f; // Sát thương nhanh
            data.attackRange = 1.35f;
            data.windupDuration = 0.22f; // Báo hiệu nhanh
            data.attackCooldown = 0.9f;
            data.scaleMultiplier = 0.75f; // Nhỏ gọn
            data.bodyColor = new Color(1.0f, 0.22f, 0.10f); // Đỏ cam rực
            data.leafColor = new Color(0.85f, 0.85f, 0.15f); // Lá vàng chanh
            data.eyeColor = new Color(1.0f, 0.9f, 0.1f); // Mắt vàng rực

            data.goldDropChance = 0.65f;
            data.minGold = 12;
            data.maxGold = 30;
            data.weaponDropChance = 0.35f; // 35% cơ hội rơi trang bị
            data.healDropChance = 0.15f;
            data.healAmount = 25f;
            return data;
        }

        public static EnemyData CreateElite()
        {
            var data = CreateInstance<EnemyData>();
            data.variantType = EnemyVariantType.Elite;
            data.enemyName = "Zombie Carrot (Tinh Nhuệ)";
            data.maxHealth = 130f; // Trâu bò
            data.moveSpeed = 1.8f; // Chậm chạp
            data.attackDamage = 30f; // Đòn đau
            data.attackRange = 1.8f;
            data.windupDuration = 0.5f;
            data.attackCooldown = 1.6f;
            data.scaleMultiplier = 1.45f; // Khổng lồ
            data.bodyColor = new Color(0.55f, 0.15f, 0.45f); // Tím cà rốt độc tố
            data.leafColor = new Color(0.2f, 0.1f, 0.35f); // Lá sẫm
            data.eyeColor = new Color(0.3f, 1.0f, 0.4f); // Mắt xanh huỳnh quang độc

            data.goldDropChance = 1.0f; // Chắc chắn rơi vàng
            data.minGold = 40;
            data.maxGold = 75;
            data.weaponDropChance = 0.85f; // 85% rơi trang bị xịn
            data.healDropChance = 0.30f; // 30% rơi hồi phục
            data.healAmount = 40f;
            return data;
        }
    }
}
