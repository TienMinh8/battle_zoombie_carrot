using UnityEngine;

namespace Apocarrot.Data
{
    public enum WeaponType
    {
        Melee,
        Ranged
    }

    [CreateAssetMenu(fileName = "NewMeleeWeapon", menuName = "Apocarrot/Combat/Melee Weapon Data")]
    public class MeleeWeaponData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string weaponId = "melee_carrot_blade";
        [SerializeField] private string weaponName = "Carrot Blade";
        [SerializeField] private int level = 1;

        [Header("Combat Attributes")]
        [Tooltip("Lượng sát thương cơ bản mỗi nhát chém")]
        [SerializeField] private float damage = 25f;

        [Tooltip("Thời gian hồi chiêu giữa hai lần chém (giây)")]
        [SerializeField] private float attackCooldown = 0.4f;

        [Tooltip("Tầm với tối đa của đòn chém (mét)")]
        [SerializeField] private float attackRange = 2.4f;

        [Tooltip("Góc quét hình nón của đòn chém (độ)")]
        [Range(30f, 180f)]
        [SerializeField] private float attackAngle = 100f;

        [Tooltip("Lực đẩy lùi kẻ địch khi trúng đòn")]
        [SerializeField] private float knockbackForce = 6f;

        [Header("Presentation")]
        [SerializeField] private Color slashColor = new Color(1f, 0.95f, 0.4f, 0.9f);
        [SerializeField] private string description = "Kiếm cà rốt sắc bén, tốc độ chém nhanh.";

        public string WeaponId => weaponId;
        public string WeaponName => weaponName;
        public int Level => level;
        public float Damage => damage;
        public float AttackCooldown => attackCooldown;
        public float AttackRange => attackRange;
        public float AttackAngle => attackAngle;
        public float KnockbackForce => knockbackForce;
        public Color SlashColor => slashColor;
        public string Description => description;

        /// <summary>
        /// Tạo nhanh cấu hình tạm thời trong code nếu chưa tạo asset ScriptableObject
        /// </summary>
        public static MeleeWeaponData CreateCarrotBlade(int level = 1)
        {
            var data = CreateInstance<MeleeWeaponData>();
            data.weaponId = "carrot_blade";
            data.weaponName = "Carrot Blade";
            data.level = level;
            data.damage = 25f * level;
            data.attackCooldown = 0.38f;
            data.attackRange = 2.4f;
            data.attackAngle = 100f;
            data.knockbackForce = 5f;
            data.slashColor = new Color(0.3f, 0.95f, 1f, 0.85f); // Nhát kiếm xanh cyan sáng rực
            data.description = "Kiếm cà rốt tốc độ nhanh, tầm đánh vừa phải.";
            return data;
        }

        public static MeleeWeaponData CreateCarrotAxe(int level = 1)
        {
            var data = CreateInstance<MeleeWeaponData>();
            data.weaponId = "carrot_axe";
            data.weaponName = "Carrot Axe";
            data.level = level;
            data.damage = 60f * level;
            data.attackCooldown = 0.85f;
            data.attackRange = 2.8f;
            data.attackAngle = 135f;
            data.knockbackForce = 12f;
            data.slashColor = new Color(1.0f, 0.45f, 0.1f, 0.9f); // Nhát rìu cam lửa nặng đô
            data.description = "Rìu cà rốt nặng, chém chậm nhưng sát thương cực mạnh và quét rộng.";
            return data;
        }
    }
}
