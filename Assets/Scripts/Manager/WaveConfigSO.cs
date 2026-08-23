using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoneHaven
{
    [Serializable]
    public class Wave
    {
        [Header("Wave Info")]
        public string waveName = "Wave 1";
        public float delayBeforeWave = 2.0f;

        [Header("Enemy Counts")]
        public int deckhandCount = 3;
        public int bombardierCount = 1;
    }

    [CreateAssetMenu(fileName = "NewBattleConfig", menuName = "BoneHaven/Battle Config")]
    public class WaveConfigSO : ScriptableObject
    {
        [Header("Battle Metadata")]
        public string battleName = "Battle 1: Beach Landing";

        [Header("Waves")]
        public List<Wave> waves = new List<Wave>();

        [Header("Battle Reward")]
        public GameObject completionReward;
    }
}