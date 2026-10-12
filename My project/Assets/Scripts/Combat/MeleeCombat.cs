using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apocarrot.Data;

namespace Apocarrot.Combat
{
    /// <summary>
    /// Xử lý cơ chế đánh cận chiến của Thỏ: bấm nút -> kiểm tra cooldown -> quét hình nón trúng đích
    /// -> gây sát thương một lần duy nhất cho mỗi kẻ địch -> hiển thị vệt chém (slash arc).
    /// </summary>
    public class MeleeCombat : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        [SerializeField] private MeleeWeaponData equippedWeapon;

        [Header("Attack Origin")]
        [SerializeField] private Transform attackPoint;

        [Header("Target Layer")]
        [SerializeField] private LayerMask enemyLayer = ~0; // Mặc định quét mọi layer có IDamageable

        private float nextAttackTime = 0f;
        private HashSet<IDamageable> hitTargetsThisSwing = new HashSet<IDamageable>();
        private GameObject slashArcObject;

        public event Action<MeleeWeaponData> OnAttackExecuted;
        public event Action<MeleeWeaponData> OnWeaponChanged;

        public MeleeWeaponData EquippedWeapon => equippedWeapon;
        public bool CanAttack => Time.time >= nextAttackTime;
        public float CooldownNormalized => equippedWeapon != null && equippedWeapon.AttackCooldown > 0 ?
            Mathf.Clamp01((nextAttackTime - Time.time) / equippedWeapon.AttackCooldown) : 0f;

        private void Awake()
        {
            if (equippedWeapon == null)
            {
                equippedWeapon = MeleeWeaponData.CreateCarrotBlade(1);
            }

            if (attackPoint == null)
            {
                attackPoint = transform;
            }

            CreateSlashArcVisual();
        }

        private void Update()
        {
            bool attackPressed = false;

            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame))
            {
                attackPressed = true;
            }

            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    attackPressed = true;
                }
            }

            if (attackPressed)
            {
                PerformAttack();
            }
        }

        /// <summary>
        /// Kích hoạt đòn chém cận chiến (gọi từ input bàn phím hoặc nút UI trên màn hình)
        /// </summary>
        public bool PerformAttack()
        {
            if (equippedWeapon == null || !CanAttack) return false;

            nextAttackTime = Time.time + equippedWeapon.AttackCooldown;
            hitTargetsThisSwing.Clear();

            // 1. Quét tìm tất cả mục tiêu trong phạm vi bán kính
            Vector3 origin = attackPoint.position;
            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Collider[] hits = Physics.OverlapSphere(origin, equippedWeapon.AttackRange, enemyLayer, QueryTriggerInteraction.Ignore);
            int hitCount = 0;

            foreach (var col in hits)
            {
                // Bỏ qua chính bản thân player
                if (col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;

                IDamageable damageable = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive && !hitTargetsThisSwing.Contains(damageable))
                {
                    Vector3 toTarget = col.transform.position - origin;
                    toTarget.y = 0;
                    float distance = toTarget.magnitude;

                    // Kiểm tra góc quét hình nón
                    if (distance <= equippedWeapon.AttackRange)
                    {
                        float angle = Vector3.Angle(forward, toTarget.normalized);
                        if (angle <= equippedWeapon.AttackAngle * 0.5f)
                        {
                            hitTargetsThisSwing.Add(damageable);
                            hitCount++;

                            // Gây sát thương một lần duy nhất (cộng thêm sát thương từ Trang Bị Balo)
                            int bonusAtk = Progression.AccountProgression.Instance?.Data != null ? Progression.AccountProgression.Instance.Data.GetTotalEquipmentAttack() : 0;
                            DamageInfo dmg = new DamageInfo(
                                equippedWeapon.Damage + bonusAtk,
                                col.transform.position,
                                forward,
                                gameObject
                            );
                            damageable.TakeDamage(dmg);
                            Apocarrot.Audio.AudioManager.Instance?.PlayMeleeHit();

                            // Đẩy lùi kẻ địch nếu có Rigidbody hoặc CharacterController
                            Rigidbody rb = col.attachedRigidbody;
                            if (rb != null && !rb.isKinematic)
                            {
                                rb.AddForce(forward * equippedWeapon.KnockbackForce, ForceMode.Impulse);
                            }
                        }
                    }
                }
            }

            // 2. Hiển thị vệt chém trực quan (Visual Slash Arc) & Phát âm thanh vung kiếm
            ShowSlashArc();
            Apocarrot.Audio.AudioManager.Instance?.PlayMeleeSwing();

            OnAttackExecuted?.Invoke(equippedWeapon);
            return true;
        }

        public void EquipWeapon(MeleeWeaponData newWeapon)
        {
            if (newWeapon == null) return;
            equippedWeapon = newWeapon;
            OnWeaponChanged?.Invoke(equippedWeapon);
            Debug.Log($"[MeleeCombat] Đã trang bị: {newWeapon.WeaponName} (Lv.{newWeapon.Level}) - Dame: {newWeapon.Damage}, Cooldown: {newWeapon.AttackCooldown}s");
        }

        private void CreateSlashArcVisual()
        {
            slashArcObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            slashArcObject.name = "Visual_SlashArc";
            slashArcObject.transform.SetParent(transform, false);
            slashArcObject.transform.localPosition = new Vector3(0, 0.6f, 1.2f);
            slashArcObject.transform.localScale = new Vector3(equippedWeapon.AttackRange, 0.05f, equippedWeapon.AttackRange * 0.7f);
            slashArcObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

            Collider col = slashArcObject.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Renderer r = slashArcObject.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = equippedWeapon.SlashColor;
            }

            slashArcObject.SetActive(false);
        }

        private void ShowSlashArc()
        {
            if (slashArcObject == null) return;

            slashArcObject.transform.localScale = new Vector3(equippedWeapon.AttackRange, 0.05f, equippedWeapon.AttackRange * 0.8f);
            Renderer r = slashArcObject.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = equippedWeapon.SlashColor;
            }

            StopCoroutine(nameof(SlashArcRoutine));
            StartCoroutine(nameof(SlashArcRoutine));
        }

        private IEnumerator SlashArcRoutine()
        {
            slashArcObject.SetActive(true);
            float duration = 0.12f;
            float elapsed = 0f;

            Vector3 startScale = new Vector3(equippedWeapon.AttackRange * 0.4f, 0.04f, equippedWeapon.AttackRange * 0.3f);
            Vector3 endScale = new Vector3(equippedWeapon.AttackRange, 0.04f, equippedWeapon.AttackRange * 0.8f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (slashArcObject != null)
                {
                    slashArcObject.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                }
                yield return null;
            }

            if (slashArcObject != null)
            {
                slashArcObject.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (equippedWeapon == null) return;

            Vector3 origin = attackPoint != null ? attackPoint.position : transform.position;
            Gizmos.color = equippedWeapon.SlashColor;
            Gizmos.DrawWireSphere(origin, equippedWeapon.AttackRange);

            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Quaternion leftRot = Quaternion.Euler(0, -equippedWeapon.AttackAngle * 0.5f, 0);
            Quaternion rightRot = Quaternion.Euler(0, equippedWeapon.AttackAngle * 0.5f, 0);

            Gizmos.DrawLine(origin, origin + leftRot * forward * equippedWeapon.AttackRange);
            Gizmos.DrawLine(origin, origin + rightRot * forward * equippedWeapon.AttackRange);
        }
    }
}
