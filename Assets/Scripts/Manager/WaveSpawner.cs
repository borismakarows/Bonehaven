using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoneHaven
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("Wave Progression")]
        [SerializeField] private List<WaveConfigSO> waves = new List<WaveConfigSO>();
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform rewardSpawnPoint;

        [Header("Enemy Prefab Lookups")]
        [SerializeField] private GameObject deckhandPrefab;
        [SerializeField] private GameObject bombardierPrefab;

        private int currentWaveIndex = 0;
        private int activeEnemiesCount = 0;
        private bool isWaveInProgress = false;

        public static event Action<int, string> OnWaveStarted;
        public static event Action<int> OnWaveCompleted;
        public static event Action OnAllWavesCleared;

        private void Start()
        {
            if (waves.Count > 0)
            {
                StartCoroutine(StartWaveRoutine(currentWaveIndex));
            }
        }

        private IEnumerator StartWaveRoutine(int waveIndex)
        {
            if (waveIndex >= waves.Count)
            {
                OnAllWavesCleared?.Invoke();
                yield break;
            }

            WaveConfigSO wave = waves[waveIndex];
            isWaveInProgress = true;
            activeEnemiesCount = 0;

            OnWaveStarted?.Invoke(wave.waveNumber, wave.waveTitle);

            yield return new WaitForSeconds(wave.initialWaveDelay);

            for (int g = 0; g < wave.spawnGroups.Count; g++)
            {
                EnemySpawnGroup group = wave.spawnGroups[g];

                // Spawn all enemies in this group simultaneously
                for (int i = 0; i < group.count; i++)
                {
                    SpawnEnemy(group.enemyConfig);
                }

                // If delay is configured, wait before executing the next element
                if (group.delayAfterGroup > 0f)
                {
                    yield return new WaitForSeconds(group.delayAfterGroup);
                }
            }
        }

        private void SpawnEnemy(EnemyConfigSO config)
        {
            if (spawnPoints.Length == 0) return;

            Transform sp = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            GameObject prefabToSpawn = deckhandPrefab;

            if (config != null && config.name.ToLower().Contains("bombardier"))
            {
                prefabToSpawn = bombardierPrefab != null ? bombardierPrefab : deckhandPrefab;
            }

            GameObject enemy = Instantiate(prefabToSpawn, sp.position, sp.rotation);
            activeEnemiesCount++;

            if (enemy.TryGetComponent(out EnemyCombatManager combatManager))
            {
                combatManager.OnDeath += HandleEnemyDeath;
            }
        }

        private void HandleEnemyDeath()
        {
            activeEnemiesCount--;

            if (activeEnemiesCount <= 0 && isWaveInProgress)
            {
                isWaveInProgress = false;
                CompleteCurrentWave();
            }
        }

        private void CompleteCurrentWave()
        {
            WaveConfigSO completedWave = waves[currentWaveIndex];
            OnWaveCompleted?.Invoke(completedWave.waveNumber);

            if (completedWave.progressionReward != null)
            {
                Vector3 spawnPos = rewardSpawnPoint != null ? rewardSpawnPoint.position : transform.position;
                Instantiate(completedWave.progressionReward, spawnPos, Quaternion.identity);
            }

            currentWaveIndex++;

            if (currentWaveIndex < waves.Count)
            {
                StartCoroutine(StartWaveRoutine(currentWaveIndex));
            }
            else
            {
                OnAllWavesCleared?.Invoke();
            }
        }
    }
}