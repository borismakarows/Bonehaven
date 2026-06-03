using System.Collections.Generic;
using UnityEngine;

//Inventory Holder
[CreateAssetMenu(fileName = "Inventory", menuName = "Inventory System/Inventory")]

// you can store different inventories for different sequences or levels
public class Inventory : ScriptableObject
{
    public List<Item> items;
}


