using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Common,
    Special,
    Medicine
}

public enum FoodState
{
    Raw,
    Cooked
}

public enum EffectType
{
    RestoreStamina,
    RestoreHunger,
    ApplyBuff,
    CureDisease
}

[System.Serializable]
public class ItemEffect
{
    public EffectType type;
    public float value;
    public BuffSO buffToApply;
    public BuffSO.DiseaseType diseaseToCure;
}

[CreateAssetMenu(fileName = "New Item", menuName = "Game Data/Item Data")]
public class ItemSO : ScriptableObject
{
    [Header("物品基础信息")]
    public string itemID;
    public string itemName;
    [TextArea] public string itemDescription;
    public Sprite itemIcon;

    [Header("物品属性和使用效果")]
    public ItemType itemType = ItemType.Common;
    public float specialItemRefreshWeight; // 特殊产品各自的刷新概率
    public float weight;
    // public float hungerToRestore;
    // public float staminaToRestore;
    public List<ItemEffect> effects;


    [Header("食物相关属性")]
    public bool isFood = false;
    public float maxFreshness;
    public FoodState foodState = FoodState.Raw;
    public ItemSO cookedVersion;
    public int stackAfterCook;

    [Header("角色专属")]
    public string requiredCharacterTag; // 角色专属标签
}
