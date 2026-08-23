using System.Collections;
using UnityEngine;

namespace BoneHaven
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider), typeof(AudioSource))]
    public class BombProjectile : MonoBehaviour
    {
        [Header("Fuse & Explosion Timing")]
        [SerializeField] private float fuseTime = 2.5f; // Bombanın patlamadan önceki geri sayım süresi
        [SerializeField] private float explosionRadius = 2.5f;
        [SerializeField] private float explosionDamage = 20f;
        [SerializeField] private LayerMask damageableLayers;

        [Header("Effects & Audio")]
        [SerializeField] private GameObject explosionVFXPrefab;
        [SerializeField] private AudioClip fuseBurnSFX; // Fitil yanma sesi (döngüde çalar)
        [SerializeField] private AudioClip explosionSFX; // Patlama sesi

        private Rigidbody rb;
        private SphereCollider bombCollider;
        private AudioSource audioSource;
        private bool hasExploded = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            bombCollider = GetComponent<SphereCollider>();
            audioSource = GetComponent<AudioSource>();

            rb.useGravity = true;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Fitil ses ayarları
            audioSource.playOnAwake = false;
            audioSource.loop = true;
        }

        public void Launch(Vector3 targetPosition, Collider throwerCollider, float flightDuration = 1.0f)
        {
            // Fırlatan düşman ile bombanın birbirine takılmasını engelle
            if (throwerCollider != null && bombCollider != null)
            {
                Physics.IgnoreCollision(throwerCollider, bombCollider, true);
            }

            // Parabolik hız hesabı
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

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = finalVelocity;

            // Havada takla atarak gitmesi için tork
            rb.AddTorque(Random.insideUnitSphere * 6f, ForceMode.Impulse);

            // Fitil sesini başlat ve geri sayım coroutine'ini çalıştır
            StartCoroutine(FuseCountdownRoutine());
        }

        private IEnumerator FuseCountdownRoutine()
        {
            if (fuseBurnSFX != null && audioSource != null)
            {
                audioSource.clip = fuseBurnSFX;
                audioSource.Play();
            }

            // Süre dolana kadar bekle (Çarpışmalar patlatmaz, bomba yerde sekip yuvarlanır)
            yield return new WaitForSeconds(fuseTime);

            Explode();
        }

        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;

            // Fitil sesini durdur
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            // Patlama sesini ayrı çal (Bomba yok olacağı için PlayClipAtPoint kullanılır)
            if (explosionSFX != null)
            {
                AudioSource.PlayClipAtPoint(explosionSFX, transform.position, 1.0f);
            }

            // Patlama görsel efekti
            if (explosionVFXPrefab != null)
            {
                Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
            }

            // Alan hasarı ve oyuncu kontrolü
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayers);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player") && hit.TryGetComponent(out IDamageable damageable))
                {
                    Vector3 dir = (hit.transform.position - transform.position).normalized;
                    damageable.TakeDamage(explosionDamage, hit.bounds.center, dir);
                }
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}