using System;
using System.Collections;
using UnityEngine;
using Apocarrot.Combat;
using Apocarrot.Data;
using Apocarrot.Gameplay;
using Apocarrot.Inventory;
using Apocarrot.Loot;

namespace Apocarrot.Enemies
{
    public enum BossState
    {
        Chase,
        MeleeWindup,
        GroundSlamCharge,
        Recovering
    }

    /// <summary>
    /// AI của King Carrot (Boss cuối Gate 6).
    /// Sở hữu:
    /// 1. Tấn công cận chiến khi lại gần (28 DMG).
    /// 2. Chiêu dậm đất diện rộng (Ground Slam AoE - 45 DMG) có vòng tròn cảnh báo đỏ (Telegraph) 1.2s trước khi dậm.
    /// 3. Pha Cuồng Nộ (Enrage) khi máu dưới 50%: tăng tốc chạy và giảm hồi chiêu dậm đất.
    /// 4. Rơi Rương Boss đặc biệt khi bị đánh bại.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(EnemyHealth))]
    public class KingCarrotBossAI : MonoBehaviour
    {
        [Header("Boss Stats")]
        [SerializeField] private float normalMoveSpeed = 2.4f;
        [SerializeField] private float enrageMoveSpeed = 3.4f;
        [SerializeField] private float rotationSpeed = 280f;

        [Header("Melee Attack")]
        [SerializeField] private float meleeRange = 2.4f;
        [SerializeField] private float meleeDamage = 28f;
        [SerializeField] private float meleeWindup = 0.45f;
        [SerializeField] private float meleeCooldown = 1.3f;

        [Header("Ground Slam AoE")]
        [SerializeField] private float slamRadius = 4.5f;
        [SerializeField] private float slamDamage = 45f;
        [SerializeField] private float slamTelegraphDuration = 1.25f;
        [SerializeField] private float slamCooldownNormal = 7.5f;
        [SerializeField] private float slamCooldownEnraged = 4.2f;

        [Header("Enrage Settings")]
        [SerializeField] private float enrageHealthPercent = 0.5f;
        [SerializeField] private Color enrageBodyColor = new Color(0.95f, 0.2f, 0.05f);

        public event Action OnBossDefeated;
        public event Action<bool> OnEnragedChanged;

        private CharacterController characterController;
        private EnemyHealth health;
        private Transform playerTarget;
        private PlayerController playerController;

        private BossState currentState = BossState.Chase;
        private bool isEnraged = false;
        private float lastMeleeTime = -99f;
        private float lastSlamTime = -99f;

        // Visual references
        private Transform visualRoot;
        private GameObject telegraphDisc;
        private Renderer telegraphRenderer;
        private Renderer bodyRenderer;
        private Renderer crownRenderer;
        private Color originalBodyColor;

        public bool IsEnraged => isEnraged;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            health = GetComponent<EnemyHealth>();
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDied += HandleDied;
        }

