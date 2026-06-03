using System;
using Unity.Loading;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    Inventory inventory;
    private const string InventoryPath = "InventorySettings/InventorySettings";
    public static event Action OnInventoryChanged;


    void OnEnable()
    {
        StarterAssetsInputs.OnInventoryInterfaceOpened += LoadInventory;
    }

    void OnDisable()
    {
        StarterAssetsInputs.OnInventoryInterfaceOpened -= LoadInventory;
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

    //Removes the Last Item
    public void RemoveLastItem()
    {
        int lastIndex = inventory.items.Count - 1;
        if (lastIndex<0) return;
        Item  temp = inventory.items[^1];
        OnInventoryChanged?.Invoke();
        inventory.items.RemoveAt(lastIndex);
    }

    public void RemoveFirstItem()
    {
        if(inventory.items.Count == 0) return;
        Item temp = inventory.items[0];
        OnInventoryChanged?.Invoke();
        inventory.items.RemoveAt(0);
    }

    public void CleanInventory()
    {
        int Count = inventory.items.Count;
        while(Count>0)
        {
            RemoveLastItem();
        }
    }
}
