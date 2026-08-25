using System.Collections;
using UnityEngine;

namespace BoneHaven
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider), typeof(AudioSource))]
    public class BombProjectile : MonoBehaviour
    {
        [Header("Fuse & Explosion Timing")]
        [SerializeField] private float fuseTime = 2.5f;
        [SerializeField] private float explosionRadius = 2.5f;
        [SerializeField] private float explosionDamage = 20f;
        [SerializeField] private LayerMask damageableLayers;

        [Header("Effects & Audio")]
        [SerializeField] private GameObject explosionVFXPrefab;
        [SerializeField] private AudioClip fuseBurnSFX;
        [SerializeField] private AudioClip explosionSFX;

        private Rigidbody rb;
        private SphereCollider bombCollider;
        private AudioSource audioSource;
        private PooledObject pooledObject;
        private Coroutine fuseRoutine;
        private bool hasExploded = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            bombCollider = GetComponent<SphereCollider>();
            audioSource = GetComponent<AudioSource>();
            pooledObject = GetComponent<PooledObject>();

            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            audioSource.playOnAwake = false;
            audioSource.loop = true;
        }

        private void OnEnable()
        {
            hasExploded = false;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        private void OnDisable()
        {
            if (fuseRoutine != null)
            {
                StopCoroutine(fuseRoutine);
                fuseRoutine = null;
            }
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        public void Launch(Vector3 targetPosition, Collider throwerCollider, float flightDuration = 1.0f)
        {
            if (throwerCollider != null && bombCollider != null)
            {
                Physics.IgnoreCollision(throwerCollider, bombCollider, true);
            }

            Vector3 startPos = transform.position;
            Vector3 displacement = targetPosition - startPos;
            Vector3 displacementXZ = new Vector3(displacement.x, 0f, displacement.z);

            float distanceXZ = displacementXZ.magnitude;
            float time = Mathf.Max(0.5f, flightDuration);

            float speedXZ = distanceXZ / time;
            Vector3 velocityXZ = displacementXZ.normalized * speedXZ;

            float gravity = Mathf.Abs(Physics.gravity.y);
            float velocityY = (displacement.y + 0.5f * gravity * (time * time)) / time;

            Vector3 finalVelocity = velocityXZ + Vector3.up * velocityY;

            rb.linearVelocity = finalVelocity;
            rb.AddTorque(Random.insideUnitSphere * 6f, ForceMode.Impulse);

            if (fuseRoutine != null) StopCoroutine(fuseRoutine);
            fuseRoutine = StartCoroutine(FuseCountdownRoutine());
        }

        private IEnumerator FuseCountdownRoutine()
        {
            if (fuseBurnSFX != null && audioSource != null)
            {
                audioSource.clip = fuseBurnSFX;
                audioSource.Play();
            }

            yield return new WaitForSeconds(fuseTime);

            Explode();
        }

        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;

            CombatJuiceManager.Instance?.TriggerScreenShake(0.5f);

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            if (explosionSFX != null)
            {
                AudioSource.PlayClipAtPoint(explosionSFX, transform.position, 1.0f);
            }

            if (explosionVFXPrefab != null)
            {
                Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayers);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player") && hit.TryGetComponent(out IDamageable damageable))
                {
                    Vector3 dir = (hit.transform.position - transform.position).normalized;
                    damageable.TakeDamage(explosionDamage, hit.bounds.center, dir);
                }
            }

            // Return to pool
            if (pooledObject != null)
            {
                pooledObject.ReturnToPool();
            }
            else if (ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.ReturnToPool("BombProjectile", gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}