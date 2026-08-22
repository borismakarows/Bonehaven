using UnityEngine;

namespace BoneHaven
{
    public enum EnemyType
    {
        MeleeDeckhand,
        RangedBombardier,
        SkeletonCaptain
    }

    [CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "BoneHaven/Enemy Config")]
    public class EnemyConfigSO : ScriptableObject
    {
        [Header("Identity & Archetype")]
        public string enemyName = "Deckhand Skeleton";
        public EnemyType enemyType = EnemyType.MeleeDeckhand;
        public GameObject enemyPrefab;

        [Header("Base Attributes")]
        public float maxHealth = 45f;
        public float moveSpeed = 3.5f;
        public float rotationSpeed = 8f;

        [Header("Perception & Detection")]
        [Range(20f, 90f)] public float visionAngle = 45f;
        public float visionDistance = 7f;
        public int patrolStartChanceRatio = 30;

        [Header("Combat & Attack Profile")]
        public float attackDamage = 15f;
        public float attackRange = 1.6f;
        public float attackWindup = 0.5f;
        public float attackCooldown = 2.0f;

        [Header("Stun & Stagger Thresholds")]
        public float stunDuration = 2.5f;
        public float unbalancedDuration = 1.2f;
        public int hitsToStunWithPowder = 1; // 1 for Deckhands, 2 for Captain

        [Header("Loot Drop Rates (0.0 to 1.0)")]
        [Range(0f, 1f)] public float gunpowderDropChance = 0.25f;
        [Range(0f, 1f)] public float ammoDropChance = 0.0f;
        public float pityIncreasePerKill = 0.15f;
    }
}