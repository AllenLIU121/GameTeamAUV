using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData : ISerializationCallbackReceiver  // Runtime数据仓库
{
    public Dictionary<string, CharacterRuntimeData> characters = new Dictionary<string, CharacterRuntimeData>();
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public float currentWeight = 0f;

    public List<NodeRuntimeData> mapNodes = new List<NodeRuntimeData>();

    #region 字典(不可序列化) 和 列表(可序列化) 转换
    [SerializeField] private List<string> characterKeys = new List<string>();
    [SerializeField] private List<CharacterRuntimeData> characterValues = new List<CharacterRuntimeData>();

    public void OnBeforeSerialize()
    {
        characterKeys.Clear();
        characterValues.Clear();

        foreach (var key in characters.Keys)
        {
            characterKeys.Add(key);
            characterValues.Add(characters[key]);
        }
    }

    public void OnAfterDeserialize()
    {
        characters = new Dictionary<string, CharacterRuntimeData>();
        for (int i = 0; i < characterKeys.Count; i++)
        {
            characters[characterKeys[i]] = characterValues[i];
        }
    }
    #endregion
}


// ------------------Runtime数据结构------------------

// 角色数据
[Serializable]
public class CharacterRuntimeData
{
    public string characterID;
    public bool isAlive;
    public float currentStamina;
    public float currentHunger;
    public float directMaxStaminaModifier;
}

// 物品栏数据
[Serializable]
public class InventorySlot
{
    public string itemID;
    public int quantity;
    public float currentFreshness;

    public bool IsEmpty() => string.IsNullOrEmpty(itemID) || quantity <= 0;

    public void Reset()
    {
        quantity = 0;
        currentFreshness = 0f;
    }

    public void SetItem(ItemSO item, int quantity)
    {
        itemID = item.itemID;
        this.quantity = quantity;
        currentFreshness = item.maxFreshness;
    }
}

// 第二章地图数据
[Serializable]
public class NodeRuntimeData
{
    public Vector2Int gridPosition;
    public NodeType nodeType;
    public bool isStore;
    public bool isSafeZone;
}
