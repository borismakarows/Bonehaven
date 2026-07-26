using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] InventoryUI inventoryUI;
    bool isInventoryOn; 
    

    void Awake()
    {
        inventoryUI.gameObject.SetActive(false);
        isInventoryOn = false;
    }

    void OnEnable()
    {
        PlayerInputManager.OnInventoryInterfaceOpened += ToggleInventoryUI;
    }

    void OnDisable()
    {
        PlayerInputManager.OnInventoryInterfaceOpened -= ToggleInventoryUI;
    }

    private void ToggleInventoryUI()
    {
        if (!isInventoryOn) {inventoryUI.gameObject.SetActive(true);}
        else {inventoryUI.gameObject.SetActive(false);}
        isInventoryOn = !isInventoryOn;
    }
}
