using UnityEngine;
using UnityEngine.EventSystems;
using Apocarrot.Gameplay;

namespace Apocarrot.UI
{
    /// <summary>
    /// Joystick ảo trên màn hình dành cho thao tác cảm ứng trên thiết bị di động.
    /// Kéo vòng tròn cần điều khiển để gửi vector di chuyển tới PlayerController.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Joystick References")]
        [SerializeField] private RectTransform backgroundRing;
        [SerializeField] private RectTransform handleKnob;

        [Header("Settings")]
        [Tooltip("Bán kính kéo tối đa của cần joystick")]
        [SerializeField] private float handleRange = 75f;

        [Header("Target Controller")]
        [SerializeField] private PlayerController playerController;

        private Vector2 inputVector = Vector2.zero;
        private Canvas parentCanvas;

        public Vector2 InputVector => inputVector;

        private void Awake()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
            }
        }

        public void SetPlayerController(PlayerController controller)
        {
            playerController = controller;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (backgroundRing == null || handleKnob == null) return;

            UnityEngine.Camera cam = parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera ? parentCanvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(backgroundRing, eventData.position, cam, out Vector2 localPoint))
            {
                // Chuẩn hóa vector theo bán kính
                inputVector = localPoint / handleRange;
                if (inputVector.magnitude > 1f)
                {
                    inputVector.Normalize();
                }

                // Cập nhật vị trí hiển thị của núm joystick
                handleKnob.anchoredPosition = inputVector * handleRange;

                // Gửi input tới Player
                if (playerController != null)
                {
                    playerController.SetJoystickInput(inputVector);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            inputVector = Vector2.zero;
            if (handleKnob != null)
            {
                handleKnob.anchoredPosition = Vector2.zero;
            }

            if (playerController != null)
            {
                playerController.SetJoystickInput(Vector2.zero);
            }
        }
    }
}
