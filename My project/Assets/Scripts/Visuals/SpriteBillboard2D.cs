using System.Collections;
using UnityEngine;
using Apocarrot.Cameras;

namespace Apocarrot.Visuals
{
    /// <summary>
    /// Quản lý hiển thị 2.5D Billboard Sprite trong không gian đấu trường 3D.
    /// Giữ Sprite luôn nghiêng góc chuẩn trực diện với Camera (55 độ),
    /// tự động lật mặt trái/phải (FlipX), hiệu ứng nhấp nhô khi bước đi (Walk Wobble),
    /// thở nhịp nhàng (Idle Breathing), chớp sáng khi dính đòn (Hit Flash)
    /// và tạo bóng tròn mềm dưới chân (Ground Shadow).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteBillboard2D : MonoBehaviour
    {
        [Header("Camera Tilt Alignment")]
        [Tooltip("Góc nghiêng trực diện với camera (mặc định 55 độ theo TopDownFollowCamera)")]
        [SerializeField] private float cameraPitch = 55.0f;
        [SerializeField] private bool autoSyncCameraPitch = true;

        [Header("Procedural Animation (Juice)")]
        [SerializeField] private bool enableAnimations = true;
        [SerializeField] private float idleBreatheSpeed = 3.5f;
        [SerializeField] private float idleBreatheScale = 0.04f;
        [SerializeField] private float walkBounceSpeed = 12.0f;
        [SerializeField] private float walkBounceHeight = 0.12f;
        [SerializeField] private float walkTiltAngle = 8.0f;

        [Header("Shadow Settings")]
        [SerializeField] private bool createGroundShadow = true;
        [SerializeField] private Vector2 shadowSize = new Vector2(1.1f, 0.55f);

        private SpriteRenderer spriteRenderer;
        private GameObject shadowObj;
        private SpriteRenderer shadowRenderer;
        private Vector3 baseLocalPosition;
        private Vector3 baseLocalScale;
        private bool isMoving = false;
        private bool facingLeft = false;
        private Coroutine flashCoroutine;
        private Color originalColor = Color.white;

        public SpriteRenderer Renderer => spriteRenderer;
        public bool FacingLeft => facingLeft;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
            baseLocalPosition = transform.localPosition;
            baseLocalScale = transform.localScale;

            if (createGroundShadow)
            {
                CreateShadow();
            }
        }

        private void Start()
        {
            AlignToCamera();
        }

        private void LateUpdate()
        {
            AlignToCamera();

            if (enableAnimations)
            {
                AnimateProceduralMotion();
            }

            // Đồng bộ vị trí bóng dưới sàn
            if (shadowObj != null && transform.parent != null)
            {
                Vector3 parentPos = transform.parent.position;
                shadowObj.transform.position = new Vector3(parentPos.x, 0.02f, parentPos.z);
            }
        }

        /// <summary>
        /// Đồng bộ góc nghiêng của Sprite với góc Pitch của Camera Top-Down
        /// </summary>
        public void AlignToCamera()
        {
            float targetPitch = cameraPitch;
            if (autoSyncCameraPitch && TopDownFollowCamera.Instance != null)
            {
                targetPitch = TopDownFollowCamera.Instance.transform.eulerAngles.x;
            }

            // Xoay quanh trục X để mặt phẳng sprite nghiêng trực diện vào mắt camera
            transform.rotation = Quaternion.Euler(targetPitch, 0f, 0f);
        }

        /// <summary>
        /// Cập nhật trạng thái di chuyển và hướng nhìn từ Player/Enemy AI
        /// </summary>
        public void SetMovementState(bool moving, float horizontalVelocity)
        {
            isMoving = moving;

            if (Mathf.Abs(horizontalVelocity) > 0.05f)
            {
                facingLeft = horizontalVelocity < 0f;
                spriteRenderer.flipX = facingLeft;
            }
        }

        /// <summary>
        /// Thiết lập trực tiếp hướng lật mặt
        /// </summary>
        public void SetFacingLeft(bool left)
        {
            facingLeft = left;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = left;
            }
        }

        /// <summary>
        /// Tạo hiệu ứng bước đi nhấp nhô hoặc thở nhịp nhàng khi đứng yên
        /// </summary>
        private void AnimateProceduralMotion()
        {
            if (isMoving)
            {
                // Khi chạy: Nảy lên xuống nhẹ + lắc người theo nhịp chân (Walk Wobble)
                float t = Time.time * walkBounceSpeed;
                float bounceY = Mathf.Abs(Mathf.Sin(t)) * walkBounceHeight;
                float tiltZ = Mathf.Sin(t) * walkTiltAngle * (facingLeft ? -1f : 1f);

                transform.localPosition = baseLocalPosition + new Vector3(0f, bounceY, 0f);
                transform.localRotation = Quaternion.Euler(cameraPitch, 0f, tiltZ);
            }
            else
            {
                // Khi đứng yên: Thở phập phồng co giãn nhẹ nhàng (Idle Breathing)
                float t = Time.time * idleBreatheSpeed;
                float scaleY = 1.0f + Mathf.Sin(t) * idleBreatheScale;
                float scaleX = 1.0f - Mathf.Sin(t) * (idleBreatheScale * 0.5f);

                transform.localPosition = baseLocalPosition;
                transform.localScale = new Vector3(baseLocalScale.x * scaleX, baseLocalScale.y * scaleY, baseLocalScale.z);
            }
        }

        /// <summary>
        /// Hiệu ứng chớp trắng/đỏ khi nhận sát thương
        /// </summary>
        public void FlashDamage(Color flashColor, float duration = 0.14f)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(DamageFlashRoutine(flashColor, duration));
        }

        private IEnumerator DamageFlashRoutine(Color flashColor, float duration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = flashColor;
            }

            yield return new WaitForSeconds(duration);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        /// <summary>
        /// Tạo bóng đổ hình elip mềm trên nền đất
        /// </summary>
        private void CreateShadow()
        {
            shadowObj = new GameObject($"{gameObject.name}_Shadow2D");
            shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();

            // Tạo Sprite hình elip mờ
            shadowRenderer.sprite = GenerateSoftShadowSprite();
            shadowRenderer.color = new Color(0f, 0f, 0f, 0.42f);
            shadowRenderer.sortingOrder = -1; // Vẽ dưới mọi nhân vật

            // Đặt bóng nằm phẳng sát mặt đất
            shadowObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            shadowObj.transform.localScale = new Vector3(shadowSize.x, shadowSize.y, 1f);

            if (transform.parent != null)
            {
                shadowObj.transform.position = new Vector3(transform.parent.position.x, 0.02f, transform.parent.position.z);
            }
        }

        private Sprite GenerateSoftShadowSprite()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float center = size / 2f;
            float maxRadius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / maxRadius;
                    float dy = (y - center) / maxRadius;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < 1.0f)
                    {
                        // Giảm dần alpha từ tâm ra rìa (Gaussian drop-off mềm)
                        float alpha = Mathf.SmoothStep(1.0f, 0.0f, dist);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }

        private void OnDestroy()
        {
            if (shadowObj != null)
            {
                Destroy(shadowObj);
            }
        }
    }
}
