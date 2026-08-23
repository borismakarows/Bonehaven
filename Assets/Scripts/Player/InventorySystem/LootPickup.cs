using UnityEngine;

namespace BoneHaven
{
    [RequireComponent(typeof(SphereCollider))]
    public class LootPickup : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private Item itemData;

        [Header("Animation")]
        [SerializeField] private float bobbingSpeed = 2.5f;
        [SerializeField] private float bobbingHeight = 0.15f;

        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;
            GetComponent<SphereCollider>().isTrigger = true;
        }

        private void Update()
        {
            transform.position = startPos + Vector3.up * (Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight);
            transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (InventoryManager.Instance != null && itemData != null)
                {
                    InventoryManager.Instance.AddItem(itemData);
                }

                Destroy(gameObject);
            }
        }
    }
}