using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour 
{
    public List<CharacterSO> charactersSO;
    private Dictionary<string, CharacterSO> characterSODict = new Dictionary<string, CharacterSO>();

    private void Awake()
    {
        GameStateManager.Instance.RegisterCharacterManager(this);

        foreach (var characterSO in charactersSO)
        {
            if (characterSO != null && !string.IsNullOrEmpty(characterSO.characterID))
            {
                characterSODict[characterSO.characterID] = characterSO;
            }
        }

        EventManager.Instance.Subscribe<OnItemUseRequest>(HandleItemUseRequest);
    }

    private void Start()
    {
        InitializeCharacters();
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

    // 恢复使用物品的角色的体力和饥饿值
    private void HandleItemUseRequest(OnItemUseRequest eventData)
    {
        var itemSO = eventData.itemSO;
        var targetCharacterID = eventData.targetCharacterID;

        if (itemSO.staminaToRestore > 0)
        {
            GameStateManager.Instance.UpdateStamina(targetCharacterID, itemSO.staminaToRestore);
        }
        if (itemSO.hungerToRestore > 0)
        {
            GameStateManager.Instance.UpdateHunger(targetCharacterID, itemSO.hungerToRestore);
        }
    }

    private void InitializeCharacters()
    {
        if (GameStateManager.Instance.GetCharacterData(charactersSO[0].characterID) == null)
        {
            foreach (var characterSO in charactersSO)
            {
                var newCharacterData = new CharacterRuntimeData
                {
                    characterID = characterSO.characterID,
                    maxStamina = characterSO.maxStamina,
                    maxHunger = characterSO.maxHunger,
                    currentStamina = characterSO.maxStamina,
                    currentHunger = characterSO.maxHunger
                };

                GameStateManager.Instance.RegisterNewCharacter(newCharacterData);
            }

            GameStateManager.Instance.PublishCharacterDataForSync();
        }
    }

    public CharacterSO GetCharacterSO(string characterID)
    {
        characterSODict.TryGetValue(characterID, out CharacterSO characterSO);
        return characterSO;
    }
}
