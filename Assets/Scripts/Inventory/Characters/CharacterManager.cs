using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    // public List<CharacterSO> charactersSO;
    private Dictionary<string, GameObject> characterGODict = new Dictionary<string, GameObject>();
    private Dictionary<string, CharacterSO> characterSODict = new Dictionary<string, CharacterSO>();

    private void Awake()
    {
        GameStateManager.Instance.RegisterCharacterManager(this);

        EventManager.Instance.Subscribe<OnItemUseRequest>(HandleItemUseRequest);
    }

    // private void Start()
    // {
    //     InitializeCharacters();
    // }

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

    // 当角色使用物品时
    private void HandleItemUseRequest(OnItemUseRequest eventData)
    {
        var itemSO = eventData.itemSO;
        var targetCharacterID = eventData.targetCharacterID;

        var characterSO = GetCharacterSO(targetCharacterID);
        var targetObject = GetCharacterGameObject(targetCharacterID);

        if (characterSO == null || targetObject == null)
        {
            Debug.LogError($"Use item failed: Can't find character SO or GameObject with ID '{targetCharacterID}'");
            return;
        }

        var characterStatus = targetObject.GetComponent<CharacterStatus>();
        if (characterStatus == null)
        {
            Debug.LogError($"Character {targetCharacterID} is missing a CharacterStatus component");
            return;
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
                    characterStatus.ModifyStamina(effect.value);
                    break;
                case EffectType.RestoreHunger:
                    characterStatus.ModifyHunger(effect.value);
                    break;
                case EffectType.ApplyBuff:
                    if (effect.buffToApply != null)
                        buffManager.ApplyBuff(targetObject, effect.buffToApply);
                    break;
                case EffectType.CureDisease:
                    buffManager.RemoveDisease(targetObject, effect.diseaseToCure);
                    break;
            }
        }
    }

    public void RegisterCharacter(CharacterSO characterSO, GameObject characterGO)
    {
        if (characterSO == null || characterGO == null) return;

        // 注册GameObject实例
        characterGODict[characterSO.characterID] = characterGO;

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

        if (characterGODict.ContainsKey(characterSO.characterID))
            characterGODict.Remove(characterSO.characterID);
    }

    // private void InitializeCharacters()
    // {
    //     if (GameStateManager.Instance.GetCharacterData(charactersSO[0].characterID) == null)
    //     {
    //         var skillManager = GameStateManager.Instance.Skill;
    //         if (skillManager == null)
    //         {
    //             Debug.LogError("[CharacterManager] SkillManager is null");
    //             return;
    //         }

    //         foreach (var characterSO in charactersSO)
    //         {
    //             // 初始化并注册角色
    //             var newCharacterData = new CharacterRuntimeData
    //             {
    //                 characterID = characterSO.characterID,
    //                 maxStamina = characterSO.maxStamina,
    //                 maxHunger = characterSO.maxHunger,
    //                 currentStamina = characterSO.maxStamina,
    //                 currentHunger = characterSO.maxHunger
    //             };
    //             GameStateManager.Instance.RegisterNewCharacter(newCharacterData);

    //             // 初始化并注册角色技能
    //             if (characterSO.skill != null)
    //             {
    //                 skillManager.RegisterCharacterSkill(characterSO.characterID, characterSO.skill);
    //             }
    //         }

    //         GameStateManager.Instance.PublishCharacterDataForSync();
    //     }
    // }

    public CharacterSO GetCharacterSO(string characterID)
    {
        characterSODict.TryGetValue(characterID, out CharacterSO characterSO);
        return characterSO;
    }

    public GameObject GetCharacterGameObject(string characterID)
    {
        characterGODict.TryGetValue(characterID, out GameObject characterGO);
        return characterGO;
    }

    public ICollection<GameObject> GetAllCharacterGOs()
    {
        return characterGODict.Values;
    }
}
