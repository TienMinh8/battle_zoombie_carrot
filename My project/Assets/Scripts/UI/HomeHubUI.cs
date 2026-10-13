using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Progression;
using Apocarrot.Inventory;
using Apocarrot.Loot;

namespace Apocarrot.UI
{
    public enum HomeHubTab
    {
        Battle = 0,    // ⚔️ Chiến Đấu (Màn chơi, Start Run)
        Equipment = 1, // 🎒 Trang Bị (8 Slot, Balo, Nâng cấp, Phân giải, Ghép đồ)
        Chests = 2,    // 🎁 Rương & Shop (Mở rương theo bảng xác suất GDD 5.3)
        Bestiary = 3,  // 📖 Sổ Tay Quái (4 Quái vật: Basic, Fast, Elite, Boss King Carrot)
        Profile = 4    // 👤 Hồ Sơ & Thành Tựu (Chỉ số tài khoản, Nhận thưởng thành tựu)
    }

    /// <summary>
    /// Giao diện Sảnh Chính Master (Home Hub UI) hoàn chỉnh theo tài liệu Gameplay Expansion Design v1.0.
    /// Bao gồm:
    /// - Thanh Top Bar tài nguyên toàn cầu (Level, Năng Lượng ⚡, Vàng 💰, Gems 💎, Scrap ⚙️).
    /// - Thanh điều hướng 5 Tab bên dưới màn hình.
    /// - Luồng bắt đầu vượt ải chuẩn mobile app với chuyển cảnh mượt mà giữa Sảnh và Đấu trường.
    /// </summary>
    public class HomeHubUI : MonoBehaviour
    {
        public static HomeHubUI Instance { get; private set; }

        public event Action OnStartRunRequested;

        [Header("Root")]
        [SerializeField] private GameObject hubRoot;

        [Header("Top Bar")]
        [SerializeField] private Text profileLevelText;
        [SerializeField] private Text energyText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text gemsText;
        [SerializeField] private Text scrapText;

        [Header("Tab Roots")]
        [SerializeField] private GameObject battleTabRoot;
        [SerializeField] private GameObject equipmentTabRoot;
        [SerializeField] private GameObject chestsTabRoot;
        [SerializeField] private GameObject bestiaryTabRoot;
        [SerializeField] private GameObject profileTabRoot;

        [Header("Bottom Nav Buttons")]
        [SerializeField] private Button[] navButtons = new Button[5];
        [SerializeField] private Text[] navButtonTexts = new Text[5];
        [SerializeField] private Image[] navButtonBgs = new Image[5];

        [Header("Battle Tab Specific")]
        [SerializeField] private Text heroAtkText;
        [SerializeField] private Text heroHpText;
        [SerializeField] private Text heroRangedText;
        [SerializeField] private Button startRunButton;
        [SerializeField] private Button quickEquipButton;

        [Header("Equipment Tab Specific")]
        [SerializeField] private Button[] equippedSlotButtons = new Button[8];
        [SerializeField] private Text[] equippedSlotTexts = new Text[8];
        [SerializeField] private Image[] equippedSlotBgs = new Image[8];
        [SerializeField] private Image[] equippedSlotIcons = new Image[8];
        [SerializeField] private Text loadoutCpText;
        [SerializeField] private Text loadoutStatsText;
        [SerializeField] private Transform inventoryGridTransform;
        [SerializeField] private Button autoFuseButton;
        [SerializeField] private Button autoSalvageCommonButton;
        [SerializeField] private Button[] inventoryFilterButtons = new Button[4];
        [SerializeField] private Image[] inventoryFilterBgs = new Image[4];
        private int currentInventoryFilter = 0;

        [Header("Equipment Detail Modal")]
        [SerializeField] private GameObject itemDetailModal;
        [SerializeField] private Text detailItemName;
        [SerializeField] private Text detailItemRarity;
        [SerializeField] private Text detailItemStats;
        [SerializeField] private Text detailItemDesc;
        [SerializeField] private Button detailEquipButton;
        [SerializeField] private Text detailEquipButtonText;
        [SerializeField] private Button detailUpgradeButton;
        [SerializeField] private Text detailUpgradeButtonText;
        [SerializeField] private Button detailSalvageButton;
        [SerializeField] private Button detailCloseButton;

        [Header("Chests Tab Specific")]
        [SerializeField] private Button openNormalChestButton;
        [SerializeField] private Button openEliteChestButton;
        [SerializeField] private Button openBossChestButton;
        [SerializeField] private Button buyEnergyButton;
        [SerializeField] private Button buyGoldButton;

        [Header("Chest Reward Popup")]
        [SerializeField] private GameObject chestRewardPopup;
        [SerializeField] private Text rewardChestTitle;
        [SerializeField] private Text rewardItemName;
        [SerializeField] private Text rewardItemRarity;
        [SerializeField] private Text rewardItemStats;
        [SerializeField] private Text rewardExtraText;
        [SerializeField] private Button rewardConfirmButton;

        [Header("Bestiary / Companion Tab Specific")]
        [SerializeField] private Transform bestiaryListTransform;
        [SerializeField] private GameObject companionViewRoot;
        [SerializeField] private GameObject bestiaryViewRoot;
        [SerializeField] private Text companionLevelText;
        [SerializeField] private Text companionCpText;
        [SerializeField] private Text companionStatsText;
        [SerializeField] private Button companionUpgradeButton;
        [SerializeField] private Text companionUpgradeBtnText;
        [SerializeField] private Button tabCompanionToggleBtn;
        [SerializeField] private Button tabBestiaryToggleBtn;
        [SerializeField] private Image tabCompanionToggleBg;
        [SerializeField] private Image tabBestiaryToggleBg;
        private bool isViewingCompanionMode = true;

        [Header("Profile Tab Specific")]
        [SerializeField] private Text profileRabbitLevel;
        [SerializeField] private Text profileXpProgress;
        [SerializeField] private Image profileXpBarFill;
        [SerializeField] private Text profileLifetimeStats;
        [SerializeField] private Transform achievementsListTransform;

        [Header("Toast Notification")]
        [SerializeField] private GameObject toastObj;
        [SerializeField] private Text toastText;
        private Coroutine toastCoroutine;

        private HomeHubTab currentTab = HomeHubTab.Battle;
        private EquipmentItem selectedDetailItem = null;
        private Font uiFont;

        public bool IsOpen => hubRoot != null && hubRoot.activeSelf;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            if (AccountProgression.Instance != null)
            {
                AccountProgression.Instance.OnAccountDataChanged += RefreshAllUI;
            }

            SetupEventListeners();
            RefreshAllUI();
            SwitchTab(HomeHubTab.Battle);
        }

        private void OnDestroy()
        {
            if (AccountProgression.Instance != null)
            {
                AccountProgression.Instance.OnAccountDataChanged -= RefreshAllUI;
            }
        }

        public void ShowHub()
        {
            if (hubRoot != null) hubRoot.SetActive(true);
            RefreshAllUI();
            SwitchTab(HomeHubTab.Battle);
        }

        public void HideHub()
        {
            if (hubRoot != null) hubRoot.SetActive(false);
            if (itemDetailModal != null) itemDetailModal.SetActive(false);
            if (chestRewardPopup != null) chestRewardPopup.SetActive(false);
        }

        private void SetupEventListeners()
        {
            // Nav Bar
            for (int i = 0; i < 5; i++)
            {
                int tabIdx = i;
                if (navButtons[i] != null)
                {
                    navButtons[i].onClick.AddListener(() => SwitchTab((HomeHubTab)tabIdx));
                }
            }

            // Battle Tab
            if (startRunButton != null)
            {
                startRunButton.onClick.AddListener(HandleStartRun);
            }
            if (quickEquipButton != null)
            {
                quickEquipButton.onClick.AddListener(() => SwitchTab(HomeHubTab.Equipment));
            }

            // Equipment Tab
            if (autoFuseButton != null)
            {
                autoFuseButton.onClick.AddListener(HandleAutoFuse);
            }
            if (autoSalvageCommonButton != null)
            {
                autoSalvageCommonButton.onClick.AddListener(HandleAutoSalvageCommon);
            }
            for (int f = 0; f < 4; f++)
            {
                int filterIdx = f;
                if (inventoryFilterButtons[f] != null)
                {
                    inventoryFilterButtons[f].onClick.AddListener(() =>
                    {
                        currentInventoryFilter = filterIdx;
                        RefreshEquipmentTab();
                    });
                }
            }

            // Detail Modal
            if (detailCloseButton != null)
            {
                detailCloseButton.onClick.AddListener(() => { if (itemDetailModal != null) itemDetailModal.SetActive(false); });
            }
            if (detailEquipButton != null)
            {
                detailEquipButton.onClick.AddListener(HandleDetailEquipOrUnequip);
            }
            if (detailUpgradeButton != null)
            {
                detailUpgradeButton.onClick.AddListener(HandleDetailUpgrade);
            }
            if (detailSalvageButton != null)
            {
                detailSalvageButton.onClick.AddListener(HandleDetailSalvage);
            }

            // Chests Tab
            if (openNormalChestButton != null)
            {
                openNormalChestButton.onClick.AddListener(() => TryOpenChest(ChestType.Normal, 100, 0));
            }
            if (openEliteChestButton != null)
            {
                openEliteChestButton.onClick.AddListener(() => TryOpenChest(ChestType.Elite, 0, 20));
            }
            if (openBossChestButton != null)
            {
                openBossChestButton.onClick.AddListener(() => TryOpenChest(ChestType.Boss, 0, 50));
            }
            if (buyEnergyButton != null)
            {
                buyEnergyButton.onClick.AddListener(HandleBuyEnergy);
            }
            if (buyGoldButton != null)
            {
                buyGoldButton.onClick.AddListener(HandleBuyGold);
            }

            // Chest Popup
            if (rewardConfirmButton != null)
            {
                rewardConfirmButton.onClick.AddListener(() => { if (chestRewardPopup != null) chestRewardPopup.SetActive(false); });
            }

            // Companion & Bestiary Tab
            if (tabCompanionToggleBtn != null)
            {
                tabCompanionToggleBtn.onClick.AddListener(() =>
                {
                    isViewingCompanionMode = true;
                    RefreshBestiaryTab();
                });
            }
            if (tabBestiaryToggleBtn != null)
            {
                tabBestiaryToggleBtn.onClick.AddListener(() =>
                {
                    isViewingCompanionMode = false;
                    RefreshBestiaryTab();
                });
            }
            if (companionUpgradeButton != null)
            {
                companionUpgradeButton.onClick.AddListener(HandleUpgradeCompanion);
            }
        }

        public void SwitchTab(HomeHubTab tab)
        {
            currentTab = tab;

            if (battleTabRoot != null) battleTabRoot.SetActive(tab == HomeHubTab.Battle);
            if (equipmentTabRoot != null) equipmentTabRoot.SetActive(tab == HomeHubTab.Equipment);
            if (chestsTabRoot != null) chestsTabRoot.SetActive(tab == HomeHubTab.Chests);
            if (bestiaryTabRoot != null) bestiaryTabRoot.SetActive(tab == HomeHubTab.Bestiary);
            if (profileTabRoot != null) profileTabRoot.SetActive(tab == HomeHubTab.Profile);

            // Update Nav visual
            Color activeColor = new Color(0.18f, 0.55f, 0.95f);
            Color inactiveColor = new Color(0.12f, 0.14f, 0.20f);

            for (int i = 0; i < 5; i++)
            {
                bool isActive = (int)tab == i;
                if (navButtonBgs[i] != null)
                {
                    navButtonBgs[i].color = isActive ? activeColor : inactiveColor;
                }
                if (navButtonTexts[i] != null)
                {
                    navButtonTexts[i].color = isActive ? new Color(1f, 0.9f, 0.2f) : new Color(0.7f, 0.75f, 0.85f);
                    navButtonTexts[i].fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;
                }
            }

            RefreshCurrentTabContent();
        }

        public void RefreshAllUI()
        {
            RefreshTopBar();
            RefreshCurrentTabContent();
        }

