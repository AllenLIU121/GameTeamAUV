using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SingleSlotPanel : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI stackNum;
    [SerializeField] private Image background; // 添加背景图片组件引用

    // 不同状态的背景图
    public Sprite emptySlotBackground;
    public Sprite filledSlotBackground;

    private int slotIndex;
    private InventoryManager inventoryManager;
    private ItemSO itemData;
    private int itemCount = 0;

    public void Initialize(int index, InventoryManager manager)
    {
        slotIndex = index;
        inventoryManager = manager;

        // 订阅物品栏变化事件
        EventManager.Instance.Subscribe<OnInventoryChanged>(HandleInventoryChanged);
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe<OnInventoryChanged>(HandleInventoryChanged);
    }

    public void UpdateSlot(InventorySlot slotData)
    {
        if (slotData == null)
        {
            // 保留原有的物品图片状态，但更新数据
            itemData = null;
            itemCount = 0;
        }
        else
        {
            itemData = ItemDatabase.Instance.GetItemSO(slotData.itemID);
            itemCount = slotData.quantity;
            // 确保有物品时显示图片
            if (itemImage != null && itemData != null)
            {
                itemImage.enabled = true;
                itemImage.sprite = itemData.itemIcon;
            }

            if (slotData.quantity >= 1)
            {
                stackNum.text = slotData.quantity.ToString();
                stackNum.enabled = true;
            }
            else
            {
                stackNum.text = "";
                stackNum.enabled = false;
            }
        }

        // 应用背景图
        ApplyBackground();
    }

    // 根据数量自动应用背景图
    private void ApplyBackground()
    {
        if (background != null)
        {
            background.sprite = itemCount > 0 ? filledSlotBackground : emptySlotBackground;
            background.enabled = true; // 始终启用背景图
        }
    }

    // 外部设置背景图 - 仅用于初始化，不应在运行时频繁调用
    public void SetBackground(Sprite emptyBackground, Sprite filledBackground)
    {
        emptySlotBackground = emptyBackground;
        filledSlotBackground = filledBackground;
        ApplyBackground();
    }

    // 只刷新槽位背景图片 - 不影响物品图片和文本
    public void RefreshSlotBackgroundOnly()
    {
        if (background != null)
        {
            // 仅根据当前itemCount状态刷新槽位背景
            background.sprite = itemCount > 0 ? filledSlotBackground : emptySlotBackground;
            background.enabled = true;
        }
        ApplyBackground();
    }

    public ItemSO GetItemData()
    {
        return itemData;
    }

    // 获取物品数量
    public int GetItemCount()
    {
        return itemCount;
    }

    // 处理物品栏变化事件
    private void HandleInventoryChanged(OnInventoryChanged eventData)
    {
        if (inventoryManager == null || GameStateManager.Instance == null)
            return;

        var gameData = GameStateManager.Instance.currentData;
        if (gameData == null || gameData.inventorySlots.Count <= slotIndex)
            return;

        // 检查当前槽位是否在更新列表中
        if (eventData.updatedSlotIndexes.Contains(slotIndex))
        {
            UpdateSlot(gameData.inventorySlots[slotIndex]);
        }
    }
}