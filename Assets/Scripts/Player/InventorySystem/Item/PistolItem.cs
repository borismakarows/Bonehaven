using UnityEngine;

[CreateAssetMenu(fileName = "Pistol", menuName = "Item/New Pistol")]
public class PistolItem : Item
{
    public override ItemTypes Type => ItemTypes.Pistol;
    public int damage;
    public int enemyDrag;
    public int capacity;
    [Range(0,100)]
    public int jamChance;
}
