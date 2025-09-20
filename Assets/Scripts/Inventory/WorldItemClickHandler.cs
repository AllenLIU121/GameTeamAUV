using UnityEngine;
using UnityEngine.EventSystems;

namespace Inventory
{
    /// <summary>
    /// 处理场景中物品点击回收的脚本
    /// 附加到拖拽到场景的物品预制体上
    /// </summary>
    public class WorldItemClickHandler : MonoBehaviour, IPointerClickHandler
    {
        [Header("物品信息")]
        [Tooltip("物品的唯一标识ID")]
        public string itemID;

        [Tooltip("物品数量")]
        public int quantity = 1;

        /// <summary>
        /// 处理鼠标点击事件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 确保点击的是左键
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // 尝试回收物品到物品栏
                RecycleItem();
            }
        }

        /// <summary>
        /// 回收物品到物品栏
        /// </summary>
        private void RecycleItem()
        {
            // 直接查找InventoryManager实例，避免依赖GameData命名空间
            InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();

            if (inventoryManager == null)
            {
                Debug.LogError("[WorldItemClickHandler] InventoryManager is not available.");
                return;
            }

            // 尝试将物品添加回物品栏
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