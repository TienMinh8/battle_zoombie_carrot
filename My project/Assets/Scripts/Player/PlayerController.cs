using UnityEngine;

namespace Apocarrot.Gameplay
{
    /// <summary>
    /// Điều khiển di chuyển của Rabbit Warrior trong đấu trường 3D.
    /// Hỗ trợ cả bàn phím (WASD / Phím mũi tên) trong Editor và Joystick ảo trên màn hình cảm ứng.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Tốc độ di chuyển của nhân vật (m/s)")]
        [SerializeField] private float moveSpeed = 6.0f;

        [Tooltip("Tốc độ quay mặt theo hướng di chuyển (độ/giây)")]
        [SerializeField] private float rotationSpeed = 720.0f;

        [Tooltip("Gia tốc mượt khi tăng/giảm tốc độ")]
        [SerializeField] private float acceleration = 25.0f;

        [Header("Arena Boundary Clamping")]
        [Tooltip("Bật giới hạn vị trí trong khuôn viên arena")]
        [SerializeField] private bool clampToArena = true;
        [SerializeField] private Vector2 arenaBoundsX = new Vector2(-18f, 18f);
        [SerializeField] private Vector2 arenaBoundsZ = new Vector2(-18f, 18f);

        [Header("Visual References")]
        [SerializeField] private Transform visualRoot;

        private CharacterController characterController;
        private Vector3 currentVelocity = Vector3.zero;
        private Vector2 externalJoystickInput = Vector2.zero;

        public Vector3 Velocity => currentVelocity;
        public bool IsMoving => currentVelocity.sqrMagnitude > 0.05f;

        private PlayerHealth playerHealth;
        private Apocarrot.Visuals.SpriteBillboard2D spriteBillboard;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerHealth = GetComponent<PlayerHealth>();
            spriteBillboard = GetComponentInChildren<Apocarrot.Visuals.SpriteBillboard2D>();
            if (visualRoot == null)
            {
                visualRoot = transform;
            }
        }

        private void Start()
        {
            if (spriteBillboard == null)
            {
                spriteBillboard = GetComponentInChildren<Apocarrot.Visuals.SpriteBillboard2D>();
            }
        }

        private void Update()
        {
            if (playerHealth != null && !playerHealth.IsAlive) return;
            HandleMovement();
        }

        /// <summary>
        /// Nhận input từ joystick ảo (UI)
        /// </summary>
        public void SetJoystickInput(Vector2 input)
        {
            externalJoystickInput = Vector2.ClampMagnitude(input, 1f);
        }

        private void HandleMovement()
        {
            // 1. Thu thập input từ bàn phím (WASD / Mũi tên)
            Vector2 keyboardInput = Vector2.zero;
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) keyboardInput.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) keyboardInput.y -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) keyboardInput.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) keyboardInput.x += 1f;
            }

            // 2. Kết hợp với input Joystick (ưu tiên joystick nếu đang kéo)
            Vector2 combinedInput = externalJoystickInput.sqrMagnitude > 0.01f ? externalJoystickInput : keyboardInput;
            Vector3 inputDirection = new Vector3(combinedInput.x, 0f, combinedInput.y);

            if (inputDirection.sqrMagnitude > 1f)
            {
                inputDirection.Normalize();
            }

            // 3. Tính toán vận tốc mục tiêu
            Vector3 targetVelocity = inputDirection * moveSpeed;

            // Áp dụng gia tốc mượt
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                targetVelocity,
                acceleration * Time.deltaTime
            );

            // Trọng lực giả lập giữ nhân vật chạm đất
            Vector3 finalMove = currentVelocity;
            if (!characterController.isGrounded)
            {
                finalMove.y += Physics.gravity.y * Time.deltaTime;
            }

            // 4. Di chuyển bằng CharacterController
            characterController.Move(finalMove * Time.deltaTime);

            // 5. Giới hạn vị trí trong biên đấu trường nếu cần
            if (clampToArena)
            {
                Vector3 pos = transform.position;
                pos.x = Mathf.Clamp(pos.x, arenaBoundsX.x, arenaBoundsX.y);
                pos.z = Mathf.Clamp(pos.z, arenaBoundsZ.x, arenaBoundsZ.y);
                transform.position = pos;
            }

            // 6. Cập nhật hướng nhìn và hoạt ảnh Sprite 2.5D
            if (spriteBillboard != null)
            {
                spriteBillboard.SetMovementState(IsMoving, currentVelocity.x);
            }
            else if (inputDirection.sqrMagnitude > 0.01f && visualRoot != null)
            {
                // Fallback nếu dùng visual mesh 3D
                Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
                visualRoot.rotation = Quaternion.RotateTowards(
                    visualRoot.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}
