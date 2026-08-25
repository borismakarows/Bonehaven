using UnityEngine;

namespace BoneHaven
{
    [RequireComponent(typeof(Collider))]
    public class ChestInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "Open Chest";
        [SerializeField] private GameObject lootDropPrefab;
        [SerializeField] private Transform spawnPoint;
        private bool isOpened = false;

        public string GetPromptText() => isOpened ? "" : prompt;

        public void Interact(GameObject interactor)
        {
            if (isOpened) return;
            isOpened = true;

            if (lootDropPrefab != null)
            {
                Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up;
                Instantiate(lootDropPrefab, spawnPos, Quaternion.identity);
            }

            Debug.Log("Chest opened via IInteractable!");
        }
    }
}