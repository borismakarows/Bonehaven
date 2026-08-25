using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoneHaven
{
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Equipped Weapons (ScriptableObjects)")]
        [SerializeField] private SwordWeaponItem equippedSword;
        [SerializeField] private PistolWeaponItem equippedPistol;
        public List<WeaponItem> ownedWeapons = new List<WeaponItem>();

        [Header("Consumable Caps & Initial Values")]
        [SerializeField] private int maxPowder = 3;
        [SerializeField] private int currentPowder = 1;

        [SerializeField] private int maxAmmo = 6;
        [SerializeField] private int currentAmmo = 3;

        // Observer Pattern Events
        public static event Action<SwordWeaponItem> OnSwordEquipped;
        public static event Action<PistolWeaponItem> OnPistolEquipped;
        public static event Action<int, int> OnPowderChanged;
        public static event Action<int, int> OnAmmoChanged;

        // Public Properties
        public int CurrentPowder => currentPowder;
        public int CurrentAmmo => currentAmmo;
        public SwordWeaponItem EquippedSword => equippedSword;
        public PistolWeaponItem EquippedPistol => equippedPistol;

        private void Start()
        {
            NotifyAll();
        }

        public void NotifyAll()
        {
            OnPowderChanged?.Invoke(currentPowder, maxPowder);
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
            if (equippedSword != null) OnSwordEquipped?.Invoke(equippedSword);
            if (equippedPistol != null) OnPistolEquipped?.Invoke(equippedPistol);
        }

        #region Weapon Swapping

        public void EquipWeapon(WeaponItem newWeapon)
        {
            if (newWeapon == null) return;

            if (newWeapon is SwordWeaponItem sword)
            {
                equippedSword = sword;
                OnSwordEquipped?.Invoke(equippedSword);
            }
            else if (newWeapon is PistolWeaponItem pistol)
            {
                equippedPistol = pistol;
                OnPistolEquipped?.Invoke(equippedPistol);
            }

            if (!ownedWeapons.Contains(newWeapon))
            {
                ownedWeapons.Add(newWeapon);
            }
        }

        #endregion

        #region Resource Consumption

        public bool TryConsumePowder()
        {
            if (currentPowder <= 0) return false;
            currentPowder--;
            OnPowderChanged?.Invoke(currentPowder, maxPowder);
            return true;
        }

        public bool TryConsumeAmmo()
        {
            if (currentAmmo <= 0) return false;
            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
            return true;
        }

        #endregion

        #region Resource Addition

        public bool AddPowder(int amount)
        {
            if (currentPowder >= maxPowder) return false;
            currentPowder = Mathf.Min(maxPowder, currentPowder + amount);
            OnPowderChanged?.Invoke(currentPowder, maxPowder);
            return true;
        }

        public bool AddAmmo(int amount)
        {
            if (currentAmmo >= maxAmmo) return false;
            currentAmmo = Mathf.Min(maxAmmo, currentAmmo + amount);
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
            return true;
        }

        #endregion
    }
}