using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Inventory;

namespace Apocarrot.UI
{
    /// <summary>
    /// HUD hiển thị tài nguyên Run (Vàng) và túi đồ (Trang bị + 4 ô Ba lô dự trữ)
    /// Cập nhật thời gian thực khi nhặt được vàng hoặc vũ khí rơi từ quái.
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

        private void Start()
        {
            if (inventory == null)
            {
                inventory = RunInventory.Instance ?? FindFirstObjectByType<RunInventory>();
            }

            if (inventory != null)
            {
                inventory.OnGoldChanged += UpdateGoldUI;
                inventory.OnInventoryChanged += UpdateInventoryUI;

                UpdateGoldUI(inventory.RunGold);
                UpdateInventoryUI();
            }
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.OnGoldChanged -= UpdateGoldUI;
                inventory.OnInventoryChanged -= UpdateInventoryUI;
            }
        }

        public void Bind(RunInventory inv, Text goldTxt, Text meleeTxt, Text rangedTxt, Text[] slotTexts, Image[] slotBgs)
        {
            inventory = inv;
            goldText = goldTxt;
            equippedMeleeText = meleeTxt;
            equippedRangedText = rangedTxt;
            backpackSlotTexts = slotTexts;
            backpackSlotBgs = slotBgs;

            if (inventory != null)
            {
                inventory.OnGoldChanged += UpdateGoldUI;
                inventory.OnInventoryChanged += UpdateInventoryUI;
                UpdateGoldUI(inventory.RunGold);
                UpdateInventoryUI();
            }
        }

        private void UpdateGoldUI(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"Vàng: {gold}";
            }
        }

        private void UpdateInventoryUI()
        {
            if (inventory == null) return;

            // Cập nhật Vũ khí Cận Chiến đang dùng
            if (equippedMeleeText != null)
            {
                if (inventory.EquippedMelee != null)
                {
                    equippedMeleeText.text = $"[CẬN CHIẾN]\n{inventory.EquippedMelee.DisplayName} Lv.{inventory.EquippedMelee.Level}";
                }
                else
                {
                    equippedMeleeText.text = "[CẬN CHIẾN]\n(Trống)";
                }
            }

            // Cập nhật Vũ khí Tầm Xa đang dùng
            if (equippedRangedText != null)
            {
                if (inventory.EquippedRanged != null)
                {
                    equippedRangedText.text = $"[TẦM XA]\n{inventory.EquippedRanged.DisplayName} Lv.{inventory.EquippedRanged.Level}";
                }
                else
                {
                    equippedRangedText.text = "[TẦM XA]\n(Trống)";
                }
            }

            // Cập nhật 4 ô Ba lô dự trữ
            if (backpackSlotTexts != null)
            {
                var backpack = inventory.Backpack;
                for (int i = 0; i < backpackSlotTexts.Length; i++)
                {
                    if (backpackSlotTexts[i] == null) continue;

                    if (i < backpack.Count)
                    {
                        var item = backpack[i];
                        backpackSlotTexts[i].text = $"{item.DisplayName}\nLv.{item.Level}";
                        if (backpackSlotBgs != null && i < backpackSlotBgs.Length && backpackSlotBgs[i] != null)
                        {
                            Color c = item.ItemColor;
                            c.a = 0.85f;
                            backpackSlotBgs[i].color = c;
                        }
                    }
                    else
                    {
                        backpackSlotTexts[i].text = $"Ô {i + 1}\n(Trống)";
                        if (backpackSlotBgs != null && i < backpackSlotBgs.Length && backpackSlotBgs[i] != null)
                        {
                            backpackSlotBgs[i].color = new Color(0.12f, 0.12f, 0.16f, 0.65f);
                        }
                    }
                }
            }
        }
    }
}
