using System;
using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Gameplay;
using Apocarrot.Inventory;

namespace Apocarrot.UI
{
    /// <summary>
    /// Màn hình Chiến Thắng khi hạ gục King Carrot (Gate 6).
    /// </summary>
    public class VictoryScreenUI : MonoBehaviour
    {
        public static VictoryScreenUI Instance { get; private set; }

        [SerializeField] private GameObject overlayRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text statsSummaryText;
        [SerializeField] private Button restartButton;

        public event Action OnRestartRequested;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }
        }

        public void Bind(GameObject root, Text title, Text statsSummary, Button restartBtn)
        {
            overlayRoot = root;
            titleText = title;
            statsSummaryText = statsSummary;
            restartButton = restartBtn;

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(HandleRestartClicked);
            }

            if (overlayRoot != null)
            {
                overlayRoot.SetActive(false);
            }
        }

        public void ShowVictory(float durationSeconds, int totalKills, int totalGold)
        {
            Time.timeScale = 0f;

            if (overlayRoot != null)
            {
                overlayRoot.SetActive(true);
            }

            int minutes = Mathf.FloorToInt(durationSeconds / 60f);
            int seconds = Mathf.FloorToInt(durationSeconds % 60f);

            var inv = RunInventory.Instance;
            string meleeName = inv?.EquippedMelee != null ? $"{inv.EquippedMelee.DisplayName} (Lv.{inv.EquippedMelee.Level})" : "Không có";
            string rangedName = inv?.EquippedRanged != null ? $"{inv.EquippedRanged.DisplayName} (Lv.{inv.EquippedRanged.Level})" : "Không có";

            if (statsSummaryText != null)
            {
                statsSummaryText.text =
                    $"⏱️ Thời gian sinh tồn: {minutes:D2}:{seconds:D2}\n\n" +
                    $"🥕 Tổng quái đã hạ: {totalKills} quái\n\n" +
                    $"💰 Vàng thu thập: {totalGold} vàng\n\n" +
                    $"🗡️ Vũ khí Cận chiến: {meleeName}\n" +
                    $"🍅 Vũ khí Tầm xa: {rangedName}\n\n" +
                    $"⭐ Hoàn thành xuất sắc mục tiêu Demo Gate 6!";
            }
        }

        private void HandleRestartClicked()
        {
            if (overlayRoot != null)
            {
                overlayRoot.SetActive(false);
            }

            Time.timeScale = 1f;
            OnRestartRequested?.Invoke();
        }
    }
}
