using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Enemies;

namespace Apocarrot.UI
{
    /// <summary>
    /// Thanh máu Boss trên đỉnh màn hình trong Wave 4 King Carrot.
    /// </summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        public static BossHealthBarUI Instance { get; private set; }

        [SerializeField] private GameObject rootPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text hpText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image ghostFill;

        private EnemyHealth trackedHealth;
        private KingCarrotBossAI trackedBoss;
        private float targetFill = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Update()
        {
            if (ghostFill != null && ghostFill.fillAmount > targetFill)
            {
                ghostFill.fillAmount = Mathf.MoveTowards(ghostFill.fillAmount, targetFill, Time.deltaTime * 0.45f);
            }
        }

        public void Bind(GameObject root, Text title, Text hp, Image fill, Image ghost)
        {
            rootPanel = root;
            titleText = title;
            hpText = hp;
            healthFill = fill;
            ghostFill = ghost;

            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }

        public void ShowBoss(EnemyHealth bossHealth, KingCarrotBossAI bossAI)
        {
            trackedHealth = bossHealth;
            trackedBoss = bossAI;

            if (rootPanel != null) rootPanel.SetActive(true);

            if (trackedHealth != null)
            {
                trackedHealth.OnHealthChanged += HandleHealthChanged;
                trackedHealth.OnDied += HandleBossDied;
                HandleHealthChanged(trackedHealth.CurrentHealth, trackedHealth.MaxHealth);
            }

            if (trackedBoss != null)
            {
                trackedBoss.OnEnragedChanged += HandleEnragedChanged;
            }

            if (titleText != null)
            {
                titleText.text = "👑 CHÚA TỂ CÀ RỐT — KING CARROT";
            }
        }

        private void HandleHealthChanged(float cur, float max)
        {
            if (max <= 0) return;

            targetFill = Mathf.Clamp01(cur / max);
            if (healthFill != null)
            {
                healthFill.fillAmount = targetFill;
            }

            if (hpText != null)
            {
                hpText.text = $"{Mathf.CeilToInt(cur)} / {Mathf.CeilToInt(max)} HP";
            }
        }

        private void HandleEnragedChanged(bool enraged)
        {
            if (titleText != null && enraged)
            {
                titleText.text = "🔥 👑 KING CARROT (CUỒNG NỘ) 👑 🔥";
                titleText.color = new Color(1f, 0.25f, 0.1f);
            }
        }

        private void HandleBossDied(EnemyHealth deadHealth)
        {
            if (titleText != null)
            {
                titleText.text = "👑 KING CARROT ĐÃ BỊ HẠ GỤC! 🎉";
            }

            Invoke(nameof(Hide), 1.8f);
        }

        public void Hide()
        {
            if (trackedHealth != null)
            {
                trackedHealth.OnHealthChanged -= HandleHealthChanged;
                trackedHealth.OnDied -= HandleBossDied;
                trackedHealth = null;
            }

            if (trackedBoss != null)
            {
                trackedBoss.OnEnragedChanged -= HandleEnragedChanged;
                trackedBoss = null;
            }

            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }
    }
}
