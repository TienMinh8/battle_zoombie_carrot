using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Inventory;
using Apocarrot.Progression;
using Apocarrot.Gameplay;
using Apocarrot.Loot;

namespace Apocarrot.UI
{
    /// <summary>
    /// GIAO DIỆN BALO & TRANG BỊ DUY NHẤT CỦA GAME (UNIFIED EQUIPMENT & SHOP BAG).
    /// Thiết kế chuẩn chỉnh theo phong cách Survivor.io / Archero:
    /// - Hợp nhất toàn bộ: Trang bị 6 ô, Balo đồ sở hữu, Ghép đồ (Fuse), Cửa hàng Run Shop và Tiếp tục Wave.
    /// - Không còn bất kỳ màn hình nào bị chồng đè hay gây khó hiểu!
    /// </summary>
    public class EquipmentBagUI : MonoBehaviour
    {
        public static EquipmentBagUI Instance { get; private set; }

        public static HashSet<string> recentlyCollectedUids = new HashSet<string>();
        public static event Action OnRecentlyCollectedChanged;

        public static void MarkRecentlyCollected(string uid)
        {
            if (string.IsNullOrEmpty(uid)) return;
            recentlyCollectedUids.Add(uid);
            OnRecentlyCollectedChanged?.Invoke();
        }

        public static int UnviewedNewItemsCount => recentlyCollectedUids.Count;

        public event Action OnContinueWaveRequested;

        [Header("Root Overlay")]
        [SerializeField] private GameObject bagRoot;

        [Header("Top Bar Elements")]
        [SerializeField] private Text levelText;
        [SerializeField] private Text energyText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text runGoldText;
        [SerializeField] private Text gemsText;
        [SerializeField] private Button closeBagButton;

        [Header("Hero Showcase & Stats")]
        [SerializeField] private Text heroNameText;
        [SerializeField] private Text totalAtkText;
        [SerializeField] private Text totalHpText;
        [SerializeField] private Button rotateHeroButton;

        [Header("6 Equipped Slots")]
        [SerializeField] private Button weaponSlotBtn;
        [SerializeField] private Button necklaceSlotBtn;
        [SerializeField] private Button glovesSlotBtn;
        [SerializeField] private Button armorSlotBtn;
        [SerializeField] private Button beltSlotBtn;
        [SerializeField] private Button bootsSlotBtn;

        [Header("Mid Bar Tabs")]
        [SerializeField] private Button tabBagButton;
        [SerializeField] private Button tabShopButton;
        [SerializeField] private Button fuseButton;
        [SerializeField] private Text tabBagText;
        [SerializeField] private Text tabShopText;

        [Header("Scroll Containers")]
        [SerializeField] private GameObject bagGridContainer;
        [SerializeField] private GameObject shopListContainer;

        [Header("Footer Action Button")]
        [SerializeField] private Button continueActionButton;
        [SerializeField] private Text continueActionText;

        [Header("Detail Modal")]
        [SerializeField] private GameObject detailModalRoot;
        [SerializeField] private Text detailItemName;
        [SerializeField] private Text detailItemRarity;
        [SerializeField] private Text detailItemStats;
        [SerializeField] private Text detailItemDesc;
        [SerializeField] private Button detailEquipButton;
        [SerializeField] private Text detailEquipButtonText;
        [SerializeField] private Button detailCloseButton;

        [Header("Fuse Modal")]
        [SerializeField] private GameObject fuseModalRoot;
        [SerializeField] private Text fuseModalNotice;
        [SerializeField] private Button autoFuseButton;
        [SerializeField] private Button closeFuseButton;

        [SerializeField] private Font uiFont;
        public Font UIFont
        {
            get
            {
                if (uiFont == null)
                {
                    uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return uiFont;
            }
            set => uiFont = value;
        }

        private EquipmentItem selectedItem;
        private bool isSelectedEquipped = false;
        private EquipmentSlotType selectedSlotType;
        private float heroRotation = 0f;
        private Transform heroPreviewTransform;

        private bool isIntermissionActive = false;
        private int currentIntermissionWave = 1;
        private bool isShopTabActive = false;
        private bool isDirty = false;

        public bool IsOpen => bagRoot != null && bagRoot.activeSelf;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (uiFont == null)
            {
                uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
        }

        private void Start()
        {
            if (uiFont == null)
            {
                uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            HookEvents();
            RefreshAll();

            if (bagRoot != null)
            {
                bagRoot.SetActive(false);
            }
        }

        private void Update()
        {
            // Phím tắt 'B' hoặc 'I' để mở/đóng túi nhanh khi chiến đấu
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.bKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
                {
                    ToggleBag();
                }
            }
        }

        private void HookEvents()
        {
            if (rotateHeroButton != null) rotateHeroButton.onClick.AddListener(RotateHeroPreview);
            if (fuseButton != null) fuseButton.onClick.AddListener(OpenFuseModal);
            if (autoFuseButton != null) autoFuseButton.onClick.AddListener(HandleAutoFuse);

            if (closeFuseButton != null) closeFuseButton.onClick.AddListener(() => {
                if (fuseModalRoot != null) fuseModalRoot.SetActive(false);
            });

            if (detailCloseButton != null) detailCloseButton.onClick.AddListener(() => {
                if (detailModalRoot != null) detailModalRoot.SetActive(false);
            });

            if (detailEquipButton != null) detailEquipButton.onClick.AddListener(HandleDetailEquipOrUnequip);
            if (closeBagButton != null) closeBagButton.onClick.AddListener(HandleCloseOrContinue);
            if (continueActionButton != null) continueActionButton.onClick.AddListener(HandleCloseOrContinue);

            if (tabBagButton != null) tabBagButton.onClick.AddListener(() => SwitchTab(false));
            if (tabShopButton != null) tabShopButton.onClick.AddListener(() => SwitchTab(true));

            // Gán sự kiện click cho 6 ô trang bị trên người
            if (weaponSlotBtn != null) weaponSlotBtn.onClick.AddListener(() => OpenEquippedDetail(EquipmentSlotType.Weapon));
            if (necklaceSlotBtn != null) necklaceSlotBtn.onClick.AddListener(() => OpenEquippedDetail(EquipmentSlotType.Necklace));
            if (glovesSlotBtn != null) glovesSlotBtn.onClick.AddListener(() => OpenEquippedDetail(EquipmentSlotType.Gloves));
            if (armorSlotBtn != null) armorSlotBtn.onClick.AddListener(() => OpenEquippedDetail(EquipmentSlotType.Armor));
            if (beltSlotBtn != null) beltSlotBtn.onClick.AddListener(() => OpenEquippedDetail(EquipmentSlotType.Belt));
            if (bootsSlotBtn != null) bootsSlotBtn.onClick.AddListener(() => OpenEquippedDetail(EquipmentSlotType.Boots));

            if (AccountProgression.Instance != null)
            {
                AccountProgression.Instance.OnAccountDataChanged += HandleAccountDataChanged;
            }
        }

        private void OnDestroy()
        {
            if (AccountProgression.Instance != null)
            {
                AccountProgression.Instance.OnAccountDataChanged -= HandleAccountDataChanged;
            }
        }

        private void HandleAccountDataChanged()
        {
            if (IsOpen)
            {
                RefreshAll();
            }
            else
            {
                isDirty = true;
            }
        }

        public void OpenBag()
        {
            isIntermissionActive = false;
            if (bagRoot != null)
            {
                bagRoot.SetActive(true);
                SwitchTab(false);
                RefreshAll();
                isDirty = false;
            }
        }

        public void OpenForIntermission(int waveCompleted)
        {
            isIntermissionActive = true;
            currentIntermissionWave = waveCompleted;

            if (bagRoot != null)
            {
                bagRoot.SetActive(true);
                // Mặc định ở giữa wave thì mở tab Balo để xem đồ nhặt được và ghép đồ
                SwitchTab(false);
                RefreshAll();
                isDirty = false;
            }
        }

        public void CloseBag()
        {
            if (bagRoot != null)
            {
                bagRoot.SetActive(false);
            }
            if (detailModalRoot != null) detailModalRoot.SetActive(false);
            if (fuseModalRoot != null) fuseModalRoot.SetActive(false);

            var player = FindFirstObjectByType<PlayerHealth>();
            if (player != null)
            {
                player.ApplyEquipmentHealthBonus();
            }
        }

        public void ToggleBag()
        {
            if (IsOpen) CloseBag();
            else OpenBag();
        }

        private void HandleCloseOrContinue()
        {
            CloseBag();

            if (isIntermissionActive)
            {
                isIntermissionActive = false;
                OnContinueWaveRequested?.Invoke();
            }
        }

        private void SwitchTab(bool showShop)
        {
            isShopTabActive = showShop;

            if (bagGridContainer != null) bagGridContainer.SetActive(!showShop);
            if (shopListContainer != null) shopListContainer.SetActive(showShop);

            if (tabBagButton != null)
            {
                tabBagButton.GetComponent<Image>().color = !showShop ? new Color(0.18f, 0.45f, 0.85f) : new Color(0.12f, 0.16f, 0.24f);
            }
            if (tabShopButton != null)
            {
                tabShopButton.GetComponent<Image>().color = showShop ? new Color(0.18f, 0.45f, 0.85f) : new Color(0.12f, 0.16f, 0.24f);
            }
            if (tabBagText != null) tabBagText.color = !showShop ? Color.yellow : Color.white;
            if (tabShopText != null) tabShopText.color = showShop ? Color.yellow : Color.white;

            if (showShop)
            {
                RefreshShopList();
            }
        }

        public void RefreshAll()
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null) return;

