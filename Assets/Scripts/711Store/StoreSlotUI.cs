using UnityEngine;
using UnityEngine.UI;

public class StoreSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemImg;
    private Button button;
    private ItemSO itemSO;
    private Color noItemColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[StoreSlotUI] button is null");
        }

        button.AddButtonListener(AddItemToInventory);
    }

    public void DisplayItem(ItemSO item)
    {
        itemSO = item;
        itemImg.sprite = item.itemIcon;

        itemImg.color = Color.white;
        button.interactable = true;
    }

    public void ClearSlot()
    {
        itemSO = null;
        itemImg.sprite = null;
    }

    private void AddItemToInventory()
    {
        if (itemSO != null)
        {
            GameStateManager.Instance.Inventory.AddItem(itemSO.itemID, 1);
        }

        itemImg.color = noItemColor;
        button.interactable = false;
    }
}
