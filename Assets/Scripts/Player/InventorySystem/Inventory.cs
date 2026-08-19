using System.Collections.Generic;
using UnityEngine;

//Inventory Holder
[CreateAssetMenu(fileName = "Inventory", menuName = "Inventory System/Inventory")]

// you can store different inventories for different sequences or levels
public class Inventory : ScriptableObject
{
    public List<Item> items;

    public void AddNewItem(Item newItem)
    {
        switch (newItem.GetItemType())
        {
            case ItemTypes.Sword:

            case ItemTypes.Throwable:
            case ItemTypes.Pistol:
            case ItemTypes.Drinkable:
            case ItemTypes.Artifact:
            default:
            break;
        }
    }

    private bool IsIdMatched(Item testItem)
    {
        string testId = testItem.GetId();

        foreach (Item item in items)
        {
            if (item.GetId() == testId)
            {
                Debug.Log("ID Matched");
                return true;
            }
        }
        return false;
    }
}



