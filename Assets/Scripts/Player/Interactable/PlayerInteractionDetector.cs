using UnityEngine;
using TMPro;

namespace BoneHaven
{
    public class PlayerInteractionDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float interactRadius = 2.2f;
        [SerializeField] private LayerMask interactableLayer;

        [Header("UI Feedback (Optional)")]
        [SerializeField] private TextMeshProUGUI promptTextUI;

        private IInteractable currentInteractable;

        private void Update()
        {
            CheckForInteractables();

            if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
            {
                currentInteractable.Interact(gameObject);
            }
        }

        private void CheckForInteractables()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius, interactableLayer);
            if (hits.Length > 0 && hits[0].TryGetComponent(out IInteractable interactable))
            {
                currentInteractable = interactable;
                if (promptTextUI != null)
                {
                    promptTextUI.text = $"[E] {interactable.GetPromptText()}";
                    promptTextUI.gameObject.SetActive(true);
                }
            }
            else
            {
                currentInteractable = null;
                if (promptTextUI != null) promptTextUI.gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}