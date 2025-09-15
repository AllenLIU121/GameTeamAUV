using System.Collections.Generic;
using UnityEngine;

public class CharacterPanel : MonoBehaviour
{
    [SerializeField] private List<CharacterSlot> characterSlots; 
    [SerializeField] private List<CharacterSO> characterSOList; 

    [Header("随时间消耗速度")]
    [SerializeField] private float staminaConsumePerSecond = 1f; 
    [SerializeField] private float hungerConsumePerSecond = 2f;  
    [SerializeField] private float consumeInterval = 1f;         
    private float consumeTimer = 0f; 

    private void Start()
    {
        if (!CheckInitValid()) return;
        BindSlotsWithSO();

        EventManager.Instance.Subscribe<OnCharacterStatChanged>(OnCharacterStatChanged);
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe<OnCharacterStatChanged>(OnCharacterStatChanged);
        }
    }

    private void Update()
    {
        Debug.Log($"[CharacterPanel] 每帧更新，当前计时器：{consumeTimer:F2}，间隔阈值：{consumeInterval}");

        consumeTimer += Time.deltaTime;

        if (consumeTimer >= consumeInterval)
        {
            Debug.Log($"[CharacterPanel] 触发消耗逻辑（间隔 {consumeInterval} 秒）");
            ExecuteStatConsume();
            consumeTimer = 0f;
        }
    }

    private void ExecuteStatConsume()
    {
        foreach (var characterSO in characterSOList)
        {
            if (characterSO == null || string.IsNullOrEmpty(characterSO.characterID))
            {
                Debug.LogWarning("[CharacterPanel] 跳过空的CharacterSO或无ID的角色");
                continue;
            }

            CharacterRuntimeData runtimeData = GameStateManager.Instance.GetCharacterData(characterSO.characterID);
            if (runtimeData == null)
            {
                Debug.LogWarning($"[CharacterPanel] 角色 {characterSO.characterID} 无运行时数据，无法消耗");
                continue;
            }

            Debug.Log($"[CharacterPanel] 角色 {characterSO.characterID} 消耗前 - 体力：{runtimeData.currentStamina}，饥饿：{runtimeData.currentHunger}");

            float targetStamina = Mathf.Max(0, runtimeData.currentStamina - staminaConsumePerSecond);
            float targetHunger = Mathf.Max(0, runtimeData.currentHunger - hungerConsumePerSecond);
            float staminaChange = targetStamina - runtimeData.currentStamina;
            float hungerChange = targetHunger - runtimeData.currentHunger;

            Debug.Log($"[CharacterPanel] 角色 {characterSO.characterID} 消耗后 - 体力：{targetStamina}（变化：{staminaChange}），饥饿：{targetHunger}（变化：{hungerChange}）");

            if (staminaChange != 0)
            {
                GameStateManager.Instance.UpdateStamina(characterSO.characterID, staminaChange);
                Debug.Log($"[CharacterPanel] 已调用UpdateStamina，角色 {characterSO.characterID}");
            }
            if (hungerChange != 0)
            {
                GameStateManager.Instance.UpdateHunger(characterSO.characterID, hungerChange);
                Debug.Log($"[CharacterPanel] 已调用UpdateHunger，角色 {characterSO.characterID}");
            }
        }   
    }

    private bool CheckInitValid()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager未初始化！");
            return false;
        }
        if (characterSlots.Count != characterSOList.Count)
        {
            Debug.LogError($"Slot数量（{characterSlots.Count}）与SO数量（{characterSOList.Count}）不匹配！");
            return false;
        }
        foreach (var slot in characterSlots)
        {
            if (slot == null)
            {
                Debug.LogError("CharacterSlots列表中存在空对象！");
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

            CharacterRuntimeData runtimeData = GameStateManager.Instance.GetCharacterData(characterSO.characterID);
            float initHunger = runtimeData != null ? runtimeData.currentHunger : characterSO.maxHunger;
            float initStamina = runtimeData != null ? runtimeData.currentStamina : characterSO.maxStamina;

            slot.InitSlot(characterSO, initHunger, initStamina);
            Debug.Log($"绑定角色 {characterSO.characterID} 到 Slot[{i}]");
        }
    }

    private void OnCharacterStatChanged(OnCharacterStatChanged eventData)
    {
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