using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace BoneHaven
{
    public class WaveSpawner : MonoBehaviour
    {
        public static WaveSpawner Instance { get; private set; }

        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject deckhandPrefab;
        [SerializeField] private GameObject bombardierPrefab;

        [Header("Dynamic Radius Settings")]
        [SerializeField] private float deckhandMinRadius = 5.0f;
        [SerializeField] private float deckhandMaxRadius = 9.0f;
        [SerializeField] private float bombardierMinRadius = 11.0f;
        [SerializeField] private float bombardierMaxRadius = 16.0f;
        [SerializeField] private float navMeshSampleRange = 4.0f;

        private int activeEnemiesCount = 0;
        private bool isSpawning = false;
        private Action currentBattleCompleteCallback;
        private Vector3 currentArenaCenter;

        public static event Action<string> OnBattleStarted;
        public static event Action<string> OnWaveStarted;
        public static event Action OnBattleCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void TriggerBattle(WaveConfigSO battleConfig, Vector3 arenaCenter, Action onComplete = null)
        {
            if (isSpawning || battleConfig == null) return;

            currentArenaCenter = arenaCenter;
            currentBattleCompleteCallback = onComplete;
            StartCoroutine(BattleRoutine(battleConfig));
        }

        private IEnumerator BattleRoutine(WaveConfigSO battleConfig)
        {
            isSpawning = true;
            OnBattleStarted?.Invoke(battleConfig.battleName);

            foreach (Wave wave in battleConfig.waves)
            {
                if (wave.delayBeforeWave > 0f)
                {
                    yield return new WaitForSeconds(wave.delayBeforeWave);
                }

                OnWaveStarted?.Invoke(wave.waveName);

                // Spawn clustered Deckhands around a shared angle
                if (wave.deckhandCount > 0)
                {
                    SpawnDeckhandSquad(wave.deckhandCount);
                }

                // Spawn scattered Bombardiers around wider perimeter
                if (wave.bombardierCount > 0)
                {
                    SpawnBombardierPerimeter(wave.bombardierCount);
                }
            }

            isSpawning = false;
        }

        private void SpawnDeckhandSquad(int count)
        {
            // Pick a shared base direction for the squad
            float baseAngle = UnityEngine.Random.Range(0f, 360f);

            for (int i = 0; i < count; i++)
            {
                // Slight angle jitter within 35 degrees to keep them in a pack
                float angle = (baseAngle + UnityEngine.Random.Range(-35f, 35f)) * Mathf.Deg2Rad;
                float dist = UnityEngine.Random.Range(deckhandMinRadius, deckhandMaxRadius);

                Vector3 offset = new Vector3(Mathf.Sin(angle) * dist, 0f, Mathf.Cos(angle) * dist);
                Vector3 rawPos = currentArenaCenter + offset;

                SpawnEnemyAtValidPosition(deckhandPrefab, rawPos);
            }
        }

        private void SpawnBombardierPerimeter(int count)
        {
            // Evenly spread out angles across the circle with jitter
            float angleStep = 360f / Mathf.Max(1, count);

            for (int i = 0; i < count; i++)
            {
                float angle = (i * angleStep + UnityEngine.Random.Range(-20f, 20f)) * Mathf.Deg2Rad;
                float dist = UnityEngine.Random.Range(bombardierMinRadius, bombardierMaxRadius);

                Vector3 offset = new Vector3(Mathf.Sin(angle) * dist, 0f, Mathf.Cos(angle) * dist);
                Vector3 rawPos = currentArenaCenter + offset;

                SpawnEnemyAtValidPosition(bombardierPrefab, rawPos);
            }
        }

        private void SpawnEnemyAtValidPosition(GameObject prefab, Vector3 targetPos)
        {
            if (prefab == null) return;

            // Project onto nearest valid NavMesh floor
            Vector3 finalPos = targetPos;
            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, navMeshSampleRange, NavMesh.AllAreas))
            {
                finalPos = hit.position;
            }

            // Find Player and calculate look rotation towards player's position
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Vector3 targetFacingPos = playerObj != null ? playerObj.transform.position : currentArenaCenter;

            Vector3 lookDir = targetFacingPos - finalPos;
            lookDir.y = 0f; // Keep rotation strictly horizontal

            Quaternion rotation = lookDir.sqrMagnitude > 0.001f 
                ? Quaternion.LookRotation(lookDir.normalized) 
                : Quaternion.identity;

            GameObject enemy = Instantiate(prefab, finalPos, rotation);
            activeEnemiesCount++;

            if (enemy.TryGetComponent(out EnemyCombatManager combatManager))
            {
                combatManager.OnDeath += HandleEnemyDeath;
            }
        }

        private void HandleEnemyDeath()
        {
            activeEnemiesCount--;

            if (activeEnemiesCount <= 0 && !isSpawning)
            {
                CompleteBattle();
            }
        }

        private void CompleteBattle()
        {
            OnBattleCompleted?.Invoke();
            currentBattleCompleteCallback?.Invoke();
            currentBattleCompleteCallback = null;
        }
    }
}