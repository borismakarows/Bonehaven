using UnityEngine;

namespace BoneHaven
{
    [RequireComponent(typeof(Collider))]
    public class BattleTriggerZone : MonoBehaviour
    {
        [Header("Battle Setup")]
        [SerializeField] private WaveConfigSO battleConfig;
        [SerializeField] private Transform rewardSpawnPoint;

        [Header("Zone Boundaries (Optional)")]
        [SerializeField] private GameObject[] boundaryBarriers;

        private Collider triggerCollider;
        private bool hasTriggered = false;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;

            if (other.CompareTag("Player"))
            {
                hasTriggered = true;
                triggerCollider.enabled = false;

                SetBarriersActive(true);

                if (WaveSpawner.Instance != null)
                {
                    WaveSpawner.Instance.TriggerBattle(battleConfig, transform.position, HandleBattleFinished);
                }
            }
        }

        private void HandleBattleFinished()
        {
            SetBarriersActive(false);

            if (battleConfig != null && battleConfig.completionReward != null)
            {
                Vector3 spawnPos = rewardSpawnPoint != null ? rewardSpawnPoint.position : transform.position;
                Instantiate(battleConfig.completionReward, spawnPos, Quaternion.identity);
            }

            gameObject.SetActive(false);
        }

        private void SetBarriersActive(bool isActive)
        {
            if (boundaryBarriers == null) return;
            foreach (var barrier in boundaryBarriers)
            {
                if (barrier != null) barrier.SetActive(isActive);
            }
        }
    }
}