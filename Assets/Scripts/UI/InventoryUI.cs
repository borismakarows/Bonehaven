using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    private VisualElement _groupBox;


    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _groupBox = root.Q<VisualElement>("GroupBox");
        
        InventoryManager.OnInventoryChanged += RefreshUI;

        RefreshUI();
    }

    void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= RefreshUI;
    }

    private void RefreshUI()
    { 
        _groupBox.Clear();

        foreach(Item item in inventory.items)
        {
            Button btn = new Button();

            btn.AddToClassList("button");

            btn.text = item.itemName;
            if (item.itemImage != null) {btn.style.backgroundImage = new StyleBackground(item.itemImage);}

            btn.style.marginRight = 5;
            btn.style.marginBottom = 5;

            btn.clicked += () => OnItemClicked(item);
            _groupBox.Add(btn);
        }
    }

    private void OnItemClicked(Item clickedItem)
    {
        Debug.Log($"Clicked To Item: {clickedItem.itemName}");
    }

}