            // 1. Top Bar
            if (levelText != null) levelText.text = $"{acc.RabbitLevel}";
            if (goldText != null) goldText.text = $"{acc.PermanentGold}";
            if (gemsText != null) gemsText.text = $"{acc.PermanentGems}";
            if (energyText != null) energyText.text = acc.UnlimitedEnergyForTesting ? "20/20" : $"{acc.CurrentEnergy}/{acc.MaxEnergy}";

            int runGold = RunInventory.Instance != null ? RunInventory.Instance.RunGold : 0;
            if (runGoldText != null) runGoldText.text = $"{runGold}";

            // 2. Hero & Stats
            if (heroNameText != null)
            {
                heroNameText.text = isIntermissionActive ? $"🎉 HOÀN THÀNH WAVE {currentIntermissionWave}! (TRẠM NGHỈ)" : acc.RabbitName;
            }

            int totalAtk = 25 + acc.GetTotalEquipmentAttack();
            int totalHp = 100 + acc.GetTotalEquipmentHealth();

            if (totalAtkText != null) totalAtkText.text = $"{totalAtk}";
            if (totalHpText != null) totalHpText.text = $"{totalHp}";

            // 3. 6 Ô Trang Bị
            UpdateSlotButton(weaponSlotBtn, acc.EquippedWeapon, EquipmentSlotType.Weapon);
            UpdateSlotButton(necklaceSlotBtn, acc.EquippedNecklace, EquipmentSlotType.Necklace);
            UpdateSlotButton(glovesSlotBtn, acc.EquippedGloves, EquipmentSlotType.Gloves);
            UpdateSlotButton(armorSlotBtn, acc.EquippedArmor, EquipmentSlotType.Armor);
            UpdateSlotButton(beltSlotBtn, acc.EquippedBelt, EquipmentSlotType.Belt);
            UpdateSlotButton(bootsSlotBtn, acc.EquippedBoots, EquipmentSlotType.Boots);

            // 4. Lưới Túi Đồ
            RefreshBagGrid(acc.OwnedEquipments);
            if (tabBagText != null)
            {
                int totalItems = acc.OwnedEquipments != null ? acc.OwnedEquipments.Count : 0;
                string newTag = recentlyCollectedUids.Count > 0 ? $" <color=#ff4444>({recentlyCollectedUids.Count} MỚI!)</color>" : "";
                tabBagText.text = $"🎒 Trang Bị ({totalItems}){newTag}";
            }

            // 5. Nút Tiếp Tục dưới đáy
            if (continueActionText != null)
            {
                if (isIntermissionActive)
                {
                    continueActionText.text = $"▶ TIẾP TỤC CHIẾN ĐẤU (WAVE {currentIntermissionWave + 1}) ◀";
                }
                else
                {
                    continueActionText.text = "▶ TIẾP TỤC CHIẾN ĐẤU ◀";
                }
            }
        }

        private void UpdateSlotButton(Button btn, EquipmentItem item, EquipmentSlotType slot)
        {
            if (btn == null) return;
            Transform itemIconTr = btn.transform.Find("ItemIcon");
            Transform levelBadgeTr = btn.transform.Find("LevelText");
            Transform slotBadgeTr = btn.transform.Find("SlotBadge");
            Transform nameTr = btn.transform.Find("ItemName");
            Image bgImage = btn.GetComponent<Image>();

            if (item != null)
            {
                if (bgImage != null) bgImage.color = item.GetRarityColor();
                if (itemIconTr != null)
                {
                    Text iconTxt = itemIconTr.GetComponent<Text>();
                    if (iconTxt != null)
                    {
                        iconTxt.text = $"[{item.GetSlotIcon()}]\n<size=11><color=#33ff88>{item.GetPrimaryStatText()}</color></size>";
                    }
                }
                if (levelBadgeTr != null)
                {
                    Text lvlTxt = levelBadgeTr.GetComponent<Text>();
                    if (lvlTxt != null) lvlTxt.text = $"Lv.{item.Level}";
                }
                if (slotBadgeTr != null)
                {
                    Text sTxt = slotBadgeTr.GetComponent<Text>();
                    if (sTxt != null) sTxt.text = item.GetSlotIcon();
                }
                if (nameTr != null)
                {
                    Text nTxt = nameTr.GetComponent<Text>();
                    if (nTxt != null)
                    {
                        nTxt.text = item.DisplayName;
                        nTxt.color = item.GetRarityColor();
                    }
                }
            }
            else
            {
                if (bgImage != null) bgImage.color = new Color(0.12f, 0.16f, 0.24f, 0.9f);
                if (itemIconTr != null)
                {
                    Text iconTxt = itemIconTr.GetComponent<Text>();
                    if (iconTxt != null) iconTxt.text = $"[TRỐNG]\n<size=10><color=#888888>{GetEmptySlotLabel(slot)}</color></size>";
                }
                if (levelBadgeTr != null)
                {
                    Text lvlTxt = levelBadgeTr.GetComponent<Text>();
                    if (lvlTxt != null) lvlTxt.text = "";
                }
                if (slotBadgeTr != null)
                {
                    Text sTxt = slotBadgeTr.GetComponent<Text>();
                    if (sTxt != null) sTxt.text = GetEmptySlotLabel(slot);
                }
                if (nameTr != null)
                {
                    Text nTxt = nameTr.GetComponent<Text>();
                    if (nTxt != null) nTxt.text = "(Trống)";
                }
            }
        }

        private string GetEmptySlotLabel(EquipmentSlotType slot)
        {
            switch (slot)
            {
                case EquipmentSlotType.Weapon: return "VŨ KHÍ";
                case EquipmentSlotType.Necklace: return "DÂY";
                case EquipmentSlotType.Gloves: return "GĂNG";
                case EquipmentSlotType.Armor: return "GIÁP";
                case EquipmentSlotType.Belt: return "ĐAI";
                case EquipmentSlotType.Boots: return "ỦNG";
                default: return "TRỐNG";
            }
        }

        private void RefreshBagGrid(List<EquipmentItem> items)
        {
            if (bagGridContainer == null) return;
            Transform gridContent = bagGridContainer.transform.Find("Viewport/GridContent") ?? bagGridContainer.transform;

            if (items == null || items.Count == 0)
            {
                for (int i = 0; i < gridContent.childCount; i++)
                {
                    gridContent.GetChild(i).gameObject.SetActive(false);
                }
                return;
            }

            int count = items.Count;
            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                if (i < gridContent.childCount)
                {
                    UpdateBagItemCard(gridContent.GetChild(i), item);
                }
                else
                {
                    CreateBagItemCard(gridContent, item);
                }
            }

