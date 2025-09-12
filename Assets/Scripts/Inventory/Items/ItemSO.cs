using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // TOADD: 是否需要烹饪
    // TOADD: 角色专属
    // TOADD: 物品特殊效果
}
