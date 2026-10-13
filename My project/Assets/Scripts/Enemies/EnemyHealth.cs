using System;
using System.Collections;
using UnityEngine;
using Apocarrot.Combat;

namespace Apocarrot.Enemies
{
    /// <summary>
    /// Quản lý lượng máu, hiệu ứng chớp trắng/đỏ khi trúng đòn và trạng thái chết của quái.
    /// </summary>
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float currentHealth;

        [Header("Visual Feedback")]
        [SerializeField] private Transform healthBarPivot;
        [SerializeField] private Color hitFlashColor = Color.white;

        public event Action<float, float> OnHealthChanged;
        public event Action<EnemyHealth> OnDied;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;

        private Renderer[] renderers;
        private Color[] originalColors;
        private Transform healthBarFill;
        private UnityEngine.Camera mainCamera;

        private void Awake()
        {
            currentHealth = maxHealth;
            RefreshOriginalColors();
            BuildSimpleWorldHealthBar();
        }

        public void SetMaxHealth(float newMaxHealth)
        {
            maxHealth = newMaxHealth;
            currentHealth = newMaxHealth;
            NotifyHealthChanged();
        }

        public void RefreshOriginalColors()
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
                }
            }
        }

        private void Start()
        {
            mainCamera = UnityEngine.Camera.main;
            NotifyHealthChanged();
        }

        private void LateUpdate()
        {
            // Billboard thanh máu hướng về phía Camera
            if (healthBarPivot != null && mainCamera != null)
            {
                healthBarPivot.rotation = mainCamera.transform.rotation;
            }
        }

        public void TakeDamage(DamageInfo info)
        {
            if (!IsAlive || info.Amount <= 0) return;

            currentHealth = Mathf.Max(0f, currentHealth - info.Amount);
            NotifyHealthChanged();

            // Hiển thị số sát thương nhảy nổi trực quan (Floating Damage Number)
            bool isCrit = info.Amount >= 50f;
            Apocarrot.UI.DamagePopupManager.ShowDamage(transform.position, info.Amount, isCrit);

            // Hit flash
            GetComponentInChildren<Apocarrot.Visuals.SpriteBillboard2D>()?.FlashDamage(hitFlashColor, 0.12f);
            StopCoroutine(nameof(HitFlashRoutine));
            StartCoroutine(nameof(HitFlashRoutine));

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            if (renderers == null) yield break;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    renderers[i].material.color = hitFlashColor;
                }
            }

            yield return new WaitForSeconds(0.08f);

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
            Apocarrot.Audio.AudioManager.Instance?.PlayEnemyDeath();
            OnDied?.Invoke(this);

            // Tắt collider để không cản trở di chuyển
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var c in colliders)
            {
                c.enabled = false;
            }

            StartCoroutine(DeathShrinkRoutine());
        }

        private IEnumerator DeathShrinkRoutine()
        {
            float duration = 0.25f;
            float elapsed = 0f;
            Vector3 initialScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
                yield return null;
            }

            Destroy(gameObject);
        }

        private void BuildSimpleWorldHealthBar()
        {
            if (healthBarPivot != null) return;

            GameObject pivotObj = new GameObject("HealthBar_Pivot");
            pivotObj.transform.SetParent(transform, false);
            pivotObj.transform.localPosition = new Vector3(0, 1.8f, 0);
            healthBarPivot = pivotObj.transform;

            // Nền đen thanh máu
            GameObject bgObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bgObj.name = "Bar_Bg";
            bgObj.transform.SetParent(healthBarPivot, false);
            bgObj.transform.localScale = new Vector3(1.0f, 0.12f, 0.05f);
            Destroy(bgObj.GetComponent<Collider>());
            bgObj.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Thanh máu màu đỏ
            GameObject fillObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fillObj.name = "Bar_Fill";
            fillObj.transform.SetParent(healthBarPivot, false);
            fillObj.transform.localScale = new Vector3(0.96f, 0.10f, 0.06f);
            Destroy(fillObj.GetComponent<Collider>());
            fillObj.GetComponent<Renderer>().material.color = new Color(0.95f, 0.2f, 0.25f);
            healthBarFill = fillObj.transform;
        }

        private void NotifyHealthChanged()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (healthBarFill != null && maxHealth > 0)
            {
                float ratio = Mathf.Clamp01(currentHealth / maxHealth);
                healthBarFill.localScale = new Vector3(0.96f * ratio, 0.10f, 0.06f);
                healthBarFill.localPosition = new Vector3((ratio - 1f) * 0.48f, 0, -0.01f);
            }
        }
    }
}
