using UnityEngine;

namespace BoneHaven
{
    [CreateAssetMenu(fileName = "NewSword", menuName = "Equipment/Weapons/Sword")]
    public class SwordWeaponItem : WeaponItem
    {
        public float heavyAttackMultiplier = 1.5f;

        private void Awake()
        {
            slotType = WeaponSlotType.MeleeSword;
        }
    }
}