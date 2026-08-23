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
        [Tooltip("Delay AFTER this entire group batch is spawned before moving to the next group")]
        public float delayAfterGroup;
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
        public GameObject progressionReward;
    }
}