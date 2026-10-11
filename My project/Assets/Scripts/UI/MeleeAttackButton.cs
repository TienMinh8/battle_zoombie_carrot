using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Combat;
using Apocarrot.Data;

namespace Apocarrot.UI
{
    /// <summary>
    /// Nút đánh cận chiến trên màn hình cảm ứng, hiển thị hồi chiêu và hỗ trợ đổi giữa Blade / Axe để test.
    /// </summary>
    public class MeleeAttackButton : MonoBehaviour
    {
        [Header("Target Combat Component")]
        [SerializeField] private MeleeCombat meleeCombat;

        [Header("UI Elements")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private Text weaponNameText;
        [SerializeField] private Button switchWeaponButton;

        private void Start()
        {
            if (meleeCombat == null)
            {
                meleeCombat = FindFirstObjectByType<MeleeCombat>();
            }

            if (attackButton != null)
            {
                attackButton.onClick.AddListener(HandleAttackClicked);
            }

            if (switchWeaponButton != null)
            {
                switchWeaponButton.onClick.AddListener(HandleSwitchWeaponClicked);
            }

            if (meleeCombat != null)
            {
                meleeCombat.OnWeaponChanged += UpdateWeaponDisplay;
                UpdateWeaponDisplay(meleeCombat.EquippedWeapon);
            }
        }

        private void Update()
        {
            if (meleeCombat == null) return;

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = meleeCombat.CooldownNormalized;
            }

            if (attackButton != null)
            {
                attackButton.interactable = meleeCombat.CanAttack;
            }
        }

        private void HandleAttackClicked()
        {
            if (meleeCombat != null)
            {
                meleeCombat.PerformAttack();
            }
        }

        private void HandleSwitchWeaponClicked()
        {
            if (meleeCombat == null) return;

            // Đổi giữa Carrot Blade (nhanh, dame vừa) và Carrot Axe (chậm, dame mạnh)
            if (meleeCombat.EquippedWeapon.WeaponId == "carrot_blade")
            {
                meleeCombat.EquipWeapon(MeleeWeaponData.CreateCarrotAxe(1));
            }
            else
            {
                meleeCombat.EquipWeapon(MeleeWeaponData.CreateCarrotBlade(1));
            }
        }

        private void UpdateWeaponDisplay(MeleeWeaponData weapon)
        {
            if (weaponNameText != null && weapon != null)
            {
                weaponNameText.text = $"{weapon.WeaponName}\n(Dame: {weapon.Damage})";
            }
        }
    }
}
