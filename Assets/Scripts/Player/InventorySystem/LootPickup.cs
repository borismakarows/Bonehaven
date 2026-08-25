using UnityEngine;

namespace BoneHaven
{
    public enum PickupType { Consumable, WeaponUnlock }
    public enum LootType { Gunpowder, Ammo, Health }

    [RequireComponent(typeof(SphereCollider))]
    public class LootPickup : MonoBehaviour
    {
        [Header("Pickup Mode")]
        public PickupType pickupType = PickupType.Consumable;

        [Header("Consumable Settings")]
        [SerializeField] private LootType lootType = LootType.Gunpowder;
        [SerializeField] private int amount = 1;
        [SerializeField] private float healthRestoreAmount = 35f;

        [Header("Weapon Settings")]
        [SerializeField] private WeaponItem weaponToGrant;

        [Header("Visual Feedback")]
        [SerializeField] private float bobbingSpeed = 3f;
        [SerializeField] private float bobbingHeight = 0.12f;

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
            if (!other.CompareTag("Player")) return;

            if (pickupType == PickupType.Consumable)
            {
                if (lootType == LootType.Health)
                {
                    if (other.TryGetComponent(out PlayerStats playerHealth))
                    {
                      
                        playerHealth.Heal(healthRestoreAmount);
                        Destroy(gameObject);
                    }
                }
                else if (other.TryGetComponent(out PlayerInventory inv))
                {
                    bool collected = lootType switch
                    {
                        LootType.Gunpowder => inv.AddPowder(amount),
                        LootType.Ammo => inv.AddAmmo(amount),
                        _ => false
                    };

                    if (collected) Destroy(gameObject);
                }
            }
            else if (pickupType == PickupType.WeaponUnlock && weaponToGrant != null)
            {
                if (other.TryGetComponent(out PlayerInventory inv))
                {
                    inv.EquipWeapon(weaponToGrant);
                    Destroy(gameObject);
                }
            }
        }
    }
}