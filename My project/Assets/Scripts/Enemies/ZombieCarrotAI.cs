using System.Collections;
using UnityEngine;
using Apocarrot.Combat;
using Apocarrot.Data;
using Apocarrot.Gameplay;
using Apocarrot.Loot;

namespace Apocarrot.Enemies
{
    public enum EnemyState
    {
        Chase,
        Windup,
        Attack,
        Cooldown
    }

    /// <summary>
    /// AI của quái Zombie Carrot: bám đuổi người chơi, báo hiệu đòn đánh (telegraph)
    /// và tung đòn tấn công khi vào tầm cận chiến.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(EnemyHealth))]
    public class ZombieCarrotAI : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private float rotationSpeed = 360f;

        [Header("Combat Stats")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackDamage = 15f;
        [SerializeField] private float windupDuration = 0.35f;
        [SerializeField] private float attackCooldown = 1.3f;

        [Header("Visual Feedback")]
        [SerializeField] private Transform visualRoot;

        private CharacterController characterController;
        private EnemyHealth health;
        private Transform playerTarget;
        private EnemyState currentState = EnemyState.Chase;
        private float stateTimer = 0f;
        private Vector3 originalVisualScale;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            health = GetComponent<EnemyHealth>();
            if (visualRoot == null)
            {
                visualRoot = transform;
            }
            originalVisualScale = visualRoot.localScale;
        }

