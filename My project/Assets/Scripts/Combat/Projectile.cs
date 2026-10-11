using System.Collections;
using UnityEngine;

namespace Apocarrot.Combat
{
    /// <summary>
    /// Viên đạn bắn ra từ vũ khí tầm xa (Tomato / Grape).
    /// Bay theo hướng chỉ định, va chạm kẻ địch thì gây sát thương và phát nổ hiệu ứng.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile State")]
        [SerializeField] private float speed = 14f;
        [SerializeField] private float damage = 20f;
        [SerializeField] private float lifetime = 3.0f;

        private Vector3 moveDirection;
        private GameObject sourceObject;
        private bool hasHit = false;
        private Color projectileColor = Color.red;

        public void Initialize(Vector3 direction, float projSpeed, float projDamage, GameObject source, Color color, Vector3 scale)
        {
            moveDirection = direction.normalized;
            speed = projSpeed;
            damage = projDamage;
            sourceObject = source;
            projectileColor = color;
            transform.localScale = scale;

            // Xoay mặt viên đạn theo hướng bay
            if (moveDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            }

            // Gán màu sắc hiển thị
            Renderer r = GetComponentInChildren<Renderer>();
            if (r != null)
            {
                r.material.color = color;
            }

            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (hasHit) return;

            // Di chuyển thẳng theo hướng bay
            transform.position += moveDirection * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit) return;

            // Bỏ qua người bắn
            if (sourceObject != null && (other.gameObject == sourceObject || other.transform.IsChildOf(sourceObject.transform)))
            {
                return;
            }

            IDamageable damageable = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                hasHit = true;
                DamageInfo info = new DamageInfo(damage, transform.position, moveDirection, sourceObject);
                damageable.TakeDamage(info);
                SpawnImpactEffect();
                Destroy(gameObject);
                return;
            }

            // Nếu va vào tường ranh giới arena
            if (other.gameObject.name.StartsWith("Wall_"))
            {
                hasHit = true;
                SpawnImpactEffect();
                Destroy(gameObject);
            }
        }

        private void SpawnImpactEffect()
        {
            // Tạo hiệu ứng giọt nước / hạt nổ tạm thời
            GameObject splash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            splash.name = "Projectile_Splash";
            splash.transform.position = transform.position;
            splash.transform.localScale = transform.localScale * 1.4f;

            Collider col = splash.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Renderer r = splash.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = projectileColor;
            }

            Destroy(splash, 0.12f);
        }

        /// <summary>
        /// Tạo Prefab GameObject hình cầu của viên đạn
        /// </summary>
        public static GameObject CreateProjectilePrefab()
        {
            GameObject proj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            proj.name = "Projectile_Sphere";

            // Thiết lập Collider dạng Trigger
            SphereCollider sphereCol = proj.GetComponent<SphereCollider>();
            if (sphereCol != null)
            {
                sphereCol.isTrigger = true;
                sphereCol.radius = 0.55f;
            }

            // Thêm Rigidbody Kinematic để Trigger hoạt động tin cậy trong Unity Physics
            Rigidbody rb = proj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            proj.AddComponent<Projectile>();
            return proj;
        }
    }
}
