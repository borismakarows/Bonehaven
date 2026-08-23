using UnityEngine;

namespace BoneHaven
{
    [CreateAssetMenu(fileName = "NewPistol", menuName = "Equipment/Weapons/Pistol")]
    public class PistolWeaponItem : WeaponItem
    {
        public float range = 18f;

        private void Awake()
        {
            slotType = WeaponSlotType.RangedPistol;
        }
    }
}