            for (int i = count; i < gridContent.childCount; i++)
            {
                gridContent.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void UpdateBagItemCard(Transform cardTr, EquipmentItem item)
        {
            cardTr.gameObject.SetActive(true);
            cardTr.name = $"Item_{item.Uid}";

            Image bg = cardTr.GetComponent<Image>();
            if (bg != null) bg.color = item.GetRarityColor();

            Transform badgeTr = cardTr.Find("TypeBadge");
            if (badgeTr != null)
            {
                Text bTxt = badgeTr.GetComponent<Text>();
                if (bTxt != null)
                {
                    bTxt.font = UIFont;
                    bTxt.text = $"[{item.GetSlotIcon()}]";
                    bTxt.color = item.GetRarityColor();
                }
            }

            Transform lvlTr = cardTr.Find("LvlText");
            if (lvlTr != null)
            {
                Text lTxt = lvlTr.GetComponent<Text>();
                if (lTxt != null)
                {
                    lTxt.font = UIFont;
                    lTxt.text = $"Lv.{item.Level}";
                }
            }

            Transform iconTr = cardTr.Find("Icon");
            if (iconTr != null)
            {
                Text iTxt = iconTr.GetComponent<Text>();
                if (iTxt != null)
                {
                    iTxt.font = UIFont;
                    iTxt.text = $"<b>[{item.GetSlotIcon()}]</b>\n<size=11><color=#33ff77>{item.GetPrimaryStatText()}</color></size>";
                }
            }

            Transform nameTr = cardTr.Find("Name");
            if (nameTr != null)
            {
                Text nTxt = nameTr.GetComponent<Text>();
                if (nTxt != null)
                {
                    nTxt.font = UIFont;
                    nTxt.text = item.DisplayName;
                    nTxt.color = item.GetRarityColor();
                }
            }

            Transform newBadgeTr = cardTr.Find("New_Badge");
            bool isNew = recentlyCollectedUids.Contains(item.Uid);
            if (newBadgeTr != null)
            {
                newBadgeTr.gameObject.SetActive(isNew);
            }

            Button cardBtn = cardTr.GetComponent<Button>();
            if (cardBtn != null)
            {
                cardBtn.onClick.RemoveAllListeners();
                cardBtn.onClick.AddListener(() => OpenBagItemDetail(item));
            }
        }

        private void CreateBagItemCard(Transform parent, EquipmentItem item)
        {
            GameObject cardObj = new GameObject($"Item_{item.Uid}");
            cardObj.transform.SetParent(parent, false);

            RectTransform rt = cardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(118, 128);

            Image bg = cardObj.AddComponent<Image>();
            bg.color = item.GetRarityColor();

            GameObject innerObj = new GameObject("Inner");
            innerObj.transform.SetParent(cardObj.transform, false);
            RectTransform inRt = innerObj.AddComponent<RectTransform>();
            inRt.anchorMin = new Vector2(0.04f, 0.04f);
            inRt.anchorMax = new Vector2(0.96f, 0.96f);
            inRt.sizeDelta = Vector2.zero;
            Image inBg = innerObj.AddComponent<Image>();
            inBg.color = new Color(0.08f, 0.11f, 0.18f, 0.96f);

            // Badge slot type
            GameObject badgeObj = new GameObject("TypeBadge");
            badgeObj.transform.SetParent(cardObj.transform, false);
            RectTransform bRt = badgeObj.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0, 1);
            bRt.anchorMax = new Vector2(0, 1);
            bRt.pivot = new Vector2(0, 1);
            bRt.anchoredPosition = new Vector2(6, -6);
            bRt.sizeDelta = new Vector2(50, 18);
            Text bTxt = badgeObj.AddComponent<Text>();
            bTxt.font = UIFont;
            bTxt.fontSize = 10;
            bTxt.fontStyle = FontStyle.Bold;
            bTxt.text = $"[{item.GetSlotIcon()}]";
            bTxt.alignment = TextAnchor.MiddleLeft;
            bTxt.color = item.GetRarityColor();

            // Level text
            GameObject lvlObj = new GameObject("LvlText");
            lvlObj.transform.SetParent(cardObj.transform, false);
            RectTransform lRt = lvlObj.AddComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0.5f, 1);
            lRt.anchorMax = new Vector2(1, 1);
            lRt.pivot = new Vector2(1, 1);
            lRt.anchoredPosition = new Vector2(-6, -6);
            lRt.sizeDelta = new Vector2(50, 18);
            Text lTxt = lvlObj.AddComponent<Text>();
            lTxt.font = UIFont;
            lTxt.fontSize = 11;
            lTxt.fontStyle = FontStyle.Bold;
            lTxt.alignment = TextAnchor.MiddleRight;
            lTxt.color = Color.white;
            lTxt.text = $"Lv.{item.Level}";

            // Center Content (Icon Symbol & Primary Stat)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cardObj.transform, false);
            RectTransform iRt = iconObj.AddComponent<RectTransform>();
            iRt.anchoredPosition = new Vector2(0, 6);
            iRt.sizeDelta = new Vector2(104, 48);
            Text iTxt = iconObj.AddComponent<Text>();
            iTxt.font = UIFont;
            iTxt.fontSize = 13;
            iTxt.fontStyle = FontStyle.Bold;
            iTxt.alignment = TextAnchor.MiddleCenter;
            iTxt.color = Color.white;
            iTxt.text = $"<b>[{item.GetSlotIcon()}]</b>\n<size=11><color=#33ff77>{item.GetPrimaryStatText()}</color></size>";

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(cardObj.transform, false);
            RectTransform nRt = nameObj.AddComponent<RectTransform>();
            nRt.anchorMin = new Vector2(0, 0);
            nRt.anchorMax = new Vector2(1, 0);
            nRt.pivot = new Vector2(0.5f, 0);
            nRt.anchoredPosition = new Vector2(0, 5);
            nRt.sizeDelta = new Vector2(0, 24);
            Text nTxt = nameObj.AddComponent<Text>();
            nTxt.font = UIFont;
            nTxt.fontSize = 10;
            nTxt.fontStyle = FontStyle.Bold;
            nTxt.alignment = TextAnchor.MiddleCenter;
            nTxt.color = item.GetRarityColor();
            nTxt.text = item.DisplayName;

            // Huy hiệu MỚI (NEW Badge) nếu vừa mới nhặt được từ quái
            bool isNew = recentlyCollectedUids.Contains(item.Uid);
            GameObject newBadgeObj = new GameObject("New_Badge");
            newBadgeObj.transform.SetParent(cardObj.transform, false);
            RectTransform nbRt = newBadgeObj.AddComponent<RectTransform>();
            nbRt.anchorMin = new Vector2(0.5f, 1f);
            nbRt.anchorMax = new Vector2(0.5f, 1f);
            nbRt.pivot = new Vector2(0.5f, 0.5f);
            nbRt.anchoredPosition = new Vector2(0, 2);
            nbRt.sizeDelta = new Vector2(66, 20);

            Image nbBg = newBadgeObj.AddComponent<Image>();
            nbBg.color = new Color(0.95f, 0.2f, 0.15f, 0.95f);

            GameObject nbTextObj = new GameObject("Text");
            nbTextObj.transform.SetParent(newBadgeObj.transform, false);
            RectTransform nbtRt = nbTextObj.AddComponent<RectTransform>();
            nbtRt.anchorMin = Vector2.zero;
            nbtRt.anchorMax = Vector2.one;
            nbtRt.sizeDelta = Vector2.zero;
            Text nbTxt = nbTextObj.AddComponent<Text>();
            nbTxt.font = UIFont;
            nbTxt.fontSize = 11;
            nbTxt.fontStyle = FontStyle.Bold;
            nbTxt.alignment = TextAnchor.MiddleCenter;
            nbTxt.color = Color.yellow;
            nbTxt.text = "✦ MỚI ✦";
            newBadgeObj.SetActive(isNew);

