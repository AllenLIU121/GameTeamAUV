using System.Collections.Generic;
using UnityEngine;

public class CharacterPanel : MonoBehaviour
{
    [Header("角色UI组件")]
    [SerializeField] private List<CharacterSlot> characterSlots; 
    [Header("角色配置数据")]
    [SerializeField] private List<CharacterSO> characterSOList; 

    private void Start()
    {
        if (!CheckInitValid()) return;
        BindSlotsWithSO();

        // 订阅角色属性变化事件
        EventManager.Instance.Subscribe<OnCharacterStatChanged>(OnCharacterStatChanged);
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe<OnCharacterStatChanged>(OnCharacterStatChanged);
        }
    }

    private bool CheckInitValid()
    {
        // 检查GameStateManager是否初始化
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager未初始化！");
            return false;
        }
        
        // 检查EventManager是否初始化
        if (EventManager.Instance == null)
        {
            Debug.LogError("EventManager未初始化！");
            return false;
        }
        
        // 检查Slot与SO数量是否匹配
        if (characterSlots == null || characterSOList == null)
        {
            Debug.LogError("characterSlots或characterSOList为null！");
            return false;
        }
        
        if (characterSlots.Count != characterSOList.Count)
        {
            Debug.LogError($"Slot数量（{characterSlots.Count}）与SO数量（{characterSOList.Count}）不匹配！");
            return false;
        }
        
        // 检查是否有空的Slot
        foreach (var slot in characterSlots)
        {
            if (slot == null)
            {
                Debug.LogError("CharacterSlots列表中存在空对象！");
                return false;
            }
        }
        
        // 检查是否有空的SO
        foreach (var characterSO in characterSOList)
        {
            if (characterSO == null)
            {
                Debug.LogError("characterSOList列表中存在空对象！");
                return false;
            }
            
            if (string.IsNullOrEmpty(characterSO.characterID))
            {
                Debug.LogError("CharacterSO中存在空的characterID！");
                return false;
            }
        }
        
        return true;
    }

    private void BindSlotsWithSO()
    {
        for (int i = 0; i < characterSlots.Count; i++)
        {
            CharacterSlot slot = characterSlots[i];
            CharacterSO characterSO = characterSOList[i];

            // 获取角色运行时数据
            CharacterRuntimeData runtimeData = GameStateManager.Instance.GetCharacterData(characterSO.characterID);
            
            // 如果没有运行时数据，使用默认值
            float initHunger = runtimeData != null ? runtimeData.currentHunger : characterSO.maxHunger;
            float initStamina = runtimeData != null ? runtimeData.currentStamina : characterSO.maxStamina;

            // 初始化槽位
            slot.InitSlot(characterSO, initHunger, initStamina);
            Debug.LogWarning($"绑定角色 {characterSO.characterID} 到 Slot[{i}]");
        }
    }

    private void OnCharacterStatChanged(OnCharacterStatChanged eventData)
    {
        // 查找对应的角色槽位
        CharacterSlot targetSlot = characterSlots.Find(slot => slot.GetCharacterID() == eventData.characterID);
        if (targetSlot == null)
        {
            Debug.LogWarning($"未找到角色ID：{eventData.characterID} 的Slot");
            return;
        }

        CharacterRuntimeData runtimeData = GameStateManager.Instance.GetCharacterData(eventData.characterID);
        if (runtimeData != null)
        {
            targetSlot.UpdateSlot(runtimeData.currentHunger, runtimeData.currentStamina);
        }
    }
}