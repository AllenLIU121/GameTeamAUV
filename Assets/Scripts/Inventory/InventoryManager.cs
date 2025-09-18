using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("物品栏配置")]
    [SerializeField] private int inventoryCapacity = 10;
    [SerializeField] private float maxWeightCapacity = 10f;

    private GameData gameData;

    private void Awake()
    {
        GameStateManager.Instance.RegisterInventoryManager(this);
    }

    private void Start()
    {
        if (GameStateManager.Instance != null)
        {
            gameData = GameStateManager.Instance.currentData;
            if (gameData == null)
            {
                Debug.LogError("[InventoryManager] GameData is null");
            }
        }
        else
        {
            Debug.LogError("[InventoryManager] GameStateManager is not initialized");
        }

        InitializeInventorySlots();
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnregisterInventoryManager();
        }
    }

    // -------------- 物品栏公开方法 --------------

    // 向背包中添加指定数量的物品
    public bool AddItem(string itemID, int quantity)
    {
        var itemSO = ItemDatabase.Instance.GetItemSO(itemID);
        if (itemSO == null || quantity <= 0)
            return false;

        float weightToAdd = itemSO.weight * quantity;
        if (gameData.currentWeight + weightToAdd > maxWeightCapacity)
        {
            Debug.LogWarning("[InventoryManager] Inventory is full]");
            return false;
        }

        int addedToSlot = -1;
        // 物品栏中已有此物品
        for (int i = 0; i < gameData.inventorySlots.Count; i++)
        {
            if (gameData.inventorySlots[i].itemID == itemID)
            {
                gameData.inventorySlots[i].quantity += quantity;
                addedToSlot = i;
                break;
            }
        }

        // 物品栏中没有此物品
        if (addedToSlot == -1)
        {
            for (int i = 0; i < gameData.inventorySlots.Count; i++)
            {
                if (gameData.inventorySlots[i].IsEmpty())
                {
                    gameData.inventorySlots[i].itemID = itemID;
                    gameData.inventorySlots[i].quantity = quantity;
                    addedToSlot = i;
                    break;
                }
            }
        }

        // 添加成功
        if (addedToSlot != -1)
        {
            UpdateCurrentWeight();
            EventManager.Instance.Publish(new OnInventoryChanged
            {
                updatedSlotIndexes = new List<int> { addedToSlot }
            });
            return true;
        }

        Debug.LogWarning("[InventoryManager] Failed to add item to inventory");
        return false;
    }

    // 在物品栏中Drop Item -> 移动/交换物品所在栏位
    public void HandleDropOnSlot(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= gameData.inventorySlots.Count ||
            toIndex < 0 || toIndex >= gameData.inventorySlots.Count ||
            fromIndex == toIndex) return;

        var fromSlot = gameData.inventorySlots[fromIndex];
        var toSlot = gameData.inventorySlots[toIndex];

        (fromSlot.itemID, fromSlot.quantity) = (toSlot.itemID, toSlot.quantity);
        (fromSlot.quantity, toSlot.quantity) = (toSlot.quantity, fromSlot.quantity);

        EventManager.Instance.Publish(new OnInventoryChanged
        {
            updatedSlotIndexes = new List<int> { fromIndex, toIndex }
        });
    }

    // 在角色上Drop Item -> 使用物品
    public void HandleDropOnCharacter(int slotIndex, string targetCharacterID)
    {
        if (slotIndex < 0 || slotIndex >= gameData.inventorySlots.Count) return;

        var slot = gameData.inventorySlots[slotIndex];
        if (slot.IsEmpty()) return;

        EventManager.Instance.Publish(new OnItemUseRequest
        {
            itemSO = ItemDatabase.Instance.GetItemSO(slot.itemID),
            targetCharacterID = targetCharacterID
        });

        slot.quantity--;
        if (slot.quantity <= 0)
        {
            slot.Clear();
        }

        UpdateCurrentWeight();
        EventManager.Instance.Publish(new OnInventoryChanged
        {
            updatedSlotIndexes = new List<int> { slotIndex }
        });
    }

    // 在世界场景中Drop Item -> 丢弃物品
    public void HandleDropOnWorld(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= gameData.inventorySlots.Count) return;

        var slot = gameData.inventorySlots[slotIndex];
        if (slot.IsEmpty()) return;

        slot.quantity -= quantity;
        if (slot.quantity <= 0)
        {
            slot.Clear();
        }

        UpdateCurrentWeight();
        EventManager.Instance.Publish(new OnInventoryChanged
        {
            updatedSlotIndexes = new List<int> { slotIndex }
        });
    }

    // -------------- End --------------

    private void UpdateCurrentWeight()
    {
        float totalWeight = 0f;
        foreach (var slot in gameData.inventorySlots)
        {
            if (!slot.IsEmpty())
            {
                var itemSO = ItemDatabase.Instance.GetItemSO(slot.itemID);
                totalWeight += itemSO.weight * slot.quantity;
            }
        }
        gameData.currentWeight = totalWeight;
    }

    private void InitializeInventorySlots()
    {
        while (gameData.inventorySlots.Count < inventoryCapacity)
        {
            gameData.inventorySlots.Add(new InventorySlot());
        }
    }
}
