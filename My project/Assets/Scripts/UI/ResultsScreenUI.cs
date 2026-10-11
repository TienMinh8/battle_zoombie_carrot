using System;
using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Progression;
using Apocarrot.Inventory;

namespace Apocarrot.UI
{
    /// <summary>
    /// Màn hình Kết Quả tổng kết lượt chơi (Results Screen - Gate 7).
    /// Báo cáo chiến tích trận đấu, tính toán phần thưởng tài khoản (XP, Cấp Thỏ, Vàng TK),
    /// và thực thi quy tắc Roguelite: đồ nhặt trong run bị mất, tài sản tài khoản vĩnh viễn được giữ.
    /// </summary>
    public class ResultsScreenUI : MonoBehaviour
    {
        public static ResultsScreenUI Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject overlayRoot;

        [Header("Header Elements")]
        [SerializeField] private Text outcomeTitleText;
        [SerializeField] private Text outcomeSubtitleText;
        [SerializeField] private Image outcomeBannerBg;

        [Header("Run Stats")]
        [SerializeField] private Text statsDetailsText;

        [Header("Account Progression Section")]
        [SerializeField] private Text accountLevelText;
        [SerializeField] private Text accountXPText;
        [SerializeField] private Image accountXPFill;
        [SerializeField] private Text accountRewardText;

        [Header("Roguelite Warning")]
        [SerializeField] private Text rogueliteNoticeText;

        [Header("Actions")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button resetProgressionButton;

        public event Action OnPlayAgainRequested;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            if (playAgainButton != null)
            {
                playAgainButton.onClick.AddListener(HandlePlayAgain);
            }

            if (resetProgressionButton != null)
            {
                resetProgressionButton.onClick.AddListener(HandleResetProgression);
            }
        }

        public void Bind(
            GameObject root,
            Text outcomeTitle, Text outcomeSub, Image bannerBg,
            Text statsDetails,
            Text rabbitLevelTxt, Text xpTxt, Image xpFill, Text rewardTxt,
            Text rogueliteNotice,
            Button playAgainBtn,
            Button resetProgressionBtn)
        {
            overlayRoot = root;
            outcomeTitleText = outcomeTitle;
            outcomeSubtitleText = outcomeSub;
            outcomeBannerBg = bannerBg;
            statsDetailsText = statsDetails;
            accountLevelText = rabbitLevelTxt;
            accountXPText = xpTxt;
            accountXPFill = xpFill;
            accountRewardText = rewardTxt;
            rogueliteNoticeText = rogueliteNotice;
            playAgainButton = playAgainBtn;
            resetProgressionButton = resetProgressionBtn;

            if (playAgainButton != null)
            {
                playAgainButton.onClick.RemoveAllListeners();
                playAgainButton.onClick.AddListener(HandlePlayAgain);
            }

            if (resetProgressionButton != null)
            {
                resetProgressionButton.onClick.RemoveAllListeners();
                resetProgressionButton.onClick.AddListener(HandleResetProgression);
            }

            if (overlayRoot != null)
            {
                overlayRoot.SetActive(false);
            }
        }

