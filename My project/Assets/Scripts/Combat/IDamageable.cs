using UnityEngine;

namespace Apocarrot.Combat
{
    public struct DamageInfo
    {
        public float Amount;
        public Vector3 HitPoint;
        public Vector3 HitDirection;
        public GameObject Source;
        public bool IsCritical;

        public DamageInfo(float amount, Vector3 hitPoint, Vector3 hitDirection, GameObject source, bool isCritical = false)
        {
            Amount = amount;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            Source = source;
            IsCritical = isCritical;
        }
    }

    public interface IDamageable
    {
        void TakeDamage(DamageInfo info);
        bool IsAlive { get; }
        Transform transform { get; }
    }
}
