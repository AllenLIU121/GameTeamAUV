using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData : ISerializationCallbackReceiver  // Runtime数据仓库
{
    public string lastSceneName;
    public Dictionary<string, CharacterRuntimeData> characters = new Dictionary<string, CharacterRuntimeData>();
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public float currentWeight = 0f;


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
    public float maxStamina;
    public float maxHunger;
    public float currentStamina;
    public float currentHunger;
}

// 物品栏数据
[Serializable]
public class InventorySlot
{
    public string itemID;
    public int quantity;
    public InventorySlot() { Clear(); }
    public void Clear() { itemID = string.Empty; quantity = 0; }
    public bool IsEmpty() => string.IsNullOrEmpty(itemID) || quantity <= 0;
}
