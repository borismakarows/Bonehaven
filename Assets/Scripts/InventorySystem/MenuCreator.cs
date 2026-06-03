using UnityEditor;
using UnityEditor.EventSystems;
using UnityEngine;


public class MenuCreator
{
    private const string InventoryFileDirectoryPath = "Resources/Inventory System/InventorySettings/";
    private const string InventoryFile = "NewInventory.asset";

    private const string ItemFileDirectoryPath = "Resources/Inventory System/Items/";
    private const string ItemFile = "NewItem.asset";

#region Inventory Creator
    //Inventory Creator
    [MenuItem("Inventory System/New Inventory")]
    public static void CreateInventorySettingFile()
    {
        var inventorySettings = Editor.CreateInstance<Inventory>();
        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.dataPath,InventoryFileDirectoryPath));
        AssetDatabase.CreateAsset(inventorySettings, "Assets/" + InventoryFileDirectoryPath + InventoryFile);
    }
#endregion

#region Item creation

    [MenuItem("Inventory System/New Item/Sword")]
    public static void CreateSwordItemFile()
    {
        var newSword = Editor.CreateInstance<SwordItem>();
        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.dataPath, ItemFileDirectoryPath));
        string path = "Assets/" + ItemFileDirectoryPath + "Sword/" + "NewSword.asset";
        AssetDatabase.CreateAsset(newSword, path);
    }

    [MenuItem("Inventory System/New Item/Pistol")]
    public static void CreatePistolItemFile()
    {
        var newPistol = Editor.CreateInstance<PistolItem>();
        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.dataPath, ItemFileDirectoryPath));
        string path = "Assets/" + ItemFileDirectoryPath + "Pistol/" + "NewPistol.asset";
        AssetDatabase.CreateAsset(newPistol,path);
    }

    [MenuItem("Inventory System/New Item/Throwable")]
    public static void CreateThrowableItemFile()
    {
        var NewThrowable = Editor.CreateInstance<ThrowableItem>();
        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.dataPath,ItemFileDirectoryPath));
        string path = "Assets/" + ItemFileDirectoryPath + "Throwable/" + "NewThrowable.asset";
        AssetDatabase.CreateAsset(NewThrowable,path);
    }

    [MenuItem("Inventory System/New Item/Drinkable")]
    public static void CreateDrinkableItemFile()
    {
        var NewDrinkable = Editor.CreateInstance<DrinkableItem>();
        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.dataPath,ItemFileDirectoryPath));
        string path = "Assets/" + ItemFileDirectoryPath + "Drinkable/" + "NewDrinkable.asset";
        AssetDatabase.CreateAsset(NewDrinkable,path);
    }

    [MenuItem("Inventory System/New Item/Artifact")]
    public static void CreateArtifactItemFile()
    {
        var newArtifact = Editor.CreateInstance<ArtifactItem>();
        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.dataPath,ItemFileDirectoryPath));
        string path = "Assets/" + ItemFileDirectoryPath + "Artifact/" + "NewArtifact.asset";
        AssetDatabase.CreateAsset(newArtifact,path);
    }

#endregion
}
