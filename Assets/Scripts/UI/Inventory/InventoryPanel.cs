using UnityEngine;

public class InventoryPanel : MonoBehaviour
{
    public ItemSlot[] slots;
    private InventoryManager inventory;

    void Start()
    {
        inventory = FindObjectOfType<InventoryManager>();
        Refresh();
    }

    public void Refresh()
    {
        if (inventory == null || GameStateManager.Instance == null) return;
        
        var gameData = GameStateManager.Instance.currentData;
        if (gameData == null) return;
        
        for (int i = 0; i < slots.Length && i < gameData.inventorySlots.Count; i++)
        {
            var slot = gameData.inventorySlots[i];
            ItemSO item = null;
            bool collected = false;
            
            if (!slot.IsEmpty())
            {
                item = ItemDatabase.Instance.GetItemSO(slot.itemID);
                collected = slot.quantity > 0;
            }
            
            slots[i].SetItem(item, collected);
        }
    }
}