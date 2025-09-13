using System.Collections.Generic;

public class ItemDatabase : Singleton<ItemDatabase>
{
    public List<ItemSO> allItems;
    private Dictionary<string, ItemSO> itemDict = new Dictionary<string, ItemSO>();

    protected override void Awake()
    {
        base.Awake();

        foreach (var itemSO in allItems)
        {
            itemDict[itemSO.itemID] = itemSO;
        }
    }

    public ItemSO GetItemSO(string itemID)
    {
        itemDict.TryGetValue(itemID, out ItemSO itemSO);
        return itemSO;
    }
}
