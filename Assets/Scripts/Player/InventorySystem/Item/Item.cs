using UnityEngine;

//Item types for filtering
public enum ItemTypes
{
    none,
    Sword,
    Throwable,
    Drinkable,
    Pistol,
    Artifact,
}

//Item Scriptable Object

public abstract class Item : ScriptableObject
{
    [SerializeField] string id;
    public string itemName;
    public abstract ItemTypes Type{get;}
    public Sprite itemImage;

    protected void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = System.Guid.NewGuid().ToString();
        }
    }

    public string GetId()
    {
        return id;
    }

    public ItemTypes GetItemType()
    {
        return Type;
    }
}