        public void ShowResults(bool won, int waveReached, int totalKills, int runGold, float durationSeconds)
        {
            Time.timeScale = 0f;

            if (overlayRoot != null)
            {
                overlayRoot.SetActive(true);
            }

            // 1. Cập nhật Tiêu đề & Banner
            if (won)
            {
                if (outcomeTitleText != null)
                {
                    outcomeTitleText.text = "🏆 CHIẾN THẮNG HUY HOÀNG! 🏆";
                    outcomeTitleText.color = new Color(1f, 0.85f, 0.2f);
                }
                if (outcomeSubtitleText != null)
                {
                    outcomeSubtitleText.text = "Bạn đã tiêu diệt King Carrot và bảo vệ thành công Đấu Trường!";
                }
                if (outcomeBannerBg != null)
                {
                    outcomeBannerBg.color = new Color(0.12f, 0.45f, 0.2f, 0.95f);
                }
            }
            else
            {
                if (outcomeTitleText != null)
                {
                    outcomeTitleText.text = "💀 THẤT BẠI TRONG TRẬN ĐẤU 💀";
                    outcomeTitleText.color = new Color(0.95f, 0.25f, 0.25f);
                }
                if (outcomeSubtitleText != null)
                {
                    outcomeSubtitleText.text = $"Chiến binh Thỏ đã ngã gục tại Wave {waveReached}. Hãy thử lại!";
                }
                if (outcomeBannerBg != null)
                {
                    outcomeBannerBg.color = new Color(0.45f, 0.12f, 0.15f, 0.95f);
                }
            }

            // 2. Thống kê chiến tích
            int minutes = Mathf.FloorToInt(durationSeconds / 60f);
            int seconds = Mathf.FloorToInt(durationSeconds % 60f);

            var inv = RunInventory.Instance;
            string meleeName = inv?.EquippedMelee != null ? $"{inv.EquippedMelee.DisplayName} (Lv.{inv.EquippedMelee.Level})" : "Không có";
            string rangedName = inv?.EquippedRanged != null ? $"{inv.EquippedRanged.DisplayName} (Lv.{inv.EquippedRanged.Level})" : "Không có";

            if (statsDetailsText != null)
            {
                statsDetailsText.text =
                    $"• Wave đạt được: {waveReached} / 4\n" +
                    $"• Thời gian sinh tồn: {minutes:D2}:{seconds:D2}\n" +
                    $"• Quái đã tiêu diệt: {totalKills} quái\n" +
                    $"• Vàng thu thập trong run: {runGold} vàng\n" +
                    $"• Vũ khí cuối trận: {meleeName} | {rangedName}";
            }

            // 3. Xử lý thưởng tài khoản qua AccountProgression
            int earnedXP = 0;
            int convertedGold = 0;
            if (AccountProgression.Instance != null)
            {
                AccountProgression.Instance.FinalizeRun(won, totalKills, runGold, out earnedXP, out convertedGold, out _);
            }

            var acc = AccountProgression.Instance != null ? AccountProgression.Instance.Data : null;
            int rabbitLevel = acc != null ? acc.RabbitLevel : 1;
            int currentXP = acc != null ? acc.CurrentXP : 0;
            int requiredXP = AccountData.GetXPRequiredForLevel(rabbitLevel);
            int permanentGold = acc != null ? acc.PermanentGold : 0;

            if (accountLevelText != null)
            {
                accountLevelText.text = $"CẤP THỎ: {rabbitLevel}  |  VÀNG TK: {permanentGold} 💰";
            }

            if (accountXPText != null)
            {
                accountXPText.text = $"XP: {currentXP} / {requiredXP} (+{earnedXP} XP trận này)";
            }

            if (accountXPFill != null && requiredXP > 0)
            {
                accountXPFill.fillAmount = Mathf.Clamp01((float)currentXP / requiredXP);
            }

            if (accountRewardText != null)
            {
                if (won)
                {
                    accountRewardText.text = $"🎁 PHẦN THƯỞNG: +{earnedXP} XP Tài Khoản, +{convertedGold} Vàng Tài Khoản vĩnh viễn!";
                }
                else
                {
                    accountRewardText.text = $"🎁 PHẦN THƯỞNG: +{earnedXP} XP Tài Khoản (ghi nhận theo quái đã hạ).";
                }
            }

            // 4. Roguelite Rule Notice
            if (rogueliteNoticeText != null)
            {
                rogueliteNoticeText.text =
                    "🛡️ QUY TẮC ROGUELITE: Vũ khí & vàng nhặt trong trận đã được reset.\n" +
                    "Cấp thỏ, XP và Thành tựu tài khoản được LƯU CỤC BỘ VĨNH VIỄN không bị mất!";
            }
        }

        private void HandlePlayAgain()
        {
            if (overlayRoot != null)
            {
                overlayRoot.SetActive(false);
            }

            Time.timeScale = 1f;
            OnPlayAgainRequested?.Invoke();
        }

        private void HandleResetProgression()
        {
            if (AccountProgression.Instance != null)
            {
                AccountProgression.Instance.ResetAllProgression();
            }

            var acc = AccountProgression.Instance?.Data;
            if (acc != null)
            {
                int reqXP = AccountData.GetXPRequiredForLevel(acc.RabbitLevel);
                if (accountLevelText != null) accountLevelText.text = $"CẤP THỎ: {acc.RabbitLevel}  |  VÀNG TK: {acc.PermanentGold} 💰";
                if (accountXPText != null) accountXPText.text = $"XP: {acc.CurrentXP} / {reqXP} (Đã reset về mặc định)";
                if (accountXPFill != null) accountXPFill.fillAmount = 0f;
            }
        }
    }
}
