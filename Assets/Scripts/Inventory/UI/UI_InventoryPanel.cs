using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryPanel : MonoBehaviour
{
    [Header("物品栏配置")]
    private SingleSlotPanel[] slots; // 使用SingleSlotPanel代替ItemSlot
    private InventoryManager inventory;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // 获取InventoryManager实例
        inventory = GameStateManager.Instance.Inventory;
        if (inventory == null)
        {
            Debug.LogWarning("[UI_InventoryPanel] InventoryManager not found");
            return;
        }

        // 初始化所有SingleSlotPanel
        InitializeSlots();
        Debug.Log($"[UI_InventoryPanel] Inventory panel initialized.");

        // 刷新UI显示
        EventManager.Instance.Subscribe<OnInventoryInitialized>(Refresh);

        // 订阅物品栏变化事件
        EventManager.Instance.Subscribe<OnInventoryChanged>(HandleInventoryChanged);
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe<OnInventoryInitialized>(Refresh);
        EventManager.Instance.Unsubscribe<OnInventoryChanged>(HandleInventoryChanged);
    }

    // 初始化所有物品槽
    private void InitializeSlots()
    {
        slots = GetComponentsInChildren<SingleSlotPanel>();

        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("[UI_InventoryPanel] No slots assigned");
            return;
        }

        var allItems = ItemDatabase.Instance.GetAllItemSOs();
        if (slots.Length != allItems.Count)
        {
            Debug.LogWarning("[UI_InventoryPanel] Slot count does not match item count in item database");
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].Initialize(allItems[i], i);
            }
        }
    }

    // 刷新整个物品栏UI
    public void Refresh(OnInventoryInitialized _)
    {
        if (inventory == null || GameStateManager.Instance == null)
        {
            Debug.LogWarning("[UI_InventoryPanel] InventoryManager or GameStateManager not initialized");
            return;
        }

        var gameData = GameStateManager.Instance.currentData;
        if (gameData == null)
        {
            Debug.LogWarning("[UI_InventoryPanel] GameData is null");
            return;
        }

        // 刷新所有物品槽
        for (int i = 0; i < slots.Length && i < gameData.inventorySlots.Count; i++)
        {
            if (slots[i] != null)
            {
                slots[i].UpdateSlot(gameData.inventorySlots[i]);
            }
            else
            {
                Debug.LogError($"[UI_InventoryPanel] Slot {i} is null");
            }
        }

        Debug.Log($"[UI_InventoryPanel] Inventory panel refreshed.]");
    }

    // 处理物品栏变化事件
    private void HandleInventoryChanged(OnInventoryChanged eventData)
    {
        // 只刷新变化的槽位，而不是整个面板
        if (inventory == null || GameStateManager.Instance == null)
        {
            Debug.LogWarning("[UI_InventoryPanel] InventoryManager or GameStateManager not initialized");
            return;
        }

        var gameData = GameStateManager.Instance.currentData;
        if (gameData == null)
        {
            Debug.LogWarning("[UI_InventoryPanel] GameData is null");
            return;
        }

        // 只刷新变化的槽位
        foreach (int slotIndex in eventData.updatedSlotIndexes)
        {
            if (slotIndex >= 0 && slotIndex < slots.Length && slotIndex < gameData.inventorySlots.Count && slots[slotIndex] != null)
            {
                slots[slotIndex].UpdateSlot(gameData.inventorySlots[slotIndex]);
            }
        }
    }
}