        private void RefreshTopBar()
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null) return;

            if (profileLevelText != null)
                profileLevelText.text = $"CẤP {acc.RabbitLevel}";

            if (energyText != null)
                energyText.text = acc.UnlimitedEnergyForTesting ? "⚡ ∞" : $"⚡ {acc.CurrentEnergy}/{acc.MaxEnergy}";

            if (goldText != null)
                goldText.text = $"💰 {acc.PermanentGold:N0}";

            if (gemsText != null)
                gemsText.text = $"💎 {acc.PermanentGems:N0}";

            if (scrapText != null)
                scrapText.text = $"⚙️ {acc.PermanentScrap:N0}";
        }

        private void RefreshCurrentTabContent()
        {
            RefreshTopBar();

            switch (currentTab)
            {
                case HomeHubTab.Battle:
                    RefreshBattleTab();
                    break;
                case HomeHubTab.Equipment:
                    RefreshEquipmentTab();
                    break;
                case HomeHubTab.Chests:
                    // Chests are mostly static pricing with live currency from top bar
                    break;
                case HomeHubTab.Bestiary:
                    RefreshBestiaryTab();
                    break;
                case HomeHubTab.Profile:
                    RefreshProfileTab();
                    break;
            }
        }

        // ========================== TAB 1: CHIẾN ĐẤU ==========================
        private void RefreshBattleTab()
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null) return;

            int totalAtk = 25; // Base attack
            int totalHp = 100; // Base HP
            string rangedName = "Chưa trang bị";

            // Tính tổng chỉ số từ trang bị
            for (int i = 0; i < 8; i++)
            {
                var item = acc.GetEquippedItem((EquipmentSlotType)i);
                if (item != null)
                {
                    totalAtk += item.GetTotalAttackBonus();
                    totalHp += item.GetTotalHealthBonus();
                    if (item.SlotType == EquipmentSlotType.RangedWeapon)
                    {
                        rangedName = $"{item.DisplayName} (Lv.{item.Level})";
                    }
                }
            }

            if (heroAtkText != null) heroAtkText.text = $"⚔️ TỔNG SÁT THƯƠNG (ATK): {totalAtk}";
            if (heroHpText != null) heroHpText.text = $"❤️ TỔNG MÁU TỐI ĐA (HP): {totalHp}";
            if (heroRangedText != null) heroRangedText.text = $"🏹 VŨ KHÍ TẦM XA: {rangedName}";
        }

        private void HandleStartRun()
        {
            var prog = AccountProgression.Instance;
            if (prog != null && !prog.ConsumeEnergy(5))
            {
                ShowToast("⚠️ Không đủ Năng Lượng! Hãy vào mục Rương & Shop để nạp thêm.");
                return;
            }

            HideHub();
            OnStartRunRequested?.Invoke();
        }

        // ========================== TAB 2: TRANG BỊ ==========================
        private void RefreshEquipmentTab()
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null) return;

            // 1. Tính tổng chỉ số trang bị và Lực chiến CP
            int totalAtk = 25;
            int totalHp = 100;
            int totalDef = 0;
            float totalSpd = 0f;

            for (int i = 0; i < 8; i++)
            {
                var item = acc.GetEquippedItem((EquipmentSlotType)i);
                if (item != null)
                {
                    totalAtk += item.GetTotalAttackBonus();
                    totalHp += item.GetTotalHealthBonus();
                    totalDef += item.ArmorBonus;
                    totalSpd += item.MoveSpeedBonus;
                }
            }

            int totalCp = totalAtk * 10 + totalHp * 2 + totalDef * 15 + (int)(totalSpd * 500) + acc.RabbitLevel * 50;
            if (loadoutCpText != null) loadoutCpText.text = $"⚡ LỰC CHIẾN CP: {totalCp:N0}";
            if (loadoutStatsText != null) loadoutStatsText.text = $"⚔️ {totalAtk} ATK   ❤️ {totalHp} HP   🛡️ {totalDef} DEF   👟 +{totalSpd * 100:F0}% SPD";

            // 2. Cập nhật 8 ô trang bị đang mặc (Khung viền phẩm chất & Icon thật)
            for (int i = 0; i < 8; i++)
            {
                EquipmentSlotType slot = (EquipmentSlotType)i;
                EquipmentItem equipped = acc.GetEquippedItem(slot);

                if (equipped != null)
                {
                    if (equippedSlotBgs[i] != null)
                    {
                        equippedSlotBgs[i].sprite = UISpriteService.GetRarityCardSlot(equipped.Rarity, 160);
                        equippedSlotBgs[i].color = Color.white;
                    }
                    if (equippedSlotIcons[i] != null)
                    {
                        equippedSlotIcons[i].gameObject.SetActive(true);
                        equippedSlotIcons[i].sprite = WeaponSpriteService.GetEquipmentSprite(equipped);
                    }
                    if (equippedSlotTexts[i] != null)
                    {
                        equippedSlotTexts[i].text = $"<b>{equipped.DisplayName}</b>\n<color=#FFD700>Lv.{equipped.Level}</color>";
                    }
                }
                else
                {
                    if (equippedSlotBgs[i] != null)
                    {
                        equippedSlotBgs[i].sprite = UISpriteService.GetRarityCardSlot(EquipmentRarity.Common, 160);
                        equippedSlotBgs[i].color = new Color(0.18f, 0.22f, 0.3f, 0.9f);
                    }
                    if (equippedSlotIcons[i] != null)
                    {
                        equippedSlotIcons[i].gameObject.SetActive(false);
                    }
                    if (equippedSlotTexts[i] != null)
                    {
                        equippedSlotTexts[i].text = $"{GetSlotShortName(slot)}\n<color=#778899>(Trống)</color>";
                    }
                }

                if (equippedSlotButtons[i] != null)
                {
                    equippedSlotButtons[i].onClick.RemoveAllListeners();
                    equippedSlotButtons[i].onClick.AddListener(() =>
                    {
                        if (equipped != null) ShowItemDetail(equipped, isEquipped: true);
                    });
                }
            }

            // 3. Cập nhật màu sắc 4 tab bộ lọc túi đồ
            for (int f = 0; f < 4; f++)
            {
                if (inventoryFilterBgs[f] != null)
                {
                    inventoryFilterBgs[f].color = (f == currentInventoryFilter) ? new Color(0.18f, 0.55f, 0.95f) : new Color(0.12f, 0.15f, 0.22f);
                }
            }

            // 4. Cập nhật Kho Đồ (Inventory Grid theo bộ lọc)
            if (inventoryGridTransform != null)
            {
                for (int i = inventoryGridTransform.childCount - 1; i >= 0; i--)
                {
                    Destroy(inventoryGridTransform.GetChild(i).gameObject);
                }

                if (acc.OwnedEquipments != null)
                {
                    foreach (var item in acc.OwnedEquipments)
                    {
                        if (MatchesFilter(item, currentInventoryFilter))
                        {
                            CreateInventoryItemCard(item);
                        }
                    }
                }
            }
        }

        private bool MatchesFilter(EquipmentItem item, int filter)
        {
            if (item == null) return false;
            switch (filter)
            {
                case 1: // Vũ Khí
                    return item.SlotType == EquipmentSlotType.Weapon || item.SlotType == EquipmentSlotType.RangedWeapon;
                case 2: // Giáp & Trang Phục
                    return item.SlotType == EquipmentSlotType.Armor || item.SlotType == EquipmentSlotType.Gloves || item.SlotType == EquipmentSlotType.Boots || item.SlotType == EquipmentSlotType.Belt;
                case 3: // Phụ Kiện
                    return item.SlotType == EquipmentSlotType.Necklace || item.SlotType == EquipmentSlotType.Badge;
                default: // Tất Cả
                    return true;
            }
        }

        private string GetSlotShortName(EquipmentSlotType slot)
        {
            switch (slot)
            {
                case EquipmentSlotType.Weapon: return "⚔️ Cận Chiến";
                case EquipmentSlotType.RangedWeapon: return "🏹 Tầm Xa";
                case EquipmentSlotType.Necklace: return "📿 Vòng Cổ";
                case EquipmentSlotType.Gloves: return "🥊 Găng Tay";
                case EquipmentSlotType.Armor: return "🛡️ Áo Giáp";
                case EquipmentSlotType.Belt: return "🥋 Thắt Lưng";
                case EquipmentSlotType.Boots: return "👢 Giày Chạy";
                case EquipmentSlotType.Badge: return "🥕 Huy Hiệu";
                default: return "Vật Phẩm";
            }
        }

        private void CreateInventoryItemCard(EquipmentItem item)
        {
            GameObject cardObj = new GameObject($"Item_{item.TemplateId}");
            cardObj.transform.SetParent(inventoryGridTransform, false);

            RectTransform rt = cardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(140, 140);

            Image bg = cardObj.AddComponent<Image>();
            bg.sprite = UISpriteService.GetRarityCardSlot(item.Rarity, 140);
            bg.color = Color.white;

            Button btn = cardObj.AddComponent<Button>();
            btn.onClick.AddListener(() => ShowItemDetail(item, isEquipped: false));

            // Icon Vũ Khí / Trang Bị từ WeaponSpriteService
            Sprite itemSprite = WeaponSpriteService.GetEquipmentSprite(item);
            if (itemSprite != null)
            {
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(cardObj.transform, false);
                RectTransform irt = iconObj.AddComponent<RectTransform>();
                irt.anchorMin = new Vector2(0.5f, 0.5f);
                irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                irt.anchoredPosition = new Vector2(0, 8);
                irt.sizeDelta = new Vector2(76, 76);

                Image iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = itemSprite;
                iconImg.preserveAspect = true;
            }

            // Huy hiệu Cấp độ ở góc trên phải (Lv Badge)
            GameObject lvlBadgeObj = new GameObject("LvlBadge");
            lvlBadgeObj.transform.SetParent(cardObj.transform, false);
            RectTransform lrt = lvlBadgeObj.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(1, 1);
            lrt.anchorMax = new Vector2(1, 1);
            lrt.pivot = new Vector2(1, 1);
            lrt.anchoredPosition = new Vector2(-6, -6);
            lrt.sizeDelta = new Vector2(46, 20);

            Image lbg = lvlBadgeObj.AddComponent<Image>();
            lbg.sprite = UISpriteService.CreateRoundedBox(46, 20, 10, new Color(0.1f, 0.12f, 0.18f, 0.9f), new Color(1f, 0.85f, 0.25f), 1);
            lbg.color = Color.white;

            Text ltxt = CreateTextElement(lvlBadgeObj, "Txt", Vector2.zero, Vector2.one, uiFont, 11, FontStyle.Bold, new Color(1f, 0.85f, 0.25f), $"Lv.{item.Level}");

            // Tên hoặc Phẩm chất ở đáy thẻ
            GameObject bottomObj = new GameObject("BottomLabel");
            bottomObj.transform.SetParent(cardObj.transform, false);
            RectTransform brt = bottomObj.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 0);
            brt.anchorMax = new Vector2(1, 0);
            brt.pivot = new Vector2(0.5f, 0);
            brt.anchoredPosition = new Vector2(0, 6);
            brt.sizeDelta = new Vector2(-12, 22);

            Text btxt = bottomObj.AddComponent<Text>();
            btxt.font = uiFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            btxt.fontSize = 11;
            btxt.fontStyle = FontStyle.Bold;
            btxt.alignment = TextAnchor.MiddleCenter;
            btxt.color = item.GetRarityColor();
            btxt.text = item.DisplayName;
        }

        private void ShowItemDetail(EquipmentItem item, bool isEquipped)
        {
            selectedDetailItem = item;
            if (itemDetailModal == null || item == null) return;

            itemDetailModal.SetActive(true);

            if (detailItemName != null)
            {
                detailItemName.text = item.DisplayName;
                detailItemName.color = item.GetRarityColor();
            }

            if (detailItemRarity != null)
            {
                detailItemRarity.text = $"{item.GetRarityName()} • {item.GetSlotName()} • Cấp {item.Level}";
                detailItemRarity.color = item.GetRarityColor();
            }

            if (detailItemStats != null)
            {
                string statsStr = "";
                if (item.AttackBonus > 0) statsStr += $"⚔️ Sát Thương: +{item.GetTotalAttackBonus()} ATK\n";
                if (item.HealthBonus > 0) statsStr += $"❤️ Máu Tối Đa: +{item.GetTotalHealthBonus()} HP\n";
                if (item.ArmorBonus > 0) statsStr += $"🛡️ Giáp Phòng Thủ: +{item.ArmorBonus}\n";
                if (item.AttackSpeedBonus > 0) statsStr += $"⚡ Tốc Độ Đánh: +{item.AttackSpeedBonus * 100:F0}%\n";
                if (item.MoveSpeedBonus > 0) statsStr += $"👟 Tốc Độ Di Chuyển: +{item.MoveSpeedBonus * 100:F0}%\n";
                detailItemStats.text = statsStr.TrimEnd();
            }

            if (detailItemDesc != null)
            {
                detailItemDesc.text = item.Description ?? "Trang bị chiến binh thỏ dũng cảm.";
            }

            if (detailEquipButton != null && detailEquipButtonText != null)
            {
                detailEquipButtonText.text = isEquipped ? "THÁO RA" : "TRANG BỊ";
            }

            if (detailUpgradeButtonText != null)
            {
                int goldCost = item.Level * 60;
                int scrapCost = item.Level * 2;
                detailUpgradeButtonText.text = $"NÂNG CẤP\n({goldCost}💰 + {scrapCost}⚙️)";
            }

            if (detailSalvageButton != null)
            {
                // Không cho phân giải đồ đang mặc trực tiếp để tránh lỗi
                detailSalvageButton.gameObject.SetActive(!isEquipped);
            }
        }

        private void HandleDetailEquipOrUnequip()
        {
            if (selectedDetailItem == null || AccountProgression.Instance == null) return;

            var acc = AccountProgression.Instance.Data;
            bool isCurrentlyEquipped = acc.GetEquippedItem(selectedDetailItem.SlotType) == selectedDetailItem;

            if (isCurrentlyEquipped)
            {
                AccountProgression.Instance.UnequipItem(selectedDetailItem.SlotType);
                ShowToast($"Đã tháo {selectedDetailItem.DisplayName} vào Balo!");
            }
            else
            {
                AccountProgression.Instance.EquipItem(selectedDetailItem);
                ShowToast($"Đã trang bị {selectedDetailItem.DisplayName} thành công!");
            }

            if (itemDetailModal != null) itemDetailModal.SetActive(false);
            RefreshEquipmentTab();
        }

        private void HandleDetailUpgrade()
        {
            if (selectedDetailItem == null || AccountProgression.Instance == null) return;

            string msg;
            bool success = AccountProgression.Instance.UpgradeEquipment(selectedDetailItem, out msg);
            ShowToast(msg);

            if (success)
            {
                ShowItemDetail(selectedDetailItem, AccountProgression.Instance.Data.GetEquippedItem(selectedDetailItem.SlotType) == selectedDetailItem);
                RefreshEquipmentTab();
            }
        }

        private void HandleDetailSalvage()
        {
            if (selectedDetailItem == null || AccountProgression.Instance == null) return;

            int scrapGained;
            string name = selectedDetailItem.DisplayName;
            bool success = AccountProgression.Instance.SalvageEquipment(selectedDetailItem, out scrapGained);

            if (success)
            {
                ShowToast($"🔨 Đã phân giải {name}, thu được +{scrapGained} Scrap ⚙️!");
                if (itemDetailModal != null) itemDetailModal.SetActive(false);
                RefreshEquipmentTab();
            }
            else
            {
                ShowToast("⚠️ Không thể phân giải món đồ này!");
            }
        }

        private void HandleAutoFuse()
        {
            if (AccountProgression.Instance == null) return;
            int count = AccountProgression.Instance.AutoFuseAll();
            if (count > 0)
            {
                ShowToast($"🧬 Đã tự động ghép thành công {count} món trang bị lên bậc cao hơn!");
                RefreshEquipmentTab();
            }
            else
            {
                ShowToast("Chưa có cặp trang bị nào cùng loại & cùng bậc để ghép.");
            }
        }

        private void HandleAutoSalvageCommon()
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null || acc.OwnedEquipments == null) return;

            int salvagedCount = 0;
            int totalScrap = 0;

            for (int i = acc.OwnedEquipments.Count - 1; i >= 0; i--)
            {
                var item = acc.OwnedEquipments[i];
                if (item.Rarity == EquipmentRarity.Common)
                {
                    int scrap;
                    if (acc.SalvageEquipment(item, out scrap))
                    {
                        salvagedCount++;
                        totalScrap += scrap;
                    }
                }
            }

            if (salvagedCount > 0)
            {
                AccountProgression.Instance.SaveAccount();
                ShowToast($"🔨 Đã phân giải {salvagedCount} món Common, nhận +{totalScrap} Scrap ⚙️!");
                RefreshEquipmentTab();
            }
            else
            {
                ShowToast("Không có trang bị Thường (Common) nào trong kho để phân giải.");
            }
        }

        // ========================== TAB 3: RƯƠNG & SHOP ==========================
        private void TryOpenChest(ChestType type, int goldCost, int gemsCost)
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null) return;

            if (goldCost > 0 && acc.PermanentGold < goldCost)
            {
                ShowToast($"⚠️ Không đủ Vàng! Cần {goldCost} Vàng.");
                return;
            }
            if (gemsCost > 0 && acc.PermanentGems < gemsCost)
            {
                ShowToast($"⚠️ Không đủ Gems! Cần {gemsCost} Gems.");
                return;
            }

            if (goldCost > 0) acc.PermanentGold -= goldCost;
            if (gemsCost > 0) acc.PermanentGems -= gemsCost;

            ChestReward reward = ChestService.OpenChest(type);
            AccountProgression.Instance.AddEquipment(reward.Item);
            AccountProgression.Instance.AddGold(reward.BonusGold);
            AccountProgression.Instance.AddScrap(reward.BonusScrap);

            ShowChestRewardPopup(reward);
            RefreshAllUI();
        }

        public void ShowChestRewardPopup(ChestReward reward)
        {
            if (chestRewardPopup == null || reward == null) return;

            chestRewardPopup.SetActive(true);

            if (rewardChestTitle != null) rewardChestTitle.text = reward.GetRewardTitle();
            if (rewardItemName != null)
            {
                rewardItemName.text = reward.Item.DisplayName;
                rewardItemName.color = reward.Item.GetRarityColor();
            }
            if (rewardItemRarity != null)
            {
                rewardItemRarity.text = $"{reward.Item.GetRarityName()} • {reward.Item.GetSlotName()}";
                rewardItemRarity.color = reward.Item.GetRarityColor();
            }
            if (rewardItemStats != null)
            {
                string s = "";
                if (reward.Item.AttackBonus > 0) s += $"⚔️ +{reward.Item.GetTotalAttackBonus()} Sát Thương   ";
                if (reward.Item.HealthBonus > 0) s += $"❤️ +{reward.Item.GetTotalHealthBonus()} Máu Tối Đa   ";
                rewardItemStats.text = s;
            }
            if (rewardExtraText != null)
            {
                rewardExtraText.text = $"Kèm theo: +{reward.BonusGold} Vàng 💰  |  +{reward.BonusScrap} Scrap ⚙️";
            }
        }

        private void HandleBuyEnergy()
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null) return;

            if (acc.PermanentGold < 50)
            {
                ShowToast("⚠️ Cần 50 Vàng để mua 5 Năng Lượng!");
                return;
            }

            acc.PermanentGold -= 50;
            AccountProgression.Instance.AddEnergy(5);
            ShowToast("⚡ Đã nạp thành công +5 Năng Lượng!");
            RefreshTopBar();
        }

        private void HandleBuyGold()
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null) return;

            if (acc.PermanentGems < 20)
            {
                ShowToast("⚠️ Cần 20 Gems để đổi 500 Vàng!");
                return;
            }

            acc.PermanentGems -= 20;
            AccountProgression.Instance.AddGold(500);
            ShowToast("💰 Đã đổi thành công +500 Vàng!");
            RefreshTopBar();
        }

        // ========================== TAB 4: TRỢ THỦ & SỔ TAY QUÁI ==========================
        private void RefreshBestiaryTab()
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null) return;

            // Chuyển đổi giữa 2 chế độ hiển thị
            if (companionViewRoot != null) companionViewRoot.SetActive(isViewingCompanionMode);
            if (bestiaryViewRoot != null) bestiaryViewRoot.SetActive(!isViewingCompanionMode);

            if (tabCompanionToggleBg != null)
                tabCompanionToggleBg.color = isViewingCompanionMode ? new Color(0.18f, 0.55f, 0.95f) : new Color(0.12f, 0.15f, 0.22f);
            if (tabBestiaryToggleBg != null)
                tabBestiaryToggleBg.color = !isViewingCompanionMode ? new Color(0.18f, 0.55f, 0.95f) : new Color(0.12f, 0.15f, 0.22f);

            if (isViewingCompanionMode)
            {
                int compLevel = acc.CompanionLevel;
                int goldCost = compLevel * 80;
                int scrapCost = compLevel * 3;
                int petCp = compLevel * 120 + 80;

                if (companionLevelText != null)
                    companionLevelText.text = $"CẤP ĐỘ {compLevel}  (EXP: {acc.CompanionExp}/1000)";

                if (companionCpText != null)
                    companionCpText.text = $"⚡ LỰC CHIẾN PET: {petCp:N0} CP";

                if (companionStatsText != null)
                {
                    int dmg = 25 + compLevel * 6;
                    float interval = Mathf.Max(1.0f, 2.4f - compLevel * 0.04f);
                    companionStatsText.text =
                        $"⚔️ Sát thương tự động: +{dmg} DMG\n" +
                        $"⏱️ Tần suất bắn: {interval:F1} giây / phát\n" +
                        $"🎯 Tầm ngắm bắn: 8.5 mét\n" +
                        $"🚀 Kỹ năng: Mưa Tên Lửa Cà Rốt (Tự Động Bắn Quái Gần Nhất)";
                }

                if (companionUpgradeBtnText != null)
                {
                    companionUpgradeBtnText.text = $"🚀 NÂNG CẤP TRỢ THỦ\n({goldCost}💰 + {scrapCost}⚙️)";
                }
            }
            else
            {
                if (bestiaryListTransform != null)
                {
                    for (int i = bestiaryListTransform.childCount - 1; i >= 0; i--)
                    {
                        Destroy(bestiaryListTransform.GetChild(i).gameObject);
                    }

                    if (acc.Bestiary == null) acc.InitializeDefaultBestiary();

                    foreach (var monster in acc.Bestiary)
                    {
                        CreateBestiaryCard(monster);
                    }
                }
            }
        }

        private void HandleUpgradeCompanion()
        {
            if (AccountProgression.Instance == null) return;
            string msg;
            bool success = AccountProgression.Instance.UpgradeCompanion(out msg);
            ShowToast(msg);
            if (success)
            {
                RefreshBestiaryTab();
            }
        }

        private void CreateBestiaryCard(BestiaryEntry entry)
        {
            GameObject cardObj = new GameObject($"Bestiary_{entry.MonsterId}");
            cardObj.transform.SetParent(bestiaryListTransform, false);

            RectTransform rt = cardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(740, 100);

            Image bg = cardObj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.15f, 0.22f, 0.95f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(cardObj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(16, 8);
            trt.offsetMax = new Vector2(-16, -8);

            Text txt = textObj.AddComponent<Text>();
            txt.font = uiFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 14;
            txt.lineSpacing = 1.25f;
            txt.color = Color.white;

            string dangerColor = entry.MonsterId == "king_carrot" ? "#FF4444" : "#FFAA00";
            txt.text =
                $"<b>{entry.MonsterName}</b> ({entry.Role}) — <color={dangerColor}>Nguy hiểm: {entry.ThreatLevel}</color>\n" +
                $"<color=#88CCFF>Đã chạm trán: {entry.Encounters} lần  |  Đã tiêu diệt: {entry.Defeated} con</color>\n" +
                $"<color=#CCCCCC><i>{entry.Description}</i></color>";
        }

        // ========================== TAB 5: HỒ SƠ & THÀNH TỰU ==========================
        private void RefreshProfileTab()
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null) return;

            int reqXp = AccountData.GetXPRequiredForLevel(acc.RabbitLevel);

            if (profileRabbitLevel != null)
                profileRabbitLevel.text = $"CẤP ĐỘ CHIẾN BINH: {acc.RabbitLevel} ({acc.RabbitName})";

            if (profileXpProgress != null)
                profileXpProgress.text = $"Tiến trình kinh nghiệm (XP): {acc.CurrentXP} / {reqXp}";

            if (profileXpBarFill != null && reqXp > 0)
                profileXpBarFill.fillAmount = Mathf.Clamp01((float)acc.CurrentXP / reqXp);

            if (profileLifetimeStats != null)
            {
                profileLifetimeStats.text =
                    $"• Tổng số trận tham gia: {acc.TotalRunsPlayed}\n" +
                    $"• Số ải hoàn thành tuyệt đối: {acc.TotalLevelsCompleted}\n" +
                    $"• Tổng quái vật đã tiêu diệt: {acc.TotalEnemiesKilled} con\n" +
                    $"• Số lần hạ gục King Carrot: {acc.TotalBossesDefeated} lần\n" +
                    $"• Số trang bị lưu giữ trong kho: {acc.OwnedEquipments?.Count ?? 0} món";
            }

            // Danh sách thành tựu
            if (achievementsListTransform != null)
            {
                for (int i = achievementsListTransform.childCount - 1; i >= 0; i--)
                {
                    Destroy(achievementsListTransform.GetChild(i).gameObject);
                }

                if (acc.Achievements == null) acc.InitializeDefaultAchievements();

                foreach (var ach in acc.Achievements)
                {
                    CreateAchievementCard(ach);
                }
            }
        }

        private void CreateAchievementCard(AchievementProgress ach)
        {
            GameObject cardObj = new GameObject($"Ach_{ach.Id}");
            cardObj.transform.SetParent(achievementsListTransform, false);

            RectTransform rt = cardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(740, 80);

            Image bg = cardObj.AddComponent<Image>();
            bg.color = ach.IsClaimed ? new Color(0.08f, 0.1f, 0.15f, 0.8f) : new Color(0.14f, 0.17f, 0.25f, 0.95f);

            // Left text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(cardObj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 0);
            trt.anchorMax = new Vector2(0.72f, 1);
            trt.offsetMin = new Vector2(16, 6);
            trt.offsetMax = new Vector2(-8, -6);

            Text txt = textObj.AddComponent<Text>();
            txt.font = uiFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 14;
            txt.color = Color.white;

            string status = ach.IsClaimed ? "<color=#00FF88>✓ Đã Nhận</color>" : (ach.IsUnlocked ? "<color=#FFD700>★ Hoàn Thành!</color>" : $"Tiến độ: {ach.CurrentProgress}/{ach.TargetProgress}");
            txt.text =
                $"<b>{ach.Title}</b> — {status}\n" +
                $"<color=#CCCCCC>{ach.Description}</color>\n" +
                $"<color=#FFD700>Phần thưởng: +{ach.RewardAccountGold} Vàng, +{ach.RewardAccountGems} Gems, +{ach.RewardAccountXP} XP</color>";

            // Right button
            GameObject btnObj = new GameObject("ClaimBtn");
            btnObj.transform.SetParent(cardObj.transform, false);
            RectTransform brt = btnObj.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.74f, 0.15f);
            brt.anchorMax = new Vector2(0.98f, 0.85f);
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;

            Image btnBg = btnObj.AddComponent<Image>();
            Button btn = btnObj.AddComponent<Button>();

            GameObject btnTxtObj = new GameObject("BtnText");
            btnTxtObj.transform.SetParent(btnObj.transform, false);
            RectTransform btrt = btnTxtObj.AddComponent<RectTransform>();
            btrt.anchorMin = Vector2.zero;
            btrt.anchorMax = Vector2.one;
            btrt.offsetMin = Vector2.zero;
            btrt.offsetMax = Vector2.zero;

            Text btnTxt = btnTxtObj.AddComponent<Text>();
            btnTxt.font = txt.font;
            btnTxt.fontSize = 14;
            btnTxt.fontStyle = FontStyle.Bold;
            btnTxt.alignment = TextAnchor.MiddleCenter;

            if (ach.IsClaimed)
            {
                btnBg.color = new Color(0.2f, 0.22f, 0.28f);
                btnTxt.text = "ĐÃ NHẬN";
                btnTxt.color = new Color(0.6f, 0.6f, 0.6f);
                btn.interactable = false;
            }
            else if (ach.IsUnlocked)
            {
                btnBg.color = new Color(0.18f, 0.75f, 0.35f);
                btnTxt.text = "NHẬN 🎁";
                btnTxt.color = Color.white;
                btn.interactable = true;
                btn.onClick.AddListener(() =>
                {
                    int g, ge, sc, x;
                    if (AccountProgression.Instance.ClaimAchievement(ach.Id, out g, out ge, out sc, out x))
                    {
                        ShowToast($"🎉 Đã nhận thưởng: +{g} Vàng, +{ge} Gems, +{x} XP!");
                        RefreshProfileTab();
                    }
                });
            }
            else
            {
                btnBg.color = new Color(0.25f, 0.28f, 0.35f);
                btnTxt.text = $"{ach.CurrentProgress}/{ach.TargetProgress}";
                btnTxt.color = new Color(0.7f, 0.7f, 0.7f);
                btn.interactable = false;
            }
        }

        // ========================== TOAST NOTIFICATION ==========================
        public void ShowToast(string message)
        {
            if (toastObj == null || toastText == null) return;

            toastText.text = message;
            toastObj.SetActive(true);

            if (toastCoroutine != null) StopCoroutine(toastCoroutine);
            toastCoroutine = StartCoroutine(ToastRoutine());
        }

        private System.Collections.IEnumerator ToastRoutine()
        {
            yield return new WaitForSecondsRealtime(2.4f);
            if (toastObj != null) toastObj.SetActive(false);
        }

        // ========================== PROCEDURAL BUILDER ==========================
        public static HomeHubUI CreateHomeHubUI(GameObject canvasObj, Font defaultFont)
        {
            GameObject root = new GameObject("HomeHub_MasterRoot");
            root.transform.SetParent(canvasObj.transform, false);

            RectTransform rootRt = root.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            Image rootBg = root.AddComponent<Image>();
            rootBg.color = new Color(0.06f, 0.08f, 0.12f, 0.98f);

            HomeHubUI hub = root.AddComponent<HomeHubUI>();
            hub.hubRoot = root;
            hub.uiFont = defaultFont;

            // 1. Top Bar Stylized (Logo + Viên Nhộng Tài Nguyên Vàng, Gems, Năng Lượng)
            GameObject topBar = new GameObject("TopBar");
            topBar.transform.SetParent(root.transform, false);
            RectTransform tbRt = topBar.AddComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0, 1);
            tbRt.anchorMax = new Vector2(1, 1);
            tbRt.pivot = new Vector2(0.5f, 1);
            tbRt.anchoredPosition = Vector2.zero;
            tbRt.sizeDelta = new Vector2(0, 72);

            Image tbBg = topBar.AddComponent<Image>();
            tbBg.color = new Color(0.08f, 0.10f, 0.16f, 0.98f);

            // Logo APOCARROT bên trái
            GameObject logoObj = new GameObject("LogoImage");
            logoObj.transform.SetParent(topBar.transform, false);
            RectTransform logoRt = logoObj.AddComponent<RectTransform>();
            logoRt.anchorMin = new Vector2(0, 0.5f);
            logoRt.anchorMax = new Vector2(0, 0.5f);
            logoRt.pivot = new Vector2(0, 0.5f);
            logoRt.anchoredPosition = new Vector2(25, 0);
            logoRt.sizeDelta = new Vector2(190, 56);
            Image logoImg = logoObj.AddComponent<Image>();
            logoImg.sprite = UISpriteService.GetLogoSprite();
            logoImg.color = Color.white;

            // Hàm tạo viên nhộng tài nguyên (Pill Badge) bo góc
            Text CreateResourcePill(string name, float xPos, Color textColor, string defaultVal)
            {
                GameObject pillObj = new GameObject(name);
                pillObj.transform.SetParent(topBar.transform, false);
                RectTransform prt = pillObj.AddComponent<RectTransform>();
                prt.anchorMin = new Vector2(1, 0.5f);
                prt.anchorMax = new Vector2(1, 0.5f);
                prt.pivot = new Vector2(1, 0.5f);
                prt.anchoredPosition = new Vector2(xPos, 0);
                prt.sizeDelta = new Vector2(140, 42);

                Image pBg = pillObj.AddComponent<Image>();
                pBg.sprite = UISpriteService.CreateRoundedBox(140, 42, 21, new Color(0.12f, 0.16f, 0.24f, 0.95f), new Color(0.24f, 0.32f, 0.48f), 2);
                pBg.color = Color.white;

                return CreateTextElement(pillObj, "Txt", Vector2.zero, Vector2.one, defaultFont, 14, FontStyle.Bold, textColor, defaultVal);
            }

            hub.scrapText = CreateResourcePill("Pill_Scrap", -25, new Color(0.9f, 0.65f, 1f), "⚙️ 20");
            hub.gemsText = CreateResourcePill("Pill_Gems", -175, new Color(0.85f, 0.4f, 1f), "💎 320");
            hub.goldText = CreateResourcePill("Pill_Gold", -325, new Color(1f, 0.85f, 0.25f), "💰 12,430");
            hub.energyText = CreateResourcePill("Pill_Energy", -475, new Color(1f, 0.75f, 0.1f), "⭐ 5/5");
            hub.profileLevelText = CreateResourcePill("Pill_Level", -625, new Color(0.35f, 0.95f, 0.5f), "CẤP 3");

            // 2. Bottom Nav Bar (H 75, Bottom)
            GameObject navBar = new GameObject("BottomNavBar");
            navBar.transform.SetParent(root.transform, false);
            RectTransform navRt = navBar.AddComponent<RectTransform>();
            navRt.anchorMin = new Vector2(0, 0);
            navRt.anchorMax = new Vector2(1, 0);
            navRt.pivot = new Vector2(0.5f, 0);
            navRt.anchoredPosition = Vector2.zero;
            navRt.sizeDelta = new Vector2(0, 75);

            Image navBg = navBar.AddComponent<Image>();
            navBg.color = new Color(0.09f, 0.11f, 0.16f, 0.98f);

            string[] tabNames = new string[] { "⚔️ CHIẾN ĐẤU", "🎒 TRANG BỊ", "🎁 RƯƠNG", "📖 SỔ TAY", "👤 HỒ SƠ" };
            float tabWidth = 1f / 5f;

            for (int i = 0; i < 5; i++)
            {
                GameObject navBtnObj = new GameObject($"NavBtn_{i}");
                navBtnObj.transform.SetParent(navBar.transform, false);
                RectTransform nbrt = navBtnObj.AddComponent<RectTransform>();
                nbrt.anchorMin = new Vector2(i * tabWidth, 0);
                nbrt.anchorMax = new Vector2((i + 1) * tabWidth, 1);
                nbrt.offsetMin = new Vector2(3, 3);
                nbrt.offsetMax = new Vector2(-3, -3);

                Image btnBg = navBtnObj.AddComponent<Image>();
                Button btn = navBtnObj.AddComponent<Button>();

                Text ntxt = CreateTextElement(navBtnObj, "Text", Vector2.zero, Vector2.one, defaultFont, 15, FontStyle.Bold, Color.white, tabNames[i]);

                hub.navButtons[i] = btn;
                hub.navButtonBgs[i] = btnBg;
                hub.navButtonTexts[i] = ntxt;
            }

            // 3. Tab Container Area
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(root.transform, false);
            RectTransform caRt = contentArea.AddComponent<RectTransform>();
            caRt.anchorMin = new Vector2(0, 0);
            caRt.anchorMax = new Vector2(1, 1);
            caRt.offsetMin = new Vector2(0, 75);
            caRt.offsetMax = new Vector2(0, -70);

            // Build individual tab panels
            hub.battleTabRoot = BuildBattleTab(contentArea, defaultFont, hub);
            hub.equipmentTabRoot = BuildEquipmentTab(contentArea, defaultFont, hub);
            hub.chestsTabRoot = BuildChestsTab(contentArea, defaultFont, hub);
            hub.bestiaryTabRoot = BuildBestiaryTab(contentArea, defaultFont, hub);
            hub.profileTabRoot = BuildProfileTab(contentArea, defaultFont, hub);

            // 4. Modals and Overlays
            hub.itemDetailModal = BuildItemDetailModal(root, defaultFont, hub);
            hub.chestRewardPopup = BuildChestRewardPopup(root, defaultFont, hub);
            hub.toastObj = BuildToastNotification(root, defaultFont, out hub.toastText);

            return hub;
        }

        private static GameObject BuildBattleTab(GameObject parent, Font font, HomeHubUI hub)
        {
            GameObject tab = new GameObject("Tab_Battle");
            tab.transform.SetParent(parent.transform, false);
            RectTransform rt = tab.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // ================= 1. CỘT TRÁI: 4 NÚT MENU (QUEST, COLLECTION, SHOP, SETTINGS) =================
            GameObject leftMenu = new GameObject("Left_Sidebar_Menu");
            leftMenu.transform.SetParent(tab.transform, false);
            RectTransform lmRt = leftMenu.AddComponent<RectTransform>();
            lmRt.anchorMin = new Vector2(0, 0.5f); lmRt.anchorMax = new Vector2(0, 0.5f);
            lmRt.pivot = new Vector2(0, 0.5f);
            lmRt.anchoredPosition = new Vector2(25, 20);
            lmRt.sizeDelta = new Vector2(100, 380);

            string[] sideNames = new string[] { "📜\nQuest", "🏆\nBộ Sưu Tập", "🛒\nCửa Hàng", "⚙️\nCài Đặt" };
            for (int i = 0; i < 4; i++)
            {
                GameObject sbtn = new GameObject($"SideBtn_{i}");
                sbtn.transform.SetParent(leftMenu.transform, false);
                RectTransform srt = sbtn.AddComponent<RectTransform>();
                srt.anchorMin = new Vector2(0.5f, 1f); srt.anchorMax = new Vector2(0.5f, 1f);
                srt.pivot = new Vector2(0.5f, 1f);
                srt.anchoredPosition = new Vector2(0, -i * 92);
                srt.sizeDelta = new Vector2(86, 78);

                Image sbImg = sbtn.AddComponent<Image>();
                sbImg.sprite = UISpriteService.CreateRoundedBox(86, 78, 18, new Color(0.12f, 0.15f, 0.24f, 0.95f), new Color(0.24f, 0.30f, 0.45f), 3);
                sbImg.color = Color.white;
                Button b = sbtn.AddComponent<Button>();

                int tabIdx = i == 2 ? 2 : (i == 3 ? -1 : 0);
                b.onClick.AddListener(() => {
                    if (tabIdx >= 0) hub.SwitchTab((HomeHubTab)tabIdx);
                    else if (PauseMenuUI.Instance != null) PauseMenuUI.Instance.TogglePause();
                });

                CreateTextElement(sbtn, "Txt", Vector2.zero, Vector2.one, font, 13, FontStyle.Bold, Color.white, sideNames[i]);
            }

            // ================= 2. TRUNG TÂM: KHUNG TRƯNG BÀY THỎ CHIẾN BINH & TRỢ THỦ CARROT DRONE =================
            GameObject heroStage = new GameObject("Hero_Center_Stage");
            heroStage.transform.SetParent(tab.transform, false);
            RectTransform hsRt = heroStage.AddComponent<RectTransform>();
            hsRt.anchorMin = new Vector2(0.5f, 0.5f); hsRt.anchorMax = new Vector2(0.5f, 0.5f);
            hsRt.pivot = new Vector2(0.5f, 0.5f);
            hsRt.anchoredPosition = new Vector2(-60, 20);
            hsRt.sizeDelta = new Vector2(460, 480);

            // Bục đá nhân vật (Pedestal circle)
            GameObject pedestal = new GameObject("Pedestal");
            pedestal.transform.SetParent(heroStage.transform, false);
            RectTransform pdRt = pedestal.AddComponent<RectTransform>();
            pdRt.anchorMin = new Vector2(0.5f, 0); pdRt.anchorMax = new Vector2(0.5f, 0);
            pdRt.pivot = new Vector2(0.5f, 0);
            pdRt.anchoredPosition = new Vector2(0, 30);
            pdRt.sizeDelta = new Vector2(340, 90);
            Image pdImg = pedestal.AddComponent<Image>();
            pdImg.sprite = UISpriteService.CreateRoundedBox(340, 90, 45, new Color(0.15f, 0.18f, 0.28f, 0.95f), new Color(0.35f, 0.42f, 0.60f), 3);
            pdImg.color = Color.white;

            // Hình ảnh Thỏ Chiến Binh 3D
            GameObject heroImgObj = new GameObject("Hero_Avatar_Preview");
            heroImgObj.transform.SetParent(heroStage.transform, false);
            RectTransform hiRt = heroImgObj.AddComponent<RectTransform>();
            hiRt.anchorMin = new Vector2(0.5f, 0.5f); hiRt.anchorMax = new Vector2(0.5f, 0.5f);
            hiRt.pivot = new Vector2(0.5f, 0.5f);
            hiRt.anchoredPosition = new Vector2(-30, 40);
            hiRt.sizeDelta = new Vector2(280, 280);
            Image hiImg = heroImgObj.AddComponent<Image>();
            hiImg.sprite = UISpriteService.GetAvatarSprite();
            hiImg.color = Color.white;

            // Trợ thủ Cà Rốt Drone (Bay lơ lửng bên phải Thỏ)
            GameObject droneObj = new GameObject("Drone_Pet_Preview");
            droneObj.transform.SetParent(heroStage.transform, false);
            RectTransform drRt = droneObj.AddComponent<RectTransform>();
            drRt.anchorMin = new Vector2(1, 0.5f); drRt.anchorMax = new Vector2(1, 0.5f);
            drRt.pivot = new Vector2(0.5f, 0.5f);
            drRt.anchoredPosition = new Vector2(40, 100);
            drRt.sizeDelta = new Vector2(150, 150);
            Image drImg = droneObj.AddComponent<Image>();
            drImg.sprite = UISpriteService.GetPetDroneSprite();
            drImg.color = Color.white;

            // ================= 3. BÊN PHẢI: BANNER CHỌN ẢI & NÚT "PLAY" PHÁT SÁNG KHỔNG LỒ =================
            GameObject rightPanel = new GameObject("Right_Action_Panel");
            rightPanel.transform.SetParent(tab.transform, false);
            RectTransform rpRt = rightPanel.AddComponent<RectTransform>();
            rpRt.anchorMin = new Vector2(1, 0.5f); rpRt.anchorMax = new Vector2(1, 0.5f);
            rpRt.pivot = new Vector2(1, 0.5f);
            rpRt.anchoredPosition = new Vector2(-40, 20);
            rpRt.sizeDelta = new Vector2(380, 480);

            // Thẻ Stage Banner (Stage 3-1: Carrot Forest)
            GameObject stageCard = new GameObject("StageBannerCard");
            stageCard.transform.SetParent(rightPanel.transform, false);
            RectTransform scRt = stageCard.AddComponent<RectTransform>();
            scRt.anchorMin = new Vector2(0.5f, 1); scRt.anchorMax = new Vector2(0.5f, 1);
            scRt.pivot = new Vector2(0.5f, 1);
            scRt.anchoredPosition = new Vector2(0, -20);
            scRt.sizeDelta = new Vector2(360, 150);

            Image scBg = stageCard.AddComponent<Image>();
            scBg.sprite = UISpriteService.CreateRoundedBox(360, 150, 20, new Color(0.12f, 0.15f, 0.24f, 0.95f), new Color(0.95f, 0.55f, 0.15f), 3);
            scBg.color = Color.white;

            CreateTextElement(stageCard, "StageTag", new Vector2(0.08f, 0.65f), new Vector2(0.92f, 0.92f), font, 20, FontStyle.Bold, new Color(1f, 0.88f, 0.35f), "💀 STAGE 3-1: CARROT FOREST");
            CreateTextElement(stageCard, "Desc", new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.62f), font, 14, FontStyle.Normal, new Color(0.85f, 0.9f, 1f), "Thử thách: 4 Đợt Quái & Trùm King Carrot\nPhần thưởng: Rương Chúa Tể + 1,200 Vàng");

            // Chỉ số tổng quan Thỏ
            GameObject statsMini = new GameObject("Stats_Mini");
            statsMini.transform.SetParent(rightPanel.transform, false);
            RectTransform smRt = statsMini.AddComponent<RectTransform>();
            smRt.anchorMin = new Vector2(0.5f, 1); smRt.anchorMax = new Vector2(0.5f, 1);
            smRt.pivot = new Vector2(0.5f, 1);
            smRt.anchoredPosition = new Vector2(0, -185);
            smRt.sizeDelta = new Vector2(360, 120);

            Image smBg = statsMini.AddComponent<Image>();
            smBg.sprite = UISpriteService.CreateRoundedBox(360, 120, 16, new Color(0.09f, 0.12f, 0.18f, 0.92f), new Color(0.20f, 0.25f, 0.38f), 2);
            smBg.color = Color.white;

            hub.heroAtkText = CreateTextElement(statsMini, "Atk", new Vector2(0.06f, 0.64f), new Vector2(0.94f, 0.95f), font, 15, FontStyle.Bold, new Color(1f, 0.45f, 0.35f), "⚔️ TỔNG SÁT THƯƠNG (ATK): 35");
            hub.heroHpText = CreateTextElement(statsMini, "Hp", new Vector2(0.06f, 0.34f), new Vector2(0.94f, 0.65f), font, 15, FontStyle.Bold, new Color(0.35f, 0.95f, 0.45f), "❤️ TỔNG MÁU TỐI ĐA (HP): 100");
            hub.heroRangedText = CreateTextElement(statsMini, "Ranged", new Vector2(0.06f, 0.05f), new Vector2(0.94f, 0.34f), font, 14, FontStyle.Normal, new Color(1f, 0.82f, 0.35f), "🏹 TẦM XA: Súng Cà Chua (Lv.1)");

            // NÚT PLAY KHỔNG LỒ RỰC RỠ (CTA BUTTON)
            GameObject playBtnObj = new GameObject("Big_PLAY_Button");
            playBtnObj.transform.SetParent(rightPanel.transform, false);
            RectTransform pbRt = playBtnObj.AddComponent<RectTransform>();
            pbRt.anchorMin = new Vector2(0.5f, 0); pbRt.anchorMax = new Vector2(0.5f, 0);
            pbRt.pivot = new Vector2(0.5f, 0);
            pbRt.anchoredPosition = new Vector2(0, 20);
            pbRt.sizeDelta = new Vector2(340, 105);

            Image pbBg = playBtnObj.AddComponent<Image>();
            pbBg.sprite = UISpriteService.GetPlayButtonSprite();
            pbBg.color = Color.white;
            hub.startRunButton = playBtnObj.AddComponent<Button>();

            // Nút chữ PLAY lớn
            GameObject ptxtObj = new GameObject("Text");
            ptxtObj.transform.SetParent(playBtnObj.transform, false);
            Text ptxt = ptxtObj.AddComponent<Text>();
            ptxt.font = font;
            ptxt.fontSize = 34;
            ptxt.fontStyle = FontStyle.Bold;
            ptxt.alignment = TextAnchor.MiddleCenter;
            ptxt.color = Color.white;
            ptxt.text = "PLAY";
            RectTransform ptxtRt = ptxtObj.GetComponent<RectTransform>();
            ptxtRt.anchorMin = Vector2.zero; ptxtRt.anchorMax = Vector2.one; ptxtRt.sizeDelta = Vector2.zero;

            return tab;
        }

        private static GameObject BuildEquipmentTab(GameObject parent, Font font, HomeHubUI hub)
        {
            GameObject tab = new GameObject("Tab_Equipment");
            tab.transform.SetParent(parent.transform, false);
            RectTransform rt = tab.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // ================= 1. KHUNG TRƯNG BÀY ANH HÙNG & TRANG BỊ ĐANG MẶC (HERO LOADOUT) =================
            GameObject loadoutCard = new GameObject("HeroLoadoutPanel");
            loadoutCard.transform.SetParent(tab.transform, false);
            RectTransform lcRt = loadoutCard.AddComponent<RectTransform>();
            lcRt.anchorMin = new Vector2(0.5f, 1f); lcRt.anchorMax = new Vector2(0.5f, 1f);
            lcRt.pivot = new Vector2(0.5f, 1f);
            lcRt.anchoredPosition = new Vector2(0, -10);
            lcRt.sizeDelta = new Vector2(860, 310);

            Image lcBg = loadoutCard.AddComponent<Image>();
            lcBg.sprite = UISpriteService.CreateRoundedBox(860, 310, 16, new Color(0.08f, 0.11f, 0.17f, 0.95f), new Color(0.18f, 0.35f, 0.65f, 0.7f), 2);
            lcBg.color = Color.white;

            // --- CỘT TRÁI: 4 Ô TRANG BỊ (0: Cận chiến, 4: Áo giáp, 2: Dây chuyền, 6: Giày) ---
            int[] leftSlots = new int[] { 0, 4, 2, 6 };
            float slotW = 168f; float slotH = 62f;
            for (int k = 0; k < 4; k++)
            {
                int slotIdx = leftSlots[k];
                GameObject slotObj = new GameObject($"EquippedSlot_{slotIdx}");
                slotObj.transform.SetParent(loadoutCard.transform, false);
                RectTransform srt = slotObj.AddComponent<RectTransform>();
                srt.anchorMin = new Vector2(0, 1); srt.anchorMax = new Vector2(0, 1);
                srt.pivot = new Vector2(0, 1);
                srt.anchoredPosition = new Vector2(18, -16 - k * (slotH + 8));
                srt.sizeDelta = new Vector2(slotW, slotH);

                hub.equippedSlotBgs[slotIdx] = slotObj.AddComponent<Image>();
                hub.equippedSlotBgs[slotIdx].sprite = UISpriteService.GetRarityCardSlot(EquipmentRarity.Common, 160);
                hub.equippedSlotBgs[slotIdx].color = Color.white;
                hub.equippedSlotButtons[slotIdx] = slotObj.AddComponent<Button>();

                // Icon ô đồ
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(slotObj.transform, false);
                RectTransform irt = iconObj.AddComponent<RectTransform>();
                irt.anchorMin = new Vector2(0, 0.5f); irt.anchorMax = new Vector2(0, 0.5f);
                irt.pivot = new Vector2(0, 0.5f);
                irt.anchoredPosition = new Vector2(28, 0);
                irt.sizeDelta = new Vector2(44, 44);
                hub.equippedSlotIcons[slotIdx] = iconObj.AddComponent<Image>();
                hub.equippedSlotIcons[slotIdx].preserveAspect = true;

                // Text ô đồ
                hub.equippedSlotTexts[slotIdx] = CreateTextElement(slotObj, "Text", new Vector2(0.36f, 0), new Vector2(0.96f, 1), font, 11, FontStyle.Bold, Color.white, $"Slot {slotIdx}");
                hub.equippedSlotTexts[slotIdx].alignment = TextAnchor.MiddleLeft;
            }

            // --- CỘT PHẢI: 4 Ô TRANG BỊ (1: Tầm xa, 3: Găng tay, 5: Thắt lưng, 7: Huy hiệu) ---
            int[] rightSlots = new int[] { 1, 3, 5, 7 };
            for (int k = 0; k < 4; k++)
            {
                int slotIdx = rightSlots[k];
                GameObject slotObj = new GameObject($"EquippedSlot_{slotIdx}");
                slotObj.transform.SetParent(loadoutCard.transform, false);
                RectTransform srt = slotObj.AddComponent<RectTransform>();
                srt.anchorMin = new Vector2(1, 1); srt.anchorMax = new Vector2(1, 1);
                srt.pivot = new Vector2(1, 1);
                srt.anchoredPosition = new Vector2(-18 - slotW, -16 - k * (slotH + 8));
                srt.sizeDelta = new Vector2(slotW, slotH);

                hub.equippedSlotBgs[slotIdx] = slotObj.AddComponent<Image>();
                hub.equippedSlotBgs[slotIdx].sprite = UISpriteService.GetRarityCardSlot(EquipmentRarity.Common, 160);
                hub.equippedSlotBgs[slotIdx].color = Color.white;
                hub.equippedSlotButtons[slotIdx] = slotObj.AddComponent<Button>();

                // Icon ô đồ
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(slotObj.transform, false);
                RectTransform irt = iconObj.AddComponent<RectTransform>();
                irt.anchorMin = new Vector2(0, 0.5f); irt.anchorMax = new Vector2(0, 0.5f);
                irt.pivot = new Vector2(0, 0.5f);
                irt.anchoredPosition = new Vector2(28, 0);
                irt.sizeDelta = new Vector2(44, 44);
                hub.equippedSlotIcons[slotIdx] = iconObj.AddComponent<Image>();
                hub.equippedSlotIcons[slotIdx].preserveAspect = true;

                // Text ô đồ
                hub.equippedSlotTexts[slotIdx] = CreateTextElement(slotObj, "Text", new Vector2(0.36f, 0), new Vector2(0.96f, 1), font, 11, FontStyle.Bold, Color.white, $"Slot {slotIdx}");
                hub.equippedSlotTexts[slotIdx].alignment = TextAnchor.MiddleLeft;
            }

            // --- KHUNG TRUNG TÂM: BỆ ĐỠ ANH HÙNG (HERO PEDESTAL) ---
            GameObject centerPedestal = new GameObject("CenterPedestal");
            centerPedestal.transform.SetParent(loadoutCard.transform, false);
            RectTransform cpRt = centerPedestal.AddComponent<RectTransform>();
            cpRt.anchorMin = new Vector2(0.5f, 0.5f); cpRt.anchorMax = new Vector2(0.5f, 0.5f);
            cpRt.pivot = new Vector2(0.5f, 0.5f);
            cpRt.anchoredPosition = new Vector2(0, 0);
            cpRt.sizeDelta = new Vector2(480, 290);

            // Bệ đỡ phát sáng
            GameObject ringObj = new GameObject("PedestalRing");
            ringObj.transform.SetParent(centerPedestal.transform, false);
            RectTransform rrt = ringObj.AddComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.5f, 0.5f); rrt.anchorMax = new Vector2(0.5f, 0.5f);
            rrt.pivot = new Vector2(0.5f, 0.5f);
            rrt.anchoredPosition = new Vector2(0, 15);
            rrt.sizeDelta = new Vector2(230, 48);
            Image ringImg = ringObj.AddComponent<Image>();
            ringImg.sprite = UISpriteService.CreateRoundedBox(230, 48, 24, new Color(0.12f, 0.35f, 0.75f, 0.45f), new Color(0.2f, 0.75f, 1f, 0.9f), 2);
            ringImg.color = Color.white;

            // Khung Avatar Thỏ Chiến Binh
            GameObject avatarBox = new GameObject("HeroAvatar");
            avatarBox.transform.SetParent(centerPedestal.transform, false);
            RectTransform avRt = avatarBox.AddComponent<RectTransform>();
            avRt.anchorMin = new Vector2(0.5f, 0.5f); avRt.anchorMax = new Vector2(0.5f, 0.5f);
            avRt.pivot = new Vector2(0.5f, 0.5f);
            avRt.anchoredPosition = new Vector2(0, 52);
            avRt.sizeDelta = new Vector2(130, 130);
            Image avImg = avatarBox.AddComponent<Image>();
            avImg.sprite = UISpriteService.GetAvatarSprite();
            avImg.preserveAspect = true;

            // Nút xoay trái ◀ và phải ▶
            GameObject rotL = new GameObject("RotateLeft");
            rotL.transform.SetParent(centerPedestal.transform, false);
            RectTransform rlRt = rotL.AddComponent<RectTransform>();
            rlRt.anchoredPosition = new Vector2(-95, 52); rlRt.sizeDelta = new Vector2(36, 36);
            Image rlBg = rotL.AddComponent<Image>();
            rlBg.sprite = UISpriteService.CreateCircle(36, new Color(0.15f, 0.2f, 0.3f, 0.8f), new Color(1f, 0.8f, 0.2f), 1);
            Button rlBtn = rotL.AddComponent<Button>();
            CreateTextElement(rotL, "Txt", Vector2.zero, Vector2.one, font, 16, FontStyle.Bold, Color.white, "◀");

            GameObject rotR = new GameObject("RotateRight");
            rotR.transform.SetParent(centerPedestal.transform, false);
            RectTransform rrRt = rotR.AddComponent<RectTransform>();
            rrRt.anchoredPosition = new Vector2(95, 52); rrRt.sizeDelta = new Vector2(36, 36);
            Image rrBg = rotR.AddComponent<Image>();
            rrBg.sprite = UISpriteService.CreateCircle(36, new Color(0.15f, 0.2f, 0.3f, 0.8f), new Color(1f, 0.8f, 0.2f), 1);
            Button rrBtn = rotR.AddComponent<Button>();
            CreateTextElement(rotR, "Txt", Vector2.zero, Vector2.one, font, 16, FontStyle.Bold, Color.white, "▶");

            // Tên Anh Hùng
            CreateTextElement(centerPedestal, "HeroName", new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.5f), font, 16, FontStyle.Bold, new Color(1f, 0.85f, 0.25f), "🐰 CHIẾN BINH THỎ APOCARROT")
                .rectTransform.anchoredPosition = new Vector2(0, -28);

            // Viên nhộng Lực Chiến (CP Pill)
            GameObject cpPill = new GameObject("CpPill");
            cpPill.transform.SetParent(centerPedestal.transform, false);
            RectTransform cpRt2 = cpPill.AddComponent<RectTransform>();
            cpRt2.anchorMin = new Vector2(0.5f, 0.5f); cpRt2.anchorMax = new Vector2(0.5f, 0.5f);
            cpRt2.pivot = new Vector2(0.5f, 0.5f);
            cpRt2.anchoredPosition = new Vector2(0, -58);
            cpRt2.sizeDelta = new Vector2(250, 28);
            Image cpBg = cpPill.AddComponent<Image>();
            cpBg.sprite = UISpriteService.CreateRoundedBox(250, 28, 14, new Color(0.1f, 0.15f, 0.25f, 0.9f), new Color(1f, 0.85f, 0.2f), 1);
            hub.loadoutCpText = CreateTextElement(cpPill, "CpTxt", Vector2.zero, Vector2.one, font, 13, FontStyle.Bold, new Color(1f, 0.9f, 0.3f), "⚡ LỰC CHIẾN CP: 1,450");

            // Dòng tóm tắt chỉ số (Stats Text)
            hub.loadoutStatsText = CreateTextElement(centerPedestal, "StatsSummary", new Vector2(0, 0.5f), new Vector2(1, 0.5f), font, 12, FontStyle.Normal, new Color(0.85f, 0.92f, 1f), "⚔️ 345 ATK   ❤️ 1,280 HP   🛡️ 85 DEF   👟 +15% SPD");
            hub.loadoutStatsText.rectTransform.anchoredPosition = new Vector2(0, -88);
            hub.loadoutStatsText.rectTransform.sizeDelta = new Vector2(460, 24);

            // ================= 2. THANH BỘ LỌC TÚI ĐỒ & THAO TÁC NHANH (FILTER & QUICK ACTIONS) =================
            GameObject filterBar = new GameObject("FilterAndActionsBar");
            filterBar.transform.SetParent(tab.transform, false);
            RectTransform fbRt = filterBar.AddComponent<RectTransform>();
            fbRt.anchorMin = new Vector2(0.5f, 1f); fbRt.anchorMax = new Vector2(0.5f, 1f);
            fbRt.pivot = new Vector2(0.5f, 1f);
            fbRt.anchoredPosition = new Vector2(0, -325);
            fbRt.sizeDelta = new Vector2(860, 42);

            // 4 Nút bộ lọc: [Tất cả] [Vũ khí] [Giáp] [Phụ kiện]
            string[] filterNames = new string[] { "TẤT CẢ", "⚔️ VŨ KHÍ", "🛡️ GIÁP", "📿 PHỤ KIỆN" };
            float fBtnW = 100f;
            for (int f = 0; f < 4; f++)
            {
                int fIdx = f;
                GameObject fbtn = new GameObject($"FilterBtn_{f}");
                fbtn.transform.SetParent(filterBar.transform, false);
                RectTransform fbrt = fbtn.AddComponent<RectTransform>();
                fbrt.anchorMin = new Vector2(0, 0.5f); fbrt.anchorMax = new Vector2(0, 0.5f);
                fbrt.pivot = new Vector2(0, 0.5f);
                fbrt.anchoredPosition = new Vector2(f * (fBtnW + 8), 0);
                fbrt.sizeDelta = new Vector2(fBtnW, 36);

                Image fbg = fbtn.AddComponent<Image>();
                fbg.sprite = UISpriteService.CreateRoundedBox((int)fBtnW, 36, 12, new Color(0.12f, 0.15f, 0.22f), new Color(0.25f, 0.35f, 0.5f), 1);
                fbg.color = Color.white;
                Button fb = fbtn.AddComponent<Button>();

                Text ftxt = CreateTextElement(fbtn, "Txt", Vector2.zero, Vector2.one, font, 11, FontStyle.Bold, Color.white, filterNames[f]);

                hub.inventoryFilterButtons[f] = fb;
                hub.inventoryFilterBgs[f] = fbg;
            }

            // Nút Auto Fuse (Bên phải)
            GameObject fuseBtnObj = new GameObject("AutoFuseBtn");
            fuseBtnObj.transform.SetParent(filterBar.transform, false);
            RectTransform afRt = fuseBtnObj.AddComponent<RectTransform>();
            afRt.anchorMin = new Vector2(1, 0.5f); afRt.anchorMax = new Vector2(1, 0.5f);
            afRt.pivot = new Vector2(1, 0.5f);
            afRt.anchoredPosition = new Vector2(-195, 0);
            afRt.sizeDelta = new Vector2(185, 36);
            Image afBg = fuseBtnObj.AddComponent<Image>();
            afBg.sprite = UISpriteService.CreateGradientBox(185, 36, 12, new Color(0.18f, 0.55f, 0.95f), new Color(0.1f, 0.35f, 0.75f), Color.white, 1);
            hub.autoFuseButton = fuseBtnObj.AddComponent<Button>();
            CreateTextElement(fuseBtnObj, "Text", Vector2.zero, Vector2.one, font, 12, FontStyle.Bold, Color.white, "🧬 GHÉP ĐỒ TỰ ĐỘNG");

            // Nút Auto Salvage (Góc phải cùng)
            GameObject salvBtnObj = new GameObject("SalvageBtn");
            salvBtnObj.transform.SetParent(filterBar.transform, false);
            RectTransform asRt = salvBtnObj.AddComponent<RectTransform>();
            asRt.anchorMin = new Vector2(1, 0.5f); asRt.anchorMax = new Vector2(1, 0.5f);
            asRt.pivot = new Vector2(1, 0.5f);
            asRt.anchoredPosition = new Vector2(0, 0);
            asRt.sizeDelta = new Vector2(185, 36);
            Image asBg = salvBtnObj.AddComponent<Image>();
            asBg.sprite = UISpriteService.CreateGradientBox(185, 36, 12, new Color(0.85f, 0.35f, 0.18f), new Color(0.6f, 0.2f, 0.1f), Color.white, 1);
            hub.autoSalvageCommonButton = salvBtnObj.AddComponent<Button>();
            CreateTextElement(salvBtnObj, "Text", Vector2.zero, Vector2.one, font, 12, FontStyle.Bold, Color.white, "🔨 PHÂN GIẢI THƯỜNG");

            // ================= 3. KHU VỰC CUỘN KHO ĐỒ (INVENTORY SCROLL VIEW) =================
            GameObject scrollObj = new GameObject("InventoryScrollView");
            scrollObj.transform.SetParent(tab.transform, false);
            RectTransform scrt = scrollObj.AddComponent<RectTransform>();
            scrt.anchorMin = new Vector2(0.5f, 0); scrt.anchorMax = new Vector2(0.5f, 1);
            scrt.pivot = new Vector2(0.5f, 0.5f);
            scrt.anchoredPosition = new Vector2(0, -195);
            scrt.sizeDelta = new Vector2(860, 260);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false; sr.vertical = true;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            RectTransform vprt = viewport.AddComponent<RectTransform>();
            vprt.anchorMin = Vector2.zero; vprt.anchorMax = Vector2.one;
            vprt.offsetMin = Vector2.zero; vprt.offsetMax = Vector2.zero;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            Image vpImg = viewport.AddComponent<Image>();
            vpImg.color = Color.white;
            sr.viewport = vprt;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform crt = content.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.sizeDelta = new Vector2(0, 500);

            GridLayoutGroup glg = content.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(136, 136);
            glg.spacing = new Vector2(14, 14);
            glg.padding = new RectOffset(12, 12, 12, 12);
            glg.childAlignment = TextAnchor.UpperLeft;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = crt;
            hub.inventoryGridTransform = content.transform;

            return tab;
        }

        private static GameObject BuildChestsTab(GameObject parent, Font font, HomeHubUI hub)
        {
            GameObject tab = new GameObject("Tab_Chests");
            tab.transform.SetParent(parent.transform, false);
            RectTransform rt = tab.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Header Banner
            CreateTextElement(tab, "Header", new Vector2(0, 0.88f), new Vector2(1, 0.98f), font, 22, FontStyle.Bold, new Color(1f, 0.85f, 0.2f), "🎁 RƯƠNG THẦN BÍ & CỬA HÀNG TÀI NGUYÊN");

            // 3 Thẻ Rương
            float cardW = 240; float cardH = 340;
            string[] chestTitles = new string[] { "📦 RƯƠNG CHIẾN TRANH\n(Wood Chest)", "🏆 RƯƠNG TINH ANH\n(Elite Chest)", "👑 RƯƠNG CHÚA TỂ\n(Boss Chest)" };
            string[] chestOdds = new string[]
            {
                "70% Thường\n25% Hiếm\n4% Sử thi\n1% Huyền thoại",
                "40% Thường\n45% Hiếm\n13% Sử thi\n2% Huyền thoại",
                "20% Thường\n45% Hiếm\n28% Sử thi\n7% Huyền thoại"
            };
            string[] btnLabels = new string[] { "MỞ (100 💰)", "MỞ (20 💎)", "MỞ (50 💎)" };
            Color[] cardColors = new Color[]
            {
                new Color(0.18f, 0.14f, 0.12f, 0.95f),
                new Color(0.12f, 0.16f, 0.26f, 0.95f),
                new Color(0.24f, 0.14f, 0.26f, 0.95f)
            };

            for (int i = 0; i < 3; i++)
            {
                GameObject card = new GameObject($"ChestCard_{i}");
                card.transform.SetParent(tab.transform, false);
                RectTransform crt = card.AddComponent<RectTransform>();
                crt.anchorMin = new Vector2(0.5f, 0.5f); crt.anchorMax = new Vector2(0.5f, 0.5f);
                crt.pivot = new Vector2(0.5f, 0.5f);
                crt.anchoredPosition = new Vector2(-270 + i * (cardW + 30), 40);
                crt.sizeDelta = new Vector2(cardW, cardH);

                Image cBg = card.AddComponent<Image>();
                Color borderCol = i == 2 ? new Color(0.9f, 0.45f, 1f) : (i == 1 ? new Color(0.35f, 0.7f, 1f) : new Color(0.95f, 0.65f, 0.2f));
                cBg.sprite = UISpriteService.CreateRoundedBox((int)cardW, (int)cardH, 20, cardColors[i], borderCol, 3);
                cBg.color = Color.white;

                CreateTextElement(card, "Title", new Vector2(0, 0.78f), new Vector2(1, 0.96f), font, 16, FontStyle.Bold, Color.yellow, chestTitles[i]);

                // Hình Ảnh Rương 3D Phát Sáng
                GameObject cImgObj = new GameObject("ChestImage");
                cImgObj.transform.SetParent(card.transform, false);
                RectTransform cirt = cImgObj.AddComponent<RectTransform>();
                cirt.anchorMin = new Vector2(0.5f, 0.5f); cirt.anchorMax = new Vector2(0.5f, 0.5f);
                cirt.pivot = new Vector2(0.5f, 0.5f);
                cirt.anchoredPosition = new Vector2(0, 15);
                cirt.sizeDelta = new Vector2(120, 120);
                Image cimg = cImgObj.AddComponent<Image>();
                cimg.sprite = i == 2 ? UISpriteService.GetChestPurpleSprite() : UISpriteService.GetChestGoldenSprite();
                cimg.color = Color.white;

                CreateTextElement(card, "Odds", new Vector2(0.05f, 0.24f), new Vector2(0.95f, 0.42f), font, 12, FontStyle.Normal, new Color(0.9f, 0.95f, 1f), $"Tỷ lệ: {chestOdds[i].Replace("\n", " • ")}");

                GameObject openBtn = new GameObject("OpenBtn");
                openBtn.transform.SetParent(card.transform, false);
                RectTransform obrt = openBtn.AddComponent<RectTransform>();
                obrt.anchorMin = new Vector2(0.1f, 0.05f); obrt.anchorMax = new Vector2(0.9f, 0.20f);
                obrt.offsetMin = Vector2.zero; obrt.offsetMax = Vector2.zero;

                Image obBg = openBtn.AddComponent<Image>();
                obBg.sprite = UISpriteService.GetActionButtonSprite("openchest", 190, 46);
                obBg.color = Color.white;
                Button b = openBtn.AddComponent<Button>();
                CreateTextElement(openBtn, "Text", Vector2.zero, Vector2.one, font, 16, FontStyle.Bold, Color.white, btnLabels[i]);

                if (i == 0) hub.openNormalChestButton = b;
                else if (i == 1) hub.openEliteChestButton = b;
                else hub.openBossChestButton = b;
            }

            // Tiệm Đổi Tài Nguyên (Shop Exchange Bottom Bar)
            GameObject exchangeBar = new GameObject("ExchangeBar");
            exchangeBar.transform.SetParent(tab.transform, false);
            RectTransform ebrt = exchangeBar.AddComponent<RectTransform>();
            ebrt.anchorMin = new Vector2(0.5f, 0); ebrt.anchorMax = new Vector2(0.5f, 0);
            ebrt.pivot = new Vector2(0.5f, 0);
            ebrt.anchoredPosition = new Vector2(0, 20);
            ebrt.sizeDelta = new Vector2(780, 60);

            Image ebBg = exchangeBar.AddComponent<Image>();
            ebBg.color = new Color(0.10f, 0.12f, 0.18f, 0.95f);

            GameObject buyEnBtnObj = new GameObject("BuyEnergyBtn");
            buyEnBtnObj.transform.SetParent(exchangeBar.transform, false);
            RectTransform bert = buyEnBtnObj.AddComponent<RectTransform>();
            bert.anchorMin = new Vector2(0.04f, 0.12f); bert.anchorMax = new Vector2(0.48f, 0.88f);
            bert.offsetMin = Vector2.zero; bert.offsetMax = Vector2.zero;
            Image beBg = buyEnBtnObj.AddComponent<Image>();
            beBg.color = new Color(0.15f, 0.45f, 0.75f);
            hub.buyEnergyButton = buyEnBtnObj.AddComponent<Button>();
            CreateTextElement(buyEnBtnObj, "Text", Vector2.zero, Vector2.one, font, 14, FontStyle.Bold, Color.white, "⚡ NẠP +5 NĂNG LƯỢNG (50 💰)");

            GameObject buyGdBtnObj = new GameObject("BuyGoldBtn");
            buyGdBtnObj.transform.SetParent(exchangeBar.transform, false);
            RectTransform bgrt = buyGdBtnObj.AddComponent<RectTransform>();
            bgrt.anchorMin = new Vector2(0.52f, 0.12f); bgrt.anchorMax = new Vector2(0.96f, 0.88f);
            bgrt.offsetMin = Vector2.zero; bgrt.offsetMax = Vector2.zero;
            Image bgBg = buyGdBtnObj.AddComponent<Image>();
            bgBg.color = new Color(0.85f, 0.65f, 0.15f);
            hub.buyGoldButton = buyGdBtnObj.AddComponent<Button>();
            CreateTextElement(buyGdBtnObj, "Text", Vector2.zero, Vector2.one, font, 14, FontStyle.Bold, Color.white, "💰 ĐỔI +500 VÀNG (20 💎)");

            return tab;
        }

        private static GameObject BuildBestiaryTab(GameObject parent, Font font, HomeHubUI hub)
        {
            GameObject tab = new GameObject("Tab_BestiaryAndCompanion");
            tab.transform.SetParent(parent.transform, false);
            RectTransform rt = tab.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // ================= 1. SUB-HEADER CHUYỂN ĐỔI: TRỢ THỦ ĐỒNG HÀNH & SỔ TAY QUÁI =================
            GameObject subNav = new GameObject("SubNavBar");
            subNav.transform.SetParent(tab.transform, false);
            RectTransform snRt = subNav.AddComponent<RectTransform>();
            snRt.anchorMin = new Vector2(0.5f, 1f); snRt.anchorMax = new Vector2(0.5f, 1f);
            snRt.pivot = new Vector2(0.5f, 1f);
            snRt.anchoredPosition = new Vector2(0, -10);
            snRt.sizeDelta = new Vector2(820, 44);

            // Nút 1: Trợ Thủ Đồng Hành
            GameObject btnComp = new GameObject("Btn_Companion");
            btnComp.transform.SetParent(subNav.transform, false);
            RectTransform bcRt = btnComp.AddComponent<RectTransform>();
            bcRt.anchorMin = new Vector2(0, 0); bcRt.anchorMax = new Vector2(0.49f, 1);
            bcRt.offsetMin = Vector2.zero; bcRt.offsetMax = Vector2.zero;
            hub.tabCompanionToggleBg = btnComp.AddComponent<Image>();
            hub.tabCompanionToggleBg.sprite = UISpriteService.CreateRoundedBox(400, 44, 14, new Color(0.18f, 0.55f, 0.95f), new Color(1f, 0.85f, 0.2f), 1);
            hub.tabCompanionToggleBtn = btnComp.AddComponent<Button>();
            CreateTextElement(btnComp, "Txt", Vector2.zero, Vector2.one, font, 14, FontStyle.Bold, Color.white, "🛸 TRỢ THỦ ĐỒNG HÀNH (CARROT DRONE)");

            // Nút 2: Sổ Tay Quái Vật
            GameObject btnBest = new GameObject("Btn_Bestiary");
            btnBest.transform.SetParent(subNav.transform, false);
            RectTransform bbRt = btnBest.AddComponent<RectTransform>();
            bbRt.anchorMin = new Vector2(0.51f, 0); bbRt.anchorMax = new Vector2(1, 1);
            bbRt.offsetMin = Vector2.zero; bbRt.offsetMax = Vector2.zero;
            hub.tabBestiaryToggleBg = btnBest.AddComponent<Image>();
            hub.tabBestiaryToggleBg.sprite = UISpriteService.CreateRoundedBox(400, 44, 14, new Color(0.12f, 0.15f, 0.22f), new Color(0.3f, 0.4f, 0.6f), 1);
            hub.tabBestiaryToggleBtn = btnBest.AddComponent<Button>();
            CreateTextElement(btnBest, "Txt", Vector2.zero, Vector2.one, font, 14, FontStyle.Bold, Color.white, "📖 SỔ TAY QUÁI VẬT (BESTIARY)");

            // ================= 2. SUB-VIEW A: TRỢ THỦ ĐỒNG HÀNH (COMPANION VIEW) =================
            GameObject compView = new GameObject("View_Companion");
            compView.transform.SetParent(tab.transform, false);
            RectTransform cvRt = compView.AddComponent<RectTransform>();
            cvRt.anchorMin = Vector2.zero; cvRt.anchorMax = Vector2.one;
            cvRt.offsetMin = new Vector2(0, 0); cvRt.offsetMax = new Vector2(0, -60);
            hub.companionViewRoot = compView;

            // Khung Bệ Đỡ Carrot Drone
            GameObject droneStage = new GameObject("DroneStage");
            droneStage.transform.SetParent(compView.transform, false);
            RectTransform dsRt = droneStage.AddComponent<RectTransform>();
            dsRt.anchorMin = new Vector2(0.5f, 1f); dsRt.anchorMax = new Vector2(0.5f, 1f);
            dsRt.pivot = new Vector2(0.5f, 1f);
            dsRt.anchoredPosition = new Vector2(0, -10);
            dsRt.sizeDelta = new Vector2(820, 240);

            Image dsBg = droneStage.AddComponent<Image>();
            dsBg.sprite = UISpriteService.CreateRoundedBox(820, 240, 18, new Color(0.08f, 0.12f, 0.18f, 0.95f), new Color(0.15f, 0.5f, 0.85f, 0.7f), 2);
            dsBg.color = Color.white;

            // Bệ phát sáng tròn
            GameObject pRing = new GameObject("GlowPedestal");
            pRing.transform.SetParent(droneStage.transform, false);
            RectTransform prRt = pRing.AddComponent<RectTransform>();
            prRt.anchorMin = new Vector2(0.5f, 0.5f); prRt.anchorMax = new Vector2(0.5f, 0.5f);
            prRt.pivot = new Vector2(0.5f, 0.5f);
            prRt.anchoredPosition = new Vector2(0, -15);
            prRt.sizeDelta = new Vector2(240, 50);
            Image prImg = pRing.AddComponent<Image>();
            prImg.sprite = UISpriteService.CreateRoundedBox(240, 50, 25, new Color(0.1f, 0.45f, 0.85f, 0.4f), new Color(0.2f, 0.85f, 1f, 0.9f), 2);

            // Hình ảnh Drone Trợ thủ (Art)
            GameObject droneArt = new GameObject("DroneArt");
            droneArt.transform.SetParent(droneStage.transform, false);
            RectTransform daRt = droneArt.AddComponent<RectTransform>();
            daRt.anchorMin = new Vector2(0.5f, 0.5f); daRt.anchorMax = new Vector2(0.5f, 0.5f);
            daRt.pivot = new Vector2(0.5f, 0.5f);
            daRt.anchoredPosition = new Vector2(0, 30);
            daRt.sizeDelta = new Vector2(150, 150);
            Image daImg = droneArt.AddComponent<Image>();
            daImg.sprite = UISpriteService.GetPetDroneSprite();
            daImg.preserveAspect = true;

            // Huy hiệu tên Drone
            CreateTextElement(droneStage, "DroneName", new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.5f), font, 18, FontStyle.Bold, new Color(1f, 0.85f, 0.25f), "🛸 CARROT DRONE MK-I")
                .rectTransform.anchoredPosition = new Vector2(0, -50);

            // Level & CP Pill
            hub.companionLevelText = CreateTextElement(droneStage, "LvlPill", new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.5f), font, 13, FontStyle.Bold, new Color(0.7f, 0.95f, 1f), "CẤP ĐỘ 5  (EXP: 240/1000)");
            hub.companionLevelText.rectTransform.anchoredPosition = new Vector2(0, -74);

            hub.companionCpText = CreateTextElement(droneStage, "CpPill", new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.5f), font, 13, FontStyle.Bold, new Color(1f, 0.9f, 0.3f), "⚡ LỰC CHIẾN PET: 680 CP");
            hub.companionCpText.rectTransform.anchoredPosition = new Vector2(0, -96);

            // Thẻ Thông Số Chiến Đấu & Kỹ Năng Trợ Thủ
            GameObject statsCard = new GameObject("CompanionStatsCard");
            statsCard.transform.SetParent(compView.transform, false);
            RectTransform scRt = statsCard.AddComponent<RectTransform>();
            scRt.anchorMin = new Vector2(0.5f, 1f); scRt.anchorMax = new Vector2(0.5f, 1f);
            scRt.pivot = new Vector2(0.5f, 1f);
            scRt.anchoredPosition = new Vector2(0, -260);
            scRt.sizeDelta = new Vector2(820, 175);

            Image scBg = statsCard.AddComponent<Image>();
            scBg.sprite = UISpriteService.CreateRoundedBox(820, 175, 16, new Color(0.09f, 0.12f, 0.19f, 0.95f), new Color(0.22f, 0.35f, 0.55f), 1);
            scBg.color = Color.white;

            CreateTextElement(statsCard, "Header", new Vector2(0.04f, 0.8f), new Vector2(0.96f, 0.96f), font, 14, FontStyle.Bold, new Color(1f, 0.85f, 0.2f), "🎯 CHỈ SỐ HỖ TRỢ CHIẾN ĐẤU:");

            hub.companionStatsText = CreateTextElement(statsCard, "StatsBody", new Vector2(0.04f, 0.28f), new Vector2(0.96f, 0.8f), font, 13, FontStyle.Normal, Color.white, "Đang tải chỉ số...");
            hub.companionStatsText.alignment = TextAnchor.UpperLeft;
            hub.companionStatsText.lineSpacing = 1.3f;

            // Dòng Nội Tại (Perks)
            Text perksText = CreateTextElement(statsCard, "Perks", new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.28f), font, 12, FontStyle.Bold, new Color(0.35f, 0.95f, 0.55f), "🥕 +15% Sát Thương Chủ Nhân   |   🧲 Tự Động Hút Cà Rốt & Vàng (8m)   |   ⚡ -10% Hồi Chiêu");
            perksText.alignment = TextAnchor.MiddleCenter;

            // Thanh Hành Động: Nâng Cấp Trợ Thủ & Trạng Thái
            GameObject actionsBar = new GameObject("CompanionActionsBar");
            actionsBar.transform.SetParent(compView.transform, false);
            RectTransform cabRt = actionsBar.AddComponent<RectTransform>();
            cabRt.anchorMin = new Vector2(0.5f, 0); cabRt.anchorMax = new Vector2(0.5f, 0);
            cabRt.pivot = new Vector2(0.5f, 0);
            cabRt.anchoredPosition = new Vector2(0, 15);
            cabRt.sizeDelta = new Vector2(820, 56);

            // Nút Nâng Cấp Trợ Thủ
            GameObject upBtnObj = new GameObject("UpgradeCompanionBtn");
            upBtnObj.transform.SetParent(actionsBar.transform, false);
            RectTransform ubRt = upBtnObj.AddComponent<RectTransform>();
            ubRt.anchorMin = new Vector2(0, 0); ubRt.anchorMax = new Vector2(0.58f, 1);
            ubRt.offsetMin = Vector2.zero; ubRt.offsetMax = Vector2.zero;

            Image ubBg = upBtnObj.AddComponent<Image>();
            ubBg.sprite = UISpriteService.CreateGradientBox(480, 56, 16, new Color(0.95f, 0.65f, 0.1f), new Color(0.85f, 0.45f, 0.05f), Color.white, 2);
            hub.companionUpgradeButton = upBtnObj.AddComponent<Button>();
            hub.companionUpgradeBtnText = CreateTextElement(upBtnObj, "Txt", Vector2.zero, Vector2.one, font, 14, FontStyle.Bold, Color.white, "🚀 NÂNG CẤP TRỢ THỦ\n(400💰 + 12⚙️)");

            // Nút Trạng Thái Xuất Trận
            GameObject statusObj = new GameObject("ActiveStatusPill");
            statusObj.transform.SetParent(actionsBar.transform, false);
            RectTransform stRt = statusObj.AddComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.62f, 0); stRt.anchorMax = new Vector2(1, 1);
            stRt.offsetMin = Vector2.zero; stRt.offsetMax = Vector2.zero;

            Image stBg = statusObj.AddComponent<Image>();
            stBg.sprite = UISpriteService.CreateRoundedBox(300, 56, 16, new Color(0.12f, 0.45f, 0.25f, 0.95f), new Color(0.3f, 0.9f, 0.45f), 1);
            CreateTextElement(statusObj, "Txt", Vector2.zero, Vector2.one, font, 14, FontStyle.Bold, Color.white, "✅ ĐANG XUẤT TRẬN\n(ACTIVE COMPANION)");

            // ================= 3. SUB-VIEW B: SỔ TAY QUÁI VẬT (BESTIARY VIEW) =================
            GameObject bestView = new GameObject("View_Bestiary");
            bestView.transform.SetParent(tab.transform, false);
            RectTransform bvRt = bestView.AddComponent<RectTransform>();
            bvRt.anchorMin = Vector2.zero; bvRt.anchorMax = Vector2.one;
            bvRt.offsetMin = new Vector2(0, 0); bvRt.offsetMax = new Vector2(0, -60);
            hub.bestiaryViewRoot = bestView;
            bestView.SetActive(false);

            GameObject scrollObj = new GameObject("BestiaryScrollView");
            scrollObj.transform.SetParent(bestView.transform, false);
            RectTransform scrt = scrollObj.AddComponent<RectTransform>();
            scrt.anchorMin = new Vector2(0.5f, 0.5f); scrt.anchorMax = new Vector2(0.5f, 0.5f);
            scrt.pivot = new Vector2(0.5f, 0.5f);
            scrt.anchoredPosition = new Vector2(0, 0);
            scrt.sizeDelta = new Vector2(820, 500);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false; sr.vertical = true;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            RectTransform vprt = viewport.AddComponent<RectTransform>();
            vprt.anchorMin = Vector2.zero; vprt.anchorMax = Vector2.one;
            vprt.offsetMin = Vector2.zero; vprt.offsetMax = Vector2.zero;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>().color = Color.white;
            sr.viewport = vprt;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform crt = content.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.sizeDelta = new Vector2(0, 500);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = crt;
            hub.bestiaryListTransform = content.transform;

            return tab;
        }

        private static GameObject BuildProfileTab(GameObject parent, Font font, HomeHubUI hub)
        {
            GameObject tab = new GameObject("Tab_Profile");
            tab.transform.SetParent(parent.transform, false);
            RectTransform rt = tab.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Top Card: Info & XP Bar
            GameObject infoCard = new GameObject("InfoCard");
            infoCard.transform.SetParent(tab.transform, false);
            RectTransform icrt = infoCard.AddComponent<RectTransform>();
            icrt.anchorMin = new Vector2(0.5f, 1); icrt.anchorMax = new Vector2(0.5f, 1);
            icrt.pivot = new Vector2(0.5f, 1);
            icrt.anchoredPosition = new Vector2(0, -15);
            icrt.sizeDelta = new Vector2(780, 200);

            Image icBg = infoCard.AddComponent<Image>();
            icBg.color = new Color(0.12f, 0.15f, 0.22f, 0.95f);

            hub.profileRabbitLevel = CreateTextElement(infoCard, "Lvl", new Vector2(0.04f, 0.75f), new Vector2(0.96f, 0.95f), font, 18, FontStyle.Bold, new Color(1f, 0.85f, 0.2f), "CẤP ĐỘ CHIẾN BINH: 1");

            // XP Bar BG & Fill
            GameObject xpBgObj = new GameObject("XpBg");
            xpBgObj.transform.SetParent(infoCard.transform, false);
            RectTransform xprt = xpBgObj.AddComponent<RectTransform>();
            xprt.anchorMin = new Vector2(0.04f, 0.60f); xprt.anchorMax = new Vector2(0.96f, 0.72f);
            xprt.offsetMin = Vector2.zero; xprt.offsetMax = Vector2.zero;
            Image xpBg = xpBgObj.AddComponent<Image>();
            xpBg.color = new Color(0.06f, 0.08f, 0.12f);

            GameObject xpFillObj = new GameObject("XpFill");
            xpFillObj.transform.SetParent(xpBgObj.transform, false);
            RectTransform xpfrt = xpFillObj.AddComponent<RectTransform>();
            xpfrt.anchorMin = Vector2.zero; xpfrt.anchorMax = Vector2.one;
            xpfrt.offsetMin = Vector2.zero; xpfrt.offsetMax = Vector2.zero;
            hub.profileXpBarFill = xpFillObj.AddComponent<Image>();
            hub.profileXpBarFill.color = new Color(0.2f, 0.75f, 0.95f);
            hub.profileXpBarFill.type = Image.Type.Filled;
            hub.profileXpBarFill.fillMethod = Image.FillMethod.Horizontal;

            hub.profileXpProgress = CreateTextElement(infoCard, "XpText", new Vector2(0.04f, 0.60f), new Vector2(0.96f, 0.72f), font, 12, FontStyle.Bold, Color.white, "XP: 0 / 100");

            hub.profileLifetimeStats = CreateTextElement(infoCard, "Stats", new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.55f), font, 14, FontStyle.Normal, new Color(0.85f, 0.9f, 1f), "Thống kê tài khoản...");

            // Bottom Area: Achievements List
            GameObject achScroll = new GameObject("AchievementsScroll");
            achScroll.transform.SetParent(tab.transform, false);
            RectTransform asrt = achScroll.AddComponent<RectTransform>();
            asrt.anchorMin = new Vector2(0.5f, 0); asrt.anchorMax = new Vector2(0.5f, 1);
            asrt.pivot = new Vector2(0.5f, 0.5f);
            asrt.anchoredPosition = new Vector2(0, -130);
            asrt.sizeDelta = new Vector2(780, 360);

            ScrollRect sr = achScroll.AddComponent<ScrollRect>();
            sr.horizontal = false; sr.vertical = true;

            GameObject vp = new GameObject("Viewport");
            vp.transform.SetParent(achScroll.transform, false);
            RectTransform vprt = vp.AddComponent<RectTransform>();
            vprt.anchorMin = Vector2.zero; vprt.anchorMax = Vector2.one;
            vprt.offsetMin = Vector2.zero; vprt.offsetMax = Vector2.zero;
            vp.AddComponent<Mask>().showMaskGraphic = false;
            vp.AddComponent<Image>().color = Color.white;
            sr.viewport = vprt;

            GameObject cnt = new GameObject("Content");
            cnt.transform.SetParent(vp.transform, false);
            RectTransform crt = cnt.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.sizeDelta = new Vector2(0, 400);

            VerticalLayoutGroup vlg = cnt.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = cnt.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = crt;
            hub.achievementsListTransform = cnt.transform;

            return tab;
        }

        private static GameObject BuildItemDetailModal(GameObject parent, Font font, HomeHubUI hub)
        {
            GameObject overlay = new GameObject("ItemDetail_Modal");
            overlay.transform.SetParent(parent.transform, false);
            RectTransform ort = overlay.AddComponent<RectTransform>();
            ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one;
            ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;

            Image oBg = overlay.AddComponent<Image>();
            oBg.color = new Color(0, 0, 0, 0.85f);

            GameObject card = new GameObject("Card");
            card.transform.SetParent(overlay.transform, false);
            RectTransform crt = card.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0.5f); crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(580, 500);

            Image cBg = card.AddComponent<Image>();
            cBg.sprite = UISpriteService.CreateRoundedBox(580, 500, 24, new Color(0.10f, 0.13f, 0.21f, 0.98f), new Color(0.24f, 0.32f, 0.48f), 3);
            cBg.color = Color.white;

            hub.detailItemName = CreateTextElement(card, "Name", new Vector2(0, 0.86f), new Vector2(1, 0.98f), font, 24, FontStyle.Bold, new Color(1f, 0.88f, 0.3f), "Tên Vật Phẩm");
            hub.detailItemRarity = CreateTextElement(card, "Rarity", new Vector2(0, 0.76f), new Vector2(1, 0.87f), font, 15, FontStyle.Bold, new Color(0.7f, 0.4f, 1f), "Độ Hiếm • Slot");
            hub.detailItemStats = CreateTextElement(card, "Stats", new Vector2(0.08f, 0.44f), new Vector2(0.92f, 0.74f), font, 16, FontStyle.Normal, Color.white, "Chỉ số thưởng...");
            hub.detailItemDesc = CreateTextElement(card, "Desc", new Vector2(0.08f, 0.26f), new Vector2(0.92f, 0.44f), font, 13, FontStyle.Italic, new Color(0.8f, 0.85f, 0.95f), "Mô tả...");

            // Buttons: Equip, Upgrade, Salvage, Close
            // Equip Button (Cyan Gradient)
            GameObject eqBtn = new GameObject("EquipBtn");
            eqBtn.transform.SetParent(card.transform, false);
            RectTransform eqrt = eqBtn.AddComponent<RectTransform>();
            eqrt.anchorMin = new Vector2(0.06f, 0.12f); eqrt.anchorMax = new Vector2(0.34f, 0.24f);
            eqrt.offsetMin = Vector2.zero; eqrt.offsetMax = Vector2.zero;
            Image eqImg = eqBtn.AddComponent<Image>();
            eqImg.sprite = UISpriteService.GetActionButtonSprite("equip", 160, 48);
            eqImg.color = Color.white;
            hub.detailEquipButton = eqBtn.AddComponent<Button>();
            hub.detailEquipButtonText = CreateTextElement(eqBtn, "Text", Vector2.zero, Vector2.one, font, 15, FontStyle.Bold, Color.white, "TRANG BỊ");

            // Upgrade Button (Gold Gradient)
            GameObject upBtn = new GameObject("UpgradeBtn");
            upBtn.transform.SetParent(card.transform, false);
            RectTransform uprt = upBtn.AddComponent<RectTransform>();
            uprt.anchorMin = new Vector2(0.36f, 0.12f); uprt.anchorMax = new Vector2(0.64f, 0.24f);
            uprt.offsetMin = Vector2.zero; uprt.offsetMax = Vector2.zero;
            Image upImg = upBtn.AddComponent<Image>();
            upImg.sprite = UISpriteService.GetActionButtonSprite("upgrade", 160, 48);
            upImg.color = Color.white;
            hub.detailUpgradeButton = upBtn.AddComponent<Button>();
            hub.detailUpgradeButtonText = CreateTextElement(upBtn, "Text", Vector2.zero, Vector2.one, font, 13, FontStyle.Bold, Color.white, "NÂNG CẤP");

            // Salvage Button (Grey Gradient)
            GameObject svBtn = new GameObject("SalvageBtn");
            svBtn.transform.SetParent(card.transform, false);
            RectTransform svrt = svBtn.AddComponent<RectTransform>();
            svrt.anchorMin = new Vector2(0.66f, 0.12f); svrt.anchorMax = new Vector2(0.94f, 0.24f);
            svrt.offsetMin = Vector2.zero; svrt.offsetMax = Vector2.zero;
            Image svImg = svBtn.AddComponent<Image>();
            svImg.sprite = UISpriteService.GetActionButtonSprite("salvage", 160, 48);
            svImg.color = Color.white;
            hub.detailSalvageButton = svBtn.AddComponent<Button>();
            CreateTextElement(svBtn, "Text", Vector2.zero, Vector2.one, font, 14, FontStyle.Bold, Color.white, "PHÂN GIẢI 🔨");

            // Close Button
            GameObject clBtn = new GameObject("CloseBtn");
            clBtn.transform.SetParent(card.transform, false);
            RectTransform clrt = clBtn.AddComponent<RectTransform>();
            clrt.anchorMin = new Vector2(0.32f, 0.02f); clrt.anchorMax = new Vector2(0.68f, 0.10f);
            clrt.offsetMin = Vector2.zero; clrt.offsetMax = Vector2.zero;
            Image clImg = clBtn.AddComponent<Image>();
            clImg.sprite = UISpriteService.CreateRoundedBox(180, 40, 12, new Color(0.2f, 0.24f, 0.35f), Color.clear, 0);
            clImg.color = Color.white;
            hub.detailCloseButton = clBtn.AddComponent<Button>();
            CreateTextElement(clBtn, "Text", Vector2.zero, Vector2.one, font, 14, FontStyle.Bold, Color.white, "ĐÓNG");

            overlay.SetActive(false);
            return overlay;
        }

        private static GameObject BuildChestRewardPopup(GameObject parent, Font font, HomeHubUI hub)
        {
            GameObject overlay = new GameObject("ChestReward_Popup");
            overlay.transform.SetParent(parent.transform, false);
            RectTransform ort = overlay.AddComponent<RectTransform>();
            ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one;
            ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;

            Image oBg = overlay.AddComponent<Image>();
            oBg.color = new Color(0, 0, 0, 0.90f);

            GameObject card = new GameObject("Card");
            card.transform.SetParent(overlay.transform, false);
            RectTransform crt = card.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0.5f); crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(620, 480);

            Image cBg = card.AddComponent<Image>();
            cBg.sprite = UISpriteService.CreateRoundedBox(620, 480, 24, new Color(0.12f, 0.15f, 0.24f, 0.98f), new Color(0.85f, 0.4f, 1f), 3);
            cBg.color = Color.white;

            hub.rewardChestTitle = CreateTextElement(card, "Title", new Vector2(0, 0.86f), new Vector2(1, 0.98f), font, 24, FontStyle.Bold, new Color(1f, 0.88f, 0.3f), "🎉 YOU GOT! 🎁");

            // Hình Ảnh Rương Mở Nắp Hoàng Gia (Open Royal Purple Chest)
            GameObject chestImgObj = new GameObject("ChestOpenImage");
            chestImgObj.transform.SetParent(card.transform, false);
            RectTransform cirt = chestImgObj.AddComponent<RectTransform>();
            cirt.anchorMin = new Vector2(0.5f, 0.5f); cirt.anchorMax = new Vector2(0.5f, 0.5f);
            cirt.pivot = new Vector2(0.5f, 0.5f);
            cirt.anchoredPosition = new Vector2(-150, 15);
            cirt.sizeDelta = new Vector2(180, 180);
            Image cimg = chestImgObj.AddComponent<Image>();
            cimg.sprite = UISpriteService.GetChestPurpleSprite();
            cimg.color = Color.white;

            // Khung Chi Tiết Vật Phẩm Bên Phải Rương
            GameObject rwdDetails = new GameObject("Reward_Details_Panel");
            rwdDetails.transform.SetParent(card.transform, false);
            RectTransform rdrt = rwdDetails.AddComponent<RectTransform>();
            rdrt.anchorMin = new Vector2(0.5f, 0.22f); rdrt.anchorMax = new Vector2(0.95f, 0.84f);
            rdrt.offsetMin = Vector2.zero; rdrt.offsetMax = Vector2.zero;

            Image rdBg = rwdDetails.AddComponent<Image>();
            rdBg.sprite = UISpriteService.CreateRoundedBox(270, 240, 16, new Color(0.08f, 0.10f, 0.16f, 0.95f), new Color(0.35f, 0.42f, 0.65f), 2);
            rdBg.color = Color.white;

            hub.rewardItemName = CreateTextElement(rwdDetails, "Name", new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.95f), font, 18, FontStyle.Bold, Color.yellow, "Carrot Hammer");
            hub.rewardItemRarity = CreateTextElement(rwdDetails, "Rarity", new Vector2(0.05f, 0.48f), new Vector2(0.95f, 0.70f), font, 14, FontStyle.Bold, new Color(0.75f, 0.35f, 1f), "Sử Thi (Epic)");
            hub.rewardItemStats = CreateTextElement(rwdDetails, "Stats", new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.48f), font, 14, FontStyle.Normal, Color.white, "⚔️ +50 Sát Thương");
            hub.rewardExtraText = CreateTextElement(rwdDetails, "Extra", new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.25f), font, 13, FontStyle.Normal, new Color(0.3f, 0.95f, 0.6f), "+800 💰 | +50 💎");

            GameObject btn = new GameObject("ConfirmBtn");
            btn.transform.SetParent(card.transform, false);
            RectTransform brt = btn.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0); brt.anchorMax = new Vector2(0.5f, 0);
            brt.pivot = new Vector2(0.5f, 0);
            brt.anchoredPosition = new Vector2(0, 20);
            brt.sizeDelta = new Vector2(240, 52);

            Image bImg = btn.AddComponent<Image>();
            bImg.sprite = UISpriteService.GetActionButtonSprite("confirm", 240, 52);
            bImg.color = Color.white;
            hub.rewardConfirmButton = btn.AddComponent<Button>();
            CreateTextElement(btn, "Text", Vector2.zero, Vector2.one, font, 18, FontStyle.Bold, Color.white, "CONFIRM");

            overlay.SetActive(false);
            return overlay;
        }

        private static GameObject BuildToastNotification(GameObject parent, Font font, out Text outText)
        {
            GameObject toast = new GameObject("Toast_Notice");
            toast.transform.SetParent(parent.transform, false);
            RectTransform trt = toast.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 0.5f); trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.anchoredPosition = new Vector2(0, -180);
            trt.sizeDelta = new Vector2(620, 60);

            Image tBg = toast.AddComponent<Image>();
            tBg.color = new Color(0.08f, 0.10f, 0.16f, 0.95f);

            outText = CreateTextElement(toast, "Text", Vector2.zero, Vector2.one, font, 16, FontStyle.Bold, new Color(1f, 0.9f, 0.2f), "Thông báo");
            toast.SetActive(false);
            return toast;
        }

        private static Text CreateTextElement(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Font font, int size, FontStyle style, Color color, string content)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Text txt = obj.AddComponent<Text>();
            txt.font = font ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = content;

            return txt;
        }
    }
}
