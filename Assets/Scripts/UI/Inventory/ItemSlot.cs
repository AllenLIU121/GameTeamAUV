using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image icon;
    public Image border;
    public Color collectedColor = Color.black;
    public Color uncollectedColor = Color.gray;

    private ItemSO itemData;

    public void SetItem(ItemSO item, bool collected)
    {
        itemData = item;
        
        if (item != null)
        {
            icon.sprite = item.itemIcon;
            border.color = collected ? collectedColor : uncollectedColor;
            icon.enabled = collected;
        }
        else
        {
            icon.sprite = null;
            border.color = uncollectedColor;
            icon.enabled = false;
        }
    }
}