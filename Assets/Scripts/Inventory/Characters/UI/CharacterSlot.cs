using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    [Header("角色UI组件")]
    public Image avatar;     // 角色头像
    public Slider hungerBar; // 饥饿值条
    public Slider staminaBar;  // 体力值条

    private string _characterID; // 角色唯一ID

    /// <summary>
    /// 初始化角色槽位
    /// </summary>
    /// <param name="characterSO">角色配置数据</param>
    /// <param name="initHunger">初始饥饿值</param>
    /// <param name="initStamina">初始体力值</param>
    public void InitSlot(CharacterSO characterSO, float initHunger, float initStamina)
    {
        // 检查参数有效性
        if (characterSO == null)
        {
            Debug.LogError("InitSlot: characterSO为空！");
            return;
        }

        if (string.IsNullOrEmpty(characterSO.characterID))
        {
            Debug.LogError("InitSlot: characterID为空！");
            return;
        }

        // 保存角色ID
        _characterID = characterSO.characterID;

        // 更新头像
        if (avatar != null)
        {
            avatar.sprite = characterSO.characterPortrait;
        }
        else
        {
            Debug.LogWarning($"[CharacterSlot] 角色 {_characterID} 的avatar未赋值！");
        }

        // 初始化饥饿值条
        if (hungerBar != null)
        {
            hungerBar.maxValue = characterSO.maxHunger;
            hungerBar.value = Mathf.Clamp(initHunger, 0, characterSO.maxHunger);
        }
        else
        {
            Debug.LogWarning($"[CharacterSlot] 角色 {_characterID} 的hungerBar未赋值！");
        }

        // 初始化体力值条
        if (staminaBar != null)
        {
            staminaBar.maxValue = characterSO.maxStamina;
            staminaBar.value = Mathf.Clamp(initStamina, 0, characterSO.maxStamina);
        }
        else
        {
            Debug.LogWarning($"[CharacterSlot] 角色 {_characterID} 的staminaBar未赋值！");
        }

        Debug.Log($"[CharacterSlot] 初始化角色 {_characterID} 的UI");
    }

    /// <summary>
    /// 更新角色槽位的UI显示
    /// </summary>
    /// <param name="newHunger">新的饥饿值</param>
    /// <param name="newStamina">新的体力值</param>
    public void UpdateSlot(BuffSO.StatType statType, float newValue)
    {
        // 首先检查UI组件是否为空
        if (hungerBar == null)
        {
            Debug.LogError($"[CharacterSlot] 角色 {_characterID} 的hungerBar未赋值！");
            return;
        }
        if (staminaBar == null)
        {
            Debug.LogError($"[CharacterSlot] 角色 {_characterID} 的staminaBar未赋值！");
            return;
        }

        // 确保数值在有效范围内
        if (statType == BuffSO.StatType.Hunger)
        {
            newValue = Mathf.Clamp(newValue, 0, hungerBar.maxValue);
            hungerBar.value = newValue;
        }
        else if (statType == BuffSO.StatType.Stamina)
        {
            newValue = Mathf.Clamp(newValue, 0, staminaBar.maxValue);
            staminaBar.value = newValue;
        }
        else if (statType == BuffSO.StatType.MaxStamina)
        {
            staminaBar.maxValue = newValue;
        }

        // Debug.Log($"[CharacterSlot] 角色 {_characterID} 更新UI - {statType}：{newValue}");
    }

    public string GetCharacterID() => _characterID;

    /// <summary>
    /// 处理物品拖拽到角色上的逻辑
    /// </summary>
    /// <param name="eventData">拖拽事件数据</param>
    // public void OnDrop(PointerEventData eventData)
    // {
    //     // 检查SingleSlotPanel中是否有物品正在被拖拽
    //     if (SingleSlotPanel.currentlyDraggedSlotIndex != -1)
    //     {
    //         // 获取InventoryManager实例
    //         InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
    //         if (inventoryManager != null)
    //         {
    //             // 调用InventoryManager处理物品拖拽到角色的逻辑
    //             inventoryManager.HandleDropOnCharacter(SingleSlotPanel.currentlyDraggedSlotIndex, _characterID);
    //             Debug.Log("[CharacterSlot] 物品拖拽到角色 " + _characterID + " 上");
    //         }
    //         else
    //         {
    //             Debug.LogWarning("[CharacterSlot] InventoryManager is null");
    //         }
    //     }
    // }
}