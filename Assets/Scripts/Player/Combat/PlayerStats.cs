using System;
using UnityEngine;

namespace BoneHaven
{
    public class PlayerStats : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0f;
        public bool IsStunned => false;
        public bool IsUnbalanced => false;

        public static event Action<float, float> OnHealthChanged;
        public static event Action OnPlayerDied;

        private PlayerCombatFSM combatFSM;

        private void Awake()
        {
            currentHealth = maxHealth;
            combatFSM = GetComponent<PlayerCombatFSM>();
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (!IsAlive) return;

         
            if (combatFSM != null && combatFSM.IsInvulnerable) return;

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            
            CombatJuiceManager.Instance?.TriggerScreenShake(0.35f);
            CombatJuiceManager.Instance?.TriggerHitStop(0.05f, 0.2f);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ApplyBlackPowder() { }
        public void Execute(Transform attacker) { }

        private void Die()
        {
            OnPlayerDied?.Invoke();
            if (combatFSM != null)
            {
                combatFSM.TriggerPlayerDeath();
            }
        }
    }
}