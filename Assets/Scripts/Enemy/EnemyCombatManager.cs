using System;
using System.Collections;
using UnityEngine;

namespace BoneHaven
{
    [RequireComponent(typeof(AI))]
    public class EnemyCombatManager : MonoBehaviour, IDamageable
    {
        [Header("Configuration")]
        [SerializeField] private EnemyConfigSO config;

        [Header("State Status")]
        [SerializeField] private float currentHealth;
        public bool IsAlive => currentHealth > 0f;
        public bool IsStunned { get; private set; } = false;
        public bool IsUnbalanced { get; private set; } = false;

        private AI aiController;
        private Coroutine statusRoutine;
        private int hitsReceivedWhilePowdered = 0;

        public event Action OnDamaged;
        public event Action OnStunStateEntered;
        public event Action OnDeath;

        private void Awake()
        {
            aiController = GetComponent<AI>();
            if (config != null)
            {
                currentHealth = config.maxHealth;
            }
        }

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (!IsAlive) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            OnDamaged?.Invoke();

            if (currentHealth <= 0f)
            {
                Die();
                return;
            }

            // Stun condition: Hit while unbalanced/powdered or reaching combo threshold
            if (IsUnbalanced)
            {
                hitsReceivedWhilePowdered++;
                if (hitsReceivedWhilePowdered >= (config != null ? config.hitsToStunWithPowder : 1))
                {
                    TriggerStun();
                    return;
                }
            }

            // Interrupt AI state to Hurt
            aiController.TriggerHurt(hitDirection);
        }

        public void ApplyBlackPowder()
        {
            if (!IsAlive || IsStunned) return;

            IsUnbalanced = true;
            hitsReceivedWhilePowdered = 0;

            if (statusRoutine != null) StopCoroutine(statusRoutine);
            statusRoutine = StartCoroutine(UnbalancedRoutine());

            aiController.TriggerUnbalanced();
        }

        private IEnumerator UnbalancedRoutine()
        {
            float duration = config != null ? config.unbalancedDuration : 1.2f;
            yield return new WaitForSeconds(duration);
            IsUnbalanced = false;
            statusRoutine = null;
        }

        private void TriggerStun()
        {
            if (!IsAlive) return;

            IsStunned = true;
            IsUnbalanced = false;

            if (statusRoutine != null) StopCoroutine(statusRoutine);
            statusRoutine = StartCoroutine(StunRoutine());

            OnStunStateEntered?.Invoke();
            aiController.TriggerStun();
        }

        private IEnumerator StunRoutine()
        {
            float duration = config != null ? config.stunDuration : 2.5f;
            yield return new WaitForSeconds(duration);
            
            IsStunned = false;
            statusRoutine = null;
            aiController.RecoverFromStun();
        }

        public void Execute(Transform attacker)
        {
            if (!IsAlive) return;

            currentHealth = 0f;
            IsStunned = false;
            IsUnbalanced = false;

            if (statusRoutine != null) StopCoroutine(statusRoutine);

            SpawnLoot(true);
            Die();
        }

        private void SpawnLoot(bool guaranteedHealth)
        {
            // Loot drop logic mapped to EnemyConfigSO drop rates
        }

        private void Die()
        {
            OnDeath?.Invoke();
            aiController.TriggerDeath();
            Destroy(gameObject, 1.5f);
        }
    }
}