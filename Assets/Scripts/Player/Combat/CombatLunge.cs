using System.Collections;
using UnityEngine;

namespace BoneHaven
{
    [RequireComponent(typeof(CharacterController))]
    public class CombatLunge : MonoBehaviour
    {
        [Header("Lunge Configuration")]
        private CharacterController controller;
        [SerializeField] private float strikeDistance = 1.3f;
        [SerializeField] private float lungeDuration = 0.12f;

        private Coroutine currentLungeRoutine;

        private void Reset()
        {
            controller = GetComponent<CharacterController>();
        }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        /// <summary>
        /// Executes a lunge toward a target, or snaps rotation to the fallback input direction on a whiff.
        /// </summary>
        public void ExecuteLunge(Transform target, Vector3 fallbackDirection)
        {
            if (currentLungeRoutine != null)
            {
                StopCoroutine(currentLungeRoutine);
            }

            if (target != null)
            {
                currentLungeRoutine = StartCoroutine(LungeRoutine(target));
            }
            else if (fallbackDirection.sqrMagnitude > 0.05f)
            {
                // Whiff swing: Snap rotation to the intended attack direction
                transform.rotation = Quaternion.LookRotation(fallbackDirection);
            }
        }

        private IEnumerator LungeRoutine(Transform target)
        {
            float elapsed = 0f;
            Vector3 startPosition = transform.position;

            // Calculate stopping position in front of the target
            Vector3 toTarget = target.position - startPosition;
            toTarget.y = 0f;

            Vector3 targetPosition = target.position - (toTarget.normalized * strikeDistance);
            targetPosition.y = startPosition.y;

            // Immediate snap rotation facing the enemy
            if (toTarget.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(toTarget.normalized);
            }

            while (elapsed < lungeDuration)
            {
                // If the enemy moves or dies mid-lunge, safely break
                if (target == null) break;

                elapsed += Time.deltaTime;
                float t = elapsed / lungeDuration;

                // SmoothStep curve gives an instant punch-in feel
                Vector3 desiredPos = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0f, 1f, t));
                Vector3 motion = desiredPos - transform.position;

                controller.Move(motion);
                yield return null;
            }

            currentLungeRoutine = null;
        }

        public void CancelLunge()
        {
            if (currentLungeRoutine != null)
            {
                StopCoroutine(currentLungeRoutine);
                currentLungeRoutine = null;
            }
        }
    }
}