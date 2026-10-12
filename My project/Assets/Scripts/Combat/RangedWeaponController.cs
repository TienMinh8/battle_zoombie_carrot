using System;
using System.Collections.Generic;
using UnityEngine;
using Apocarrot.Data;
using Apocarrot.Enemies;

namespace Apocarrot.Combat
{
    /// <summary>
    /// Điều khiển vũ khí tầm xa tự động: quét tìm kẻ địch gần nhất trong tầm bắn
    /// và tự động khai hỏa khi hết hồi chiêu. KHÔNG có nút bắn thủ công.
    /// </summary>
    public class RangedWeaponController : MonoBehaviour
    {
        [Header("Equipped Weapon")]
        [SerializeField] private RangedWeaponData equippedWeapon;

        [Header("Fire Origin")]
        [SerializeField] private Transform firePoint;

        private float nextFireTime = 0f;
        private Transform currentTarget = null;

        public event Action<RangedWeaponData> OnWeaponChanged;
        public event Action<RangedWeaponData, Transform> OnWeaponFired;

        public RangedWeaponData EquippedWeapon => equippedWeapon;
        public Transform CurrentTarget => currentTarget;
        public bool CanFire => Time.time >= nextFireTime;
        public float CooldownNormalized => equippedWeapon != null && equippedWeapon.FireCooldown > 0 ?
            Mathf.Clamp01((nextFireTime - Time.time) / equippedWeapon.FireCooldown) : 0f;

        private void Awake()
        {
            if (equippedWeapon == null)
            {
                equippedWeapon = RangedWeaponData.CreateTomatoBlaster(1);
            }

            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Update()
        {
            if (equippedWeapon == null) return;

            // 1. Quét tìm kẻ địch gần nhất trong tầm bắn
            currentTarget = ScanClosestTarget(equippedWeapon.Range);

            // 2. Chỉ bắn khi CÓ mục tiêu hợp lệ VÀ đã hết hồi chiêu
            // (Nếu không có quái trong tầm, không bắn lãng phí và không mất lượt hồi chiêu)
            if (currentTarget != null && CanFire)
            {
                FireAtTarget(currentTarget);
            }
        }

        private Transform ScanClosestTarget(float maxRange)
        {
            // Quét tất cả quái vật và Boss đang còn sống trong scene
            EnemyHealth[] allEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            if (allEnemies == null || allEnemies.Length == 0) return null;

            Transform closest = null;
            float minDistanceSqr = maxRange * maxRange;
            Vector3 myPos = transform.position;

            foreach (var enemy in allEnemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;

                Vector3 toEnemy = enemy.transform.position - myPos;
                toEnemy.y = 0; // Tính trên mặt phẳng X-Z
                float distSqr = toEnemy.sqrMagnitude;

                if (distSqr <= minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    closest = enemy.transform;
                }
            }

            return closest;
        }

        private void FireAtTarget(Transform target)
        {
            nextFireTime = Time.time + equippedWeapon.FireCooldown;

            Vector3 spawnPos = firePoint.position + Vector3.up * 0.8f; // Cao ngang ngực thỏ
            Vector3 aimDirection = (target.position - spawnPos);
            aimDirection.y = 0;
            aimDirection.Normalize();

            int count = Mathf.Max(1, equippedWeapon.ProjectileCount);
            float spread = equippedWeapon.SpreadAngle;

            if (count == 1 || spread <= 0f)
            {
                // Bắn 1 viên thẳng vào mục tiêu (Tomato Blaster)
                SpawnSingleProjectile(spawnPos, aimDirection);
            }
            else
            {
                // Bắn chùm nhiều viên tỏa quạt (Grape Splitter)
                float angleStep = spread / (count - 1);
                float startAngle = -spread * 0.5f;

                for (int i = 0; i < count; i++)
                {
                    float currentAngle = startAngle + (angleStep * i);
                    Quaternion rot = Quaternion.Euler(0, currentAngle, 0);
                    Vector3 spreadDir = rot * aimDirection;
                    SpawnSingleProjectile(spawnPos, spreadDir);
                }
            }

            Apocarrot.Audio.AudioManager.Instance?.PlayRangedShot();
            OnWeaponFired?.Invoke(equippedWeapon, target);
        }

        private void SpawnSingleProjectile(Vector3 position, Vector3 direction)
        {
            GameObject projObj = Projectile.CreateProjectilePrefab();
            projObj.transform.position = position;

            Projectile proj = projObj.GetComponent<Projectile>();
            if (proj != null)
            {
                int bonusAtk = Progression.AccountProgression.Instance?.Data != null ? Progression.AccountProgression.Instance.Data.GetTotalEquipmentAttack() : 0;
                int perPelletBonus = equippedWeapon.ProjectileCount > 1 ? Mathf.Max(1, Mathf.RoundToInt((float)bonusAtk / equippedWeapon.ProjectileCount)) : bonusAtk;
                proj.Initialize(
                    direction,
                    equippedWeapon.ProjectileSpeed,
                    equippedWeapon.Damage + perPelletBonus,
                    gameObject,
                    equippedWeapon.ProjectileColor,
                    equippedWeapon.ProjectileScale
                );
            }
        }

        public void EquipRangedWeapon(RangedWeaponData newWeapon)
        {
            if (newWeapon == null) return;
            equippedWeapon = newWeapon;
            OnWeaponChanged?.Invoke(equippedWeapon);
            Debug.Log($"[RangedWeaponController] Đã trang bị: {newWeapon.WeaponName} (Lv.{newWeapon.Level}) - Dame: {newWeapon.Damage}, Cooldown: {newWeapon.FireCooldown}s, Số đạn: {newWeapon.ProjectileCount}");
        }

        private void OnDrawGizmosSelected()
        {
            if (equippedWeapon == null) return;
            Gizmos.color = equippedWeapon.ProjectileColor;
            Gizmos.DrawWireSphere(transform.position, equippedWeapon.Range);
        }
    }
}
