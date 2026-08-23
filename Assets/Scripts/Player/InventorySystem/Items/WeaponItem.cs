using UnityEngine;

namespace BoneHaven
{
    public enum WeaponSlotType { MeleeSword, RangedPistol }

    public abstract class WeaponItem : ScriptableObject
    {
        [SerializeField] private string id;
        public string weaponName;
        public WeaponSlotType slotType;
        public Sprite icon;
        public GameObject weaponModelPrefab;
        public float baseDamage = 20f;
        public float attackSpeed = 1f;

        protected void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
            }
        }

        public string GetId() => id;
    }

}