            Button cardBtn = cardObj.AddComponent<Button>();
            cardBtn.onClick.AddListener(() => OpenBagItemDetail(item));
        }

        private void RefreshShopList()
        {
            if (shopListContainer == null) return;
            Transform shopContent = shopListContainer.transform.Find("Viewport/ShopContent") ?? shopListContainer.transform;

            var stock = ShopItemData.GetDefaultStock();
            int currentRunGold = RunInventory.Instance != null ? RunInventory.Instance.RunGold : 0;

            for (int i = 0; i < stock.Length; i++)
            {
                var item = stock[i];
                if (i < shopContent.childCount)
                {
                    UpdateShopItemCard(shopContent.GetChild(i), item, currentRunGold);
                }
                else
                {
                    CreateShopItemCard(shopContent, item, currentRunGold);
                }
            }

            for (int i = stock.Length; i < shopContent.childCount; i++)
            {
                shopContent.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void UpdateShopItemCard(Transform cardTr, ShopItemData data, int currentRunGold)
        {
            cardTr.gameObject.SetActive(true);
            cardTr.name = $"ShopItem_{data.ItemId}";

            Transform iconTr = cardTr.Find("Icon");
            if (iconTr != null)
            {
                Text iTxt = iconTr.GetComponent<Text>();
                if (iTxt != null)
                {
                    iTxt.font = UIFont;
                    iTxt.text = data.ItemType == ShopItemType.Heal ? "❤️" : "🗡️";
                }
            }

            Transform infoTr = cardTr.Find("Info");
            if (infoTr != null)
            {
                Text inTxt = infoTr.GetComponent<Text>();
                if (inTxt != null)
                {
                    inTxt.font = UIFont;
                    inTxt.text = $"<b>{data.Title}</b>\n<color=#aaaaaa>{data.Description}</color>";
                }
            }

            bool canAfford = currentRunGold >= data.Price;
            Transform buyBtnTr = cardTr.Find("BuyButton");
            if (buyBtnTr != null)
            {
                Image bImg = buyBtnTr.GetComponent<Image>();
                if (bImg != null) bImg.color = canAfford ? new Color(0.2f, 0.75f, 0.35f) : new Color(0.35f, 0.35f, 0.4f);

                Transform bTxtTr = buyBtnTr.Find("Text");
                if (bTxtTr != null)
                {
                    Text bTxt = bTxtTr.GetComponent<Text>();
                    if (bTxt != null)
                    {
                        bTxt.font = UIFont;
                        bTxt.text = canAfford ? $"MUA {data.Price}💰" : $"THIẾU {data.Price}💰";
                    }
                }

                Button bBtn = buyBtnTr.GetComponent<Button>();
                if (bBtn != null)
                {
                    bBtn.interactable = canAfford;
                    bBtn.onClick.RemoveAllListeners();
                    bBtn.onClick.AddListener(() => HandleBuyShopItem(data));
                }
            }
        }

        private void CreateShopItemCard(Transform parent, ShopItemData data, int currentRunGold)
        {
            GameObject cardObj = new GameObject($"ShopItem_{data.ItemId}");
            cardObj.transform.SetParent(parent, false);

            RectTransform rt = cardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(580, 72);

            Image bg = cardObj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cardObj.transform, false);
            RectTransform iRt = iconObj.AddComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0, 0.5f);
            iRt.anchorMax = new Vector2(0, 0.5f);
            iRt.pivot = new Vector2(0, 0.5f);
            iRt.anchoredPosition = new Vector2(16, 0);
            iRt.sizeDelta = new Vector2(48, 48);
            Text iTxt = iconObj.AddComponent<Text>();
            iTxt.font = UIFont;
            iTxt.fontSize = 28;
            iTxt.alignment = TextAnchor.MiddleCenter;
            iTxt.text = data.ItemType == ShopItemType.Heal ? "❤️" : "🗡️";

            // Title & Desc
            GameObject infoObj = new GameObject("Info");
            infoObj.transform.SetParent(cardObj.transform, false);
            RectTransform inRt = infoObj.AddComponent<RectTransform>();
            inRt.anchorMin = new Vector2(0, 0);
            inRt.anchorMax = new Vector2(1, 1);
            inRt.offsetMin = new Vector2(75, 8);
            inRt.offsetMax = new Vector2(-150, -8);
            Text inTxt = infoObj.AddComponent<Text>();
            inTxt.font = UIFont;
            inTxt.fontSize = 14;
            inTxt.alignment = TextAnchor.MiddleLeft;
            inTxt.color = Color.white;
            inTxt.text = $"<b>{data.Title}</b>\n<color=#aaaaaa>{data.Description}</color>";

            // Button Mua
            bool canAfford = currentRunGold >= data.Price;
            GameObject buyBtnObj = new GameObject("BuyButton");
            buyBtnObj.transform.SetParent(cardObj.transform, false);
            RectTransform bRt = buyBtnObj.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(1, 0.5f);
            bRt.anchorMax = new Vector2(1, 0.5f);
            bRt.pivot = new Vector2(1, 0.5f);
            bRt.anchoredPosition = new Vector2(-16, 0);
            bRt.sizeDelta = new Vector2(120, 44);

            Image bImg = buyBtnObj.AddComponent<Image>();
            bImg.color = canAfford ? new Color(0.2f, 0.75f, 0.35f) : new Color(0.35f, 0.35f, 0.4f);
            bImg.color = canAfford ? new Color(0.2f, 0.75f, 0.35f) : new Color(0.35f, 0.35f, 0.4f);

            GameObject bTxtObj = new GameObject("Text");
            bTxtObj.transform.SetParent(buyBtnObj.transform, false);
            RectTransform btRt = bTxtObj.AddComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.sizeDelta = Vector2.zero;
            Text bTxt = bTxtObj.AddComponent<Text>();
            bTxt.font = uiFont;
            bTxt.fontSize = 15;
            bTxt.fontStyle = FontStyle.Bold;
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color = Color.white;
            bTxt.text = $"{data.Price} 💰";

            Button buyBtn = buyBtnObj.AddComponent<Button>();
            buyBtn.interactable = canAfford;
            buyBtn.onClick.AddListener(() => HandleBuyShopItem(data));
        }

        private void HandleBuyShopItem(ShopItemData data)
        {
            if (RunInventory.Instance == null || data == null) return;

            if (RunInventory.Instance.TrySpendGold(data.Price))
            {
                if (data.ItemType == ShopItemType.Heal)
                {
                    var player = FindFirstObjectByType<PlayerHealth>();
                    if (player != null)
                    {
                        player.Heal(data.HealAmount);
                    }
                    LootToastManager.ShowWorldToast(Vector3.up * 2f, $"❤️ HỒI PHỤC +{data.HealAmount} HP!", Color.green);
                }
                else if (data.ItemType == ShopItemType.Weapon)
                {
                    // Thêm vũ khí mới vào Balo trang bị
                    EquipmentItem newEq = data.ItemId.Contains("blade") ? EquipmentItem.CreateCarrotBlade(EquipmentRarity.Great) :
                                          data.ItemId.Contains("axe") ? EquipmentItem.CreateCarrotBlade(EquipmentRarity.Rare) :
                                          data.ItemId.Contains("tomato") ? EquipmentItem.CreateTomatoBlaster(EquipmentRarity.Great) :
                                          EquipmentItem.CreateGrapeCannon(EquipmentRarity.Great);

                    AccountProgression.Instance?.AddEquipment(newEq);
                    LootToastManager.ShowWorldToast(Vector3.up * 2f, $"🎁 NHẬN ĐƯỢC: {newEq.DisplayName}!", Color.cyan);
                }

                RefreshAll();
                RefreshShopList();
            }
        }

        private void OpenBagItemDetail(EquipmentItem item)
        {
            if (item == null) return;
            if (recentlyCollectedUids.Remove(item.Uid))
            {
                OnRecentlyCollectedChanged?.Invoke();
                if (tabBagText != null && AccountProgression.Instance?.Data != null)
                {
                    int totalItems = AccountProgression.Instance.Data.OwnedEquipments.Count;
                    string newTag = recentlyCollectedUids.Count > 0 ? $" <color=#ff4444>({recentlyCollectedUids.Count} MỚI!)</color>" : "";
                    tabBagText.text = $"🎒 Trang Bị ({totalItems}){newTag}";
                }
            }
            selectedItem = item;
            isSelectedEquipped = false;
            ShowDetailModal();
        }

        private void OpenEquippedDetail(EquipmentSlotType slot)
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null) return;

            var item = acc.GetEquippedItem(slot);
            if (item == null) return;

            selectedItem = item;
            isSelectedEquipped = true;
            selectedSlotType = slot;
            ShowDetailModal();
        }

        private void ShowDetailModal()
        {
            if (detailModalRoot == null || selectedItem == null) return;

            detailModalRoot.SetActive(true);

            if (detailItemName != null)
            {
                detailItemName.text = $"[{selectedItem.GetSlotIcon()}] {selectedItem.DisplayName}";
                detailItemName.color = selectedItem.GetRarityColor();
            }

            if (detailItemRarity != null)
            {
                detailItemRarity.text = $"Phẩm chất: {selectedItem.GetRarityName()} | Loại: {selectedItem.GetSlotName()} | Level: {selectedItem.Level}";
            }

            if (detailItemStats != null)
            {
                string statsStr = "";
                if (selectedItem.TotalAttack > 0) statsStr += $"Sát thương: +{selectedItem.TotalAttack} ATK   ";
                if (selectedItem.TotalHealth > 0) statsStr += $"Máu tối đa: +{selectedItem.TotalHealth} HP";
                detailItemStats.text = statsStr;
            }

            if (detailItemDesc != null)
            {
                detailItemDesc.text = selectedItem.Description;
            }

            if (detailEquipButtonText != null)
            {
                detailEquipButtonText.text = isSelectedEquipped ? "THÁO TRANG BỊ" : "TRANG BỊ (EQUIP)";
            }
        }

        private void HandleDetailEquipOrUnequip()
        {
            if (selectedItem == null || AccountProgression.Instance == null) return;

            if (isSelectedEquipped)
            {
                AccountProgression.Instance.UnequipItem(selectedSlotType);
            }
            else
            {
                AccountProgression.Instance.EquipItem(selectedItem);
            }

            if (detailModalRoot != null) detailModalRoot.SetActive(false);
            RefreshAll();
        }

        private void OpenFuseModal()
        {
            if (fuseModalRoot == null) return;
            fuseModalRoot.SetActive(true);

            var acc = AccountProgression.Instance?.Data;
            if (acc == null || acc.OwnedEquipments == null) return;

            int possiblePairs = 0;
            for (int i = 0; i < acc.OwnedEquipments.Count; i++)
            {
                for (int j = i + 1; j < acc.OwnedEquipments.Count; j++)
                {
                    if (acc.OwnedEquipments[i].CanFuseWith(acc.OwnedEquipments[j]))
                    {
                        possiblePairs++;
                    }
                }
            }

            if (fuseModalNotice != null)
            {
                if (possiblePairs > 0)
                {
                    fuseModalNotice.text = $"✨ Tìm thấy {possiblePairs} cặp trang bị cùng loại & phẩm chất đủ điều kiện Hợp Nhất (Fuse)!\nBấm TỰ ĐỘNG GHÉP để nâng cấp lên phẩm cấp cao hơn.";
                }
                else
                {
                    fuseModalNotice.text = "Hiện chưa có cặp trang bị nào cùng loại & phẩm chất trong túi đồ để ghép.\nHãy thu thập thêm trang bị từ các đợt càn quét quái vật!";
                }
            }
        }

        private void HandleAutoFuse()
        {
            if (AccountProgression.Instance == null) return;

            int count = AccountProgression.Instance.AutoFuseAll();
            if (fuseModalNotice != null)
            {
                if (count > 0)
                {
                    fuseModalNotice.text = $"🎉 GHÉP THÀNH CÔNG {count} MÓN TRANG BỊ LÊN PHẨM CHẤT CAO HƠN!\nChỉ số ATK & HP của Thỏ đã được tăng cường!";
                }
                else
                {
                    fuseModalNotice.text = "Không có cặp trang bị nào đủ điều kiện để ghép.";
                }
            }

            RefreshAll();
        }

        private void RotateHeroPreview()
        {
            heroRotation = (heroRotation + 45f) % 360f;
            if (heroPreviewTransform != null)
            {
                heroPreviewTransform.localRotation = Quaternion.Euler(0, heroRotation, 0);
            }
        }

        // ========================== BUILDER TẠO TOÀN BỘ GIAO DIỆN RUNTIME ==========================
        public static EquipmentBagUI CreateEquipmentBagUI(GameObject canvasObj, Font defaultFont)
        {
            GameObject bagRootObj = new GameObject("Unified_EquipmentBag_Overlay");
            bagRootObj.transform.SetParent(canvasObj.transform, false);

            RectTransform rootRt = bagRootObj.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;

            Image rootBg = bagRootObj.AddComponent<Image>();
            rootBg.color = new Color(0.06f, 0.08f, 0.14f, 0.98f);

            GameObject contentPanel = new GameObject("Content_Panel");
            contentPanel.transform.SetParent(bagRootObj.transform, false);
            RectTransform contentRt = contentPanel.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0.5f, 0.5f);
            contentRt.anchorMax = new Vector2(0.5f, 0.5f);
            contentRt.pivot = new Vector2(0.5f, 0.5f);
            contentRt.sizeDelta = new Vector2(960, 1020);

            // ------------------ 1. TOP BAR ------------------
            GameObject topBar = new GameObject("TopBar");
            topBar.transform.SetParent(contentPanel.transform, false);
            RectTransform topRt = topBar.AddComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0, 1);
            topRt.anchorMax = new Vector2(1, 1);
            topRt.pivot = new Vector2(0.5f, 1);
            topRt.anchoredPosition = new Vector2(0, -10);
            topRt.sizeDelta = new Vector2(0, 54);

            CreateResourcePill(topBar, new Vector2(-360, 0), new Vector2(130, 44), new Color(0.15f, 0.22f, 0.35f), "CẤP: 1", out Text lvlTxt, defaultFont);
            CreateResourcePill(topBar, new Vector2(-200, 0), new Vector2(130, 44), new Color(0.12f, 0.35f, 0.22f), "NL: 20/20", out Text energyTxt, defaultFont);
            CreateResourcePill(topBar, new Vector2(-40, 0), new Vector2(140, 44), new Color(0.35f, 0.28f, 0.08f), "TK: 150", out Text goldTxt, defaultFont);
            CreateResourcePill(topBar, new Vector2(120, 0), new Vector2(130, 44), new Color(0.45f, 0.35f, 0.1f), "RUN: 0", out Text runGoldTxt, defaultFont);
            CreateResourcePill(topBar, new Vector2(270, 0), new Vector2(120, 44), new Color(0.32f, 0.12f, 0.38f), "GEM: 50", out Text gemTxt, defaultFont);
            GameObject closeBtnObj = CreateButton(topBar, new Vector2(430, 0), new Vector2(44, 44), new Color(0.85f, 0.2f, 0.25f), "✕", defaultFont, 22);

            // ------------------ 2. HERO STAGE & 6 SLOTS ------------------
            GameObject heroSection = new GameObject("Hero_Section");
            heroSection.transform.SetParent(contentPanel.transform, false);
            RectTransform heroSecRt = heroSection.AddComponent<RectTransform>();
            heroSecRt.anchorMin = new Vector2(0, 1);
            heroSecRt.anchorMax = new Vector2(1, 1);
            heroSecRt.pivot = new Vector2(0.5f, 1);
            heroSecRt.anchoredPosition = new Vector2(0, -70);
            heroSecRt.sizeDelta = new Vector2(0, 390);

            // Stage Center
            GameObject stageObj = new GameObject("Center_Stage");
            stageObj.transform.SetParent(heroSection.transform, false);
            RectTransform stageRt = stageObj.AddComponent<RectTransform>();
            stageRt.anchoredPosition = new Vector2(0, -20);
            stageRt.sizeDelta = new Vector2(340, 330);
            Image stageBg = stageObj.AddComponent<Image>();
            stageBg.color = new Color(0.12f, 0.18f, 0.28f, 0.5f);

            // Tên Nhân vật
            GameObject heroNameObj = new GameObject("HeroName");
            heroNameObj.transform.SetParent(stageObj.transform, false);
            RectTransform hnRt = heroNameObj.AddComponent<RectTransform>();
            hnRt.anchoredPosition = new Vector2(0, 135);
            hnRt.sizeDelta = new Vector2(360, 30);
            Text hnTxt = heroNameObj.AddComponent<Text>();
            hnTxt.font = defaultFont;
            hnTxt.fontSize = 18;
            hnTxt.fontStyle = FontStyle.Bold;
            hnTxt.alignment = TextAnchor.MiddleCenter;
            hnTxt.color = Color.white;
            hnTxt.text = "Rabbit Warrior";

            // Visual Avatar Thỏ (Hero Crest)
            GameObject heroVisualObj = new GameObject("HeroVisual");
            heroVisualObj.transform.SetParent(stageObj.transform, false);
            RectTransform hvRt = heroVisualObj.AddComponent<RectTransform>();
            hvRt.anchoredPosition = new Vector2(0, 20);
            hvRt.sizeDelta = new Vector2(200, 140);
            Image hvBg = heroVisualObj.AddComponent<Image>();
            hvBg.color = new Color(0.15f, 0.22f, 0.38f, 0.85f);

            GameObject hvTxtObj = new GameObject("Text");
            hvTxtObj.transform.SetParent(heroVisualObj.transform, false);
            RectTransform hvtRt = hvTxtObj.AddComponent<RectTransform>();
            hvtRt.anchorMin = Vector2.zero;
            hvtRt.anchorMax = Vector2.one;
            hvtRt.sizeDelta = Vector2.zero;
            Text hvTxt = hvTxtObj.AddComponent<Text>();
            hvTxt.font = defaultFont;
            hvTxt.fontSize = 17;
            hvTxt.fontStyle = FontStyle.Bold;
            hvTxt.alignment = TextAnchor.MiddleCenter;
            hvTxt.color = Color.white;
            hvTxt.text = "★ THỎ CHIẾN BINH ★\n<size=12><color=#66ddff>Đấu Sĩ Sinh Tồn Cà Rốt</color></size>";

            // Nút Xoay 🔄
            GameObject rotBtnObj = CreateButton(stageObj, new Vector2(0, -80), new Vector2(50, 44), new Color(0.2f, 0.5f, 0.9f), "XOAY", defaultFont, 14);

            // Bảng chỉ số ATK & HP dưới chân Thỏ
            GameObject statBar = new GameObject("Stat_Bar");
            statBar.transform.SetParent(heroSection.transform, false);
            RectTransform sbRt = statBar.AddComponent<RectTransform>();
            sbRt.anchoredPosition = new Vector2(0, -355);
            sbRt.sizeDelta = new Vector2(380, 44);

            CreateResourcePill(statBar, new Vector2(-100, 0), new Vector2(170, 40), new Color(0.22f, 0.14f, 0.14f), "ATK: 137", out Text atkTxt, defaultFont);
            CreateResourcePill(statBar, new Vector2(100, 0), new Vector2(170, 40), new Color(0.12f, 0.24f, 0.14f), "HP: 1572", out Text hpTxt, defaultFont);

            // 6 Ô Trang Bị
            Button wpnBtn = CreateEquipmentSlotUI(heroSection, new Vector2(-300, -55), "KIẾM", "Vũ Khí", defaultFont);
            Button neckBtn = CreateEquipmentSlotUI(heroSection, new Vector2(-300, -170), "DÂY", "Dây Chuyền", defaultFont);
            Button gloveBtn = CreateEquipmentSlotUI(heroSection, new Vector2(-300, -285), "GĂNG", "Găng Tay", defaultFont);

            Button armBtn = CreateEquipmentSlotUI(heroSection, new Vector2(300, -55), "GIÁP", "Áo Giáp", defaultFont);
            Button beltBtn = CreateEquipmentSlotUI(heroSection, new Vector2(300, -170), "ĐAI", "Thắt Lưng", defaultFont);
            Button bootBtn = CreateEquipmentSlotUI(heroSection, new Vector2(300, -285), "ỦNG", "Ủng Chiến", defaultFont);

            // ------------------ 3. MID ACTION BAR TABS ------------------
            GameObject midBar = new GameObject("MidBar");
            midBar.transform.SetParent(contentPanel.transform, false);
            RectTransform mbRt = midBar.AddComponent<RectTransform>();
            mbRt.anchorMin = new Vector2(0, 1);
            mbRt.anchorMax = new Vector2(1, 1);
            mbRt.pivot = new Vector2(0.5f, 1);
            mbRt.anchoredPosition = new Vector2(0, -480);
            mbRt.sizeDelta = new Vector2(0, 50);

            // Tab 1: Trang Bị & Balo
            GameObject tabBagObj = CreateButton(midBar, new Vector2(-240, 0), new Vector2(210, 44), new Color(0.18f, 0.45f, 0.85f), "🎒 Trang Bị (Bag)", defaultFont, 16);
            Text tBagTxt = tabBagObj.GetComponentInChildren<Text>();

            // Tab 2: Cửa Hàng Run Shop
            GameObject tabShopObj = CreateButton(midBar, new Vector2(0, 0), new Vector2(210, 44), new Color(0.12f, 0.16f, 0.24f), "🛒 Cửa Hàng (Shop)", defaultFont, 16);
            Text tShopTxt = tabShopObj.GetComponentInChildren<Text>();

            // Nút Fuse
            GameObject fuseBtnObj = CreateButton(midBar, new Vector2(260, 0), new Vector2(180, 44), new Color(0.95f, 0.65f, 0.1f), "✨ Fuse (Ghép)", defaultFont, 16);

            // ------------------ 4. SCROLL CONTAINERS (BAG GRID & SHOP) ------------------
            // 4A. Bag Grid Scroll
            GameObject bagScrollObj = new GameObject("Bag_Grid_Scroll");
            bagScrollObj.transform.SetParent(contentPanel.transform, false);
            RectTransform bsRt = bagScrollObj.AddComponent<RectTransform>();
            bsRt.anchorMin = new Vector2(0, 1);
            bsRt.anchorMax = new Vector2(1, 1);
            bsRt.pivot = new Vector2(0.5f, 1);
            bsRt.anchoredPosition = new Vector2(0, -540);
            bsRt.sizeDelta = new Vector2(0, 360);

            ScrollRect bagScroll = bagScrollObj.AddComponent<ScrollRect>();
            bagScroll.horizontal = false;
            bagScroll.vertical = true;
            bagScrollObj.AddComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f, 0.5f);

            GameObject bViewport = new GameObject("Viewport");
            bViewport.transform.SetParent(bagScrollObj.transform, false);
            RectTransform bvRt = bViewport.AddComponent<RectTransform>();
            bvRt.anchorMin = Vector2.zero;
            bvRt.anchorMax = Vector2.one;
            bvRt.sizeDelta = Vector2.zero;
            bViewport.AddComponent<RectMask2D>();
            bagScroll.viewport = bvRt;

            GameObject bContent = new GameObject("GridContent");
            bContent.transform.SetParent(bViewport.transform, false);
            RectTransform bcRt = bContent.AddComponent<RectTransform>();
            bcRt.anchorMin = new Vector2(0, 1);
            bcRt.anchorMax = new Vector2(1, 1);
            bcRt.pivot = new Vector2(0.5f, 1);
            bcRt.sizeDelta = new Vector2(0, 500);

            GridLayoutGroup bGlg = bContent.AddComponent<GridLayoutGroup>();
            bGlg.cellSize = new Vector2(120, 130);
            bGlg.spacing = new Vector2(16, 16);
            bGlg.padding = new RectOffset(20, 20, 20, 20);
            bContent.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            bagScroll.content = bcRt;

            // 4B. Shop Scroll
            GameObject shopScrollObj = new GameObject("Shop_List_Scroll");
            shopScrollObj.transform.SetParent(contentPanel.transform, false);
            RectTransform ssRt = shopScrollObj.AddComponent<RectTransform>();
            ssRt.anchorMin = new Vector2(0, 1);
            ssRt.anchorMax = new Vector2(1, 1);
            ssRt.pivot = new Vector2(0.5f, 1);
            ssRt.anchoredPosition = new Vector2(0, -540);
            ssRt.sizeDelta = new Vector2(0, 360);

            ScrollRect shopScroll = shopScrollObj.AddComponent<ScrollRect>();
            shopScroll.horizontal = false;
            shopScroll.vertical = true;
            shopScrollObj.AddComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f, 0.5f);

            GameObject sViewport = new GameObject("Viewport");
            sViewport.transform.SetParent(shopScrollObj.transform, false);
            RectTransform svRt = sViewport.AddComponent<RectTransform>();
            svRt.anchorMin = Vector2.zero;
            svRt.anchorMax = Vector2.one;
            svRt.sizeDelta = Vector2.zero;
            sViewport.AddComponent<RectMask2D>();
            shopScroll.viewport = svRt;

            GameObject sContent = new GameObject("ShopContent");
            sContent.transform.SetParent(sViewport.transform, false);
            RectTransform scRt = sContent.AddComponent<RectTransform>();
            scRt.anchorMin = new Vector2(0, 1);
            scRt.anchorMax = new Vector2(1, 1);
            scRt.pivot = new Vector2(0.5f, 1);
            scRt.sizeDelta = new Vector2(0, 450);

            VerticalLayoutGroup sVlg = sContent.AddComponent<VerticalLayoutGroup>();
            sVlg.spacing = 10;
            sVlg.padding = new RectOffset(16, 16, 16, 16);
            sVlg.childControlWidth = true;
            sVlg.childForceExpandWidth = true;
            sContent.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            shopScroll.content = scRt;
            shopScrollObj.SetActive(false);

            // ------------------ 5. FOOTER BUTTON: TIẾP TỤC CHIẾN ĐẤU ------------------
            GameObject footerBar = new GameObject("FooterBar");
            footerBar.transform.SetParent(contentPanel.transform, false);
            RectTransform fbRt = footerBar.AddComponent<RectTransform>();
            fbRt.anchorMin = new Vector2(0, 0);
            fbRt.anchorMax = new Vector2(1, 0);
            fbRt.pivot = new Vector2(0.5f, 0);
            fbRt.anchoredPosition = new Vector2(0, 15);
            fbRt.sizeDelta = new Vector2(0, 75);

            GameObject contBtnObj = CreateButton(footerBar, new Vector2(0, 0), new Vector2(620, 64), new Color(0.18f, 0.78f, 0.35f), "▶ TIẾP TỤC CHIẾN ĐẤU ◀", defaultFont, 22);
            Text contTxt = contBtnObj.GetComponentInChildren<Text>();

            // ------------------ 6. MODALS ------------------
            GameObject detailModal = CreateDetailModalUI(bagRootObj, defaultFont, out Text dName, out Text dRarity, out Text dStats, out Text dDesc, out Button dEquipBtn, out Text dEquipTxt, out Button dCloseBtn);
            detailModal.SetActive(false);

            GameObject fuseModal = CreateFuseModalUI(bagRootObj, defaultFont, out Text fNotice, out Button fAutoBtn, out Button fCloseBtn);
            fuseModal.SetActive(false);

            // Binding
            EquipmentBagUI ui = bagRootObj.AddComponent<EquipmentBagUI>();
            ui.bagRoot = bagRootObj;
            ui.uiFont = defaultFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            ui.levelText = lvlTxt;
            ui.energyText = energyTxt;
            ui.goldText = goldTxt;
            ui.runGoldText = runGoldTxt;
            ui.gemsText = gemTxt;
            ui.closeBagButton = closeBtnObj.GetComponent<Button>();

            ui.heroNameText = hnTxt;
            ui.totalAtkText = atkTxt;
            ui.totalHpText = hpTxt;
            ui.rotateHeroButton = rotBtnObj.GetComponent<Button>();
            ui.heroPreviewTransform = heroVisualObj.transform;

            ui.weaponSlotBtn = wpnBtn;
            ui.necklaceSlotBtn = neckBtn;
            ui.glovesSlotBtn = gloveBtn;
            ui.armorSlotBtn = armBtn;
            ui.beltSlotBtn = beltBtn;
            ui.bootsSlotBtn = bootBtn;

            ui.tabBagButton = tabBagObj.GetComponent<Button>();
            ui.tabShopButton = tabShopObj.GetComponent<Button>();
            ui.fuseButton = fuseBtnObj.GetComponent<Button>();
            ui.tabBagText = tBagTxt;
            ui.tabShopText = tShopTxt;

            ui.bagGridContainer = bagScrollObj;
            ui.shopListContainer = shopScrollObj;

            ui.continueActionButton = contBtnObj.GetComponent<Button>();
            ui.continueActionText = contTxt;

            ui.detailModalRoot = detailModal;
            ui.detailItemName = dName;
            ui.detailItemRarity = dRarity;
            ui.detailItemStats = dStats;
            ui.detailItemDesc = dDesc;
            ui.detailEquipButton = dEquipBtn;
            ui.detailEquipButtonText = dEquipTxt;
            ui.detailCloseButton = dCloseBtn;

            ui.fuseModalRoot = fuseModal;
            ui.fuseModalNotice = fNotice;
            ui.autoFuseButton = fAutoBtn;
            ui.closeFuseButton = fCloseBtn;

            return ui;
        }

        private static Button CreateEquipmentSlotUI(GameObject parent, Vector2 pos, string defaultIcon, string slotName, Font font)
        {
            GameObject slotObj = new GameObject($"Slot_{slotName}");
            slotObj.transform.SetParent(parent.transform, false);
            RectTransform rt = slotObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(105, 105);

            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.45f, 0.85f);

            GameObject inner = new GameObject("Inner");
            inner.transform.SetParent(slotObj.transform, false);
            RectTransform inRt = inner.AddComponent<RectTransform>();
            inRt.anchorMin = new Vector2(0.04f, 0.04f);
            inRt.anchorMax = new Vector2(0.96f, 0.96f);
            inRt.sizeDelta = Vector2.zero;
            Image inBg = inner.AddComponent<Image>();
            inBg.color = new Color(0.07f, 0.12f, 0.22f, 0.95f);

            // Badge slot type (Top-Left)
            GameObject badge = new GameObject("SlotBadge");
            badge.transform.SetParent(slotObj.transform, false);
            RectTransform bRt = badge.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0, 1);
            bRt.anchorMax = new Vector2(0, 1);
            bRt.pivot = new Vector2(0, 1);
            bRt.anchoredPosition = new Vector2(6, -6);
            bRt.sizeDelta = new Vector2(46, 18);
            Text bTxt = badge.AddComponent<Text>();
            bTxt.font = font;
            bTxt.fontSize = 10;
            bTxt.fontStyle = FontStyle.Bold;
            bTxt.alignment = TextAnchor.MiddleLeft;
            bTxt.color = new Color(1f, 0.85f, 0.3f);
            bTxt.text = defaultIcon;

            // Level text (Top-Right)
            GameObject lvlObj = new GameObject("LevelText");
            lvlObj.transform.SetParent(slotObj.transform, false);
            RectTransform lRt = lvlObj.AddComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0.5f, 1);
            lRt.anchorMax = new Vector2(1, 1);
            lRt.pivot = new Vector2(1, 1);
            lRt.anchoredPosition = new Vector2(-6, -6);
            lRt.sizeDelta = new Vector2(48, 18);
            Text lTxt = lvlObj.AddComponent<Text>();
            lTxt.font = font;
            lTxt.fontSize = 11;
            lTxt.fontStyle = FontStyle.Bold;
            lTxt.alignment = TextAnchor.MiddleRight;
            lTxt.color = Color.white;
            lTxt.text = "Lv.1";

            // Center Content (Icon / Primary Stat)
            GameObject iconObj = new GameObject("ItemIcon");
            iconObj.transform.SetParent(slotObj.transform, false);
            RectTransform iRt = iconObj.AddComponent<RectTransform>();
            iRt.anchoredPosition = new Vector2(0, 8);
            iRt.sizeDelta = new Vector2(98, 42);
            Text iTxt = iconObj.AddComponent<Text>();
            iTxt.font = font;
            iTxt.fontSize = 13;
            iTxt.fontStyle = FontStyle.Bold;
            iTxt.alignment = TextAnchor.MiddleCenter;
            iTxt.color = Color.white;
            iTxt.text = $"[{defaultIcon}]";

            // Item Name (Bottom)
            GameObject nameObj = new GameObject("ItemName");
            nameObj.transform.SetParent(slotObj.transform, false);
            RectTransform nRt = nameObj.AddComponent<RectTransform>();
            nRt.anchorMin = new Vector2(0, 0);
            nRt.anchorMax = new Vector2(1, 0);
            nRt.pivot = new Vector2(0.5f, 0);
            nRt.anchoredPosition = new Vector2(0, 5);
            nRt.sizeDelta = new Vector2(0, 20);
            Text nTxt = nameObj.AddComponent<Text>();
            nTxt.font = font;
            nTxt.fontSize = 10;
            nTxt.fontStyle = FontStyle.Bold;
            nTxt.alignment = TextAnchor.MiddleCenter;
            nTxt.color = Color.white;
            nTxt.text = slotName;

            return slotObj.AddComponent<Button>();
        }

        private static GameObject CreateResourcePill(GameObject parent, Vector2 pos, Vector2 size, Color color, string initialText, out Text outText, Font font)
        {
            GameObject pill = new GameObject("Pill");
            pill.transform.SetParent(parent.transform, false);
            RectTransform rt = pill.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image bg = pill.AddComponent<Image>();
            bg.color = color;

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(pill.transform, false);
            RectTransform tRt = txtObj.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;
            Text txt = txtObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 15;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = initialText;

            outText = txt;
            return pill;
        }

        private static GameObject CreateButton(GameObject parent, Vector2 pos, Vector2 size, Color color, string label, Font font, int fontSize)
        {
            GameObject btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent.transform, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image bg = btnObj.AddComponent<Image>();
            bg.color = color;

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform tRt = txtObj.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;
            Text txt = txtObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = label;

            btnObj.AddComponent<Button>();
            return btnObj;
        }

        private static GameObject CreateDetailModalUI(GameObject parent, Font font, out Text nameTxt, out Text rarityTxt, out Text statsTxt, out Text descTxt, out Button equipBtn, out Text equipBtnTxt, out Button closeBtn)
        {
            GameObject modal = new GameObject("ItemDetail_Modal");
            modal.transform.SetParent(parent.transform, false);
            RectTransform rt = modal.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image mBg = modal.AddComponent<Image>();
            mBg.color = new Color(0, 0, 0, 0.8f);

            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(modal.transform, false);
            RectTransform pRt = panel.AddComponent<RectTransform>();
            pRt.anchoredPosition = Vector2.zero;
            pRt.sizeDelta = new Vector2(580, 400);
            Image pBg = panel.AddComponent<Image>();
            pBg.color = new Color(0.1f, 0.15f, 0.25f, 0.98f);

            GameObject nObj = new GameObject("Name");
            nObj.transform.SetParent(panel.transform, false);
            RectTransform nRt = nObj.AddComponent<RectTransform>();
            nRt.anchoredPosition = new Vector2(0, 140);
            nRt.sizeDelta = new Vector2(520, 36);
            nameTxt = nObj.AddComponent<Text>();
            nameTxt.font = font;
            nameTxt.fontSize = 22;
            nameTxt.fontStyle = FontStyle.Bold;
            nameTxt.alignment = TextAnchor.MiddleCenter;
            nameTxt.color = Color.white;

            GameObject rObj = new GameObject("Rarity");
            rObj.transform.SetParent(panel.transform, false);
            RectTransform rRt = rObj.AddComponent<RectTransform>();
            rRt.anchoredPosition = new Vector2(0, 100);
            rRt.sizeDelta = new Vector2(520, 26);
            rarityTxt = rObj.AddComponent<Text>();
            rarityTxt.font = font;
            rarityTxt.fontSize = 15;
            rarityTxt.alignment = TextAnchor.MiddleCenter;
            rarityTxt.color = new Color(0.8f, 0.85f, 0.95f);

            GameObject sObj = new GameObject("Stats");
            sObj.transform.SetParent(panel.transform, false);
            RectTransform sRt = sObj.AddComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(0, 45);
            sRt.sizeDelta = new Vector2(520, 45);
            statsTxt = sObj.AddComponent<Text>();
            statsTxt.font = font;
            statsTxt.fontSize = 19;
            statsTxt.fontStyle = FontStyle.Bold;
            statsTxt.alignment = TextAnchor.MiddleCenter;
            statsTxt.color = new Color(0.25f, 0.95f, 0.45f);

            GameObject dObj = new GameObject("Desc");
            dObj.transform.SetParent(panel.transform, false);
            RectTransform dRt = dObj.AddComponent<RectTransform>();
            dRt.anchoredPosition = new Vector2(0, -20);
            dRt.sizeDelta = new Vector2(480, 55);
            descTxt = dObj.AddComponent<Text>();
            descTxt.font = font;
            descTxt.fontSize = 14;
            descTxt.alignment = TextAnchor.MiddleCenter;
            descTxt.color = new Color(0.7f, 0.75f, 0.85f);

            GameObject eqBtnObj = CreateButton(panel, new Vector2(0, -115), new Vector2(250, 48), new Color(0.2f, 0.75f, 0.35f), "TRANG BỊ (EQUIP)", font, 17);
            equipBtn = eqBtnObj.GetComponent<Button>();
            equipBtnTxt = eqBtnObj.GetComponentInChildren<Text>();

            GameObject clBtnObj = CreateButton(panel, new Vector2(255, 165), new Vector2(40, 40), new Color(0.7f, 0.2f, 0.2f), "✕", font, 18);
            closeBtn = clBtnObj.GetComponent<Button>();

            return modal;
        }

        private static GameObject CreateFuseModalUI(GameObject parent, Font font, out Text noticeTxt, out Button autoFuseBtn, out Button closeBtn)
        {
            GameObject modal = new GameObject("Fuse_Modal");
            modal.transform.SetParent(parent.transform, false);
            RectTransform rt = modal.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image mBg = modal.AddComponent<Image>();
            mBg.color = new Color(0, 0, 0, 0.8f);

            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(modal.transform, false);
            RectTransform pRt = panel.AddComponent<RectTransform>();
            pRt.anchoredPosition = Vector2.zero;
            pRt.sizeDelta = new Vector2(620, 380);
            Image pBg = panel.AddComponent<Image>();
            pBg.color = new Color(0.12f, 0.16f, 0.26f, 0.98f);

            GameObject tObj = new GameObject("Title");
            tObj.transform.SetParent(panel.transform, false);
            RectTransform tRt = tObj.AddComponent<RectTransform>();
            tRt.anchoredPosition = new Vector2(0, 130);
            tRt.sizeDelta = new Vector2(560, 40);
            Text tTxt = tObj.AddComponent<Text>();
            tTxt.font = font;
            tTxt.fontSize = 22;
            tTxt.fontStyle = FontStyle.Bold;
            tTxt.alignment = TextAnchor.MiddleCenter;
            tTxt.color = new Color(1.0f, 0.8f, 0.2f);
            tTxt.text = "✨ LÒ HỢP NHẤT TRANG BỊ (FUSE) ✨";

            GameObject nObj = new GameObject("Notice");
            nObj.transform.SetParent(panel.transform, false);
            RectTransform nRt = nObj.AddComponent<RectTransform>();
            nRt.anchoredPosition = new Vector2(0, 15);
            nRt.sizeDelta = new Vector2(540, 110);
            noticeTxt = nObj.AddComponent<Text>();
            noticeTxt.font = font;
            noticeTxt.fontSize = 16;
            noticeTxt.alignment = TextAnchor.MiddleCenter;
            noticeTxt.color = Color.white;

            GameObject afBtnObj = CreateButton(panel, new Vector2(0, -105), new Vector2(320, 52), new Color(0.95f, 0.65f, 0.1f), "⚡ TỰ ĐỘNG GHÉP TẤT CẢ ⚡", font, 18);
            autoFuseBtn = afBtnObj.GetComponent<Button>();

            GameObject clBtnObj = CreateButton(panel, new Vector2(275, 155), new Vector2(40, 40), new Color(0.7f, 0.2f, 0.2f), "✕", font, 18);
            closeBtn = clBtnObj.GetComponent<Button>();

            return modal;
        }
    }
}
