using UnityEngine;
using TMPro;

namespace BoneHaven
{
    public class CombatHUD : MonoBehaviour
    {
        [Header("Resource Counters")]
        [SerializeField] private TextMeshProUGUI powderText;
        [SerializeField] private TextMeshProUGUI ammoText;

        [Header("Weapon Labels")]
        [SerializeField] private TextMeshProUGUI equippedSwordText;
        [SerializeField] private TextMeshProUGUI equippedPistolText;

        private void OnEnable()
        {
            PlayerInventory.OnPowderChanged += UpdatePowderUI;
            PlayerInventory.OnAmmoChanged += UpdateAmmoUI;
            PlayerInventory.OnSwordEquipped += UpdateSwordUI;
            PlayerInventory.OnPistolEquipped += UpdatePistolUI;
        }

        private void OnDisable()
        {
            PlayerInventory.OnPowderChanged -= UpdatePowderUI;
            PlayerInventory.OnAmmoChanged -= UpdateAmmoUI;
            PlayerInventory.OnSwordEquipped -= UpdateSwordUI;
            PlayerInventory.OnPistolEquipped -= UpdatePistolUI;
        }

        private void UpdatePowderUI(int current, int max)
        {
            if (powderText != null) powderText.text = $"Powder: {current}/{max}";
        }

        private void UpdateAmmoUI(int current, int max)
        {
            if (ammoText != null) ammoText.text = $"Ammo: {current}/{max}";
        }

        private void UpdateSwordUI(SwordWeaponItem sword)
        {
            if (equippedSwordText != null && sword != null)
                equippedSwordText.text = sword.weaponName;
        }

        private void UpdatePistolUI(PistolWeaponItem pistol)
        {
            if (equippedPistolText != null && pistol != null)
                equippedPistolText.text = pistol.weaponName;
        }
    }
}