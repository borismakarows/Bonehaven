using UnityEngine;

[CreateAssetMenu(fileName = "Throwable", menuName = "Item/New Throwable")]
public class ThrowableItem : Item
{
    public override ItemTypes Type => ItemTypes.Throwable;
    public int damage;
    public bool AOE;
    public int amount;
    public bool AOERange;
    public float throwTime;
}
