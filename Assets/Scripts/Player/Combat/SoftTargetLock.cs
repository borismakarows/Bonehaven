using UnityEngine;

namespace BoneHaven
{
    public class SoftTargetLock : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 7f;
        [SerializeField] private float maxLockAngle = 70f;
        [SerializeField] private float playerHeightOffset = 1.0f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask obstructionLayer = ~0; // Set to Default / Environment

        private Transform currentTarget;

        /// <summary>
        /// Finds the best target based on player position, camera-relative input direction, and line-of-sight.
        /// </summary>
        public Transform GetTarget(Vector3 inputVector, Transform cameraTransform)
        {
            Vector3 playerOrigin = transform.position + Vector3.up * playerHeightOffset;
            Vector3 aimDirection;
            bool hasDirectionalInput = inputVector.sqrMagnitude > 0.05f;

            if (hasDirectionalInput && cameraTransform != null)
            {
                // Project camera forward/right onto the horizontal XZ plane
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                aimDirection = (camForward.normalized * inputVector.z + camRight.normalized * inputVector.x).normalized;
            }
            else
            {
                // Neutral input: Stick to current valid target or fallback to player's current forward
                if (currentTarget != null && IsTargetValid(currentTarget, playerOrigin))
                {
                    return currentTarget;
                }
                aimDirection = transform.forward;
            }

            // Query potential targets around the player
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
            Transform bestTarget = null;
            float bestScore = float.MinValue;

            foreach (var col in hits)
            {
                Vector3 enemyCenter = col.bounds.center;
                Vector3 toEnemy = enemyCenter - playerOrigin;
                
                // Horizontal vector for angle check
                Vector3 toEnemyHorizontal = new Vector3(toEnemy.x, 0f, toEnemy.z);
                float distance = toEnemyHorizontal.magnitude;

                if (distance < 0.1f) continue;

                Vector3 dirToEnemy = toEnemyHorizontal.normalized;
                float angle = Vector3.Angle(aimDirection, dirToEnemy);

                // Discard targets outside the frontal acquisition cone
                if (angle > maxLockAngle) continue;

                // Line of Sight Check: Ensure no wall or pillar blocks the ray from player to enemy
                if (Physics.Raycast(playerOrigin, toEnemy.normalized, out RaycastHit hit, distance, obstructionLayer))
                {
                    // If the ray hit something that isn't the enemy (or part of the enemy hierarchy), ignore it
                    if (hit.transform != col.transform && !hit.transform.IsChildOf(col.transform))
                    {
                        continue;
                    }
                }

                // Arkham Scoring: 60% angle alignment + 40% distance proximity from player
                float angleScore = Mathf.Cos(angle * Mathf.Deg2Rad);
                float distanceScore = 1f - (distance / detectionRadius);
                float totalScore = (angleScore * 0.6f) + (distanceScore * 0.4f);

                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestTarget = col.transform;
                }
            }

            currentTarget = bestTarget;
            return currentTarget;
        }

        private bool IsTargetValid(Transform target, Vector3 playerOrigin)
        {
            if (!target.gameObject.activeInHierarchy) return false;
            
            Vector3 diff = target.position - transform.position;
            diff.y = 0f;
            if (diff.sqrMagnitude > (detectionRadius * detectionRadius)) return false;

            // Verify line of sight is still clear
            Vector3 targetCenter = target.position + Vector3.up * playerHeightOffset;
            Vector3 dir = targetCenter - playerOrigin;
            if (Physics.Raycast(playerOrigin, dir.normalized, out RaycastHit hit, dir.magnitude, obstructionLayer))
            {
                if (hit.transform != target && !hit.transform.IsChildOf(target)) return false;
            }

            return true;
        }

        public void ClearTarget() => currentTarget = null;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + Vector3.up * playerHeightOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, detectionRadius);

            // Draw forward acquisition cone
            Vector3 forward = transform.forward * detectionRadius;
            Quaternion leftRot = Quaternion.Euler(0, -maxLockAngle, 0);
            Quaternion rightRot = Quaternion.Euler(0, maxLockAngle, 0);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + leftRot * forward);
            Gizmos.DrawLine(origin, origin + rightRot * forward);
        }
#endif
    }
}