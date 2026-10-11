using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Gameplay;
using Apocarrot.Inventory;

namespace Apocarrot.UI
{
    /// <summary>
    /// Giao diện màn hình nghỉ giữa Wave (Intermission UI) - Gate 5.
    /// Tạm dừng combat để người chơi:
    /// 1. Xem và thay đổi vũ khí đang trang bị.
    /// 2. Ghép 2 vũ khí cùng loại + cùng cấp -> nâng cấp lên cấp cao hơn (Lv.1 -> Lv.2 -> Lv.3).
    /// 3. Mua vũ khí hoặc hồi phục từ Cửa hàng bằng số Vàng kiếm được trong run.
    /// 4. Vứt bỏ đồ (Discard) khi ba lô đầy.
    /// 5. Bấm Tiếp tục để tiếp tục chiến đấu.
    /// </summary>
    public class IntermissionUI : MonoBehaviour
    {
        public static IntermissionUI Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject overlayRoot;

        [Header("Header Elements")]
        [SerializeField] private Text goldText;

        [Header("Equipped Section")]
        [SerializeField] private Text equippedMeleeTitle;
        [SerializeField] private Text equippedMeleeStats;
        [SerializeField] private Image equippedMeleeBg;

        [SerializeField] private Text equippedRangedTitle;
        [SerializeField] private Text equippedRangedStats;
        [SerializeField] private Image equippedRangedBg;

        [Header("Backpack Section")]
        [SerializeField] private Transform backpackContainer;
        [SerializeField] private Button autoMergeButton;
        [SerializeField] private Text autoMergeButtonText;

        [Header("Shop Section")]
        [SerializeField] private Transform shopContainer;

        [Header("Footer")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Text statusNoticeText;

        private RunInventory inventory;
        private PlayerHealth playerHealth;
        private ShopItemData[] shopStock;
        private Font uiFont;

        public bool IsOpen => overlayRoot != null && overlayRoot.activeSelf;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            shopStock = ShopItemData.GetDefaultStock();
        }

        private void Start()
        {
            inventory = RunInventory.Instance ?? FindFirstObjectByType<RunInventory>();
            playerHealth = FindFirstObjectByType<PlayerHealth>();

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(CloseIntermission);
            }

            if (autoMergeButton != null)
            {
                autoMergeButton.onClick.AddListener(HandleAutoMerge);
            }
        }

        public void Bind(
            GameObject root,
            Text goldTxt,
            Text meleeTitle, Text meleeStats, Image meleeBg,
            Text rangedTitle, Text rangedStats, Image rangedBg,
            Transform bagContainer,
            Button autoMergeBtn, Text autoMergeBtnTxt,
            Transform shopCont,
            Button continueBtn,
            Text statusNotice,
            Font font)
        {
            overlayRoot = root;
            goldText = goldTxt;
            equippedMeleeTitle = meleeTitle;
            equippedMeleeStats = meleeStats;
            equippedMeleeBg = meleeBg;
            equippedRangedTitle = rangedTitle;
            equippedRangedStats = rangedStats;
            equippedRangedBg = rangedBg;
            backpackContainer = bagContainer;
            autoMergeButton = autoMergeBtn;
            autoMergeButtonText = autoMergeBtnTxt;
            shopContainer = shopCont;
            continueButton = continueBtn;
            statusNoticeText = statusNotice;
            uiFont = font;

            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(CloseIntermission);
            }

