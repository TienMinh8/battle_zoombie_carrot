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

        private Vector3 currentVelocity;

        public Transform Target
        {
            get => target;
            set => target = value;
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

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;
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
