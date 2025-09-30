using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WeightedItem
{
    public ItemSO item;
    public float weight;
}

[CreateAssetMenu(fileName = "Store Config", menuName = "Game Data/Store Configuration")]
public class StoreConfigSO : ScriptableObject
{
    [Header("商店配置")]
    public int numberOfSlots = 10;
    [Range(0, 1)]
    public float essentialItemRate = 0.8f;
    public List<ItemSO> essentialItemsPool;
    public List<WeightedItem> specialItemsPool;
}
