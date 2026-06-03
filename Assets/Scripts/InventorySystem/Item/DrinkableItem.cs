using UnityEngine;


[CreateAssetMenu(fileName = "Drinkable", menuName = "Item/New Drinkable")]
public class DrinkableItem : Item
{
    public override ItemTypes Type => ItemTypes.Drinkable;
    //Effect will come here
    public int amount;
}
