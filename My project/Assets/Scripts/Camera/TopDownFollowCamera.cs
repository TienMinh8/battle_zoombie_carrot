using UnityEngine;

namespace Apocarrot.Cameras
{
    /// <summary>
    /// Camera 3D top-down góc nghiêng 3/4 đi theo nhân vật Thỏ Rabbit Warrior.
    /// Giữ toàn bộ đấu trường và các mối nguy hiểm dễ quan sát, không xoay tròn gây chóng mặt.
    /// </summary>
    public class TopDownFollowCamera : MonoBehaviour
    {
        [Header("Target Tracking")]
        [Tooltip("Mục tiêu camera đi theo (thường là Player)")]
        [SerializeField] private Transform target;

        [Header("Camera Positioning & Angle")]
        [Tooltip("Độ lệch vị trí camera so với mục tiêu (X, Y, Z)")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 13f, -9f);

        [Tooltip("Góc nghiêng chúc xuống của camera (Pitch angle)")]
        [SerializeField] private float pitchAngle = 55.0f;

        [Header("Smoothing")]
        [Tooltip("Thời gian làm mượt chuyển động của camera (giây)")]
        [SerializeField] private float smoothTime = 0.15f;

        public static TopDownFollowCamera Instance { get; private set; }

        private Vector3 currentVelocity;
        private float shakeDuration = 0f;
        private float shakeIntensity = 0f;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ApplyCameraRotation();
            if (target != null)
            {
                // Khởi tạo vị trí ngay lập tức không bị giật
                transform.position = target.position + offset;
            }
        }

        public static void Shake(float duration = 0.22f, float intensity = 0.35f)
        {
            if (Instance != null)
            {
                Instance.TriggerShake(duration, intensity);
            }
        }

        public void TriggerShake(float duration, float intensity)
        {
            shakeDuration = Mathf.Max(shakeDuration, duration);
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;

            // Thêm rung chấn Camera Shake
            if (shakeDuration > 0f)
            {
                shakeDuration -= Time.deltaTime;
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-1f, 1f)
                ) * shakeIntensity * Mathf.Clamp01(shakeDuration / 0.2f);
                targetPosition += shakeOffset;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref currentVelocity,
                smoothTime
            );
        }

        private void OnValidate()
        {
            ApplyCameraRotation();
        }

        private void ApplyCameraRotation()
        {
            transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
        }
    }
}