            if (autoMergeButton != null)
            {
                autoMergeButton.onClick.RemoveAllListeners();
                autoMergeButton.onClick.AddListener(HandleAutoMerge);
            }
        }

        public void OpenIntermission()
        {
            if (inventory == null) inventory = RunInventory.Instance ?? FindFirstObjectByType<RunInventory>();
            if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();

            // Tạm dừng thời gian trong game
            Time.timeScale = 0f;

            if (overlayRoot != null)
            {
                overlayRoot.SetActive(true);
            }

            RefreshAllUI();
            ShowNotice("Chào mừng tới Trạm Nghỉ! Hãy ghép đồ và ghé Cửa hàng trước khi tiếp tục.", Color.white);
        }

        public event Action OnContinueClicked;

        public void CloseIntermission()
        {
            if (overlayRoot != null)
            {
                overlayRoot.SetActive(false);
            }

            // Tiếp tục thời gian trong game
            Time.timeScale = 1f;

            OnContinueClicked?.Invoke();
        }

        public void RefreshAllUI()
        {
            if (inventory == null) return;

            // 1. Cập nhật Vàng
            if (goldText != null)
            {
                goldText.text = $"💰 VÀNG HIỆN CÓ: {inventory.RunGold}";
            }

            // 2. Cập nhật Trang bị đang dùng
            if (inventory.EquippedMelee != null)
            {
                equippedMeleeTitle.text = $"🗡️ {inventory.EquippedMelee.DisplayName} (Lv.{inventory.EquippedMelee.Level})";
                equippedMeleeStats.text = inventory.EquippedMelee.GetStatsSummary();
                if (equippedMeleeBg != null)
                {
                    Color c = inventory.EquippedMelee.ItemColor;
                    c.a = 0.9f;
                    equippedMeleeBg.color = c;
                }
            }
            else
            {
                equippedMeleeTitle.text = "🗡️ Chưa trang bị cận chiến";
                equippedMeleeStats.text = "";
                if (equippedMeleeBg != null) equippedMeleeBg.color = new Color(0.18f, 0.18f, 0.22f, 0.8f);
            }

            if (inventory.EquippedRanged != null)
            {
                equippedRangedTitle.text = $"🏹 {inventory.EquippedRanged.DisplayName} (Lv.{inventory.EquippedRanged.Level})";
                equippedRangedStats.text = inventory.EquippedRanged.GetStatsSummary();
                if (equippedRangedBg != null)
                {
                    Color c = inventory.EquippedRanged.ItemColor;
                    c.a = 0.9f;
                    equippedRangedBg.color = c;
                }
            }
            else
            {
                equippedRangedTitle.text = "🏹 Chưa trang bị tầm xa";
                equippedRangedStats.text = "";
                if (equippedRangedBg != null) equippedRangedBg.color = new Color(0.18f, 0.18f, 0.22f, 0.8f);
            }

            // 3. Cập nhật Ba lô 4 ô
            BuildBackpackSlots();

            // 4. Cập nhật Cửa hàng
            BuildShopItems();

            // 5. Cập nhật Nút Ghép Nhanh
            bool canMerge = inventory.CanMergeAny();
            if (autoMergeButton != null)
            {
                autoMergeButton.interactable = canMerge;
                Image btnImg = autoMergeButton.GetComponent<Image>();
                if (btnImg != null)
                {
                    btnImg.color = canMerge ? new Color(0.95f, 0.72f, 0.1f) : new Color(0.35f, 0.35f, 0.4f);
                }
            }
            if (autoMergeButtonText != null)
            {
                autoMergeButtonText.text = canMerge ? "⚡ GHÉP VŨ KHÍ (CÓ CẶP TRÙNG!)" : "⚡ Ghép vũ khí (Không có cặp trùng)";
            }
        }

        private void BuildBackpackSlots()
        {
            if (backpackContainer == null) return;

            // Xóa các ô cũ
            foreach (Transform child in backpackContainer)
            {
                Destroy(child.gameObject);
            }

            var bag = inventory.Backpack;

            for (int i = 0; i < RunInventory.MAX_BACKPACK_SLOTS; i++)
            {
                int index = i;
                bool hasItem = index < bag.Count;
                WeaponItem item = hasItem ? bag[index] : null;

                GameObject slotObj = new GameObject($"Slot_{i + 1}");
                slotObj.transform.SetParent(backpackContainer, false);
                RectTransform slotRect = slotObj.AddComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(340, 68);

                Image slotBg = slotObj.AddComponent<Image>();
                slotBg.color = hasItem ? new Color(0.16f, 0.18f, 0.24f, 0.95f) : new Color(0.1f, 0.1f, 0.14f, 0.5f);

                // Viền màu nếu có đồ
                if (hasItem && item != null)
                {
                    GameObject borderObj = new GameObject("Border");
                    borderObj.transform.SetParent(slotObj.transform, false);
                    Image borderImg = borderObj.AddComponent<Image>();
                    Color bCol = item.ItemColor;
                    bCol.a = 0.8f;
                    borderImg.color = bCol;
                    RectTransform bRect = borderObj.GetComponent<RectTransform>();
                    bRect.anchorMin = Vector2.zero;
                    bRect.anchorMax = new Vector2(0, 1);
                    bRect.pivot = new Vector2(0, 0.5f);
                    bRect.anchoredPosition = new Vector2(0, 0);
                    bRect.sizeDelta = new Vector2(6, 0);
                }

                // Nhãn thông tin
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(slotObj.transform, false);
                Text label = labelObj.AddComponent<Text>();
                label.font = uiFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 13;
                label.alignment = TextAnchor.MiddleLeft;

                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0, 0);
                labelRect.anchorMax = new Vector2(0.65f, 1);
                labelRect.offsetMin = new Vector2(16, 4);
                labelRect.offsetMax = new Vector2(0, -4);

                if (hasItem && item != null)
                {
                    label.color = Color.white;
                    label.text = $"<b>{item.DisplayName} (Lv.{item.Level})</b>\n<color=#B0B0B0>{item.GetStatsSummary()}</color>";

                    // Nút Đổi trang bị (Equip)
                    CreateMiniButton(slotObj.transform, "Trang Bị", new Vector2(245, 0), new Vector2(65, 26), new Color(0.2f, 0.6f, 0.9f), () =>
                    {
                        inventory.SwapWithEquipped(index);
                        RefreshAllUI();
                        ShowNotice($"Đã đổi trang bị {item.DisplayName} vào vị trí hoạt động!", Color.cyan);
                    });

                    // Nút Vứt bỏ (Discard)
                    CreateMiniButton(slotObj.transform, "Vứt", new Vector2(310, 0), new Vector2(40, 26), new Color(0.8f, 0.25f, 0.25f), () =>
                    {
                        inventory.DiscardBackpackItem(index);
                        RefreshAllUI();
                        ShowNotice("Đã vứt bỏ 1 món đồ trong ba lô.", new Color(1f, 0.4f, 0.4f));
                    });
                }
                else
                {
                    label.color = new Color(0.5f, 0.5f, 0.6f);
                    label.text = $"<i>Ô ba lô {i + 1} (Trống)</i>";
                }
            }
        }

        private void BuildShopItems()
        {
            if (shopContainer == null || shopStock == null) return;

            foreach (Transform child in shopContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < shopStock.Length; i++)
            {
                ShopItemData item = shopStock[i];

                GameObject cardObj = new GameObject($"ShopItem_{i}");
                cardObj.transform.SetParent(shopContainer, false);
                RectTransform cardRect = cardObj.AddComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(320, 58);

                Image cardBg = cardObj.AddComponent<Image>();
                cardBg.color = new Color(0.14f, 0.15f, 0.20f, 0.95f);

                // Nhãn thông tin món bán
                GameObject infoObj = new GameObject("Info");
                infoObj.transform.SetParent(cardObj.transform, false);
                Text info = infoObj.AddComponent<Text>();
                info.font = uiFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                info.fontSize = 12;
                info.alignment = TextAnchor.MiddleLeft;
                info.color = Color.white;
                info.text = $"<b>{item.Title}</b>\n<color=#A0A0B0>{item.Description}</color>";

                RectTransform infoRect = infoObj.GetComponent<RectTransform>();
                infoRect.anchorMin = new Vector2(0, 0);
                infoRect.anchorMax = new Vector2(0.7f, 1);
                infoRect.offsetMin = new Vector2(10, 4);
                infoRect.offsetMax = new Vector2(0, -4);

                // Nút Mua hàng
                bool canAfford = inventory != null && inventory.RunGold >= item.Price;
                Color btnColor = canAfford ? new Color(0.18f, 0.75f, 0.35f) : new Color(0.35f, 0.35f, 0.4f);

                CreateMiniButton(cardObj.transform, $"{item.Price} 💰", new Vector2(265, 0), new Vector2(85, 36), btnColor, () =>
                {
                    HandleBuyItem(item);
                });
            }
        }

        private void HandleBuyItem(ShopItemData item)
        {
            if (inventory == null) return;

            if (inventory.RunGold < item.Price)
            {
                ShowNotice("Không đủ vàng để mua món này!", Color.red);
                return;
            }

            if (item.ItemType == ShopItemType.Weapon)
            {
                if (inventory.IsBackpackFull)
                {
                    ShowNotice("Ba lô đã đầy (4/4)! Hãy vứt hoặc ghép vũ khí trước khi mua thêm.", Color.yellow);
                    return;
                }

                if (inventory.TrySpendGold(item.Price))
                {
                    inventory.TryAddWeapon(item.Weapon.Clone());
                    RefreshAllUI();
                    ShowNotice($"Đã mua thành công: {item.Title} (-{item.Price} Vàng)!", Color.green);
                }
            }
            else if (item.ItemType == ShopItemType.Heal)
            {
                if (inventory.TrySpendGold(item.Price))
                {
                    if (playerHealth != null)
                    {
                        playerHealth.Heal(item.HealAmount);
                    }
                    RefreshAllUI();
                    ShowNotice($"Đã hồi phục +{item.HealAmount} HP (-{item.Price} Vàng)!", Color.green);
                }
            }
        }

        private void HandleAutoMerge()
        {
            if (inventory == null) return;

            bool merged = inventory.AutoMergeAny();
            if (merged)
            {
                RefreshAllUI();
                ShowNotice("⚡ Ghép thành công! Vũ khí đã nâng lên cấp mới!", Color.yellow);
            }
            else
            {
                ShowNotice("Không tìm thấy 2 vũ khí nào cùng loại và cùng cấp để ghép.", Color.white);
            }
        }

        private void CreateMiniButton(Transform parent, string labelText, Vector2 anchoredPos, Vector2 size, Color color, Action onClick)
        {
            GameObject btnObj = new GameObject("Btn_" + labelText);
            btnObj.transform.SetParent(parent, false);

            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = color;

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            Text txt = txtObj.AddComponent<Text>();
            txt.font = uiFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 12;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = labelText;

            RectTransform txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
        }

        private void ShowNotice(string text, Color color)
        {
            if (statusNoticeText != null)
            {
                statusNoticeText.text = text;
                statusNoticeText.color = color;
            }
        }
    }
}
