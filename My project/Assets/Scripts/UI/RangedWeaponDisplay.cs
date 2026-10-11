using UnityEngine;
using UnityEngine.UI;
using Apocarrot.Combat;
using Apocarrot.Data;

namespace Apocarrot.UI
{
    /// <summary>
    /// Hiển thị thông tin vũ khí tầm xa tự động (Tên, hồi chiêu, trạng thái khóa mục tiêu)
    /// và nút đổi nhanh giữa Tomato Blaster / Grape Splitter để kiểm thử.
    /// </summary>
    public class RangedWeaponDisplay : MonoBehaviour
    {
        [Header("Target Controller")]
        [SerializeField] private RangedWeaponController rangedController;

        [Header("UI Elements")]
        [SerializeField] private Text weaponNameText;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private Text statusText;
        [SerializeField] private Button switchRangedButton;

        private void Start()
        {
            if (rangedController == null)
            {
                rangedController = FindFirstObjectByType<RangedWeaponController>();
            }

            if (switchRangedButton != null)
            {
                switchRangedButton.onClick.AddListener(HandleSwitchWeaponClicked);
            }

            if (rangedController != null)
            {
                rangedController.OnWeaponChanged += UpdateDisplay;
                UpdateDisplay(rangedController.EquippedWeapon);
            }
        }

        private void Update()
        {
            if (rangedController == null) return;

            // Cập nhật vòng quay hồi chiêu
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = rangedController.CooldownNormalized;
            }

            // Trạng thái tìm mục tiêu
            if (statusText != null)
            {
                if (rangedController.CurrentTarget != null)
                {
                    statusText.text = "🎯 ĐÃ KHÓA MỤC TIÊU";
                    statusText.color = new Color(1f, 0.35f, 0.35f);
                }
                else
                {
                    statusText.text = "🔍 Đang quét tìm quái...";
                    statusText.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);
                }
            }
        }

        private void HandleSwitchWeaponClicked()
        {
            if (rangedController == null) return;

            // Đổi giữa Tomato Blaster (1 viên thẳng) và Grape Splitter (chùm 3 viên)
            if (rangedController.EquippedWeapon.WeaponId == "tomato_blaster")
            {
                rangedController.EquipRangedWeapon(RangedWeaponData.CreateGrapeSplitter(1));
            }
            else
            {
                rangedController.EquipRangedWeapon(RangedWeaponData.CreateTomatoBlaster(1));
            }
        }

        private void UpdateDisplay(RangedWeaponData weapon)
        {
            if (weaponNameText != null && weapon != null)
            {
                weaponNameText.text = $"🏹 {weapon.WeaponName}\n(Tự bắn mỗi {weapon.FireCooldown:0.0}s)";
            }
        }
    }
}
