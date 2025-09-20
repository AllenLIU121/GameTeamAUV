using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterPanel : MonoBehaviour
{
    [Header("角色UI组件")]
    [SerializeField] private List<CharacterSlot> characterSlots;
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
            GameObject characterGO = GameStateManager.Instance.Character.GetCharacterGameObject(characterSO.characterID);
            CharacterStatus characterStatus = null;
            
            if (characterGO != null)
            {
                characterStatus = characterGO.GetComponent<CharacterStatus>();
            }

            // 使用合理的默认值，确保即使没有运行时数据也能正常初始化
            float initHunger = characterStatus != null ? characterStatus.CurrentHunger : characterSO.maxHunger;
            float initStamina = characterStatus != null ? characterStatus.CurrentStamina : characterSO.maxStamina;

            // 初始化槽位
            slot.InitSlot(characterSO, initHunger, initStamina);
            Debug.LogWarning($"绑定角色 {characterSO.characterID} 到 Slot[{i}]");
        }
    }

    private void OnCharacterStatChanged(OnCharacterStatChanged eventData)
    {
        // 记录接收到的事件
        Debug.Log($"[CharacterPanel] 接收到角色状态变化事件 - 角色ID: {eventData.characterID}, 状态类型: {eventData.statType}, 新值: {eventData.newValue}, 变化量: {eventData.changeAmount}");
        
        // 查找对应的角色槽位
        CharacterSlot targetSlot = characterSlots.Find(slot => slot.GetCharacterID() == eventData.characterID);
        if (targetSlot == null)
        {
            Debug.LogWarning($"未找到角色ID：{eventData.characterID} 的Slot");
            return;
        }
        
        Debug.Log($"[CharacterPanel] 找到对应槽位，正在更新UI");
        targetSlot.UpdateSlot(eventData.statType, eventData.newValue);
    }
}