using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FoodState
{
    Raw,
    Cooked
}

[CreateAssetMenu(fileName = "New Item", menuName = "Game Data/Item Data")]
public class ItemSO : ScriptableObject
{
    [Header("物品基础信息")]
    public string itemID;
    public string itemName;
    [TextArea] public string itemDescription;
    public Sprite itemIcon;

    [Header("物品属性和效果")]
    public float weight;
    public float hungerToRestore;
    public float staminaToRestore;

    [Header("食物相关属性")]
    public bool isFood = false;
    public float maxFreshness;
    public FoodState foodState = FoodState.Raw;
    public ItemSO cookedVersion;
    public int stackAfterCook;
}
