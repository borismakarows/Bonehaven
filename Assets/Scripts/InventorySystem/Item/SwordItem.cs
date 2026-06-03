using UnityEngine;

[CreateAssetMenu(fileName = "Sword", menuName = "Item/New Sword")]
public class SwordItem : Item
{
    public override ItemTypes Type => ItemTypes.Sword;

    public int damage; 
    public int swingRange;
    public int swingSpeed;
}
