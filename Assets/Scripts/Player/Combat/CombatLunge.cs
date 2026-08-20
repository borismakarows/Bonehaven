using System;
using System.Collections;
using UnityEngine;

namespace BoneHaven
{
    [RequireComponent(typeof(CharacterController))]
    public class CombatLunge : MonoBehaviour
    {
        [Header("Lunge Configuration")]
        [SerializeField] private float strikeDistance = 1.3f;
        [SerializeField] private float lungeDuration = 0.12f;
        [SerializeField] private float minDashLungeDistance = 1.8f; // Triggers dash leg animation if target is further than this

        public event Action OnLungeDashTriggered; // Event for Animation Controller

        private CharacterController controller;
        private Coroutine currentLungeRoutine;

        private void Reset()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Awake()
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
                transform.rotation = Quaternion.LookRotation(fallbackDirection);
            }
        }

        private IEnumerator LungeRoutine(Transform target)
        {
            float elapsed = 0f;
            Vector3 startPosition = transform.position;

            Vector3 toTarget = target.position - startPosition;
            toTarget.y = 0f;
            float totalDistance = toTarget.magnitude;

            // Trigger lower-body slide/dash animation if closing a noticeable gap
            if (totalDistance > minDashLungeDistance)
            {
                OnLungeDashTriggered?.Invoke();
            }

            Vector3 targetPosition = target.position - (toTarget.normalized * strikeDistance);
            targetPosition.y = startPosition.y;

            if (toTarget.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(toTarget.normalized);
            }

            while (elapsed < lungeDuration)
            {
                if (target == null) break;

                elapsed += Time.deltaTime;
                float t = elapsed / lungeDuration;

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