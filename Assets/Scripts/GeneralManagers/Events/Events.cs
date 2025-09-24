using System.Collections.Generic;
using UnityEngine;

public struct OnGameStateChanged   // 游戏状态改变时发布
{
    public GameState newState;
}

public struct OnSceneLoaded { }    // 场景加载完毕时发布

public struct OnGameRollback { }    // 游戏回滚时发布

// public struct OnCharacterRegistered     // 角色注册时发布
// {
//     public CharacterSO characterSO;
//     public GameObject characterGO;
// }

public struct OnCharacterDied       // 角色死亡时发布
{
    public CharacterSO characterSO;
}

public struct OnCharacterRevived     // 角色复活时发布
{
    public CharacterSO characterSO;
}


public struct OnCharacterStatChanged     // 角色属性改变时发布
{
    public string characterID;
    public BuffSO.StatType statType; // 角色具体变化的属性
    public float newValue;
    public float changeAmount;
}

public struct OnInventoryInitialized { }    // 物品栏初始化时发布

public struct OnInventoryChanged     // 物品栏发生变化时发布
{
    public List<int> updatedSlotIndexes;
}

public struct OnItemUseRequest     // 角色使用物品时发布
{
    public ItemSO itemSO;
    public string targetCharacterID;
    public float itemFreshness;
}

public struct OnItemFreshnessChanged
{
    public int slotIndex;
    public float currentFreshness;
    // public float maxFreshness;
}

public struct OnSkillActivated //角色技能激活 
{
    public string characterID;
}

public struct OnSkillCooldownStarted  // 角色技能冷却开始
{
    public string characterID;
    public float maxCooldown;
}

public struct OnSkillCooldownEnded    // 角色技能冷却结束
{
    public string characterID;
}

// BuffEvents.cs
public struct OnBuffApplied
{
    public CharacterSO target;
    public BuffSO buff;
    public CharacterSO source;
}

public struct OnBuffRemoved
{
    public CharacterSO target;
    public BuffSO buff;
}

public struct OnBuffUpdated
{
    public CharacterSO target;
    public BuffSO buff;
    public  float remainingTime;
}

public struct OnDiseaseContracted
{
    public string characterID;
    public BuffSO buffSO;
}

public struct OnDiseaseCured
{
    public string characterID;
    public BuffSO buffSO;
}
