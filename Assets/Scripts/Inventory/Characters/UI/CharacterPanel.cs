using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanel : MonoBehaviour
{
    [Header("角色UI组件")]
    [SerializeField] private List<CharacterSlot> characterSlots;
    [SerializeField] private List<CharacterSO> characterSOList;
    [SerializeField] private List<Image> buffIconList;

    private void Awake()
    {
        // 订阅角色属性变化事件
        EventManager.Instance.Subscribe<OnCharacterStatChanged>(OnCharacterStatChanged);
    }

    private void Start()
    {
        StartCoroutine(InitializePanelAfterFrame());
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe<OnCharacterStatChanged>(OnCharacterStatChanged);
        }
    }

    private IEnumerator InitializePanelAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        if (!CheckInitValid()) yield break;
        BindSlotsWithSO();
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
            CharacterStatus characterStatus = GameStateManager.Instance.Character.GetCharacterGameObject(characterSO.characterID).GetComponent<CharacterStatus>();

            // 如果没有运行时数据，使用默认值
            float initHunger = characterStatus != null ? characterStatus.CurrentHunger : characterStatus.MaxHunger;
            float initStamina = characterStatus != null ? characterStatus.CurrentStamina : characterStatus.MaxStamina;

            // 初始化槽位
            slot.InitSlot(characterSO, initHunger, initStamina);
            Debug.LogWarning($"绑定角色 {characterSO.characterID} 到 Slot[{i}]");

            // 监听带有主动技能的角色按钮
            if (slot.TryGetComponent(out Button btn))
            {
                if (characterSO.skill.cooldownTime > 0)
                    btn.AddButtonListener(() => EventManager.Instance.Publish(new OnSkillActivated() { characterID = characterSO.characterID }));
            }
        }
    }

    private void OnCharacterStatChanged(OnCharacterStatChanged eventData)
    {
        // 记录接收到的事件
        // Debug.Log($"[CharacterPanel] 接收到角色状态变化事件 - 角色ID: {eventData.characterID}, 状态类型: {eventData.statType}, 新值: {eventData.newValue}, 变化量: {eventData.changeAmount}");
        
        // 查找对应的角色槽位
        CharacterSlot targetSlot = characterSlots.Find(slot => slot.GetCharacterID() == eventData.characterID);
        if (targetSlot == null)
        {
            Debug.LogWarning($"未找到角色ID：{eventData.characterID} 的Slot");
            return;
        }

        targetSlot.UpdateSlot(eventData.statType, eventData.newValue);
    }

}