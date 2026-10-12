using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Apocarrot.Inventory;

namespace Apocarrot.UI
{
    /// <summary>
    /// Xử lý tương tác Kéo - Thả (Drag & Drop) cho Trang bị và Balo:
    /// 1. Kéo vật phẩm từ Balo lên 6 ô trang bị -> Tự động trang bị / thay thế.
    /// 2. Kéo 2 món đồ cùng loại, cùng phẩm chất đè lên nhau -> Ghép (Merge / Fuse) nâng cấp.
    /// 3. Kéo từ ô trang bị xuống Balo -> Tháo trang bị.
    /// 4. Click / Tap nhẹ -> Mở bảng thông tin chi tiết (Stats Modal).
    /// </summary>
    public class UIDragDropItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public EquipmentItem Item { get; private set; }
        public bool IsEquippedSlot { get; private set; }
        public EquipmentSlotType SlotType { get; private set; }

        private bool isDragging = false;
        private Canvas rootCanvas;

        // Proxy ảnh lơ lửng khi người chơi kéo ngón tay / chuột
        private static GameObject ghostObject;
        private static Image ghostImage;
        private static RectTransform ghostRect;
        private static Canvas cachedRootCanvas;

        public void BindBagItem(EquipmentItem item)
        {
            this.Item = item;
            this.IsEquippedSlot = false;
            this.SlotType = item != null ? item.SlotType : EquipmentSlotType.Weapon;
        }

        public void BindEquippedSlot(EquipmentSlotType slot, EquipmentItem item)
        {
            this.SlotType = slot;
            this.Item = item;
            this.IsEquippedSlot = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isDragging) return;
            if (Item == null && !IsEquippedSlot) return;

            if (EquipmentBagUI.Instance != null)
            {
                if (IsEquippedSlot)
                {
                    EquipmentBagUI.Instance.OpenEquippedDetail(SlotType);
                }
                else if (Item != null)
                {
                    EquipmentBagUI.Instance.OpenBagItemDetail(Item);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Item == null) return;

            isDragging = true;
            EnsureGhost(transform);

            Sprite sp = WeaponSpriteService.GetEquipmentSprite(Item);
            if (sp != null && ghostImage != null)
            {
                ghostImage.sprite = sp;
                ghostImage.color = Color.white;
            }
            if (ghostObject != null)
            {
                ghostObject.SetActive(true);
                ghostObject.transform.SetAsLastSibling();
            }

            UpdateGhostPosition(eventData);

            // Làm mờ nhẹ thẻ gốc để tạo phản hồi thị giác
            Image img = GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 0.45f;
                img.color = c;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            UpdateGhostPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            isDragging = false;

            if (ghostObject != null)
            {
                ghostObject.SetActive(false);
            }

            // Khôi phục màu thẻ gốc
            Image img = GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 1.0f;
                img.color = c;
            }

            HandleDropTarget(eventData);
        }

        private void UpdateGhostPosition(PointerEventData eventData)
        {
            if (ghostRect == null) return;
            if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();

            if (rootCanvas != null && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                ghostRect.position = eventData.position;
            }
            else if (rootCanvas != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rootCanvas.transform as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint
                );
                ghostRect.localPosition = localPoint;
            }
            else
            {
                ghostRect.position = eventData.position;
            }
        }

        private void HandleDropTarget(PointerEventData eventData)
        {
            if (EquipmentBagUI.Instance == null || Item == null) return;

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                if (result.gameObject == gameObject) continue;

                var targetDragItem = result.gameObject.GetComponentInParent<UIDragDropItem>();
                if (targetDragItem != null)
                {
                    // 1. Thả vào một Ô Trang Bị trên người -> Thay thế / Trang bị
                    if (targetDragItem.IsEquippedSlot)
                    {
                        EquipmentBagUI.Instance.HandleDropOnSlot(this.Item, targetDragItem.SlotType);
                        return;
                    }

                    // 2. Thả vào một thẻ đồ khác trong Balo -> Ghép nâng cấp (Fuse)
                    if (!targetDragItem.IsEquippedSlot && targetDragItem.Item != null && targetDragItem.Item.Uid != this.Item.Uid)
                    {
                        EquipmentBagUI.Instance.HandleDropOnBagItem(this.Item, targetDragItem.Item);
                        return;
                    }
                }
            }

            // 3. Nếu kéo từ ô trang bị trên người ra vùng trống lưới Balo -> Tháo đồ về túi
            if (IsEquippedSlot)
            {
                foreach (var result in results)
                {
                    string rName = result.gameObject.name;
                    if (rName.Contains("Bag_Grid") || rName.Contains("GridContent") || rName.Contains("Viewport"))
                    {
                        EquipmentBagUI.Instance.HandleUnequipSlot(this.SlotType);
                        return;
                    }
                }
            }
        }

        private static void EnsureGhost(Transform origin)
        {
            if (ghostObject == null)
            {
                cachedRootCanvas = origin.GetComponentInParent<Canvas>();
                ghostObject = new GameObject("DragDrop_GhostProxy");
                ghostObject.transform.SetParent(cachedRootCanvas != null ? cachedRootCanvas.transform : origin.root, false);

                ghostRect = ghostObject.AddComponent<RectTransform>();
                ghostRect.sizeDelta = new Vector2(72, 72);
                ghostRect.pivot = new Vector2(0.5f, 0.5f);

                ghostImage = ghostObject.AddComponent<Image>();
                ghostImage.raycastTarget = false; // Không chặn Raycast khi thả
                ghostImage.preserveAspect = true;

                ghostObject.SetActive(false);
            }
            else
            {
                ghostObject.transform.SetAsLastSibling();
            }
        }
    }
}
