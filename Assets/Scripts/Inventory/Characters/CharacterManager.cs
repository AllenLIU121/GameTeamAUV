using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    private Dictionary<string, CharacterStatus> characterStatusDict = new Dictionary<string, CharacterStatus>();
    private Dictionary<string, CharacterSO> characterSODict = new Dictionary<string, CharacterSO>();

    private void Awake()
    {
        GameStateManager.Instance.RegisterCharacterManager(this);
        EventManager.Instance.Subscribe<OnItemUseRequest>(HandleItemUseRequest);
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnregisterCharacterManager();
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe<OnItemUseRequest>(HandleItemUseRequest);
        }
    }

    // 所有角色减少体力值
    public void ReduceAllAliveCharactersStamina()
    {
        foreach (var status in GetAllAliveCharacterStatus())
        {
            status.ModifyStamina(-(float)0.3 * status.MaxStamina);
        }
    }

    // 当角色使用物品时
    private void HandleItemUseRequest(OnItemUseRequest eventData)
    {
        if (eventData.itemFreshness <= 0f) return;

        var itemSO = eventData.itemSO;
        var targetCharacterID = eventData.targetCharacterID;

        var characterSO = GetCharacterSO(targetCharacterID);
        var characterStatus = GetCharacterStatus(targetCharacterID);

        if (characterSO == null || characterStatus == null)
        {
            Debug.LogError($"Use item failed: Can't find character SO or Status with ID '{targetCharacterID}'");
            return;
        }

        // 妈妈存活时 黄桃罐头对弟弟妹妹加成效果
        if (itemSO.itemID == "canned_yellow_peach" && characterSO.characterTag == "child")
        {
            var momStatus = GetCharacterStatus("mom");
            if (momStatus != null && momStatus.IsAlive)
            {
                characterStatus.ModifyStamina(characterSO.maxStamina, true);
                characterStatus.ModifyHunger(characterSO.maxHunger, true);
                Debug.Log($"Mom's passive skill activated! {characterSO.characterID}'s stamina and hunger have been fully recovered.");
                return;
            }
        }

        // 专属物品
        if (!string.IsNullOrEmpty(itemSO.requiredCharacterTag) && characterSO.characterTag != itemSO.requiredCharacterTag)
        {
            Debug.LogWarning($"Character '{characterSO.characterID}' can't use this item. Only characters with tag '{itemSO.requiredCharacterTag}' can use it.");
            return;
        }

        // 应用效果
        foreach (var effect in itemSO.effects)
        {
            var buffManager = GameStateManager.Instance.Buff;
            switch (effect.type)
            {
                case EffectType.RestoreStamina:
                    characterStatus.ModifyStamina(eventData.itemFreshness < 20f ? effect.value / 2 : effect.value);
                    break;
                case EffectType.RestoreHunger:
                    characterStatus.ModifyHunger(eventData.itemFreshness < 20f ? effect.value / 2 : effect.value);
                    break;
                case EffectType.ApplyBuff:
                    if (effect.buffToApply != null)
                        buffManager.ApplyBuff(characterSO, effect.buffToApply);
                    break;
                case EffectType.CureDisease:
                    buffManager.RemoveDisease(characterSO, effect.diseaseToCure);
                    break;
            }
        }
    }

    public void RegisterCharacter(CharacterSO characterSO, CharacterStatus characterStatus)
    {
        if (characterSO == null || characterStatus == null) return;

        // 注册GameObject实例
        characterStatusDict[characterSO.characterID] = characterStatus;

        // 注册SO数据
        if (!characterSODict.ContainsKey(characterSO.characterID))
        {
            characterSODict[characterSO.characterID] = characterSO;
        }

        // 注册角色技能
        var skillManager = GameStateManager.Instance.Skill;
        if (skillManager != null && characterSO.skill != null)
        {
            skillManager.RegisterCharacterSkill(characterSO.characterID, characterSO.skill);
        }
    }

    public void UnregisterCharacter(CharacterSO characterSO)
    {
        if (characterSO == null) return;

        if (characterStatusDict.ContainsKey(characterSO.characterID))
        {
            characterStatusDict.Remove(characterSO.characterID);
        }
    }

    // 获取单个角色运行时数据
    public CharacterRuntimeData GetCharacterData(string characterID)
    {
        GameStateManager.Instance.currentData.characters.TryGetValue(characterID, out CharacterRuntimeData characterData);
        return characterData;
    }

    public List<CharacterSO> GetAllAliveCharacterSOs()
    {
        List<CharacterSO> aliveCharacters = new List<CharacterSO>();
        foreach (var entry in characterStatusDict)
        {
            if (entry.Value != null && entry.Value.IsAlive)
            {
                aliveCharacters.Add(entry.Value.characterSO);
            }
        }
        return aliveCharacters;
    }

    public List<CharacterStatus> GetAllAliveCharacterStatus()
    {
        List<CharacterStatus> aliveCharacters = new List<CharacterStatus>();
        foreach (var entry in characterStatusDict)
        {
            if (entry.Value != null && entry.Value.IsAlive)
            {
                aliveCharacters.Add(entry.Value);
            }
        }

        return aliveCharacters;
    }

    public List<CharacterSO> GetAllCharacterSOs()
    {
        return characterSODict.Values.ToList();
    }

    public CharacterSO GetCharacterSO(string characterID)
    {
        characterSODict.TryGetValue(characterID, out CharacterSO characterSO);
        return characterSO;
    }

    public CharacterStatus GetCharacterStatus(string characterID)
    {
        characterStatusDict.TryGetValue(characterID, out CharacterStatus status);
        return status;
    }

    public ICollection<CharacterStatus> GetAllCharacterStatus()
    {
        return characterStatusDict.Values;
    }
}
