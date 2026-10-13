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
        [SerializeField] private Transform inventoryGridTransform;
        [SerializeField] private Button autoFuseButton;
        [SerializeField] private Button autoSalvageCommonButton;

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

        [Header("Bestiary Tab Specific")]
        [SerializeField] private Transform bestiaryListTransform;

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

            // 1. Cập nhật 8 ô trang bị
            for (int i = 0; i < 8; i++)
            {
                EquipmentSlotType slot = (EquipmentSlotType)i;
                EquipmentItem equipped = acc.GetEquippedItem(slot);

                if (equippedSlotTexts[i] != null)
                {
                    if (equipped != null)
                    {
                        equippedSlotTexts[i].text = $"{GetSlotShortName(slot)}\n<b>{equipped.DisplayName}</b>\n<color=#FFD700>Lv.{equipped.Level}</color>";
                    }
                    else
                    {
                        equippedSlotTexts[i].text = $"{GetSlotShortName(slot)}\n<color=#888888>(Trống)</color>";
                    }
                }

                if (equippedSlotBgs[i] != null)
                {
                    equippedSlotBgs[i].color = equipped != null ? equipped.GetRarityColor() * 0.45f + Color.black * 0.55f : new Color(0.12f, 0.15f, 0.22f);
                }

                int slotIdx = i;
                if (equippedSlotButtons[i] != null)
                {
                    equippedSlotButtons[i].onClick.RemoveAllListeners();
                    equippedSlotButtons[i].onClick.AddListener(() =>
                    {
                        if (equipped != null) ShowItemDetail(equipped, isEquipped: true);
                    });
                }
            }

            // 2. Cập nhật Kho Đồ (Inventory Grid)
            if (inventoryGridTransform != null)
            {
                // Clear old items
                for (int i = inventoryGridTransform.childCount - 1; i >= 0; i--)
                {
                    Destroy(inventoryGridTransform.GetChild(i).gameObject);
                }

                if (acc.OwnedEquipments != null)
                {
                    foreach (var item in acc.OwnedEquipments)
                    {
                        CreateInventoryItemCard(item);
                    }
                }
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
            rt.sizeDelta = new Vector2(160, 110);

            Image bg = cardObj.AddComponent<Image>();
            bg.color = item.GetRarityColor() * 0.4f + new Color(0.1f, 0.12f, 0.18f, 0.95f);

            Button btn = cardObj.AddComponent<Button>();
            btn.onClick.AddListener(() => ShowItemDetail(item, isEquipped: false));

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(cardObj.transform, false);
            RectTransform trt = txtObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(6, 6);
            trt.offsetMax = new Vector2(-6, -6);

            Text txt = txtObj.AddComponent<Text>();
            txt.font = uiFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 13;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = $"<b>{item.DisplayName}</b>\n<color=#{ColorUtility.ToHtmlStringRGB(item.GetRarityColor())}>{item.GetRarityName()}</color>\nLv.{item.Level}";
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

        // ========================== TAB 4: SỔ TAY QUÁI ==========================
        private void RefreshBestiaryTab()
        {
            var acc = AccountProgression.Instance?.Data;
            if (acc == null || bestiaryListTransform == null) return;

            // Clear old entries
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

            // 1. Top Bar (H 70, Top)
            GameObject topBar = new GameObject("TopBar");
            topBar.transform.SetParent(root.transform, false);
            RectTransform tbRt = topBar.AddComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0, 1);
            tbRt.anchorMax = new Vector2(1, 1);
            tbRt.pivot = new Vector2(0.5f, 1);
            tbRt.anchoredPosition = Vector2.zero;
            tbRt.sizeDelta = new Vector2(0, 70);

            Image tbBg = topBar.AddComponent<Image>();
            tbBg.color = new Color(0.10f, 0.12f, 0.18f, 0.98f);

            // Top Bar Elements: Profile Level, Energy, Gold, Gems, Scrap
            hub.profileLevelText = CreateTextElement(topBar, "LvlText", new Vector2(0, 0), new Vector2(0.2f, 1), defaultFont, 16, FontStyle.Bold, new Color(1f, 0.85f, 0.2f), "CẤP 1 (Thỏ)");
            hub.energyText = CreateTextElement(topBar, "EnergyText", new Vector2(0.22f, 0), new Vector2(0.4f, 1), defaultFont, 16, FontStyle.Bold, new Color(0.3f, 0.85f, 1f), "⚡ 20/20");
            hub.goldText = CreateTextElement(topBar, "GoldText", new Vector2(0.42f, 0), new Vector2(0.6f, 1), defaultFont, 16, FontStyle.Bold, new Color(1f, 0.8f, 0.1f), "💰 150");
            hub.gemsText = CreateTextElement(topBar, "GemsText", new Vector2(0.62f, 0), new Vector2(0.8f, 1), defaultFont, 16, FontStyle.Bold, new Color(0.3f, 1f, 0.5f), "💎 50");
            hub.scrapText = CreateTextElement(topBar, "ScrapText", new Vector2(0.82f, 0), new Vector2(1.0f, 1), defaultFont, 16, FontStyle.Bold, new Color(0.9f, 0.6f, 1f), "⚙️ 20");

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

            // Banner Chương 1
            GameObject banner = new GameObject("ChapterBanner");
            banner.transform.SetParent(tab.transform, false);
            RectTransform brt = banner.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 1f); brt.anchorMax = new Vector2(0.5f, 1f);
            brt.pivot = new Vector2(0.5f, 1f);
            brt.anchoredPosition = new Vector2(0, -25);
            brt.sizeDelta = new Vector2(800, 150);

            Image bBg = banner.AddComponent<Image>();
            bBg.color = new Color(0.14f, 0.18f, 0.28f, 0.95f);

            CreateTextElement(banner, "Title", new Vector2(0, 0.5f), new Vector2(1, 1), font, 24, FontStyle.Bold, new Color(1f, 0.85f, 0.2f), "🌿 CHƯƠNG 1: KHU VƯỜN ĐỘT BIẾN");
            CreateTextElement(banner, "Sub", new Vector2(0, 0), new Vector2(1, 0.55f), font, 15, FontStyle.Normal, new Color(0.85f, 0.9f, 1f), "Màn 1-1: Căn Cứ Cà Rốt • Thử thách 4 Đợt Quái & Trùm King Carrot\nTiêu diệt toàn bộ để mở khóa Rương Chúa Tể!");

            // Stats Card
            GameObject statsCard = new GameObject("StatsCard");
            statsCard.transform.SetParent(tab.transform, false);
            RectTransform srt = statsCard.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f); srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0, 30);
            srt.sizeDelta = new Vector2(800, 220);

            Image sBg = statsCard.AddComponent<Image>();
            sBg.color = new Color(0.10f, 0.13f, 0.20f, 0.95f);

            CreateTextElement(statsCard, "Header", new Vector2(0, 0.72f), new Vector2(1, 1), font, 18, FontStyle.Bold, new Color(0.3f, 0.95f, 0.6f), "🛡️ THỐNG KÊ CHIẾN LỰC THỎ CHIẾN BINH");
            hub.heroAtkText = CreateTextElement(statsCard, "Atk", new Vector2(0.08f, 0.45f), new Vector2(0.92f, 0.72f), font, 16, FontStyle.Bold, Color.white, "⚔️ TỔNG SÁT THƯƠNG (ATK): 35");
            hub.heroHpText = CreateTextElement(statsCard, "Hp", new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.48f), font, 16, FontStyle.Bold, Color.white, "❤️ TỔNG MÁU TỐI ĐA (HP): 100");
            hub.heroRangedText = CreateTextElement(statsCard, "Ranged", new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.26f), font, 15, FontStyle.Normal, new Color(1f, 0.8f, 0.4f), "🏹 VŨ KHÍ TẦM XA: Súng Cà Chua (Lv.1)");

            // Start Battle CTA Button
            GameObject startBtnObj = new GameObject("StartBattle_Button");
            startBtnObj.transform.SetParent(tab.transform, false);
            RectTransform sbrt = startBtnObj.AddComponent<RectTransform>();
            sbrt.anchorMin = new Vector2(0.5f, 0); sbrt.anchorMax = new Vector2(0.5f, 0);
            sbrt.pivot = new Vector2(0.5f, 0);
            sbrt.anchoredPosition = new Vector2(0, 35);
            sbrt.sizeDelta = new Vector2(480, 68);

            Image sbBg = startBtnObj.AddComponent<Image>();
            sbBg.color = new Color(0.18f, 0.75f, 0.32f);
            hub.startRunButton = startBtnObj.AddComponent<Button>();

            CreateTextElement(startBtnObj, "Text", Vector2.zero, Vector2.one, font, 22, FontStyle.Bold, Color.white, "▶ BẮT ĐẦU VƯỢT ẢI (5 ⚡) ◀");

            return tab;
        }

        private static GameObject BuildEquipmentTab(GameObject parent, Font font, HomeHubUI hub)
        {
            GameObject tab = new GameObject("Tab_Equipment");
            tab.transform.SetParent(parent.transform, false);
            RectTransform rt = tab.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Top Area: 8 Equipped Slots (2 hàng 4 cột)
            GameObject slotsPanel = new GameObject("EquippedSlotsPanel");
            slotsPanel.transform.SetParent(tab.transform, false);
            RectTransform sprt = slotsPanel.AddComponent<RectTransform>();
            sprt.anchorMin = new Vector2(0.5f, 1); sprt.anchorMax = new Vector2(0.5f, 1);
            sprt.pivot = new Vector2(0.5f, 1);
            sprt.anchoredPosition = new Vector2(0, -15);
            sprt.sizeDelta = new Vector2(800, 210);

            Image spBg = slotsPanel.AddComponent<Image>();
            spBg.color = new Color(0.09f, 0.12f, 0.18f, 0.95f);

            CreateTextElement(slotsPanel, "Title", new Vector2(0, 0.82f), new Vector2(1, 1), font, 16, FontStyle.Bold, new Color(1f, 0.85f, 0.2f), "👑 TRANG BỊ ĐANG MẶC (8 Ô CHUẨN THỎ CHIẾN BINH)");

            float slotW = 180; float slotH = 72;
            for (int i = 0; i < 8; i++)
            {
                int row = i / 4;
                int col = i % 4;

                GameObject slotObj = new GameObject($"Slot_{i}");
                slotObj.transform.SetParent(slotsPanel.transform, false);
                RectTransform srt = slotObj.AddComponent<RectTransform>();
                srt.anchorMin = new Vector2(0, 1); srt.anchorMax = new Vector2(0, 1);
                srt.pivot = new Vector2(0, 1);
                srt.anchoredPosition = new Vector2(18 + col * (slotW + 15), -45 - row * (slotH + 12));
                srt.sizeDelta = new Vector2(slotW, slotH);

                hub.equippedSlotBgs[i] = slotObj.AddComponent<Image>();
                hub.equippedSlotBgs[i].color = new Color(0.14f, 0.18f, 0.26f);
                hub.equippedSlotButtons[i] = slotObj.AddComponent<Button>();
                hub.equippedSlotTexts[i] = CreateTextElement(slotObj, "Text", Vector2.zero, Vector2.one, font, 12, FontStyle.Normal, Color.white, $"Slot {i}");
            }

            // Quick Actions Bar (Auto Fuse, Salvage Common)
            GameObject actionsBar = new GameObject("ActionsBar");
            actionsBar.transform.SetParent(tab.transform, false);
            RectTransform abrt = actionsBar.AddComponent<RectTransform>();
            abrt.anchorMin = new Vector2(0.5f, 1); abrt.anchorMax = new Vector2(0.5f, 1);
            abrt.pivot = new Vector2(0.5f, 1);
            abrt.anchoredPosition = new Vector2(0, -235);
            abrt.sizeDelta = new Vector2(800, 48);

            GameObject fuseBtnObj = new GameObject("AutoFuseBtn");
            fuseBtnObj.transform.SetParent(actionsBar.transform, false);
            RectTransform fbrt = fuseBtnObj.AddComponent<RectTransform>();
            fbrt.anchorMin = new Vector2(0, 0); fbrt.anchorMax = new Vector2(0.48f, 1);
            fbrt.offsetMin = Vector2.zero; fbrt.offsetMax = Vector2.zero;
            Image fbBg = fuseBtnObj.AddComponent<Image>();
            fbBg.color = new Color(0.18f, 0.55f, 0.85f);
            hub.autoFuseButton = fuseBtnObj.AddComponent<Button>();
            CreateTextElement(fuseBtnObj, "Text", Vector2.zero, Vector2.one, font, 15, FontStyle.Bold, Color.white, "🧬 GHÉP ĐỒ TỰ ĐỘNG (FUSE)");

            GameObject salvBtnObj = new GameObject("SalvageBtn");
            salvBtnObj.transform.SetParent(actionsBar.transform, false);
            RectTransform sbrt = salvBtnObj.AddComponent<RectTransform>();
            sbrt.anchorMin = new Vector2(0.52f, 0); sbrt.anchorMax = new Vector2(1, 1);
            sbrt.offsetMin = Vector2.zero; sbrt.offsetMax = Vector2.zero;
            Image sbBg = salvBtnObj.AddComponent<Image>();
            sbBg.color = new Color(0.85f, 0.35f, 0.18f);
            hub.autoSalvageCommonButton = salvBtnObj.AddComponent<Button>();
            CreateTextElement(salvBtnObj, "Text", Vector2.zero, Vector2.one, font, 15, FontStyle.Bold, Color.white, "🔨 PHÂN GIẢI ĐỒ THƯỜNG (SALVAGE ⚙️)");

            // Bottom Area: Scrollable Inventory Grid
            GameObject scrollObj = new GameObject("InventoryScrollView");
            scrollObj.transform.SetParent(tab.transform, false);
            RectTransform scrt = scrollObj.AddComponent<RectTransform>();
            scrt.anchorMin = new Vector2(0.5f, 0); scrt.anchorMax = new Vector2(0.5f, 1);
            scrt.pivot = new Vector2(0.5f, 0.5f);
            scrt.anchoredPosition = new Vector2(0, -145);
            scrt.sizeDelta = new Vector2(800, 400);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;

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
            glg.cellSize = new Vector2(178, 105);
            glg.spacing = new Vector2(16, 16);
            glg.padding = new RectOffset(16, 16, 16, 16);
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
                cBg.color = cardColors[i];

                CreateTextElement(card, "Title", new Vector2(0, 0.72f), new Vector2(1, 0.96f), font, 16, FontStyle.Bold, Color.yellow, chestTitles[i]);
                CreateTextElement(card, "Odds", new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.70f), font, 14, FontStyle.Normal, new Color(0.9f, 0.95f, 1f), $"Tỷ lệ rơi đồ:\n{chestOdds[i]}");

                GameObject openBtn = new GameObject("OpenBtn");
                openBtn.transform.SetParent(card.transform, false);
                RectTransform obrt = openBtn.AddComponent<RectTransform>();
                obrt.anchorMin = new Vector2(0.1f, 0.08f); obrt.anchorMax = new Vector2(0.9f, 0.24f);
                obrt.offsetMin = Vector2.zero; obrt.offsetMax = Vector2.zero;

                Image obBg = openBtn.AddComponent<Image>();
                obBg.color = i == 0 ? new Color(0.2f, 0.65f, 0.35f) : (i == 1 ? new Color(0.2f, 0.5f, 0.85f) : new Color(0.75f, 0.2f, 0.85f));
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
            GameObject tab = new GameObject("Tab_Bestiary");
            tab.transform.SetParent(parent.transform, false);
            RectTransform rt = tab.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            CreateTextElement(tab, "Header", new Vector2(0, 0.88f), new Vector2(1, 0.98f), font, 22, FontStyle.Bold, new Color(1f, 0.85f, 0.2f), "📖 SỔ TAY QUÁI VẬT (BESTIARY)");

            GameObject scrollObj = new GameObject("BestiaryScrollView");
            scrollObj.transform.SetParent(tab.transform, false);
            RectTransform scrt = scrollObj.AddComponent<RectTransform>();
            scrt.anchorMin = new Vector2(0.5f, 0.5f); scrt.anchorMax = new Vector2(0.5f, 0.5f);
            scrt.pivot = new Vector2(0.5f, 0.5f);
            scrt.anchoredPosition = new Vector2(0, -30);
            scrt.sizeDelta = new Vector2(780, 480);

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
            crt.sizeDelta = new Vector2(560, 480);

            Image cBg = card.AddComponent<Image>();
            cBg.color = new Color(0.12f, 0.15f, 0.22f, 0.98f);

            hub.detailItemName = CreateTextElement(card, "Name", new Vector2(0, 0.85f), new Vector2(1, 0.98f), font, 22, FontStyle.Bold, Color.yellow, "Tên Vật Phẩm");
            hub.detailItemRarity = CreateTextElement(card, "Rarity", new Vector2(0, 0.74f), new Vector2(1, 0.86f), font, 15, FontStyle.Bold, Color.cyan, "Độ Hiếm • Slot");
            hub.detailItemStats = CreateTextElement(card, "Stats", new Vector2(0.08f, 0.44f), new Vector2(0.92f, 0.73f), font, 16, FontStyle.Normal, Color.white, "Chỉ số thưởng...");
            hub.detailItemDesc = CreateTextElement(card, "Desc", new Vector2(0.08f, 0.26f), new Vector2(0.92f, 0.44f), font, 13, FontStyle.Italic, new Color(0.8f, 0.85f, 0.9f), "Mô tả...");

            // Buttons: Equip, Upgrade, Salvage, Close
            // Equip Button
            GameObject eqBtn = new GameObject("EquipBtn");
            eqBtn.transform.SetParent(card.transform, false);
            RectTransform eqrt = eqBtn.AddComponent<RectTransform>();
            eqrt.anchorMin = new Vector2(0.06f, 0.12f); eqrt.anchorMax = new Vector2(0.34f, 0.24f);
            eqrt.offsetMin = Vector2.zero; eqrt.offsetMax = Vector2.zero;
            eqBtn.AddComponent<Image>().color = new Color(0.18f, 0.65f, 0.35f);
            hub.detailEquipButton = eqBtn.AddComponent<Button>();
            hub.detailEquipButtonText = CreateTextElement(eqBtn, "Text", Vector2.zero, Vector2.one, font, 15, FontStyle.Bold, Color.white, "TRANG BỊ");

            // Upgrade Button
            GameObject upBtn = new GameObject("UpgradeBtn");
            upBtn.transform.SetParent(card.transform, false);
            RectTransform uprt = upBtn.AddComponent<RectTransform>();
            uprt.anchorMin = new Vector2(0.36f, 0.12f); uprt.anchorMax = new Vector2(0.64f, 0.24f);
            uprt.offsetMin = Vector2.zero; uprt.offsetMax = Vector2.zero;
            upBtn.AddComponent<Image>().color = new Color(0.18f, 0.50f, 0.85f);
            hub.detailUpgradeButton = upBtn.AddComponent<Button>();
            hub.detailUpgradeButtonText = CreateTextElement(upBtn, "Text", Vector2.zero, Vector2.one, font, 13, FontStyle.Bold, Color.white, "NÂNG CẤP");

            // Salvage Button
            GameObject svBtn = new GameObject("SalvageBtn");
            svBtn.transform.SetParent(card.transform, false);
            RectTransform svrt = svBtn.AddComponent<RectTransform>();
            svrt.anchorMin = new Vector2(0.66f, 0.12f); svrt.anchorMax = new Vector2(0.94f, 0.24f);
            svrt.offsetMin = Vector2.zero; svrt.offsetMax = Vector2.zero;
            svBtn.AddComponent<Image>().color = new Color(0.85f, 0.32f, 0.18f);
            hub.detailSalvageButton = svBtn.AddComponent<Button>();
            CreateTextElement(svBtn, "Text", Vector2.zero, Vector2.one, font, 14, FontStyle.Bold, Color.white, "PHÂN GIẢI 🔨");

            // Close Button
            GameObject clBtn = new GameObject("CloseBtn");
            clBtn.transform.SetParent(card.transform, false);
            RectTransform clrt = clBtn.AddComponent<RectTransform>();
            clrt.anchorMin = new Vector2(0.32f, 0.02f); clrt.anchorMax = new Vector2(0.68f, 0.10f);
            clrt.offsetMin = Vector2.zero; clrt.offsetMax = Vector2.zero;
            clBtn.AddComponent<Image>().color = new Color(0.35f, 0.38f, 0.45f);
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
            crt.sizeDelta = new Vector2(580, 420);

            Image cBg = card.AddComponent<Image>();
            cBg.color = new Color(0.14f, 0.16f, 0.25f, 0.98f);

            hub.rewardChestTitle = CreateTextElement(card, "Title", new Vector2(0, 0.82f), new Vector2(1, 0.98f), font, 22, FontStyle.Bold, Color.yellow, "👑 RƯƠNG PHẦN THƯỞNG");
            hub.rewardItemName = CreateTextElement(card, "Name", new Vector2(0, 0.65f), new Vector2(1, 0.82f), font, 24, FontStyle.Bold, Color.green, "Kiếm Cà Rốt Hoàng Gia");
            hub.rewardItemRarity = CreateTextElement(card, "Rarity", new Vector2(0, 0.52f), new Vector2(1, 0.65f), font, 16, FontStyle.Bold, Color.cyan, "Sử Thi • Cận Chiến");
            hub.rewardItemStats = CreateTextElement(card, "Stats", new Vector2(0.05f, 0.34f), new Vector2(0.95f, 0.52f), font, 17, FontStyle.Normal, Color.white, "⚔️ +50 Sát Thương");
            hub.rewardExtraText = CreateTextElement(card, "Extra", new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.34f), font, 14, FontStyle.Normal, new Color(1f, 0.85f, 0.4f), "+120 Vàng | +5 Scrap");

            GameObject btn = new GameObject("ConfirmBtn");
            btn.transform.SetParent(card.transform, false);
            RectTransform brt = btn.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.28f, 0.06f); brt.anchorMax = new Vector2(0.72f, 0.18f);
            brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
            btn.AddComponent<Image>().color = new Color(0.18f, 0.75f, 0.35f);
            hub.rewardConfirmButton = btn.AddComponent<Button>();
            CreateTextElement(btn, "Text", Vector2.zero, Vector2.one, font, 18, FontStyle.Bold, Color.white, "TUYỆT VỜI / NHẬN ĐỒ");

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
