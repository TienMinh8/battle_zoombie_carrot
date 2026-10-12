using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Inventory;
using Apocarrot.Progression;
using Apocarrot.Loot;

namespace Apocarrot.UI
{
    /// <summary>
    /// HUD hiển thị tài nguyên Run (Vàng) và túi đồ (Trang bị + 4 ô Ba lô dự trữ).
    /// Cập nhật thời gian thực khi nhặt được vàng, vũ khí hoặc trang bị (Áo, Ủng, Găng, Dây chuyền).
    /// Hỗ trợ Banner thông báo nổi bật ngay trên HUD khi vừa nhặt được đồ mới.
    /// </summary>
    public class RunInventoryHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RunInventory inventory;
        [SerializeField] private Text goldText;
        [SerializeField] private Text equippedMeleeText;
        [SerializeField] private Text equippedRangedText;
        [SerializeField] private Text[] backpackSlotTexts;
        [SerializeField] private Image[] backpackSlotBgs;
        [SerializeField] private Text notificationBannerText;

        private Coroutine bannerCoroutine;

        private void Start()
        {
            if (inventory == null)
            {
                inventory = RunInventory.Instance ?? FindFirstObjectByType<RunInventory>();
            }

            HookEvents();
            UpdateAllUI();
        }

        private void HookEvents()
        {
            if (inventory != null)
            {
                inventory.OnGoldChanged += UpdateGoldUI;
                inventory.OnInventoryChanged += UpdateInventoryUI;
            }

            if (AccountProgression.Instance != null)
            {
                AccountProgression.Instance.OnAccountDataChanged += UpdateInventoryUI;
            }

            LootToastManager.OnToastNotice += HandleLootToast;
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.OnGoldChanged -= UpdateGoldUI;
                inventory.OnInventoryChanged -= UpdateInventoryUI;
            }

            if (AccountProgression.Instance != null)
            {
                AccountProgression.Instance.OnAccountDataChanged -= UpdateInventoryUI;
            }