        private void Start()
        {
            FindPlayerTarget();
            lastSlamTime = Time.time - 2.5f; // Chuẩn bị sẵn chiêu dậm đất sau khi bắt đầu vài giây
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
                health.OnDied -= HandleDied;
            }
        }

        private void FindPlayerTarget()
        {
            playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                playerTarget = playerController.transform;
            }
        }

        private void Update()
        {
            if (!health.IsAlive) return;

            if (playerTarget == null)
            {
                FindPlayerTarget();
                return;
            }

            switch (currentState)
            {
                case BossState.Chase:
                    HandleChaseState();
                    break;
            }
        }

        private void HandleChaseState()
        {
            Vector3 toPlayer = playerTarget.position - transform.position;
            toPlayer.y = 0;
            float distance = toPlayer.magnitude;

            // Xoay mặt về phía thỏ
            if (toPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            // Kiểm tra thi triển chiêu Dậm Đất AoE
            float slamCooldown = isEnraged ? slamCooldownEnraged : slamCooldownNormal;
            if (Time.time - lastSlamTime >= slamCooldown && distance <= slamRadius * 1.5f)
            {
                StartCoroutine(PerformGroundSlamRoutine());
                return;
            }

            // Kiểm tra cận chiến
            if (distance <= meleeRange && Time.time - lastMeleeTime >= meleeCooldown)
            {
                StartCoroutine(PerformMeleeRoutine());
                return;
            }

            // Di chuyển lại gần thỏ
            float speed = isEnraged ? enrageMoveSpeed : normalMoveSpeed;
            Vector3 moveDir = toPlayer.normalized * speed;
            if (!characterController.isGrounded)
            {
                moveDir.y += Physics.gravity.y * Time.deltaTime;
            }
            characterController.Move(moveDir * Time.deltaTime);
        }

        private IEnumerator PerformMeleeRoutine()
        {
            currentState = BossState.MeleeWindup;
            lastMeleeTime = Time.time;

            // Gồng đòn: lùi nhẹ và đổi màu mắt/thân
            Vector3 startPos = transform.position;
            float elapsed = 0f;
            while (elapsed < meleeWindup)
            {
                elapsed += Time.deltaTime;
                // Nhấp nháy scale nhẹ để báo hiệu
                if (visualRoot != null)
                {
                    float scaleMod = 1f + Mathf.Sin(elapsed * 25f) * 0.08f;
                    visualRoot.localScale = Vector3.one * 2.3f * scaleMod;
                }
                yield return null;
            }

            if (visualRoot != null) visualRoot.localScale = Vector3.one * 2.3f;

            // Lao tới chém (Lunge)
            if (playerTarget != null)
            {
                Vector3 toPlayer = playerTarget.position - transform.position;
                toPlayer.y = 0;
                characterController.Move(toPlayer.normalized * 1.2f);

                // Gây sát thương nếu còn trong phạm vi
                if (toPlayer.magnitude <= meleeRange + 0.8f)
                {
                    IDamageable playerDam = playerTarget.GetComponent<IDamageable>();
                    if (playerDam != null)
                    {
                        DamageInfo info = new DamageInfo(meleeDamage, playerTarget.position, (playerTarget.position - transform.position).normalized, gameObject);
                        playerDam.TakeDamage(info);
                        LootToastManager.ShowWorldToast(playerTarget.position + Vector3.up * 1.2f, $"-{meleeDamage:0} HP!", new Color(1f, 0.3f, 0.3f));
                    }
                }
            }

            yield return new WaitForSeconds(0.2f);
            currentState = BossState.Chase;
        }

        private IEnumerator PerformGroundSlamRoutine()
        {
            currentState = BossState.GroundSlamCharge;
            lastSlamTime = Time.time;

            // Kích hoạt vòng tròn cảnh báo đỏ (Telegraph Red Circle) trên mặt đất
            if (telegraphDisc != null)
            {
                telegraphDisc.SetActive(true);
                telegraphDisc.transform.position = new Vector3(transform.position.x, 0.04f, transform.position.z);
                telegraphDisc.transform.localScale = new Vector3(slamRadius * 2f, 0.02f, slamRadius * 2f);
            }

            LootToastManager.ShowWorldToast(transform.position + Vector3.up * 3.8f, "⚠️ NÉ ĐÒN DẬM ĐẤT!", new Color(1f, 0.85f, 0.2f));
            Apocarrot.Audio.AudioManager.Instance?.PlayBossTelegraph();

            // Boss nhảy lên nhẹ / nhún gồng mình tích năng lượng
            float elapsed = 0f;
            Vector3 initialVisualPos = visualRoot != null ? visualRoot.localPosition : Vector3.zero;

            while (elapsed < slamTelegraphDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / slamTelegraphDuration;

                // Telegraph chớp nháy nhanh dần
                if (telegraphRenderer != null)
                {
                    float alpha = Mathf.Lerp(0.35f, 0.85f, Mathf.PingPong(elapsed * 8f, 1f));
                    telegraphRenderer.material.color = new Color(1f, 0.1f, 0.1f, alpha);
                }

                if (visualRoot != null)
                {
                    // Nâng lên cao chuẩn bị dậm
                    visualRoot.localPosition = initialVisualPos + new Vector3(0, Mathf.Sin(progress * Mathf.PI) * 0.8f, 0);
                }

                yield return null;
            }

            // DẬM XUỐNG! (SLAM!)
            if (visualRoot != null)
            {
                visualRoot.localPosition = initialVisualPos;
            }

            if (telegraphDisc != null)
            {
                telegraphDisc.SetActive(false);
            }

            // Phát âm thanh dậm đất uy lực và Rung chấn Camera!
            Apocarrot.Audio.AudioManager.Instance?.PlayBossSlamImpact();
            Apocarrot.Cameras.TopDownFollowCamera.Shake(0.35f, 0.5f);

            // Gây sát thương AoE cho người chơi nếu đứng trong bán kính
            if (playerTarget != null)
            {
                Vector3 toPlayer = playerTarget.position - transform.position;
                toPlayer.y = 0;
                float dist = toPlayer.magnitude;

                if (dist <= slamRadius)
                {
                    IDamageable playerDam = playerTarget.GetComponent<IDamageable>();
                    if (playerDam != null)
                    {
                        DamageInfo info = new DamageInfo(slamDamage, playerTarget.position, Vector3.up, gameObject);
                        playerDam.TakeDamage(info);
                        LootToastManager.ShowWorldToast(playerTarget.position + Vector3.up * 1.5f, $"💥 BỊ DẬM TRÚNG -{slamDamage:0} HP!", Color.red);
                    }
                }
            }

            // Tạo hiệu ứng sóng xung kích dậm đất (Shockwave ring trên sàn)
            SpawnSlamShockwaveVisual(transform.position, slamRadius);

            yield return new WaitForSeconds(0.45f);
            currentState = BossState.Chase;
        }

        private void SpawnSlamShockwaveVisual(Vector3 center, float maxRadius)
        {
            GameObject shockwave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shockwave.name = "Boss_Slam_Shockwave";
            shockwave.transform.position = new Vector3(center.x, 0.05f, center.z);
            shockwave.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);
            Destroy(shockwave.GetComponent<Collider>());

            Renderer rend = shockwave.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = new Color(1f, 0.4f, 0.1f, 0.8f);
            }

            StartCoroutine(AnimateShockwave(shockwave, maxRadius));
        }

        private IEnumerator AnimateShockwave(GameObject wave, float targetRadius)
        {
            float duration = 0.35f;
            float elapsed = 0f;
            Renderer rend = wave.GetComponent<Renderer>();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float currentR = Mathf.Lerp(0.5f, targetRadius * 2f, t);
                if (wave != null)
                {
                    wave.transform.localScale = new Vector3(currentR, 0.02f, currentR);
                    if (rend != null)
                    {
                        rend.material.color = new Color(1f, 0.4f, 0.1f, Mathf.Lerp(0.8f, 0f, t));
                    }
                }
                yield return null;
            }

            if (wave != null) Destroy(wave);
        }

        private void HandleHealthChanged(float cur, float max)
        {
            if (!isEnraged && cur > 0 && cur <= max * enrageHealthPercent)
            {
                TriggerEnrage();
            }
        }

        private void TriggerEnrage()
        {
            isEnraged = true;
            OnEnragedChanged?.Invoke(true);

            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = enrageBodyColor;
            }

            if (crownRenderer != null)
            {
                crownRenderer.material.color = new Color(1f, 0.85f, 0.1f);
            }

            LootToastManager.ShowWorldToast(
                transform.position + Vector3.up * 4.2f,
                "👑 KING CARROT CUỒNG NỘ! TỐC ĐỘ VÀ SÁT THƯƠNG TĂNG CAO!",
                new Color(1f, 0.2f, 0.2f)
            );
        }

        private void HandleDied(EnemyHealth deadHealth)
        {
            if (telegraphDisc != null)
            {
                Destroy(telegraphDisc);
            }

            // Rơi Rương Báu Boss (Boss Chest)
            SpawnBossChestLoot(transform.position);

            OnBossDefeated?.Invoke();
        }

        private void SpawnBossChestLoot(Vector3 position)
        {
            // 1. Rơi 5 đồng tiền vàng lớn xung quanh
            for (int i = 0; i < 5; i++)
            {
                float angle = i * (Mathf.PI * 2f / 5f);
                Vector3 offset = new Vector3(Mathf.Cos(angle) * 1.8f, 0.3f, Mathf.Sin(angle) * 1.8f);
                LootPickup.CreateGoldPickup(position + offset, 20); // 5 x 20 = 100 Vàng!
            }

            // 2. Rơi 2 Củ Cà Rốt Hồi Máu Siêu Cấp
            LootPickup.CreateHealthPickup(position + new Vector3(1.2f, 0.3f, 0), 50f);
            LootPickup.CreateHealthPickup(position + new Vector3(-1.2f, 0.3f, 0), 50f);

            // 3. Rơi 3 RƯƠNG TRANG BỊ CAO CẤP (Hiếm & Sử Thi) vào Balo
            EquipmentItem bossWeapon = EquipmentItem.CreateCarrotBlade(EquipmentRarity.Rare, 2);
            LootPickup.CreateEquipmentPickup(position + new Vector3(0, 0.3f, 1.4f), bossWeapon);

            EquipmentItem bossVest = EquipmentItem.CreateRabbitVest(EquipmentRarity.Epic, 1);
            LootPickup.CreateEquipmentPickup(position + new Vector3(-1.0f, 0.3f, 1.0f), bossVest);

            EquipmentItem bossBoots = EquipmentItem.CreateSwiftBoots(EquipmentRarity.Rare, 1);
            LootPickup.CreateEquipmentPickup(position + new Vector3(1.0f, 0.3f, 1.0f), bossBoots);

            LootToastManager.ShowWorldToast(position + Vector3.up * 2f, "👑 RƯƠNG THẮNG TRẬN KING CARROT! 3 TRANG BỊ QUÝ ĐÃ RƠI!", new Color(1f, 0.85f, 0.1f));
        }

        /// <summary>
        /// Tạo Prefab Boss King Carrot với kích thước to lớn, Vương miện Hoàng Đế và Vòng cảnh báo đỏ.
        /// </summary>
        public static GameObject CreateKingCarrotPrefab(Vector3 spawnPosition)
        {
            GameObject boss = new GameObject("Boss_KingCarrot");
            boss.transform.position = spawnPosition;

            float scale = 2.3f;

            CharacterController cc = boss.AddComponent<CharacterController>();
            cc.height = 1.8f * scale;
            cc.radius = 0.5f * scale;
            cc.center = new Vector3(0, 0.9f * scale, 0);

            EnemyHealth bossHealth = boss.AddComponent<EnemyHealth>();
            bossHealth.SetMaxHealth(1200f); // 1200 HP: boss trận chiến lớn, thử thách 35-45 giây né đòn và dồn sát thương!

            KingCarrotBossAI bossAI = boss.AddComponent<KingCarrotBossAI>();

            // Visual Root
            GameObject visual = new GameObject("Visual_Root");
            visual.transform.SetParent(boss.transform, false);
            visual.transform.localScale = Vector3.one * scale;
            bossAI.visualRoot = visual.transform;

            // Thân Cà Rốt Hoàng Đế (Màu cam sậm rực rỡ)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body_KingCarrot";
            body.transform.SetParent(visual.transform, false);
            body.transform.localPosition = new Vector3(0, 0.9f, 0);
            body.transform.localScale = new Vector3(0.85f, 0.95f, 0.85f);
            Destroy(body.GetComponent<Collider>());
            bossAI.bodyRenderer = body.GetComponent<Renderer>();
            bossAI.originalBodyColor = new Color(0.95f, 0.42f, 0.05f);
            bossAI.bodyRenderer.material.color = bossAI.originalBodyColor;

            // Vương Miện Vàng 👑 (Cylinder dẹt + chóp nhọn)
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crown.name = "Royal_Crown";
            crown.transform.SetParent(visual.transform, false);
            crown.transform.localPosition = new Vector3(0, 1.95f, 0);
            crown.transform.localScale = new Vector3(0.6f, 0.18f, 0.6f);
            Destroy(crown.GetComponent<Collider>());
            bossAI.crownRenderer = crown.GetComponent<Renderer>();
            bossAI.crownRenderer.material.color = new Color(1f, 0.84f, 0.0f); // Vàng gold 24k

            // Viên hồng ngọc đính trên vương miện
            GameObject ruby = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ruby.name = "Crown_Ruby";
            ruby.transform.SetParent(crown.transform, false);
            ruby.transform.localPosition = new Vector3(0, 0.6f, 0.5f);
            ruby.transform.localScale = new Vector3(0.25f, 0.35f, 0.25f);
            Destroy(ruby.GetComponent<Collider>());
            ruby.GetComponent<Renderer>().material.color = new Color(0.9f, 0.05f, 0.15f);

            // Chùm Lá Xanh Hoàng Đế trên đỉnh đầu
            GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leaves.name = "Royal_Leaves";
            leaves.transform.SetParent(visual.transform, false);
            leaves.transform.localPosition = new Vector3(0, 2.2f, 0);
            leaves.transform.localScale = new Vector3(0.45f, 0.35f, 0.45f);
            Destroy(leaves.GetComponent<Collider>());
            leaves.GetComponent<Renderer>().material.color = new Color(0.12f, 0.72f, 0.25f);

            // Đôi Mắt Zombie Quỷ Đỏ
            GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeL.name = "Eye_Left";
            eyeL.transform.SetParent(visual.transform, false);
            eyeL.transform.localPosition = new Vector3(-0.22f, 1.25f, 0.38f);
            eyeL.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
            Destroy(eyeL.GetComponent<Collider>());
            eyeL.GetComponent<Renderer>().material.color = new Color(1f, 0.1f, 0.15f);

            GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeR.name = "Eye_Right";
            eyeR.transform.SetParent(visual.transform, false);
            eyeR.transform.localPosition = new Vector3(0.22f, 1.25f, 0.38f);
            eyeR.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
            Destroy(eyeR.GetComponent<Collider>());
            eyeR.GetComponent<Renderer>().material.color = new Color(1f, 0.1f, 0.15f);

            // Vòng tròn cảnh báo dậm đất Telegraph Ground Disc
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Telegraph_Ground_Disc";
            disc.transform.position = new Vector3(spawnPosition.x, 0.03f, spawnPosition.z);
            disc.transform.localScale = new Vector3(9f, 0.01f, 9f);
            Destroy(disc.GetComponent<Collider>());
            bossAI.telegraphRenderer = disc.GetComponent<Renderer>();
            bossAI.telegraphRenderer.material.color = new Color(1f, 0.15f, 0.15f, 0.5f);
            bossAI.telegraphDisc = disc;
            disc.SetActive(false);

            bossHealth.RefreshOriginalColors();
            return boss;
        }
    }
}
