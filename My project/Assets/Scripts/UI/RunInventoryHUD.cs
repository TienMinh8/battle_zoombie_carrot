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
                var meleeParent = equippedMeleeText.transform.parent;
                Transform mSpTr = meleeParent != null ? meleeParent.Find("SpriteImg") : null;
                if (meleeParent != null && mSpTr == null)
                {
                    GameObject sObj = new GameObject("SpriteImg");
                    sObj.transform.SetParent(meleeParent, false);
                    RectTransform sRt = sObj.AddComponent<RectTransform>();
                    sRt.anchorMin = new Vector2(0, 0.5f);
                    sRt.anchorMax = new Vector2(0, 0.5f);
                    sRt.pivot = new Vector2(0, 0.5f);
                    sRt.anchoredPosition = new Vector2(6, 0);
                    sRt.sizeDelta = new Vector2(38, 38);
                    Image sImg = sObj.AddComponent<Image>();
                    sImg.preserveAspect = true;
                    mSpTr = sObj.transform;
                }

                Image mImg = mSpTr != null ? mSpTr.GetComponent<Image>() : null;
                WeaponItem wItem = inventory != null ? inventory.EquippedMelee : null;
                var accWpn = AccountProgression.Instance?.Data?.EquippedWeapon;

                if (wItem != null)
                {
                    equippedMeleeText.text = $"[CẬN CHIẾN]\n{wItem.DisplayName} Lv.{wItem.Level}";
                    Sprite sp = WeaponSpriteService.GetWeaponItemSprite(wItem);
                    if (mImg != null && sp != null)
                    {
                        mImg.sprite = sp;
                        mImg.enabled = true;
                    }
                }
                else if (accWpn != null)
                {
                    equippedMeleeText.text = $"[CẬN CHIẾN]\n{accWpn.DisplayName} Lv.{accWpn.Level}";
                    Sprite sp = WeaponSpriteService.GetEquipmentSprite(accWpn);
                    if (mImg != null && sp != null)
                    {
                        mImg.sprite = sp;
                        mImg.enabled = true;
                    }
                }
                else
                {
                    equippedMeleeText.text = "[CẬN CHIẾN]\n(Trống)";
                    if (mImg != null) mImg.enabled = false;
                }
            }

            // 2. Cập nhật Vũ khí Tầm Xa đang dùng
            if (equippedRangedText != null)
            {
                var rangedParent = equippedRangedText.transform.parent;
                Transform rSpTr = rangedParent != null ? rangedParent.Find("SpriteImg") : null;
                if (rangedParent != null && rSpTr == null)
                {
                    GameObject sObj = new GameObject("SpriteImg");
                    sObj.transform.SetParent(rangedParent, false);
                    RectTransform sRt = sObj.AddComponent<RectTransform>();
                    sRt.anchorMin = new Vector2(0, 0.5f);
                    sRt.anchorMax = new Vector2(0, 0.5f);
                    sRt.pivot = new Vector2(0, 0.5f);
                    sRt.anchoredPosition = new Vector2(6, 0);
                    sRt.sizeDelta = new Vector2(38, 38);
                    Image sImg = sObj.AddComponent<Image>();
                    sImg.preserveAspect = true;
                    rSpTr = sObj.transform;
                }

                Image rImg = rSpTr != null ? rSpTr.GetComponent<Image>() : null;
                WeaponItem rItem = inventory != null ? inventory.EquippedRanged : null;

                if (rItem != null)
                {
                    equippedRangedText.text = $"[TẦM XA]\n{rItem.DisplayName} Lv.{rItem.Level}";
                    Sprite sp = WeaponSpriteService.GetWeaponItemSprite(rItem);
                    if (rImg != null && sp != null)
                    {
                        rImg.sprite = sp;
                        rImg.enabled = true;
                    }
                }
                else
                {
                    equippedRangedText.text = "[TẦM XA]\n(Trống)";
                    if (rImg != null) rImg.enabled = false;
                }
            }

            // 3. Cập nhật 4 ô Ba lô dự trữ (Hiển thị vũ khí run HOẶC trang bị vừa nhặt được)
            if (backpackSlotTexts == null) return;

            var backpackWeapons = inventory != null ? inventory.Backpack : null;
            var ownedEquipments = AccountProgression.Instance?.Data?.OwnedEquipments;

            for (int i = 0; i < backpackSlotTexts.Length; i++)
            {
                if (backpackSlotTexts[i] == null) continue;

                Image slotBg = (backpackSlotBgs != null && i < backpackSlotBgs.Length) ? backpackSlotBgs[i] : null;
                Transform spTr = slotBg != null ? slotBg.transform.Find("SpriteImg") : null;
                if (slotBg != null && spTr == null)
                {
                    GameObject sObj = new GameObject("SpriteImg");
                    sObj.transform.SetParent(slotBg.transform, false);
                    RectTransform sRt = sObj.AddComponent<RectTransform>();
                    sRt.anchorMin = new Vector2(0, 0.5f);
                    sRt.anchorMax = new Vector2(0, 0.5f);
                    sRt.pivot = new Vector2(0, 0.5f);
                    sRt.anchoredPosition = new Vector2(6, 0);
                    sRt.sizeDelta = new Vector2(36, 36);
                    Image sImg = sObj.AddComponent<Image>();
                    sImg.preserveAspect = true;
                    spTr = sObj.transform;
                }
                Image slotSprite = spTr != null ? spTr.GetComponent<Image>() : null;

                // Ưu tiên 1: Vũ khí dự trữ trong Run
                if (backpackWeapons != null && i < backpackWeapons.Count)
                {
                    var wpn = backpackWeapons[i];
                    backpackSlotTexts[i].text = $"<b>{wpn.DisplayName}</b>\n<size=10><color=#ffff55>Lv.{wpn.Level}</color></size>";
                    if (slotBg != null)
                    {
                        Color c = wpn.ItemColor;
                        c.a = 0.85f;
                        slotBg.color = c;
                    }

                    Sprite sp = WeaponSpriteService.GetWeaponItemSprite(wpn);
                    if (slotSprite != null && sp != null)
                    {
                        slotSprite.sprite = sp;
                        slotSprite.enabled = true;
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
                        if (slotBg != null)
                        {
                            Color c = eq.GetRarityColor();
                            c.a = 0.85f;
                            slotBg.color = c;
                        }

                        Sprite sp = WeaponSpriteService.GetEquipmentSprite(eq);
                        if (slotSprite != null && sp != null)
                        {
                            slotSprite.sprite = sp;
                            slotSprite.enabled = true;
                        }
                    }
                    else
                    {
                        backpackSlotTexts[i].text = $"Ô {i + 1}\n<size=10><color=#888888>(Trống)</color></size>";
                        if (slotBg != null)
                        {
                            slotBg.color = new Color(0.12f, 0.14f, 0.20f, 0.65f);
                        }
                        if (slotSprite != null) slotSprite.enabled = false;
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
