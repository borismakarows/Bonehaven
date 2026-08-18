using System;
using UnityEngine;


namespace BoneHaven
{
   public class InventoryManager : MonoBehaviour
    {
        Inventory inventory;
        private const string InventoryPath = "InventorySettings/InventorySettings";
        public static event Action OnInventoryChanged;


        void OnEnable()
        {
            PlayerInputManager.OnInventoryInterfaceOpened += LoadInventory;
        }

        void OnDisable()
        {
            PlayerInputManager.OnInventoryInterfaceOpened -= LoadInventory;
        }

        //Loads the saved Inventory
        private void LoadInventory()
        {
            inventory = Resources.Load<Inventory>(InventoryPath);
            OnInventoryChanged?.Invoke();
        }

        //Get all the names of items
        private void AllItemNames()
        {   
            foreach(Item item in inventory.items)
            {
                Debug.Log(item.name);
            }
        }

        private void AddItem(Item newItem)
        {
        
        }
    } 
}

