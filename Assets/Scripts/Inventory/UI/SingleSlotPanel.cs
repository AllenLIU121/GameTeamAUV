using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleSlotPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI stackNum;
    private int slotIndex;
    private InventoryManager inventoryManager;

    public void Initialize(int index, InventoryManager manager)
    {
        slotIndex = index;
        inventoryManager = manager;
    }

    public void UpdateSlot(InventorySlot slotData)
    {
        if (slotData == null || slotData.IsEmpty())
        {
            stackNum.enabled = false;
            itemImage.sprite = null;
            itemImage.enabled = false;
        }
        else
        {
            var itemSO = ItemDatabase.Instance.GetItemSO(slotData.itemID);
            itemImage.enabled = true;
            itemImage.sprite = itemSO.itemIcon;

            if (slotData.quantity > 1)
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
    }

    // ------------------- 物品拖拽功能 -------------------

    private int draggedSlotIndex = -1;   // 拖拽时栏位的索引
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemImage.enabled)
        {
            draggedSlotIndex = slotIndex;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // TODO: Item following cursor
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // TODO: Collision check 
    }

    public void OnDrop(PointerEventData eventData)
    {
        // TODO: Item drop logic
    }
}
