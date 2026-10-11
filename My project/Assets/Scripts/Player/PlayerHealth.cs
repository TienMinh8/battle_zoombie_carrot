using System;
using System.Collections;
using UnityEngine;
using Apocarrot.Combat;

namespace Apocarrot.Gameplay
{
    /// <summary>
    /// Quản lý lượng máu của Rabbit Warrior, sự kiện nhận sát thương, hit flash, chết và hồi sinh.
    /// Tích hợp thanh máu 3D trên đầu (Overhead Health Bar) luôn quay về phía Camera.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Stats")]
        [Tooltip("Lượng máu tối đa ban đầu")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Feedback Settings")]
        [SerializeField] private float invulnerabilityDuration = 0.25f;

        public event Action<float, float> OnHealthChanged; // (current, max)
        public event Action OnPlayerDied;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;

        private float lastDamageTime = -99f;
        private Renderer[] renderers;
        private Color[] originalColors;

        // 3D Overhead World Health Bar
        private Transform overheadPivot;
        private Transform overheadFill;
        private Renderer overheadFillRenderer;
        private Camera cachedCamera;

        private void Awake()
        {
            currentHealth = maxHealth;
            RefreshRenderers();
            BuildOverheadHealthBar();
        }

        private void Start()
        {
            ApplyEquipmentHealthBonus();
            RefreshRenderers();
            NotifyHealthChanged();
        }

        public void ApplyEquipmentHealthBonus()
        {
            int bonusHp = Progression.AccountProgression.Instance?.Data != null ? Progression.AccountProgression.Instance.Data.GetTotalEquipmentHealth() : 0;
            if (bonusHp > 0)
            {
                maxHealth = 100f + bonusHp;
                currentHealth = maxHealth;
            }
        }

        private void LateUpdate()
        {
            // Billboard thanh máu trên đầu hướng về Camera người chơi
            if (overheadPivot != null && overheadPivot.gameObject.activeSelf)
            {
                if (cachedCamera == null) cachedCamera = Camera.main;
                if (cachedCamera != null)
                {
                    overheadPivot.rotation = cachedCamera.transform.rotation;
                }
            }
        }

        private void BuildOverheadHealthBar()
        {
            if (overheadPivot != null) return;

            GameObject pivotObj = new GameObject("Player_Overhead_HealthBar");
            pivotObj.transform.SetParent(transform, false);
            pivotObj.transform.localPosition = new Vector3(0, 2.3f, 0);
            overheadPivot = pivotObj.transform;

            // Nền đen/viền đỏ đậm
            GameObject bgObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bgObj.name = "Player_HP_Bg";
            bgObj.transform.SetParent(overheadPivot, false);
            bgObj.transform.localScale = new Vector3(1.36f, 0.20f, 0.04f);
            Destroy(bgObj.GetComponent<Collider>());
            bgObj.GetComponent<Renderer>().material.color = new Color(0.12f, 0.03f, 0.03f, 0.95f);

            // Thanh máu màu xanh lá tươi sáng
            GameObject fillObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fillObj.name = "Player_HP_Fill";
            fillObj.transform.SetParent(overheadPivot, false);
            fillObj.transform.localScale = new Vector3(1.30f, 0.15f, 0.05f);
            Destroy(fillObj.GetComponent<Collider>());
            overheadFillRenderer = fillObj.GetComponent<Renderer>();
            overheadFillRenderer.material.color = new Color(0.22f, 0.88f, 0.35f);
            overheadFill = fillObj.transform;
        }

        public void RefreshRenderers()
        {
            renderers = GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                originalColors = new Color[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].material.HasProperty("_Color"))
                    {
                        originalColors[i] = renderers[i].material.color;
                    }
                    else if (renderers[i].material.HasProperty("_BaseColor"))
                    {
                        originalColors[i] = renderers[i].material.GetColor("_BaseColor");
                    }
                }
            }
        }

        public void TakeDamage(DamageInfo info)
        {
            if (!IsAlive || info.Amount <= 0) return;

            // Thời gian bất tử ngắn tránh spam sát thương nhiều quái cùng 1 tick
            if (Time.time - lastDamageTime < invulnerabilityDuration) return;
            lastDamageTime = Time.time;

            currentHealth = Mathf.Max(0f, currentHealth - info.Amount);
            NotifyHealthChanged();

            // Hiệu ứng chớp đỏ khi bị đánh trúng
            StartCoroutine(FlashRedRoutine());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            NotifyHealthChanged();
        }

        private IEnumerator FlashRedRoutine()
        {
            if (renderers == null || renderers.Length == 0) yield break;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    renderers[i].material.color = Color.red;
                }
            }

            yield return new WaitForSeconds(0.12f);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null && originalColors != null && i < originalColors.Length)
                {
                    renderers[i].material.color = originalColors[i];
                }
            }
        }

        private void Die()
        {
            currentHealth = 0f;
            NotifyHealthChanged();

            if (overheadPivot != null)
            {
                overheadPivot.gameObject.SetActive(false);
            }

            // Vô hiệu hóa điều khiển và vũ khí
            var pc = GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;
            var mc = GetComponent<MeleeCombat>();
            if (mc != null) mc.enabled = false;
            var rc = GetComponent<RangedWeaponController>();
            if (rc != null) rc.enabled = false;

            // Làm thỏ ngã gục xuống sàn
            Transform visual = transform.Find("Visual_Body") ?? transform;
            visual.localRotation = Quaternion.Euler(75f, 0, 0);
            visual.localPosition = new Vector3(0, 0.35f, 0);

            OnPlayerDied?.Invoke();
            Debug.Log("[PlayerHealth] Rabbit Warrior đã bị hạ gục!");
        }

        public void Revive(float healthAmount = 100f)
        {
            maxHealth = Mathf.Max(maxHealth, healthAmount);
            currentHealth = healthAmount;

            if (overheadPivot != null)
            {
                overheadPivot.gameObject.SetActive(true);
            }

            NotifyHealthChanged();

            // Phục hồi tư thế đứng thẳng
            Transform visual = transform.Find("Visual_Body") ?? transform;
            visual.localRotation = Quaternion.identity;
            visual.localPosition = new Vector3(0, 0.9f, 0);

            // Bật lại điều khiển và chiến đấu
            var pc = GetComponent<PlayerController>();
            if (pc != null) pc.enabled = true;
            var mc = GetComponent<MeleeCombat>();
            if (mc != null) mc.enabled = true;
            var rc = GetComponent<RangedWeaponController>();
            if (rc != null) rc.enabled = true;

            RefreshRenderers();
            Debug.Log("[PlayerHealth] Rabbit Warrior đã được hồi sinh!");
        }

        private void NotifyHealthChanged()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Cập nhật thanh máu 3D trên đầu Thỏ
            if (overheadFill != null && maxHealth > 0)
            {
                float ratio = Mathf.Clamp01(currentHealth / maxHealth);
                overheadFill.localScale = new Vector3(1.30f * ratio, 0.15f, 0.05f);
                overheadFill.localPosition = new Vector3((ratio - 1f) * 0.65f, 0, -0.01f);

                if (overheadFillRenderer != null && overheadFillRenderer.material != null)
                {
                    if (ratio > 0.5f)
                    {
                        overheadFillRenderer.material.color = Color.Lerp(new Color(1f, 0.85f, 0.15f), new Color(0.22f, 0.88f, 0.35f), (ratio - 0.5f) * 2f);
                    }
                    else
                    {
                        overheadFillRenderer.material.color = Color.Lerp(new Color(0.95f, 0.15f, 0.2f), new Color(1f, 0.85f, 0.15f), ratio * 2f);
                    }
                }
            }

            if (overheadPivot != null)
            {
                overheadPivot.gameObject.SetActive(IsAlive);
            }
        }
    }
}
