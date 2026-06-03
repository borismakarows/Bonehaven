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
        StarterAssetsInputs.OnInventoryInterfaceOpened += ToggleInventoryUI;
    }

    void OnDisable()
    {
        StarterAssetsInputs.OnInventoryInterfaceOpened -= ToggleInventoryUI;
    }

    private void ToggleInventoryUI()
    {
        if (!isInventoryOn) {inventoryUI.gameObject.SetActive(true);}
        else {inventoryUI.gameObject.SetActive(false);}
        isInventoryOn = !isInventoryOn;
    }
}