        private void Start()
        {
            FindPlayerTarget();
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
                case EnemyState.Chase:
                    HandleChaseState();
                    break;
                case EnemyState.Windup:
                    HandleWindupState();
                    break;
                case EnemyState.Cooldown:
                    HandleCooldownState();
                    break;
            }
        }

        private void FindPlayerTarget()
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                playerTarget = player.transform;
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

            // Nếu đã vào tầm đánh -> Bắt đầu báo hiệu đòn đánh (Windup Telegraph)
            if (distance <= attackRange)
            {
                EnterWindupState();
                return;
            }

            // Di chuyển đuổi theo thỏ
            Vector3 moveDir = toPlayer.normalized * moveSpeed;
            if (!characterController.isGrounded)
            {
                moveDir.y += Physics.gravity.y * Time.deltaTime;
            }
            characterController.Move(moveDir * Time.deltaTime);
        }

        private void EnterWindupState()
        {
            currentState = EnemyState.Windup;
            stateTimer = windupDuration;

            // Telegraph: phồng to người lên cảnh báo người chơi sắp bị cắn
            StartCoroutine(TelegraphScaleRoutine());
        }

        private void HandleWindupState()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
            {
                ExecuteAttack();
            }
        }

        private void ExecuteAttack()
        {
            if (playerTarget != null)
            {
                // Xoay mặt về phía thỏ và lao nhẹ tới trước khi cắn
                Vector3 toPlayer = playerTarget.position - transform.position;
                toPlayer.y = 0;
                if (toPlayer.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
                    characterController.Move(transform.forward * 0.45f);
                }

                // Tính khoảng cách ngang 2D (loại bỏ chênh lệch trục Y giữa thỏ và quái)
                float horizontalDist = toPlayer.magnitude;

                // Kiểm tra tầm đánh hợp lý
                if (horizontalDist <= attackRange + 1.1f)
                {
                    IDamageable playerDamageable = playerTarget.GetComponent<IDamageable>() ??
                                                  playerTarget.GetComponentInParent<IDamageable>();
                    if (playerDamageable != null && playerDamageable.IsAlive)
                    {
                        Vector3 hitDir = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : transform.forward;
                        DamageInfo dmg = new DamageInfo(attackDamage, playerTarget.position, hitDir, gameObject);
                        playerDamageable.TakeDamage(dmg);
                        Debug.Log($"[ZombieCarrotAI] Đã cắn Rabbit Warrior trúng đích: -{attackDamage} HP!");
                    }
                }
            }

            // Chuyển sang trạng thái hồi chiêu
            currentState = EnemyState.Cooldown;
            stateTimer = attackCooldown;
        }

        private void HandleCooldownState()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
            {
                currentState = EnemyState.Chase;
            }
        }

        private IEnumerator TelegraphScaleRoutine()
        {
            float elapsed = 0f;
            while (elapsed < windupDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / windupDuration;
                // Phồng to 1.25 lần
                if (visualRoot != null)
                {
                    visualRoot.localScale = Vector3.Lerp(originalVisualScale, originalVisualScale * 1.25f, t);
                }
                yield return null;
            }

            if (visualRoot != null)
            {
                visualRoot.localScale = originalVisualScale;
            }
        }

        /// <summary>
        /// Cấu hình các chỉ số và visual của quái từ EnemyData
        private EnemyData enemyData;
        public EnemyVariantType VariantType => enemyData != null ? enemyData.VariantType : EnemyVariantType.Basic;

        /// <summary>
        /// Áp dụng các chỉ số từ ScriptableObject EnemyData (Gate 4)
        /// </summary>
        public void Configure(EnemyData data)
        {
            if (data == null) return;
            enemyData = data;

            moveSpeed = data.MoveSpeed;
            attackDamage = data.AttackDamage;
            attackRange = data.AttackRange;
            windupDuration = data.WindupDuration;
            attackCooldown = data.AttackCooldown;

            if (health != null)
            {
                health.SetMaxHealth(data.MaxHealth);
            }
        }

        /// <summary>
        /// Tạo Prefab/GameObject mẫu Zombie Carrot theo biến thể (Basic, Fast, Elite)
        /// với hình dáng củ cà rốt + cuống lá + mắt quái + LootDropper.
        /// </summary>
        public static GameObject CreateZombieCarrotPrefab(Vector3 spawnPosition, EnemyData data = null)
        {
            if (data == null)
            {
                data = EnemyData.CreateBasic();
            }

            GameObject enemy = new GameObject($"Enemy_{data.EnemyName}");
            enemy.transform.position = spawnPosition;

            float scale = data.ScaleMultiplier;

            CharacterController cc = enemy.AddComponent<CharacterController>();
            cc.height = 1.6f * scale;
            cc.radius = 0.45f * scale;
            cc.center = new Vector3(0, 0.8f * scale, 0);

            EnemyHealth enemyHealth = enemy.AddComponent<EnemyHealth>();
            ZombieCarrotAI ai = enemy.AddComponent<ZombieCarrotAI>();

            // Gắn hệ thống LootDropper để rơi đồ theo tỷ lệ của biến thể
            LootDropper dropper = enemy.AddComponent<LootDropper>();
            dropper.Initialize(data);

            // Visual Root
            GameObject visual = new GameObject("Visual_Root");
            visual.transform.SetParent(enemy.transform, false);
            visual.transform.localScale = Vector3.one * scale;
            ai.visualRoot = visual.transform;
            ai.originalVisualScale = visual.transform.localScale;

            // Thân củ cà rốt (Màu sắc theo EnemyData)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body_Carrot";
            body.transform.SetParent(visual.transform, false);
            body.transform.localPosition = new Vector3(0, 0.8f, 0);
            body.transform.localScale = new Vector3(0.7f, 0.8f, 0.7f);
            Destroy(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().material.color = data.BodyColor;

            // Cuống lá trên đầu (Màu theo EnemyData)
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leaf.name = "Leaves_Green";
            leaf.transform.SetParent(visual.transform, false);
            leaf.transform.localPosition = new Vector3(0, 1.6f, 0);
            leaf.transform.localScale = new Vector3(0.35f, 0.3f, 0.35f);
            Destroy(leaf.GetComponent<Collider>());
            leaf.GetComponent<Renderer>().material.color = data.LeafColor;

            // Mắt Zombie phát sáng (Màu theo EnemyData)
            GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeL.name = "Eye_Left";
            eyeL.transform.SetParent(visual.transform, false);
            eyeL.transform.localPosition = new Vector3(-0.16f, 1.1f, 0.3f);
            eyeL.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            Destroy(eyeL.GetComponent<Collider>());
            eyeL.GetComponent<Renderer>().material.color = data.EyeColor;

            GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeR.name = "Eye_Right";
            eyeR.transform.SetParent(visual.transform, false);
            eyeR.transform.localPosition = new Vector3(0.16f, 1.1f, 0.3f);
            eyeR.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            Destroy(eyeR.GetComponent<Collider>());
            eyeR.GetComponent<Renderer>().material.color = data.EyeColor;

            // Áp dụng chỉ số từ EnemyData
            ai.Configure(data);
            enemyHealth.RefreshOriginalColors();

            return enemy;
        }
    }
}