            LootToastManager.OnToastNotice -= HandleLootToast;
        }

        public void Bind(RunInventory inv, Text goldTxt, Text meleeTxt, Text rangedTxt, Text[] slotTexts, Image[] slotBgs, Text bannerTxt = null)
        {
            inventory = inv;
            goldText = goldTxt;
            equippedMeleeText = meleeTxt;
            equippedRangedText = rangedTxt;
            backpackSlotTexts = slotTexts;
            backpackSlotBgs = slotBgs;
            notificationBannerText = bannerTxt;

            HookEvents();
            UpdateAllUI();
        }

        private void UpdateAllUI()
        {
            if (inventory != null) UpdateGoldUI(inventory.RunGold);
            UpdateInventoryUI();
        }

        private void UpdateGoldUI(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"VÀNG: {gold} 💰";
            }
        }

        public void UpdateInventoryUI()
        {
            // 1. Cập nhật Vũ khí Cận Chiến đang dùng
            if (equippedMeleeText != null)
            {
                if (inventory != null && inventory.EquippedMelee != null)
                {
                    equippedMeleeText.text = $"[CẬN CHIẾN]\n{inventory.EquippedMelee.DisplayName} Lv.{inventory.EquippedMelee.Level}";
                }
                else
                {
                    var accWpn = AccountProgression.Instance?.Data?.EquippedWeapon;
                    if (accWpn != null)
                    {
                        equippedMeleeText.text = $"[CẬN CHIẾN]\n{accWpn.DisplayName} Lv.{accWpn.Level}";
                    }
                    else
                    {
                        equippedMeleeText.text = "[CẬN CHIẾN]\n(Trống)";
                    }
                }
            }

            // 2. Cập nhật Vũ khí Tầm Xa đang dùng
            if (equippedRangedText != null)
            {
                if (inventory != null && inventory.EquippedRanged != null)
                {
                    equippedRangedText.text = $"[TẦM XA]\n{inventory.EquippedRanged.DisplayName} Lv.{inventory.EquippedRanged.Level}";
                }
                else
                {
                    equippedRangedText.text = "[TẦM XA]\n(Trống)";
                }
            }

            // 3. Cập nhật 4 ô Ba lô dự trữ (Hiển thị vũ khí run HOẶC trang bị vừa nhặt được trong túi tài khoản)
            if (backpackSlotTexts == null) return;

            var backpackWeapons = inventory != null ? inventory.Backpack : null;
            var ownedEquipments = AccountProgression.Instance?.Data?.OwnedEquipments;

            for (int i = 0; i < backpackSlotTexts.Length; i++)
            {
                if (backpackSlotTexts[i] == null) continue;

                // Ưu tiên 1: Vũ khí dự trữ trong Run
                if (backpackWeapons != null && i < backpackWeapons.Count)
                {
                    var wpn = backpackWeapons[i];
                    backpackSlotTexts[i].text = $"<b>{wpn.DisplayName}</b>\n<size=10><color=#ffff55>Lv.{wpn.Level}</color></size>";
                    if (backpackSlotBgs != null && i < backpackSlotBgs.Length && backpackSlotBgs[i] != null)
                    {
                        Color c = wpn.ItemColor;
                        c.a = 0.85f;
                        backpackSlotBgs[i].color = c;
                    }
                }
                // Ưu tiên 2: Trang bị nhặt được (Giáp, Ủng, Găng, Dây chuyền, v.v.)
                else
                {
                    int eqIndex = i - (backpackWeapons != null ? backpackWeapons.Count : 0);
                    if (ownedEquipments != null && eqIndex >= 0 && eqIndex < ownedEquipments.Count)
                    {
                        var eq = ownedEquipments[eqIndex];
                        backpackSlotTexts[i].text = $"<b>[{eq.GetSlotIcon()}] {eq.DisplayName}</b>\n<size=10><color=#33ff88>{eq.GetPrimaryStatText()}</color></size>";
                        if (backpackSlotBgs != null && i < backpackSlotBgs.Length && backpackSlotBgs[i] != null)
                        {
                            Color c = eq.GetRarityColor();
                            c.a = 0.85f;
                            backpackSlotBgs[i].color = c;
                        }
                    }
                    else
                    {
                        backpackSlotTexts[i].text = $"Ô {i + 1}\n<size=10><color=#888888>(Trống)</color></size>";
                        if (backpackSlotBgs != null && i < backpackSlotBgs.Length && backpackSlotBgs[i] != null)
                        {
                            backpackSlotBgs[i].color = new Color(0.12f, 0.14f, 0.20f, 0.65f);
                        }
                    }
                }
            }
        }

        private void HandleLootToast(string message, Color color)
        {
            // Nếu là thông báo nhặt đồ (có icon 🎁 hoặc 💰 hoặc 🌟), hiển thị trên HUD
            if (message.Contains("🎁") || message.Contains("💰") || message.Contains("🌟"))
            {
                ShowPickupNotification(message, color);
            }
        }

        public void ShowPickupNotification(string message, Color color)
        {
            if (notificationBannerText == null) return;

            if (bannerCoroutine != null) StopCoroutine(bannerCoroutine);
            bannerCoroutine = StartCoroutine(BannerRoutine(message, color));
        }

        private IEnumerator BannerRoutine(string message, Color color)
        {
            notificationBannerText.gameObject.SetActive(true);
            notificationBannerText.text = message;
            notificationBannerText.color = color;

            float showTime = 3.2f;
            float elapsed = 0f;

            while (elapsed < showTime)
            {
                elapsed += Time.deltaTime;
                if (elapsed > showTime - 0.6f)
                {
                    float alpha = Mathf.Clamp01((showTime - elapsed) / 0.6f);
                    Color c = color;
                    c.a = alpha;
                    notificationBannerText.color = c;
                }
                yield return null;
            }

            notificationBannerText.gameObject.SetActive(false);
            bannerCoroutine = null;
        }
    }
}
