using UnityEngine;
using UnityEngine.EventSystems;

namespace Inventory
{
    // 场景中物品点击回收
    public class WorldItemClickHandler : MonoBehaviour, IPointerClickHandler
    {
        [Header("物品信息")]
        [Tooltip("物品的唯一标识ID")]
        public string itemID;

        [Tooltip("物品数量")]
        public int quantity = 1;

        // 处理鼠标点击
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // 回收物品到物品栏
                RecycleItem();
            }
        }

        // 回收物品到物品栏
        private void RecycleItem()
        {
            InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();

            if (inventoryManager == null)
            {
                Debug.LogError("[WorldItemClickHandler] InventoryManager is not available.");
                return;
            }

            // 将物品添加回物品栏
            if (inventoryManager.AddItem(itemID, quantity))
            {
                Debug.Log("[WorldItemClickHandler] Item recycled successfully: " + itemID + ", Quantity: " + quantity);

                // 回收成功后销毁游戏对象
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("[WorldItemClickHandler] Failed to recycle item: " + itemID);
            }
        }
    }
}