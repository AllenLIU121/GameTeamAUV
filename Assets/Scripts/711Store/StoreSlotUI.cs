using UnityEngine;
using UnityEngine.UI;

public class StoreSlotUI : MonoBehaviour
{
    private Image itemIcon;

    private void Awake()
    {
        itemIcon = GetComponent<Image>();
        if (itemIcon == null)
        {
            Debug.LogWarning("[StoreSlotUI] itemIcon is null");
        }
    }

    public void DisplayItem(ItemSO item)
    {
        itemIcon.sprite = item.itemIcon;
    }

    public void ClearSlot()
    {
        itemIcon.sprite = null;
    }
}
