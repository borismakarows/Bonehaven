using System.Collections;
using UnityEngine;

namespace BoneHaven
{
    public class PooledObject : MonoBehaviour
    {
        [SerializeField] private string poolTag;
        [SerializeField] private float autoReturnDelay = 0f;

        private Coroutine returnRoutine;

        public string PoolTag => poolTag;

        public void SetPoolTag(string tag) => poolTag = tag;

        private void OnEnable()
        {
            if (autoReturnDelay > 0f)
            {
                if (returnRoutine != null) StopCoroutine(returnRoutine);
                returnRoutine = StartCoroutine(AutoReturnRoutine());
            }
        }

        private IEnumerator AutoReturnRoutine()
        {
            yield return new WaitForSeconds(autoReturnDelay);
            ReturnToPool();
        }

        public void ReturnToPool()
        {
            if (returnRoutine != null)
            {
                StopCoroutine(returnRoutine);
                returnRoutine = null;
            }

            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
    }
}