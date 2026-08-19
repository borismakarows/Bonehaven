using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoneHaven
{
    [Serializable]
    public struct EnemySpawnGroup
    {
        public EnemyConfigSO enemyConfig;
        public int count;
        public float spawnDelay;
    }

    [CreateAssetMenu(fileName = "NewWaveConfig", menuName = "BoneHaven/Wave Config")]
    public class WaveConfigSO : ScriptableObject
    {
        [Header("Wave Meta")]
        public int waveNumber = 1;
        public string waveTitle = "Wave 1: The Beach Landing";

        [Header("Spawning Sequence")]
        public List<EnemySpawnGroup> spawnGroups = new List<EnemySpawnGroup>();
        public float initialWaveDelay = 2.0f;

        [Header("Progression Rewards")]
        public GameObject keyDropPrefab; // Fortress Gate Key (Wave 1) or Ship Key (Wave 2)
    }
}