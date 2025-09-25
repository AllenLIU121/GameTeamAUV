using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    [Header("角色UI组件")]
    public Image avatar;     // 角色头像
    public Slider hungerBar; // 饥饿值条
    public Slider staminaBar;  // 体力值条

    // 技能冷却相关
    [Header("技能冷却UI")]
    [SerializeField] private Image cooldownMask;
    [SerializeField] private TextMeshProUGUI cooldownTimer;
    private float maxCooldown;
    private float currentCooldown;
    private bool isCooldown = false;

    [Header("Buff Prefab")]
    [SerializeField] private GameObject buffPrefab;
    private Dictionary<string, GameObject> buffs = new Dictionary<string, GameObject>();

    public string GetCharacterID() => _characterID;
    private string _characterID; // 角色唯一ID

    private void Awake()
    {
        EventManager.Instance.Subscribe<OnDiseaseContracted>(HandleDiseaseContracted);
        EventManager.Instance.Subscribe<OnDiseaseCured>(HandleDiseaseCured);
        EventManager.Instance.Subscribe<OnCharacterDied>(HandleCharacterDied);
        EventManager.Instance.Subscribe<OnCharacterRevived>(HandleCharacterRevived);
    }

    // 每帧更新UI技能冷却显示
    private void Update()
    {
        if (!isCooldown) return;

        currentCooldown -= Time.deltaTime;
        if (currentCooldown > 0)
        {
            cooldownMask.fillAmount = currentCooldown / maxCooldown;
            cooldownTimer.text = currentCooldown.ToString("F1");
        }
        else
        {
            isCooldown = false;
            cooldownMask.fillAmount = 0f;
            cooldownTimer.text = "";
        }
    }

    private void OnDestroy()
    {
        EventManager.Instance.Subscribe<OnDiseaseContracted>(HandleDiseaseContracted);
        EventManager.Instance.Subscribe<OnDiseaseCured>(HandleDiseaseCured);
        EventManager.Instance.Unsubscribe<OnSkillCooldownStarted>(HandleCooldownStart);
        EventManager.Instance.Unsubscribe<OnSkillCooldownEnded>(HandleCooldownEnd);
        EventManager.Instance.Unsubscribe<OnCharacterDied>(HandleCharacterDied);
        EventManager.Instance.Unsubscribe<OnCharacterRevived>(HandleCharacterRevived);
    }

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
        else
        {
            curCharacterSO = characterSO;
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

        // 是否含有冷却时间UI
        if (characterSO.skill != null && characterSO.skill.cooldownTime > 0f)
        {
            if (cooldownMask == null || cooldownTimer == null)
            {
                Debug.LogError($"[CharacterSlot] Character {characterSO.characterID}'s skill UI components are not found");
            }
            else
            {
                cooldownMask.fillAmount = 0f;
                cooldownTimer.text = "";
            }

            // 订阅技能冷却事件
            EventManager.Instance.Subscribe<OnSkillCooldownStarted>(HandleCooldownStart);
            EventManager.Instance.Subscribe<OnSkillCooldownEnded>(HandleCooldownEnd);
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

    private void HandleDiseaseContracted(OnDiseaseContracted eventData)
    {
        if (eventData.characterID != _characterID) return;

        GameObject diseaseBuffGO = Instantiate(buffPrefab, transform);
        if (!buffs.ContainsKey(eventData.buffSO.buffID))
        {
            buffs[eventData.buffSO.buffID] = diseaseBuffGO;
            diseaseBuffGO.GetComponent<Image>().sprite = eventData.buffSO.icon;
        }
    }

    private void HandleDiseaseCured(OnDiseaseCured eventData)
    {
        if (eventData.characterID != _characterID) return;

        if (buffs.ContainsKey(eventData.buffSO.buffID))
        {
            Destroy(buffs[eventData.buffSO.buffID]);
            buffs.Remove(eventData.buffSO.buffID);
        }
    }

    private void HandleCooldownStart(OnSkillCooldownStarted eventData)
    {
        if (eventData.characterID != _characterID) return;

        isCooldown = true;
        maxCooldown = eventData.maxCooldown;
        currentCooldown = maxCooldown;
    }

    private void HandleCooldownEnd(OnSkillCooldownEnded eventData)
    {
        if (eventData.characterID != _characterID) return;

        isCooldown = false;
        //防Update误差
        cooldownMask.fillAmount = 0;
        cooldownTimer.text = "";
    }

    private void HandleCharacterDied(OnCharacterDied eventData)
    {
        if (eventData.characterSO.characterID != _characterID) return;

        if (buffs.Count > 0)
        {
            foreach (var buffGO in buffs.Values)
            {
                Destroy(buffGO);
            }
            buffs.Clear();
        }

        avatar.color = new Color(0.2f, 0.2f, 0.2f, 1f);
    }

    private void HandleCharacterRevived(OnCharacterRevived eventData)
    {
        if (eventData.characterSO.characterID != _characterID) return;

        avatar.color = new Color(1f, 1f, 1f, 1f);
    }

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