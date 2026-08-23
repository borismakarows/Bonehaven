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

        [Header("Death Settings")]
        [SerializeField] private float despawnDelay = 3.5f;

        [Header("Melee Attack Settings")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float attackRadius = 1.2f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Ranged Attack Settings")]
        [SerializeField] private string bombPoolTag = "BombProjectile";
        [SerializeField] private GameObject bombPrefabFallback;
        [SerializeField] private Transform throwPoint;

        [Header("Loot Drop Prefabs")]
        [SerializeField] private GameObject powderDropPrefab;
        [SerializeField] private GameObject ammoDropPrefab;
        [SerializeField] private GameObject healthDropPrefab;

        [Header("State Status")]
        [SerializeField] private float currentHealth;
        public bool IsAlive => currentHealth > 0f;
        public bool IsStunned { get; private set; } = false;
        public bool IsUnbalanced { get; private set; } = false;

        private AI aiController;
        private Coroutine statusRoutine;
        
        [Header("Combo & Stun Tracking")]
        [SerializeField] private int comboHitCount = 0;
        private float lastHitTime;
        private const float comboResetDuration = 2.0f;
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

            if (Time.time - lastHitTime > comboResetDuration) comboHitCount = 0;
            
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            lastHitTime = Time.time;
            comboHitCount++;

            CombatJuiceManager.Instance?.TriggerScreenShake(0.2f);
            CombatJuiceManager.Instance?.TriggerHitStop(0.04f, 0.2f);

            OnDamaged?.Invoke();

            if (currentHealth <= 0f)
            {
                Die();
                return;
            }

            // 1. Combo Stun (3 Hits)
            if (comboHitCount >= 3)
            {
                TriggerStun();
                return;
            }

            // 2. Powder then Hit Stun
            if (IsUnbalanced)
            {
                hitsReceivedWhilePowdered++;
                int requiredHits = config != null ? config.hitsToStunWithPowder : 1;

                if (hitsReceivedWhilePowdered >= requiredHits)
                {
                    TriggerStun();
                    return;
                }
            }

            // 3. Hurt Reaction
            if (!IsStunned)
            {
                aiController.TriggerHurt(hitDirection);
            }
        }

        public void ApplyBlackPowder()
        {
            if (!IsAlive || IsStunned) return;

            // Hit then Powder Stun
            if (Time.time - lastHitTime < 0.8f)
            {
                TriggerStun();
                return;
            }

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

            if (IsAlive && !IsStunned)
            {
                aiController.RecoverFromUnbalanced();
            }
        }

        private void TriggerStun()
        {
            if (!IsAlive) return;

            IsStunned = true;
            IsUnbalanced = false;
            comboHitCount = 0;

            
            CombatJuiceManager.Instance?.TriggerScreenShake(0.4f);
            CombatJuiceManager.Instance?.TriggerHitStop(0.08f, 0.1f);

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

            CombatJuiceManager.Instance?.TriggerScreenShake(0.6f);
            CombatJuiceManager.Instance?.TriggerHitStop(0.12f, 0.05f);

            if (statusRoutine != null) StopCoroutine(statusRoutine);

            SpawnLoot(true);
            Die();
        }

        public void OnBombThrowAnimationEvent()
        {
            if (!IsAlive || throwPoint == null) return;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) return;

            Vector3 spawnPos = throwPoint.position;
            GameObject bombObj = null;

            if (ObjectPooler.Instance != null)
            {
                bombObj = ObjectPooler.Instance.SpawnFromPool(bombPoolTag, spawnPos, Quaternion.identity);
            }
            else if (bombPrefabFallback != null)
            {
                bombObj = Instantiate(bombPrefabFallback, spawnPos, Quaternion.identity);
            }

            if (bombObj == null) return;

            Collider enemyCollider = GetComponent<Collider>();
            if (bombObj.TryGetComponent(out BombProjectile projectile))
            {
                Vector3 targetPos = playerObj.transform.position;
                projectile.Launch(targetPos, enemyCollider, 1.1f);
            }
        }

        public void OnMeleeAttackHitEvent()
        {
            if (!IsAlive) return;

            Transform hitOrigin = attackPoint != null ? attackPoint : transform;
            Collider[] hits = Physics.OverlapSphere(hitOrigin.position, attackRadius, playerLayer);

            float damage = config != null ? config.attackDamage : 15f;

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player") && hit.TryGetComponent(out IDamageable playerDamageable))
                {
                    CombatJuiceManager.Instance?.TriggerScreenShake(0.35f);

                    Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                    playerDamageable.TakeDamage(damage, hit.bounds.center, pushDir);
                    break;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
            }
        }

        private void SpawnLoot(bool guaranteedHealth)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

            if (guaranteedHealth && healthDropPrefab != null)
            {
                Instantiate(healthDropPrefab, spawnPos, Quaternion.identity);
            }

            if (config == null) return;

            if (powderDropPrefab != null && UnityEngine.Random.value <= config.gunpowderDropChance)
            {
                Vector3 offset = UnityEngine.Random.insideUnitSphere * 0.4f;
                offset.y = 0;
                Instantiate(powderDropPrefab, spawnPos + offset, Quaternion.identity);
            }

            if (ammoDropPrefab != null && UnityEngine.Random.value <= config.ammoDropChance)
            {
                Vector3 offset = UnityEngine.Random.insideUnitSphere * 0.4f;
                offset.y = 0;
                Instantiate(ammoDropPrefab, spawnPos + offset, Quaternion.identity);
            }
        }

        private void Die()
        {
            if (statusRoutine != null) StopCoroutine(statusRoutine);
            StopAllCoroutines();

            SpawnLoot(false);
            OnDeath?.Invoke();
            aiController.TriggerDeath();

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Destroy(gameObject, despawnDelay);
        }
    }
}