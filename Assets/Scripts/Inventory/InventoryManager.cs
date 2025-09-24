using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("物品栏配置")]
    [SerializeField] private float maxWeightCapacity = 10f;

    private Dictionary<string, int> itemIDtoSlotIndexMap = new Dictionary<string, int>();
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

        // 初始化物品栏位
        InitializeInventorySlots();

        EventManager.Instance.Publish(new OnInventoryInitialized());

        StartCoroutine(FreshnessDecayCoroutine());
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnregisterInventoryManager();
        }
    }

    #region ------- 物品栏公开方法 ---------

    // 向背包中添加指定数量的物品
    public bool AddItem(string itemID, int amount)
    {
        var itemSO = ItemDatabase.Instance.GetItemSO(itemID);
        if (itemSO == null || amount <= 0)
        {
            Debug.LogWarning($"ItemID: '{itemID}' not found in the ItemDatabse");
            return false;
        }

        float weightToAdd = itemSO.weight * amount;
        if (gameData.currentWeight + weightToAdd > maxWeightCapacity)
        {
            Debug.LogWarning("[InventoryManager] Can't add item as weight has exceeded the limit.");
            return false;
        }

        if (!itemIDtoSlotIndexMap.ContainsKey(itemID))
        {
            Debug.LogError($"Item: '{itemID}' doesn't have a slot. Check ItemDatabase.");
            return false;
        }

        int slotIndex = itemIDtoSlotIndexMap[itemID];
        var slot = gameData.inventorySlots[slotIndex];

        if (slot.quantity <= 0)
        {
            // 如果是新槽位, 初始化数值
            slot.SetItem(itemSO, amount);
            Debug.Log($"[InventoryManager] Add Item: {itemSO.itemName}, Quantity: {amount}, Index: {slotIndex}, Freshness: {slot.currentFreshness}");
        }
        else
        {
            // 更新数量, 新鲜度取平均值
            float oldTotalFreshness = slot.currentFreshness * slot.quantity;
            float newTotalFreshness = itemSO.maxFreshness * amount;
            slot.quantity += amount;
            slot.currentFreshness = (oldTotalFreshness + newTotalFreshness) / slot.quantity;
            Debug.Log($"[InventoryManager] Add Item: {itemSO.itemName}, Quantity: {amount}, Index: {slotIndex}, Freshness: {slot.currentFreshness}");
        }

        UpdateCurrentWeight(itemSO.weight * amount);
        RefreshSlotUIRequest(new List<int> { slotIndex });
        return true;
    }

    // // 在物品栏中Drop Item -> 移动/交换物品所在栏位
    // public void HandleDropOnSlot(int fromIndex, int toIndex)
    // {
    //     if (fromIndex < 0 || fromIndex >= gameData.inventorySlots.Count ||
    //         toIndex < 0 || toIndex >= gameData.inventorySlots.Count ||
    //         fromIndex == toIndex) return;

    //     var fromSlot = gameData.inventorySlots[fromIndex];
    //     var toSlot = gameData.inventorySlots[toIndex];

    //     (fromSlot.itemID, fromSlot.quantity) = (toSlot.itemID, toSlot.quantity);
    //     (fromSlot.quantity, toSlot.quantity) = (toSlot.quantity, fromSlot.quantity);

        // EventManager.Instance.Publish(new OnInventoryChanged 
        // {
        //     updatedSlotIndexes = new List<int> { fromIndex, toIndex }
        // });
    // }

    // 在角色上Drop Item -> 使用物品
    public void HandleDropOnCharacter(int slotIndex, string targetCharacterID)
    {
        if (slotIndex < 0 || slotIndex >= gameData.inventorySlots.Count) return;

        var slot = gameData.inventorySlots[slotIndex];
        if (slot.IsEmpty()) return;

        Debug.Log($"[InventoryManager] OnItemUseRequest Event Published. itemID: {slot.itemID}, itemFreshness: {slot.currentFreshness}");
        EventManager.Instance.Publish(new OnItemUseRequest
        {
            itemSO = ItemDatabase.Instance.GetItemSO(slot.itemID),
            targetCharacterID = targetCharacterID,
            itemFreshness = slot.currentFreshness
        });

        DecreaseItemQuantity(slotIndex);
    }

    // 在世界场景中Drop Item -> 丢弃物品
    public void HandleDropOnWorld(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= gameData.inventorySlots.Count) return;

        var slot = gameData.inventorySlots[slotIndex];
        if (slot.IsEmpty()) return;

        DecreaseItemQuantity(slotIndex);
    }

    #endregion

    #region ------- 角色技能SkillManager接口 ---------

    // 姥姥保鲜技能
    public bool RestoreItemRefreshness(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= gameData.inventorySlots.Count) return false;

        var slot = gameData.inventorySlots[slotIndex];
        if (slot.IsEmpty()) return false;

        var itemSO = ItemDatabase.Instance.GetItemSO(slot.itemID);

        // 恢复到最大新鲜度
        slot.currentFreshness = itemSO.maxFreshness;
        Debug.Log($"<color=green>[InventoryManager] Item '{itemSO.itemName}' has been refreshed!</color>");

        RefreshSlotUIRequest(new List<int> { slotIndex });
        return true;
    }

    // 奶奶烹饪技能
    public bool CookItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= gameData.inventorySlots.Count) return false;

        var slot = gameData.inventorySlots[slotIndex];
        if (slot.IsEmpty()) return false;

        var itemSO = GetItemSO(slotIndex);
        // 不需要烹饪, 或者已经烹饪过
        if (itemSO == null || itemSO.cookedVersion == null || !itemSO.cookNeeded)
        {
            Debug.Log($"[InventoryManager] Cook Failed. The item cannot be cooked or is already cooked.");
            return false;
        }

        DecreaseItemQuantity(slotIndex);
        AddItem(itemSO.cookedVersion.itemID, itemSO.stackAfterCook);
        Debug.Log($"<color=green>[InventoryManager] Cook Succeed. Consume 1 {itemSO.itemName} and recieve {itemSO.cookedVersion.stackAfterCook} {itemSO.cookedVersion.itemName}</color>");
        return true;
    }

    // 妈妈增加每个药品*1技能
    public void AddEachMedicine()
    {
        foreach (var item in gameData.inventorySlots)
        {
            var itemSO = ItemDatabase.Instance.GetItemSO(item.itemID);
            if (itemSO.itemType == ItemType.Medicine)
            {
                AddItem(itemSO.itemID, 1);
                UpdateCurrentWeight(itemSO.weight);
                RefreshSlotUIRequest(new List<int> { itemIDtoSlotIndexMap[itemSO.itemID] });
                Debug.Log($"<color=green>[InventoryManager] Medicine has been added: {itemSO.itemName}, Quantity: {item.quantity}</color>");
            }
        }
    }

    // 爸爸增加负重技能
    public void ModifyMaxWeightCapacity(float amount)
    {
        maxWeightCapacity += amount;
        Debug.Log($"<color=green>[InventoryManager] Current weight capacity: {maxWeightCapacity}</color>");
    }

    #endregion  --------- End ----------

    // 获取指定栏位的物品SO
    private ItemSO GetItemSO(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= gameData.inventorySlots.Count) return null;
        var slot = gameData.inventorySlots[slotIndex];
        if (slot.IsEmpty()) return null;
        return ItemDatabase.Instance.GetItemSO(slot.itemID);
    }

    // 减少指定栏位的物品数量
    public bool DecreaseItemQuantity(int slotIndex, int amount = 1)
    {
        var slot = gameData.inventorySlots[slotIndex];
        var itemSO = GetItemSO(slotIndex);
        if (slot.quantity < amount)
        {
            Debug.LogWarning("Item quantity is not enough to decrease");
            return false;
        }

        slot.quantity -= amount;
        if (slot.quantity <= 0)
        {
            slot.Reset();
        }

        UpdateCurrentWeight(-(itemSO.weight * amount));
        RefreshSlotUIRequest(new List<int> { slotIndex });
        return true;
    }

    // 发布指定物品栏位UI更新请求
    private void RefreshSlotUIRequest(List<int> slotIndex)
    {
        EventManager.Instance.Publish(new OnInventoryChanged
        {
            updatedSlotIndexes = slotIndex
        });
    }

    // 更新当前重量
    private void UpdateCurrentWeight(float weightDelta)
    {
        gameData.currentWeight += weightDelta;
    }

    // 物品栏新鲜值持续衰减协程
    private IEnumerator FreshnessDecayCoroutine()
    {
        var waitForOneSecond = new WaitForSeconds(1.0f);
        while (true)
        {
            yield return waitForOneSecond;
            ProcessFreshnessDecay(1.0f);
        }
    }

    // 物品栏新鲜值持续衰减
    private void ProcessFreshnessDecay(float deltaTime)
    {
        if (gameData == null || gameData.inventorySlots == null) return;

        for (int i = 0; i < gameData.inventorySlots.Count; i++)
        {
            var slot = gameData.inventorySlots[i];
            if (slot.IsEmpty()) continue;

            var itemSO = GetItemSO(i);
            if (itemSO == null || itemSO.decayRate <= 0) continue;

            float effectiveDecayRate = itemSO.decayRate;
            if (slot.quantity > 1)
            {
                effectiveDecayRate /= slot.quantity;
            }

            slot.currentFreshness -= effectiveDecayRate * deltaTime;
            EventManager.Instance.Publish(new OnItemFreshnessChanged
            {
                slotIndex = i,
                currentFreshness = slot.currentFreshness
            });

            if (slot.currentFreshness <= 0)
            {
                UpdateCurrentWeight(-(itemSO.weight * slot.quantity));
                slot.Reset();

                RefreshSlotUIRequest(new List<int> { i });
            }
        }
    }

    // 初始化所有物品栏位
    private void InitializeInventorySlots()
    {
        var allItems = ItemDatabase.Instance.GetAllItemSOs();
        gameData.currentWeight = 0f;
        gameData.inventorySlots.Clear();
        itemIDtoSlotIndexMap.Clear();

        for (int i = 0; i < allItems.Count; i++)
        {
            var item = allItems[i];
            var newSlot = new InventorySlot
            {
                itemID = item.itemID,
                quantity = 0,
                currentFreshness = 0f
            };
            gameData.inventorySlots.Add(newSlot);
            itemIDtoSlotIndexMap[item.itemID] = i;
        }
    }

    // 物品栏数据同步
    public void SyncFromGameData()
    {
        gameData = GameStateManager.Instance.currentData;

        // 所有物品数量同步
        itemIDtoSlotIndexMap.Clear();
        var allItems = ItemDatabase.Instance.GetAllItemSOs();
        if (allItems.Count != gameData.inventorySlots.Count)
        {
            Debug.LogError("[InventoryManager] Item count mismatch");
            return;
        }

        for (int i = 0; i < allItems.Count; i++)
        {
            itemIDtoSlotIndexMap[allItems[i].itemID] = i;
        }

        // 更新物品栏UI
        var allIndexes = Enumerable.Range(0, gameData.inventorySlots.Count).ToList();
        RefreshSlotUIRequest(allIndexes);
        Debug.Log("[InventoryManager] Inventory data synced.");
    }
}
