using UnityEngine;

namespace BoneHaven
{
    public enum LootType { Gunpowder, Ammo, Health }

    [RequireComponent(typeof(SphereCollider))]
    public class LootPickup : MonoBehaviour
    {
        [SerializeField] private LootType lootType;
        [SerializeField] private int amount = 1;
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
                // Hook to your PlayerInventory / Stats manager here
                // e.g., other.GetComponent<PlayerInventory>().AddLoot(lootType, amount);
                Destroy(gameObject);
            }
        }
    }
}