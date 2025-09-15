using System.Collections.Generic;
using UnityEngine;

public class CharacterPanel : MonoBehaviour
{
    [Header("绑定配置")]
    [SerializeField] private List<CharacterSlot> characterSlots; // 拖入8个固定角色Slot
    [SerializeField] private List<CharacterSO> characterSOList;  // 拖入对应的8个CharacterSO（顺序匹配Slot）

    [Header("随时间消耗配置")]
    [SerializeField] private float staminaConsumePerSecond = 1f; // 每秒消耗的体力值
    [SerializeField] private float hungerConsumePerSecond = 2f;  // 每秒消耗的饥饿值
    [SerializeField] private float consumeInterval = 1f;         // 消耗间隔（1秒一次，避免每帧消耗）
    private float consumeTimer = 0f; // 计时器：控制消耗频率

    private void Start()
    {
        // 1. 初始化检查与Slot绑定（原有逻辑）
        if (!CheckInitValid()) return;
        BindSlotsWithSO();

        // 2. 订阅角色属性变化事件（原有逻辑：接收数据变化，更新UI）
        EventManager.Instance.Subscribe<OnCharacterStatChanged>(OnCharacterStatChanged);
    }

    private void OnDestroy()
    {
        // 取消事件订阅，避免内存泄漏（原有逻辑）
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe<OnCharacterStatChanged>(OnCharacterStatChanged);
        }
    }

    // ---------------- 新增：每帧更新计时器，实现随时间消耗 ----------------
    private void Update()
    {
        // 每帧输出日志，确认Update正常执行
        Debug.Log($"[CharacterPanel] 每帧更新，当前计时器：{consumeTimer:F2}，间隔阈值：{consumeInterval}");

        consumeTimer += Time.deltaTime;

        if (consumeTimer >= consumeInterval)
        {
            Debug.Log($"[CharacterPanel] 触发消耗逻辑（间隔 {consumeInterval} 秒）");
            ExecuteStatConsume();
            consumeTimer = 0f;
        }
    }

    // ---------------- 新增：体力/饥饿消耗的核心逻辑 ----------------
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

            // 输出当前数据
            Debug.Log($"[CharacterPanel] 角色 {characterSO.characterID} 消耗前 - 体力：{runtimeData.currentStamina}，饥饿：{runtimeData.currentHunger}");

            // 计算消耗后的值
            float targetStamina = Mathf.Max(0, runtimeData.currentStamina - staminaConsumePerSecond);
            float targetHunger = Mathf.Max(0, runtimeData.currentHunger - hungerConsumePerSecond);
            float staminaChange = targetStamina - runtimeData.currentStamina;
            float hungerChange = targetHunger - runtimeData.currentHunger;

            // 输出消耗后的数据和变化量
            Debug.Log($"[CharacterPanel] 角色 {characterSO.characterID} 消耗后 - 体力：{targetStamina}（变化：{staminaChange}），饥饿：{targetHunger}（变化：{hungerChange}）");

            // 调用更新方法
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

    // ---------------- 原有逻辑：初始化检查（保持不变） ----------------
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

    // ---------------- 原有逻辑：绑定Slot与CharacterSO（保持不变） ----------------
    private void BindSlotsWithSO()
    {
        for (int i = 0; i < characterSlots.Count; i++)
        {
            CharacterSlot slot = characterSlots[i];
            CharacterSO characterSO = characterSOList[i];

            // 获取初始数据（有运行时数据用运行时，没有用SO默认值）
            CharacterRuntimeData runtimeData = GameStateManager.Instance.GetCharacterData(characterSO.characterID);
            float initHunger = runtimeData != null ? runtimeData.currentHunger : characterSO.maxHunger;
            float initStamina = runtimeData != null ? runtimeData.currentStamina : characterSO.maxStamina;

            // 初始化Slot的UI
            slot.InitSlot(characterSO, initHunger, initStamina);
            Debug.Log($"绑定角色 {characterSO.characterID} 到 Slot[{i}]");
        }
    }

    // ---------------- 原有逻辑：接收事件更新UI（保持不变） ----------------
    private void OnCharacterStatChanged(OnCharacterStatChanged eventData)
    {
        // 找到对应ID的Slot
        CharacterSlot targetSlot = characterSlots.Find(slot => slot.GetCharacterID() == eventData.characterID);
        if (targetSlot == null)
        {
            Debug.LogWarning($"未找到角色ID：{eventData.characterID} 的Slot");
            return;
        }

        // 从GameStateManager获取最新完整数据，更新Slot UI
        CharacterRuntimeData runtimeData = GameStateManager.Instance.GetCharacterData(eventData.characterID);
        if (runtimeData != null)
        {
            targetSlot.UpdateSlot(runtimeData.currentHunger, runtimeData.currentStamina);
        }
    }
}