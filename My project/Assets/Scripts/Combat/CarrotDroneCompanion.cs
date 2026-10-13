using UnityEngine;
using Apocarrot.Enemies;
using Apocarrot.Progression;
using Apocarrot.UI;
using Apocarrot.Audio;

namespace Apocarrot.Combat
{
    /// <summary>
    /// Trợ thủ Carrot Drone bay đồng hành cùng Thỏ trong trận đấu (Gate 8 & Benchmark Mockup).
    /// Bay lơ lửng, bám theo nhân vật, tự động ngắm bắn zombie gần nhất theo chu kỳ.
    /// </summary>
    public class CarrotDroneCompanion : MonoBehaviour
    {
        [Header("Target & Follow")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 followOffset = new Vector3(1.1f, 1.45f, -0.6f);
        [SerializeField] private float followSpeed = 6.5f;

        [Header("Combat Stats")]
        [SerializeField] private float detectionRadius = 8.5f;
        [SerializeField] private float baseShootInterval = 2.4f;
        [SerializeField] private int baseDamage = 25;

        private float shootTimer = 0f;
        private Transform propellerTransform;
        private ZombieCarrotAI currentTarget;

        public Transform FollowTarget
        {
            get => followTarget;
            set => followTarget = value;
        }

        private void Update()
        {
            if (followTarget == null) return;

            // 1. Bay lơ lửng & theo sau mượt mà
            Vector3 hoverOffset = Vector3.up * (Mathf.Sin(Time.time * 3.5f) * 0.12f);
            Vector3 targetPosition = followTarget.position + followOffset + hoverOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

            // 2. Xoay cánh quạt
            if (propellerTransform != null)
            {
                propellerTransform.Rotate(Vector3.up, 720f * Time.deltaTime, Space.Self);
            }

            // 3. Quét zombie gần nhất
            currentTarget = FindNearestZombie();

            int level = AccountProgression.Instance != null ? AccountProgression.Instance.Data.CompanionLevel : 5;
            float currentInterval = Mathf.Max(1.0f, baseShootInterval - (level * 0.04f));

            if (currentTarget != null)
            {
                // Hướng mặt về phía mục tiêu
                Vector3 aimDir = (currentTarget.transform.position - transform.position);
                aimDir.y = 0;
                if (aimDir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(aimDir.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
                }

                // Đếm nhịp bắn
                shootTimer += Time.deltaTime;
                if (shootTimer >= currentInterval)
                {
                    shootTimer = 0f;
                    FireMiniCarrotRocket(currentTarget, level);
                }
            }
            else
            {
                // Khi không có quái, xoay theo góc nhìn của Player
                transform.rotation = Quaternion.Slerp(transform.rotation, followTarget.rotation, Time.deltaTime * 4f);
                shootTimer = Mathf.Min(shootTimer + Time.deltaTime, currentInterval * 0.8f);
            }
        }

        private ZombieCarrotAI FindNearestZombie()
        {
            ZombieCarrotAI[] allZombies = FindObjectsByType<ZombieCarrotAI>(FindObjectsSortMode.None);
            ZombieCarrotAI best = null;
            float minSqrDist = detectionRadius * detectionRadius;

            Vector3 myPos = transform.position;
            foreach (var z in allZombies)
            {
                EnemyHealth zh = z != null ? z.GetComponent<EnemyHealth>() : null;
                if (zh == null || !zh.IsAlive) continue;

                float sqrDist = (z.transform.position - myPos).sqrMagnitude;
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    best = z;
                }
            }

            return best;
        }

        private void FireMiniCarrotRocket(ZombieCarrotAI target, int companionLevel)
        {
            EnemyHealth targetHealth = target != null ? target.GetComponent<EnemyHealth>() : null;
            if (targetHealth == null || !targetHealth.IsAlive) return;

            int damage = baseDamage + (companionLevel * 6);

            // Tạo tia đạn mini carrot missile
            GameObject missile = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            missile.name = "MiniCarrotRocket";
            missile.transform.position = transform.position + transform.forward * 0.4f;
            missile.transform.localScale = new Vector3(0.12f, 0.28f, 0.12f);
            missile.transform.rotation = Quaternion.LookRotation((target.transform.position + Vector3.up * 0.8f - transform.position).normalized);

            Destroy(missile.GetComponent<Collider>());
            Renderer mr = missile.GetComponent<Renderer>();
            if (mr != null)
            {
                mr.material.color = new Color(1f, 0.55f, 0.1f); // Màu cam cà rốt
            }

            // Gắn controller bay trúng mục tiêu
            MiniMissileFlight flight = missile.AddComponent<MiniMissileFlight>();
            flight.Init(target, damage);

            Apocarrot.Audio.AudioManager.Instance?.PlayRangedShot();
        }

        /// <summary>
        /// Tạo và gắn Trợ thủ Carrot Drone vào người chơi.
        /// </summary>
        public static CarrotDroneCompanion SpawnDrone(GameObject player)
        {
            if (player == null) return null;

            GameObject droneObj = new GameObject("CarrotDrone_Companion");
            droneObj.transform.position = player.transform.position + new Vector3(1.1f, 1.45f, -0.6f);

            // Gắn hình thể 2.5D Sprite Trợ thủ Carrot Drone
            var billboard = Apocarrot.Visuals.SpriteCharacterFactory.AttachCharacterSprite(droneObj, Apocarrot.Visuals.CharacterSpriteType.CarrotDrone, Vector3.zero, 0.85f);

            CarrotDroneCompanion comp = droneObj.AddComponent<CarrotDroneCompanion>();
            comp.followTarget = player.transform;
            comp.propellerTransform = billboard != null ? billboard.transform : droneObj.transform;

            return comp;
        }
    }

    /// <summary>
    /// Điều khiển đường bay của tên lửa mini từ Carrot Drone tới mục tiêu.
    /// </summary>
    public class MiniMissileFlight : MonoBehaviour
    {
        private ZombieCarrotAI target;
        private int damage;
        private float speed = 14f;
        private float lifeTime = 2.0f;

        public void Init(ZombieCarrotAI targetZombie, int dmg)
        {
            target = targetZombie;
            damage = dmg;
        }

        private void Update()
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            EnemyHealth th = target != null ? target.GetComponent<EnemyHealth>() : null;
            Vector3 aimPos = (th != null && th.IsAlive)
                ? target.transform.position + Vector3.up * 0.8f
                : transform.position + transform.forward * 2f;

            Vector3 dir = (aimPos - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
            if (dir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }

            if (th != null && th.IsAlive && (transform.position - aimPos).sqrMagnitude < 0.6f)
            {
                HitTarget();
            }
        }

        private void HitTarget()
        {
            IDamageable damageable = target != null ? target.GetComponent<IDamageable>() : null;
            if (damageable != null && damageable.IsAlive)
            {
                var info = new DamageInfo(
                    amount: damage,
                    hitPoint: target.transform.position,
                    hitDirection: (target.transform.position - transform.position).normalized,
                    source: gameObject,
                    isCritical: false
                );
                damageable.TakeDamage(info);
                DamagePopupManager.ShowDamage(target.transform.position + Vector3.up * 1.2f, damage, false);
            }
            Destroy(gameObject);
        }
    }
}
