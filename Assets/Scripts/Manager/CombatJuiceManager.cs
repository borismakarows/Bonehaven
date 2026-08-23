using System.Collections;
using UnityEngine;
using Unity.Cinemachine; // Or Cinemachine for older versions

namespace BoneHaven
{
    public class CombatJuiceManager : MonoBehaviour
    {
        public static CombatJuiceManager Instance { get; private set; }

        [SerializeField] private CinemachineImpulseSource impulseSource;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (impulseSource == null) impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        public void TriggerHitStop(float duration = 0.08f, float timeScale = 0.1f)
        {
            StartCoroutine(HitStopRoutine(duration, timeScale));
        }

        public void TriggerScreenShake(float force = 1f)
        {
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulse(force);
            }
        }

        private IEnumerator HitStopRoutine(float duration, float targetScale)
        {
            Time.timeScale = targetScale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1.0f;
        }
    }
}