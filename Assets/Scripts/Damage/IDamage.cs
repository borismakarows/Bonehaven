using UnityEngine;

namespace BoneHaven
{
    public interface IDamageable
    {
        void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection);
        void ApplyBlackPowder();
        void Execute(Transform attacker);
        bool IsStunned { get; }
        bool IsUnbalanced { get; }
        bool IsAlive { get; }
    }
}