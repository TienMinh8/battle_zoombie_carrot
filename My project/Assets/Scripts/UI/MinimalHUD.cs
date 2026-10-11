using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Gameplay;

namespace Apocarrot.UI
{
    /// <summary>
    /// Giao diện HUD hiển thị máu của Chiến Binh Thỏ và tiến độ màn chơi.
    /// Cập nhật thời gian thực khi Thỏ nhận sát thương hoặc hồi phục.
    /// </summary>
    public class MinimalHUD : MonoBehaviour
    {
        [Header("Player Reference")]
        [SerializeField] private PlayerHealth playerHealth;

        [Header("UI Elements")]
        [SerializeField] private Image healthFillImage;
        [SerializeField] private Text healthText;
        [SerializeField] private Text waveText;

        public void Initialize(PlayerHealth player, Image fill, Text text, Text wave)
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }

            playerHealth = player;
            healthFillImage = fill;
            healthText = text;
            waveText = wave;

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += HandleHealthChanged;
                HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }

        private void Start()
        {
            // Tự động tìm PlayerHealth nếu chưa được gán
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            // Tự động tìm Fill và Text trong các GameObject con nếu chưa được bind
            if (healthFillImage == null)
            {
                Transform fillTr = transform.Find("HP_Bar_Background/HP_Bar_Fill");
                if (fillTr != null) healthFillImage = fillTr.GetComponent<Image>();
            }

            if (healthText == null)
            {
                Transform textTr = transform.Find("HP_Bar_Background/HP_Text");
                if (textTr != null) healthText = textTr.GetComponent<Text>();
            }

            if (waveText == null)
            {
                Transform waveTr = transform.Find("WaveText");
                if (waveTr != null) waveText = waveTr.GetComponent<Text>();
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
                playerHealth.OnHealthChanged += HandleHealthChanged;
                HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            float ratio = max > 0 ? Mathf.Clamp01(current / max) : 0f;

            if (healthFillImage != null)
            {
                healthFillImage.fillAmount = ratio;

                // Đổi màu sinh động: Xanh lá (dồi dào) -> Vàng cam (trung bình) -> Đỏ rực (nguy hiểm)
                if (ratio > 0.5f)
                {
                    healthFillImage.color = Color.Lerp(new Color(1f, 0.85f, 0.15f), new Color(0.22f, 0.88f, 0.35f), (ratio - 0.5f) * 2f);
                }
                else
                {
                    healthFillImage.color = Color.Lerp(new Color(0.95f, 0.15f, 0.2f), new Color(1f, 0.85f, 0.15f), ratio * 2f);
                }
            }

            if (healthText != null)
            {
                int curInt = Mathf.CeilToInt(current);
                int maxInt = Mathf.CeilToInt(max);
                healthText.text = $"❤️ HP THỎ: {curInt} / {maxInt}";
            }
        }
    }
}
