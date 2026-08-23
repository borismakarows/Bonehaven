using UnityEngine;

namespace BoneHaven
{
    public class PlayerStats : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        public float CurrentHealth { get; private set; }

        public bool IsAlive => CurrentHealth > 0;
        public bool IsStunned => false;      // Player için gerekmiyorsa false
        public bool IsUnbalanced => false;   // Player için gerekmiyorsa false

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (!IsAlive) return;

            // PlayerCombatFSM üzerindeki i-frame (dash) kontrolü
            if (TryGetComponent(out PlayerCombatFSM combatFSM) && combatFSM.IsInvulnerable)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            Debug.Log($"Player damaged! Remaining: {CurrentHealth}");

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            Debug.Log($"Player healed! Current: {CurrentHealth}");
        }

        public void ApplyBlackPowder() { } 
        public void Execute(Transform attacker) { }

        private void Die()
        {
            Debug.Log("Player Died!");
        }
    }
}